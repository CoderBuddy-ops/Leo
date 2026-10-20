using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Leo.Application.Interfaces;
using Leo.Core.Models;
using Leo.Infrastructure.Security;

namespace Leo.Infrastructure.Providers;

public class GeminiProvider : IAiProvider
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(60) };

    public AiProviderType ProviderType => AiProviderType.Gemini;
    public string CurrentModel { get; set; } = "gemini-2.5-flash";

    public static readonly string[] SupportedModels =
    [
        "gemini-2.5-flash",
        "gemini-2.5-pro",
        "gemini-1.5-flash",
        "gemini-1.5-pro"
    ];

    public string GetApiKey()
    {
        return WindowsCredentialService.GetSecret("GeminiApiKey") ?? string.Empty;
    }

    public void SetApiKey(string key)
    {
        WindowsCredentialService.SaveSecret("GeminiApiKey", key.Trim());
    }

    public string RedactSecret(string message)
    {
        string key = GetApiKey();
        if (string.IsNullOrEmpty(key) || key.Length < 4) return message;
        return message.Replace(key, "[REDACTED_API_KEY]");
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        string apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new ConnectionTestResult
            {
                Ok = false,
                Message = "No Gemini API key configured. Enter your key in Settings.",
                Model = CurrentModel,
                IsLocal = false
            };
        }

        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{CurrentModel}?key={apiKey}";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(6));

            using var response = await HttpClient.GetAsync(endpoint, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                return new ConnectionTestResult
                {
                    Ok = true,
                    Message = $"Connected successfully to Google Gemini ({CurrentModel})",
                    Model = CurrentModel,
                    IsLocal = false
                };
            }

            if ((int)response.StatusCode is 400 or 403)
            {
                return new ConnectionTestResult
                {
                    Ok = false,
                    Message = "Invalid API key or unauthorized access. Please verify your Google AI key.",
                    Model = CurrentModel,
                    IsLocal = false
                };
            }

            return new ConnectionTestResult
            {
                Ok = false,
                Message = $"Google Gemini service returned HTTP {(int)response.StatusCode}",
                Model = CurrentModel,
                IsLocal = false
            };
        }
        catch (Exception ex)
        {
            return new ConnectionTestResult
            {
                Ok = false,
                Message = RedactSecret($"Connection error: {ex.Message}"),
                Model = CurrentModel,
                IsLocal = false
            };
        }
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<ChatMessage> history,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Please enter a Google Gemini API key in Settings.");
        }

        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{CurrentModel}:streamGenerateContent?alt=sse&key={apiKey}";

        var contents = history
            .Where(m => m.Role is "user" or "assistant")
            .Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            })
            .ToList();

        var requestBody = new
        {
            contents,
            generationConfig = new
            {
                temperature = 0.7
            }
        };

        string jsonPayload = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string err = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(RedactSecret($"Gemini error ({response.StatusCode}): {err}"));
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            string dataJson = line[6..].Trim();
            if (dataJson == "[DONE]")
                break;

            string? tokenText = ExtractTextFromChunk(dataJson);
            if (!string.IsNullOrEmpty(tokenText))
            {
                yield return tokenText;
            }
        }
    }

    private static string? ExtractTextFromChunk(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var first = candidates[0];
                if (first.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    if (parts[0].TryGetProperty("text", out var textElem))
                    {
                        return textElem.GetString();
                    }
                }
            }
        }
        catch
        {
            // Ignore incomplete or unparseable SSE chunk
        }
        return null;
    }
}
