using System.Text.RegularExpressions;

namespace FindThatBook.Application.Extensions;

public static class StringExtensions
{
    public static string SanitizeForPrompt(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var safe = input.Length > 200 ? input[..200] : input;
        safe = safe.Replace("<<<", "").Replace(">>>", "");
        return safe.Trim();
    }

    public static string ToNormalizedString(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input.ToLower(), @"[^a-z0-9\s]", "").Trim();
    }

    public static string SanitizeJson(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "{}";
        return input.Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
    }
}