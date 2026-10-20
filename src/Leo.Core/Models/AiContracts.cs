using System;

namespace Leo.Core.Models;

public enum AiProviderType
{
    Gemini,
    Local
}

public class ChatMessage
{
    public string Role { get; set; } = "user"; // "user", "assistant", "system"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ChatMessage() { }

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
        Timestamp = DateTime.UtcNow;
    }
}

public class AiProviderConfig
{
    public AiProviderType ProviderType { get; set; } = AiProviderType.Gemini;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gemini-2.5-flash";
    public string LocalEndpointUrl { get; set; } = "http://127.0.0.1:11434/v1";
    public double Temperature { get; set; } = 0.7;
}

public class ConnectionTestResult
{
    public bool Ok { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
}

public static class NetworkPolicy
{
    /// <summary>
    /// Checks whether the target endpoint is purely a local loopback interface.
    /// Addresses on 127.0.0.1, localhost, ::1, or local subnets are marked local.
    /// </summary>
    public static bool IsLoopback(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        try
        {
            var uri = new Uri(url);
            string host = uri.Host.ToLowerInvariant();
            return host == "localhost" ||
                   host == "127.0.0.1" ||
                   host == "::1" ||
                   host == "[::1]" ||
                   host.StartsWith("192.168.") ||
                   host.StartsWith("10.");
        }
        catch
        {
            return false;
        }
    }
}
