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

namespace Leo.Infrastructure.Providers;

public class LocalAiProvider : IAiProvider
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    public AiProviderType ProviderType => AiProviderType.Local;
    public string CurrentModel { get; set; } = "qwen2.5:0.5b";
    public string EndpointUrl { get; set; } = "http://127.0.0.1:11434/v1";

    public async Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        string modelsUrl = EndpointUrl.TrimEnd('/') + "/models";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            using var response = await HttpClient.GetAsync(modelsUrl, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var models = await DiscoverModelsAsync(cancellationToken);
                string modelSummary = models.Count > 0 ? $"{models.Count} models detected" : "Ready";
                return new ConnectionTestResult
                {
                    Ok = true,
                    Message = $"Local AI endpoint reachable ({modelSummary})",
                    Model = CurrentModel,
                    IsLocal = true
                };
            }

            return new ConnectionTestResult
            {
                Ok = false,
                Message = $"Local server returned HTTP {(int)response.StatusCode}",
                Model = CurrentModel,
                IsLocal = true
            };
        }
        catch (Exception ex)
        {
            return new ConnectionTestResult
            {
                Ok = false,
                Message = $"Cannot connect to local server: {ex.Message}",
                Model = CurrentModel,
                IsLocal = true
            };
        }
    }

    public async Task<List<string>> DiscoverModelsAsync(CancellationToken cancellationToken = default)
    {
        var modelList = new List<string>();
        string modelsUrl = EndpointUrl.TrimEnd('/') + "/models";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            string json = await HttpClient.GetStringAsync(modelsUrl, cts.Token);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out var idElem))
                    {
                        string? id = idElem.GetString();
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            modelList.Add(id);
                        }
                    }
                }
            }
        }
        catch
        {
            // If models list request fails, return empty
        }

        return modelList;
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<ChatMessage> history,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string completionsUrl = EndpointUrl.TrimEnd('/') + "/chat/completions";

        var messages = history
            .Where(m => m.Role is "user" or "assistant" or "system")
            .Select(m => new { role = m.Role, content = m.Content })
            .ToList();

        var requestBody = new
        {
            model = CurrentModel,
            messages,
            stream = true,
            temperature = 0.7
        };

        string jsonPayload = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, completionsUrl)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string err = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Local AI error ({response.StatusCode}): {err}");
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

            string? token = ExtractToken(dataJson);
            if (!string.IsNullOrEmpty(token))
            {
                yield return token;
            }
        }
    }

    private static string? ExtractToken(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var choice = choices[0];
                if (choice.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("content", out var contentElem))
                {
                    return contentElem.GetString();
                }
            }
        }
        catch
        {
            // Ignore incomplete SSE line
        }
        return null;
    }
}
