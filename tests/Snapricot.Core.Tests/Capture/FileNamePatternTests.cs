using Snapricot.Core.Capture;

namespace Snapricot.Core.Tests.Capture;

public sealed class FileNamePatternTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 10, 9, 14, 32, 7, 45, TimeSpan.Zero);

    [Theory]
    [InlineData("{date}_{time}", "2026-10-09_14-32-07")]
    [InlineData("Snapricot {DATE} {Time}.{ms}", "Snapricot 2026-10-09 14-32-07.045")]
    [InlineData("bug-report", "bug-report")]
    public void Format_ReplacesTokens(string pattern, string expected)
    {
        Assert.Equal(expected, FileNamePattern.Format(pattern, Timestamp));
    }

    [Fact]
    public void Format_ReplacesInvalidFileNameCharacters()
    {
        Assert.Equal("a_b_c", FileNamePattern.Format("a/b\0c", Timestamp));
    }

    [Fact]
    public void Format_TrimsSurroundingWhitespace()
    {
        Assert.Equal("shot", FileNamePattern.Format("  shot  ", Timestamp));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Format_BlankPattern_Throws(string pattern)
    {
        Assert.Throws<ArgumentException>(() => FileNamePattern.Format(pattern, Timestamp));
    }

    [Fact]
    public void Default_ProducesDateAndTime()
    {
        Assert.Equal("2026-10-09_14-32-07", FileNamePattern.Format(FileNamePattern.Default, Timestamp));
    }
}
