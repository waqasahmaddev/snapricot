using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Snapricot.Core.Modules;
using Snapricot.Core.Platform;
using Snapricot.Core.Settings;

namespace Snapricot.Core.Hotkeys;

public enum HotkeyStatus
{
    /// <summary>Registered with the OS and working.</summary>
    Active,

    /// <summary>Another app owns this combination.</summary>
    Unavailable,

    /// <summary>The saved text isn't a valid gesture.</summary>
    Invalid,

    /// <summary>Temporarily off (e.g. while the user records a new shortcut).</summary>
    Suspended,
}

/// <summary>The live state of one hotkey, for the Settings page.</summary>
public sealed record HotkeyState(HotkeyBinding Binding, string Gesture, HotkeyStatus Status)
{
    public bool IsDefault => string.Equals(Gesture, Binding.DefaultGesture, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Persisted user overrides (section <c>"hotkeys"</c>): binding id → gesture text.</summary>
public sealed record HotkeySettings
{
    public Dictionary<string, string> Overrides { get; set; } = [];
}

[JsonSerializable(typeof(HotkeySettings))]
internal sealed partial class HotkeySettingsJson : JsonSerializerContext;

/// <summary>
/// Owns every module's global hotkey: registers defaults or user overrides at startup, rebinds from Settings
/// (rolling back if the new combination is taken), and reports live status. A hotkey that fails never stops startup.
/// </summary>
public sealed partial class HotkeyManager(
    ModuleCatalog catalog,
    IHotkeyService hotkeys,
    ISettingsStore store,
    IServiceProvider services,
    ILogger<HotkeyManager> logger) : IDisposable
{
    public const string Section = "hotkeys";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<string, IDisposable> _registrations = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HotkeyState> _states = new(StringComparer.Ordinal);

    /// <summary>How handlers are run; the shell sets this to marshal onto the UI thread.</summary>
    public Func<Func<Task>, Task> Dispatch { get; set; } = work => work();

    /// <summary>Raised after any state change (status or gesture).</summary>
    public event Action? Changed;

    public IReadOnlyList<HotkeyState> States =>
        [.. catalog.Hotkeys.Select(b => _states.TryGetValue(b.Id, out var s) ? s : new HotkeyState(b, b.DefaultGesture, HotkeyStatus.Suspended))];

    private HotkeySettings Settings => store.Load(Section, HotkeySettingsJson.Default.HotkeySettings, () => new HotkeySettings());

    public async Task RegisterAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var overrides = Settings.Overrides;
            foreach (var binding in catalog.Hotkeys)
            {
                var gesture = overrides.TryGetValue(binding.Id, out var custom) ? custom : binding.DefaultGesture;
                await RegisterAsync(binding, gesture, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _gate.Release();
        }

        Changed?.Invoke();
    }

    /// <summary>
    /// Switches <paramref name="bindingId"/> to <paramref name="gestureText"/>. If the OS refuses it, the previous
    /// gesture is restored and the result says why. Successful changes are saved.
    /// </summary>
    public async Task<HotkeyStatus> RebindAsync(string bindingId, string gestureText, CancellationToken cancellationToken)
    {
        var binding = catalog.Hotkeys.FirstOrDefault(b => b.Id == bindingId)
            ?? throw new ArgumentException($"Unknown hotkey '{bindingId}'.", nameof(bindingId));
        if (!HotkeyGesture.TryParse(gestureText, out var parsed))
        {
            return HotkeyStatus.Invalid;
        }

        HotkeyStatus status;
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var previous = _states.TryGetValue(bindingId, out var old) ? old.Gesture : binding.DefaultGesture;
            Unregister(bindingId);

            status = await RegisterAsync(binding, parsed.ToString(), cancellationToken).ConfigureAwait(false);
            if (status == HotkeyStatus.Active)
            {
                SaveOverride(binding, parsed.ToString());
            }
            else
            {
                await RegisterAsync(binding, previous, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _gate.Release();
        }

        Changed?.Invoke();
        return status;
    }

    /// <summary>Clears every override and re-registers the defaults.</summary>
    public async Task ResetAllAsync(CancellationToken cancellationToken)
    {
        store.Save(Section, new HotkeySettings(), HotkeySettingsJson.Default.HotkeySettings);
        await SuspendAsync(cancellationToken).ConfigureAwait(false);
        await RegisterAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Unregisters everything (so a key press can be recorded), keeping the chosen gestures.</summary>
    public async Task SuspendAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var id in _registrations.Keys.ToList())
            {
                Unregister(id);
            }
        }
        finally
        {
            _gate.Release();
        }

        Changed?.Invoke();
    }

    /// <summary>Re-registers everything after <see cref="SuspendAsync"/>.</summary>
    public Task ResumeAsync(CancellationToken cancellationToken) => RegisterAllAsync(cancellationToken);

    public void Dispose()
    {
        foreach (var registration in _registrations.Values)
        {
            registration.Dispose();
        }

        _registrations.Clear();
        _gate.Dispose();
    }

    private async Task<HotkeyStatus> RegisterAsync(HotkeyBinding binding, string gestureText, CancellationToken cancellationToken)
    {
        Unregister(binding.Id);

        HotkeyStatus status;
        if (!HotkeyGesture.TryParse(gestureText, out var gesture))
        {
            status = HotkeyStatus.Invalid;
            LogInvalid(binding.Id, gestureText);
        }
        else
        {
            try
            {
                _registrations[binding.Id] = await hotkeys.RegisterAsync(
                    gesture,
                    token => Dispatch(() => binding.Execute(services, token)),
                    cancellationToken).ConfigureAwait(false);
                status = HotkeyStatus.Active;
                LogRegistered(binding.Id, gesture.ToString());
            }
            catch (HotkeyUnavailableException)
            {
                status = HotkeyStatus.Unavailable;
                LogUnavailable(binding.Id, gesture.ToString());
            }

            gestureText = gesture.ToString();
        }

        _states[binding.Id] = new HotkeyState(binding, Display(gestureText), status);
        return status;
    }

    private void Unregister(string bindingId)
    {
        if (_registrations.Remove(bindingId, out var registration))
        {
            registration.Dispose();
        }

        if (_states.TryGetValue(bindingId, out var state))
        {
            _states[bindingId] = state with { Status = HotkeyStatus.Suspended };
        }
    }

    private void SaveOverride(HotkeyBinding binding, string gesture)
    {
        var overrides = new Dictionary<string, string>(Settings.Overrides, StringComparer.Ordinal);
        var isDefault = HotkeyGesture.TryParse(binding.DefaultGesture, out var defaultGesture) && defaultGesture.ToString() == gesture;
        if (isDefault)
        {
            overrides.Remove(binding.Id);
        }
        else
        {
            overrides[binding.Id] = gesture;
        }

        store.Save(Section, new HotkeySettings { Overrides = overrides }, HotkeySettingsJson.Default.HotkeySettings);
    }

    /// <summary>Friendly form for the UI: <c>Ctrl+PrintScreen</c> → <c>Ctrl+PrtSc</c>.</summary>
    private static string Display(string gesture) => gesture.Replace("PrintScreen", "PrtSc", StringComparison.Ordinal);

    [LoggerMessage(Level = LogLevel.Information, Message = "Hotkey {HotkeyId} registered as {Gesture}")]
    private partial void LogRegistered(string hotkeyId, string gesture);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Hotkey {HotkeyId} ({Gesture}) is in use by another app")]
    private partial void LogUnavailable(string hotkeyId, string gesture);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Hotkey {HotkeyId} has an invalid gesture '{Gesture}'")]
    private partial void LogInvalid(string hotkeyId, string gesture);
}
