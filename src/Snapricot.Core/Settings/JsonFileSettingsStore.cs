using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Snapricot.Core.Settings;

/// <summary>
/// Stores all settings in a single JSON file (by default <c>%APPDATA%\Snapricot\settings.json</c>).
/// Writes are atomic (temp file, then replace), so a crash never leaves a half-written file.
/// A missing or corrupt file simply means "use defaults".
/// </summary>
public sealed class JsonFileSettingsStore(string filePath) : ISettingsStore
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private readonly Lock _gate = new();
    private JsonObject? _root;

    public string FilePath { get; } = filePath;

    public T Load<T>(string section, JsonTypeInfo<T> typeInfo, Func<T> defaults)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(section);
        ArgumentNullException.ThrowIfNull(typeInfo);
        ArgumentNullException.ThrowIfNull(defaults);

        lock (_gate)
        {
            try
            {
                return Root()[section] is { } node && node.Deserialize(typeInfo) is { } value ? value : defaults();
            }
            catch (JsonException)
            {
                return defaults();
            }
        }
    }

    public void Save<T>(string section, T value, JsonTypeInfo<T> typeInfo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(section);
        ArgumentNullException.ThrowIfNull(typeInfo);

        lock (_gate)
        {
            var root = Root();
            root[section] = JsonSerializer.SerializeToNode(value, typeInfo);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
            var temp = FilePath + ".tmp";
            File.WriteAllText(temp, root.ToJsonString(WriteOptions));
            File.Move(temp, FilePath, overwrite: true);
        }
    }

    private JsonObject Root()
    {
        if (_root is not null)
        {
            return _root;
        }

        try
        {
            _root = File.Exists(FilePath) ? JsonNode.Parse(File.ReadAllText(FilePath)) as JsonObject : null;
        }
        catch (JsonException)
        {
            _root = null; // corrupt file: start fresh; the next Save replaces it
        }

        return _root ??= [];
    }
}
