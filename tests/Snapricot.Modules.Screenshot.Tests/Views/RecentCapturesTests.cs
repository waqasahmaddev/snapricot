using Snapricot.Modules.Screenshot.Tests.Capture;
using Snapricot.Modules.Screenshot.Views;

namespace Snapricot.Modules.Screenshot.Tests.Views;

public sealed class RecentCapturesTests : IDisposable
{
    private readonly TempFolder _temp = new();

    public void Dispose() => _temp.Dispose();

    private string Touch(string name, DateTime written)
    {
        var path = Path.Combine(_temp.Path, name);
        File.WriteAllBytes(path, [1]);
        File.SetLastWriteTime(path, written);
        return path;
    }

    [Fact]
    public void Find_ReturnsImagesNewestFirst_AndRespectsMax()
    {
        var now = DateTime.Now;
        Touch("old.png", now.AddHours(-3));
        Touch("newest.jpg", now);
        Touch("middle.jpeg", now.AddHours(-1));
        Touch("notes.txt", now.AddMinutes(1));

        var found = RecentCaptures.Find(_temp.Path, max: 2);

        Assert.Equal(["newest.jpg", "middle.jpeg"], found.Select(f => f.Name));
    }

    [Fact]
    public void Find_MissingFolder_ReturnsNothing()
    {
        Assert.Empty(RecentCaptures.Find(Path.Combine(_temp.Path, "nope"), 8));
    }

    [Fact]
    public void Find_ZeroMax_ReturnsNothing()
    {
        Touch("a.png", DateTime.Now);

        Assert.Empty(RecentCaptures.Find(_temp.Path, 0));
    }

    [Theory]
    [InlineData("2026-10-09 13:34", "13:34 · PNG")]
    [InlineData("2026-10-08 22:00", "Yesterday · PNG")]
    public void Caption_TodayAndYesterday(string written, string expected)
    {
        var now = new DateTime(2026, 10, 9, 15, 0, 0);

        Assert.Equal(expected, RecentCaptures.Caption(DateTime.Parse(written, System.Globalization.CultureInfo.InvariantCulture), now, ".png"));
    }

    [Fact]
    public void Caption_OlderDatesAndJpegName()
    {
        var now = new DateTime(2026, 10, 9);

        Assert.EndsWith("· JPG", RecentCaptures.Caption(new DateTime(2026, 3, 2), now, ".jpeg"), StringComparison.Ordinal);
        Assert.Contains("2025", RecentCaptures.Caption(new DateTime(2025, 3, 2), now, ".png"), StringComparison.Ordinal);
    }
}
