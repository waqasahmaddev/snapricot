using Snapricot.Modules.Screenshot.Capture;

namespace Snapricot.Modules.Screenshot.Tests.Capture;

public sealed class FileNameRulesTests
{
    [Theory]
    [InlineData("login-error", "login-error")]
    [InlineData("  spaced  ", "spaced")]
    [InlineData("shot.png", "shot")]
    [InlineData("shot.PNG ", "shot")]
    [InlineData("shot.jpg", "shot")]
    [InlineData("shot.JPEG", "shot")]
    [InlineData("v1.2 notes", "v1.2 notes")]
    public void Normalize_TrimsAndDropsTypedExtension(string input, string expected)
    {
        Assert.Equal(expected, FileNameRules.Normalize(input));
    }

    [Theory]
    [InlineData("2026-10-09_13-11-20")]
    [InlineData("Bug report (login)")]
    [InlineData("ünïcødé 截图")]
    public void Validate_GoodNames_ReturnsNull(string name)
    {
        Assert.Null(FileNameRules.Validate(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(".png")]
    [InlineData("a/b")]
    [InlineData("a:b")]
    [InlineData("what?")]
    [InlineData("ends with dot.")]
    [InlineData("CON")]
    [InlineData("lpt1")]
    public void Validate_BadNames_ReturnsAMessage(string name)
    {
        Assert.NotNull(FileNameRules.Validate(name));
    }

    [Fact]
    public void Validate_TooLong_ReturnsAMessage()
    {
        Assert.NotNull(FileNameRules.Validate(new string('a', 201)));
    }
}
