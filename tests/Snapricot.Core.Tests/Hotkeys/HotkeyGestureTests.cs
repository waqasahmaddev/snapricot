using Snapricot.Core.Hotkeys;

namespace Snapricot.Core.Tests.Hotkeys;

public sealed class HotkeyGestureTests
{
    [Theory]
    [InlineData("Ctrl+PrtSc", HotkeyModifiers.Ctrl, "PrintScreen")]
    [InlineData("alt + printscreen", HotkeyModifiers.Alt, "PrintScreen")]
    [InlineData("Shift+Print", HotkeyModifiers.Shift, "PrintScreen")]
    [InlineData("Ctrl+Shift+s", HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, "S")]
    [InlineData("Control+Option+Cmd+7", HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Meta, "7")]
    [InlineData("Win+F12", HotkeyModifiers.Meta, "F12")]
    [InlineData("PrtSc", HotkeyModifiers.None, "PrintScreen")]
    [InlineData("Ctrl+PgUp", HotkeyModifiers.Ctrl, "PageUp")]
    public void Parse_ValidText_ReturnsNormalizedGesture(string text, HotkeyModifiers modifiers, string key)
    {
        var gesture = HotkeyGesture.Parse(text);

        Assert.Equal(new HotkeyGesture(modifiers, key), gesture);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ctrl+")]
    [InlineData("Ctrl+Shift")]
    [InlineData("Ctrl+A+B")]
    [InlineData("Ctrl+F25")]
    [InlineData("Ctrl+Banana")]
    [InlineData("Ctrl++A")]
    public void TryParse_InvalidText_ReturnsFalse(string text)
    {
        Assert.False(HotkeyGesture.TryParse(text, out _));
    }

    [Fact]
    public void Parse_InvalidText_ThrowsInvalidHotkeyGestureException()
    {
        Assert.Throws<InvalidHotkeyGestureException>(() => HotkeyGesture.Parse("Ctrl+Banana"));
    }

    [Fact]
    public void ToString_UsesCanonicalModifierOrder()
    {
        var gesture = HotkeyGesture.Parse("Win+Shift+Alt+Ctrl+S");

        Assert.Equal("Ctrl+Alt+Shift+Meta+S", gesture.ToString());
    }

    [Fact]
    public void ToString_RoundTripsThroughParse()
    {
        var gesture = HotkeyGesture.Parse("shift+prtsc");

        Assert.Equal(gesture, HotkeyGesture.Parse(gesture.ToString()));
    }
}
