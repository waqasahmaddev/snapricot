using Snapricot.Core.Hotkeys;
using static Snapricot.Platform.Windows.Interop.NativeMethods;

namespace Snapricot.Platform.Windows;

/// <summary>Maps platform-neutral <see cref="HotkeyGesture"/>s to Win32 modifier flags and virtual-key codes.</summary>
internal static class Win32Keys
{
    private static readonly Dictionary<string, uint> NamedKeys = new(StringComparer.Ordinal)
    {
        ["PrintScreen"] = 0x2C,
        ["Space"] = 0x20,
        ["PageUp"] = 0x21,
        ["PageDown"] = 0x22,
        ["End"] = 0x23,
        ["Home"] = 0x24,
        ["Insert"] = 0x2D,
        ["Delete"] = 0x2E,
        ["Pause"] = 0x13,
    };

    public static (uint Modifiers, uint VirtualKey) ToNative(HotkeyGesture gesture)
    {
        var modifiers = 0u;
        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Ctrl))
        {
            modifiers |= MOD_CONTROL;
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            modifiers |= MOD_ALT;
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            modifiers |= MOD_SHIFT;
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Meta))
        {
            modifiers |= MOD_WIN;
        }

        return (modifiers, ToVirtualKey(gesture.Key));
    }

    private static uint ToVirtualKey(string key)
    {
        if (NamedKeys.TryGetValue(key, out var named))
        {
            return named;
        }

        if (key.Length == 1 && char.IsAsciiLetterOrDigit(key[0]))
        {
            return char.ToUpperInvariant(key[0]); // VK_A..VK_Z and VK_0..VK_9 match ASCII
        }

        if (key.StartsWith('F') && int.TryParse(key.AsSpan(1), out var number) && number is >= 1 and <= 24)
        {
            return (uint)(0x70 + number - 1); // VK_F1 = 0x70
        }

        throw new InvalidHotkeyGestureException(key);
    }
}
