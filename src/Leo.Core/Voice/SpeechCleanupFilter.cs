using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Leo.Core.Voice;

public class SpeechCleanupOptions
{
    public bool RemoveFillers { get; set; } = true;
    public bool ResolveCorrections { get; set; } = true;
    public bool FormatBulletLists { get; set; } = true;
    public bool VerbatimMode { get; set; } = false;
}

/// <summary>
/// Speech Transcript Cleanup and Formatting Pipeline.
/// Implements deterministic rule-based transformations to remove verbal filler words,
/// resolve spoken self-corrections, and format spoken lists cleanly.
/// </summary>
public static class SpeechCleanupFilter
{
    private static readonly Regex FillersRegex = new(
        @"\b(um|uh|er|ah|like,\s*actually|you know)\b[\s,]*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex BracketFillersRegex = new(
        @"\[(?:um|uh),?\]\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SelfCorrectionRegex1 = new(
        @"([a-zA-Z0-9_\-]+),\s*(?:actually|wait no|I mean|make that)\s+([a-zA-Z0-9_\-]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SelfCorrectionRegex2 = new(
        @"\[([^\]]+),\s*actually\]\s*([a-zA-Z0-9_\-]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex BulletPointSplitRegex = new(
        @"\{?Bullet point\}?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MultiSpaceRegex = new(
        @"\s{2,}",
        RegexOptions.Compiled);

    public static string Clean(string? transcript, SpeechCleanupOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(transcript))
            return transcript ?? string.Empty;

        options ??= new SpeechCleanupOptions();
        if (options.VerbatimMode)
            return transcript;

        string text = transcript;

        // 1. Resolve spoken self-corrections (e.g. "Thursday, actually Friday" -> "Friday")
        if (options.ResolveCorrections)
        {
            text = SelfCorrectionRegex1.Replace(text, "$2");
            text = SelfCorrectionRegex2.Replace(text, "$2");
        }

        // 2. Remove filler words (ums, uhs)
        if (options.RemoveFillers)
        {
            text = FillersRegex.Replace(text, string.Empty);
            text = BracketFillersRegex.Replace(text, string.Empty);
        }

        // 3. Convert spoken list syntax into clean markdown bullet points
        if (options.FormatBulletLists && BulletPointSplitRegex.IsMatch(text))
        {
            var parts = BulletPointSplitRegex.Split(text);
            string header = parts[0].Trim();
            var items = parts.Skip(1)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(item =>
                {
                    string trimmed = item.Trim();
                    if (trimmed.Length == 0) return "-";
                    return "- " + char.ToUpperInvariant(trimmed[0]) + trimmed[1..];
                });

            text = string.IsNullOrEmpty(header)
                ? string.Join("\n", items)
                : header + "\n" + string.Join("\n", items);
        }

        // 4. Clean up whitespace and punctuation
        text = MultiSpaceRegex.Replace(text, " ").Trim();
        if (text.Length > 0)
        {
            text = char.ToUpperInvariant(text[0]) + text[1..];
            if (!text.EndsWith('.') && !text.EndsWith('!') && !text.EndsWith('?') && !text.Contains('\n'))
            {
                text += ".";
            }
        }

        return text;
    }
}
