using System.Globalization;

namespace Snapricot.Core.Capture;

/// <summary>
/// Turns a pattern such as <c>{date}_{time}</c> into a file name (without extension).
/// Tokens: <c>{date}</c> = 2026-10-09, <c>{time}</c> = 14-32-07, <c>{ms}</c> = 045.
/// </summary>
public static class FileNamePattern
{
    public const string Default = "{date}_{time}";

    public static string Format(string pattern, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        var name = pattern
            .Replace("{date}", timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{time}", timestamp.ToString("HH-mm-ss", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{ms}", timestamp.ToString("fff", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);

        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c)).Trim();
    }
}
