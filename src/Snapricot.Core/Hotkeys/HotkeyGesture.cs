using System.Text;

namespace Snapricot.Core.Hotkeys;

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Ctrl = 1,
    Alt = 2,
    Shift = 4,

    /// <summary>The Windows key on Windows, Command on macOS.</summary>
    Meta = 8,
}

/// <summary>
/// A platform-neutral global hotkey, e.g. <c>Ctrl+PrintScreen</c>. Platform projects map <see cref="Key"/> to native key codes.
/// Key names: <c>A</c>–<c>Z</c>, <c>0</c>–<c>9</c>, <c>F1</c>–<c>F24</c>, <c>PrintScreen</c>, <c>Space</c>, <c>Insert</c>,
/// <c>Delete</c>, <c>Home</c>, <c>End</c>, <c>PageUp</c>, <c>PageDown</c>, <c>Pause</c>.
/// </summary>
public sealed record HotkeyGesture(HotkeyModifiers Modifiers, string Key)
{
    private static readonly Dictionary<string, HotkeyModifiers> ModifierNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ctrl"] = HotkeyModifiers.Ctrl,
        ["control"] = HotkeyModifiers.Ctrl,
        ["alt"] = HotkeyModifiers.Alt,
        ["option"] = HotkeyModifiers.Alt,
        ["shift"] = HotkeyModifiers.Shift,
        ["win"] = HotkeyModifiers.Meta,
        ["meta"] = HotkeyModifiers.Meta,
        ["cmd"] = HotkeyModifiers.Meta,
        ["command"] = HotkeyModifiers.Meta,
    };

    private static readonly Dictionary<string, string> NamedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["printscreen"] = "PrintScreen",
        ["prtsc"] = "PrintScreen",
        ["prtscn"] = "PrintScreen",
        ["print"] = "PrintScreen",
        ["space"] = "Space",
        ["insert"] = "Insert",
        ["ins"] = "Insert",
        ["delete"] = "Delete",
        ["del"] = "Delete",
        ["home"] = "Home",
        ["end"] = "End",
        ["pageup"] = "PageUp",
        ["pgup"] = "PageUp",
        ["pagedown"] = "PageDown",
        ["pgdn"] = "PageDown",
        ["pause"] = "Pause",
    };

    public static HotkeyGesture Parse(string text) =>
        TryParse(text, out var gesture) ? gesture : throw new InvalidHotkeyGestureException(text);

    public static bool TryParse(string? text, out HotkeyGesture gesture)
    {
        gesture = null!;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var modifiers = HotkeyModifiers.None;
        string? key = null;

        foreach (var rawPart in text.Split('+'))
        {
            var part = rawPart.Trim();
            if (part.Length == 0)
            {
                return false;
            }

            if (ModifierNames.TryGetValue(part, out var modifier))
            {
                modifiers |= modifier;
            }
            else if (key is null && TryNormalizeKey(part, out var normalized))
            {
                key = normalized;
            }
            else
            {
                return false;
            }
        }

        if (key is null)
        {
            return false;
        }

        gesture = new HotkeyGesture(modifiers, key);
        return true;
    }

    /// <summary>Canonical form, e.g. <c>Ctrl+Shift+PrintScreen</c>.</summary>
    public override string ToString()
    {
        var builder = new StringBuilder();
        Append(HotkeyModifiers.Ctrl, "Ctrl");
        Append(HotkeyModifiers.Alt, "Alt");
        Append(HotkeyModifiers.Shift, "Shift");
        Append(HotkeyModifiers.Meta, "Meta");
        return builder.Append(Key).ToString();

        void Append(HotkeyModifiers flag, string name)
        {
            if (Modifiers.HasFlag(flag))
            {
                builder.Append(name).Append('+');
            }
        }
    }

    private static bool TryNormalizeKey(string part, out string key)
    {
        if (NamedKeys.TryGetValue(part, out key!))
        {
            return true;
        }

        if (part.Length == 1 && char.IsAsciiLetterOrDigit(part[0]))
        {
            key = part.ToUpperInvariant();
            return true;
        }

        if (part.Length is 2 or 3 && part[0] is 'F' or 'f' && int.TryParse(part.AsSpan(1), out var number) && number is >= 1 and <= 24)
        {
            key = $"F{number}";
            return true;
        }

        key = string.Empty;
        return false;
    }
}

public sealed class InvalidHotkeyGestureException(string? text)
    : FormatException($"'{text}' is not a valid hotkey. Use a form like 'Ctrl+Shift+S' or 'Ctrl+PrintScreen'.");

/// <summary>Thrown when the OS refuses a hotkey, usually because another app already owns it.</summary>
public sealed class HotkeyUnavailableException(HotkeyGesture gesture)
    : InvalidOperationException($"The hotkey {gesture} is already in use by another app.")
{
    public HotkeyGesture Gesture { get; } = gesture;
}
