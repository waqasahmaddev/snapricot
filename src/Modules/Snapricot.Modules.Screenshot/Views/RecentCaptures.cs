using System.Globalization;

namespace Snapricot.Modules.Screenshot.Views;

/// <summary>Finds the newest captures in a folder and labels them for the home screen's Recent strip.</summary>
public static class RecentCaptures
{
    private static readonly string[] Extensions = [".png", ".jpg", ".jpeg"];

    /// <summary>The newest image files in <paramref name="folder"/>, newest first. A missing folder yields nothing.</summary>
    public static IReadOnlyList<FileInfo> Find(string folder, int max)
    {
        if (max <= 0 || !Directory.Exists(folder))
        {
            return [];
        }

        try
        {
            return
            [
                .. new DirectoryInfo(folder).EnumerateFiles()
                    .Where(f => Extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .Take(max),
            ];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    /// <summary><c>13:34</c> today, <c>Yesterday</c>, <c>Oct 8</c> this year, otherwise <c>Oct 8, 2025</c>; plus the format.</summary>
    public static string Caption(DateTime written, DateTime now, string extension)
    {
        var day = written.Date == now.Date ? written.ToString("HH:mm", CultureInfo.CurrentCulture)
            : written.Date == now.Date.AddDays(-1) ? "Yesterday"
            : written.Year == now.Year ? written.ToString("MMM d", CultureInfo.CurrentCulture)
            : written.ToString("MMM d, yyyy", CultureInfo.CurrentCulture);
        var format = extension.TrimStart('.').ToUpperInvariant() is "JPEG" ? "JPG" : extension.TrimStart('.').ToUpperInvariant();
        return $"{day} · {format}";
    }
}
