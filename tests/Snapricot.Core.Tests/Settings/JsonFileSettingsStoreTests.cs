using System.Text.Json.Serialization;
using Snapricot.Core.Settings;

namespace Snapricot.Core.Tests.Settings;

public sealed record SampleSettings(string? Folder, int Count);

[JsonSerializable(typeof(SampleSettings))]
internal sealed partial class SampleSettingsJson : JsonSerializerContext;

public sealed class JsonFileSettingsStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "snapricot-tests", Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_directory, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Load_WhenFileMissing_ReturnsDefaults()
    {
        var store = new JsonFileSettingsStore(FilePath);

        var settings = store.Load("screenshot", SampleSettingsJson.Default.SampleSettings, () => new SampleSettings("default", 1));

        Assert.Equal(new SampleSettings("default", 1), settings);
    }

    [Fact]
    public void Save_ThenLoadFromNewStore_RoundTrips()
    {
        new JsonFileSettingsStore(FilePath).Save("screenshot", new SampleSettings(@"D:\Shots", 3), SampleSettingsJson.Default.SampleSettings);

        var loaded = new JsonFileSettingsStore(FilePath).Load("screenshot", SampleSettingsJson.Default.SampleSettings, () => new SampleSettings(null, 0));

        Assert.Equal(new SampleSettings(@"D:\Shots", 3), loaded);
    }

    [Fact]
    public void Sections_AreIndependent()
    {
        var store = new JsonFileSettingsStore(FilePath);
        store.Save("a", new SampleSettings("A", 1), SampleSettingsJson.Default.SampleSettings);
        store.Save("b", new SampleSettings("B", 2), SampleSettingsJson.Default.SampleSettings);

        var reloaded = new JsonFileSettingsStore(FilePath);

        Assert.Equal("A", reloaded.Load("a", SampleSettingsJson.Default.SampleSettings, () => null!).Folder);
        Assert.Equal("B", reloaded.Load("b", SampleSettingsJson.Default.SampleSettings, () => null!).Folder);
    }

    [Fact]
    public void Load_WhenFileIsCorrupt_ReturnsDefaults_AndNextSaveRepairsIt()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(FilePath, "{ not json");
        var store = new JsonFileSettingsStore(FilePath);

        Assert.Equal(0, store.Load("s", SampleSettingsJson.Default.SampleSettings, () => new SampleSettings(null, 0)).Count);

        store.Save("s", new SampleSettings(null, 5), SampleSettingsJson.Default.SampleSettings);
        Assert.Equal(5, new JsonFileSettingsStore(FilePath).Load("s", SampleSettingsJson.Default.SampleSettings, () => null!).Count);
    }

    [Fact]
    public void Load_WhenSectionHasWrongShape_ReturnsDefaults()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(FilePath, """{ "s": "not an object" }""");

        var settings = new JsonFileSettingsStore(FilePath).Load("s", SampleSettingsJson.Default.SampleSettings, () => new SampleSettings("d", 0));

        Assert.Equal("d", settings.Folder);
    }
}
