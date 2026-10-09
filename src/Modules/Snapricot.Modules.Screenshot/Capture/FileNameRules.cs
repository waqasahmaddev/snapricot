namespace Snapricot.Modules.Screenshot.Capture;

/// <summary>Validates a file name typed in the Save dialog (without extension).</summary>
public static class FileNameRules
{
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    // Windows rules, applied on every OS so a name that works here works everywhere.
    private static readonly char[] InvalidCharacters = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

    private static readonly string[] TypedExtensions = [".png", ".jpg", ".jpeg"];

    /// <summary>Trims whitespace and a typed ".png"/".jpg" (the extension is added automatically).</summary>
    public static string Normalize(string? name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        var extension = TypedExtensions.FirstOrDefault(e => trimmed.EndsWith(e, StringComparison.OrdinalIgnoreCase));
        return extension is null ? trimmed : trimmed[..^extension.Length].TrimEnd();
    }

    /// <summary>Returns an error message to show, or <see langword="null"/> when the name is fine.</summary>
    public static string? Validate(string? name)
    {
        var normalized = Normalize(name);
        if (normalized.Length == 0)
        {
            return "Enter a file name.";
        }

        if (normalized.IndexOfAny(InvalidCharacters) >= 0 || normalized.Any(char.IsControl))
        {
            return "A file name can't contain \\ / : * ? \" < > |";
        }

        if (normalized.EndsWith('.'))
        {
            return "A file name can't end with a dot.";
        }

        if (ReservedNames.Contains(normalized))
        {
            return $"\"{normalized}\" is reserved by Windows. Try another name.";
        }

        return normalized.Length > 200 ? "That name is too long. Keep it under 200 characters." : null;
    }
}
