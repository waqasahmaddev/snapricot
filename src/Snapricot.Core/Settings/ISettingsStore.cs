using System.Text.Json.Serialization.Metadata;

namespace Snapricot.Core.Settings;

/// <summary>
/// Persistent user settings, one JSON section per module (e.g. <c>"screenshot"</c>).
/// Callers pass source-generated <see cref="JsonTypeInfo{T}"/> so serialization stays Native AOT safe (ADR-0012).
/// </summary>
public interface ISettingsStore
{
    /// <summary>Returns the saved section, or <paramref name="defaults"/> when it's missing or unreadable.</summary>
    T Load<T>(string section, JsonTypeInfo<T> typeInfo, Func<T> defaults);

    void Save<T>(string section, T value, JsonTypeInfo<T> typeInfo);
}
