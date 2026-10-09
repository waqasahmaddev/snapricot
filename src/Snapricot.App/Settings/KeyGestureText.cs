using Avalonia.Input;

namespace Snapricot.App.Settings;

/// <summary>Turns a key press recorded in the UI into gesture text that <c>HotkeyGesture.Parse</c> accepts.</summary>
internal static class KeyGestureText
{
    /// <summary>
    /// Returns <see langword="null"/> while only modifiers are held, or for keys a hotkey can't use.
    /// Plain letters/digits need a modifier, so typing never triggers a capture by accident.
    /// </summary>
    public static string? From(Key key, KeyModifiers modifiers)
    {
        var name = key switch
        {
            >= Key.A and <= Key.Z => key.ToString(),
            >= Key.D0 and <= Key.D9 => ((char)('0' + (key - Key.D0))).ToString(),
            >= Key.F1 and <= Key.F24 => key.ToString(),
            Key.Snapshot => "PrintScreen",
            Key.Space => "Space",
            Key.Insert => "Insert",
            Key.Delete => "Delete",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PageUp",
            Key.PageDown => "PageDown",
            Key.Pause => "Pause",
            _ => null,
        };
        if (name is null)
        {
            return null;
        }

        var parts = new List<string>(4);
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            parts.Add("Win");
        }

        var standsAlone = name is "PrintScreen" or "Pause" || key is >= Key.F1 and <= Key.F24;
        if (parts.Count == 0 && !standsAlone)
        {
            return null;
        }

        parts.Add(name);
        return string.Join('+', parts);
    }
}
