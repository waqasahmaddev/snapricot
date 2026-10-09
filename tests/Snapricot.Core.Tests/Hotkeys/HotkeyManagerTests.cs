using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Snapricot.Core.Hotkeys;
using Snapricot.Core.Modules;
using Snapricot.Core.Platform;
using Snapricot.Core.Settings;

namespace Snapricot.Core.Tests.Hotkeys;

public sealed class HotkeyManagerTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "snapricot-tests", Guid.NewGuid().ToString("N"));
    private readonly FakeHotkeys _os = new();
    private readonly List<string> _executed = [];
    private readonly ModuleCatalog _catalog;
    private readonly ServiceProvider _services = new ServiceCollection().BuildServiceProvider();

    public HotkeyManagerTests()
    {
        _catalog = ModuleLoader.Load(new ServiceCollection(), [new HotkeyModule(_executed)]);
    }

    public void Dispose()
    {
        _services.Dispose();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private HotkeyManager Create() =>
        new(_catalog, _os, new JsonFileSettingsStore(Path.Combine(_directory, "settings.json")), _services, NullLogger<HotkeyManager>.Instance);

    private static HotkeyState State(HotkeyManager manager, string id) => manager.States.Single(s => s.Binding.Id == id);

    [Fact]
    public async Task RegisterAll_UsesDefaults_AndShowsFriendlyNames()
    {
        using var manager = Create();

        await manager.RegisterAllAsync(Token);

        Assert.Equal(["Alt+PrintScreen", "Ctrl+PrintScreen"], _os.Active.Keys.Order());
        Assert.Equal(new HotkeyState(State(manager, "region").Binding, "Ctrl+PrtSc", HotkeyStatus.Active), State(manager, "region"));
        Assert.True(State(manager, "region").IsDefault);
    }

    [Fact]
    public async Task TakenHotkey_IsReportedUnavailable_AndDoesNotStopOthers()
    {
        _os.Taken.Add("Ctrl+PrintScreen");
        using var manager = Create();

        await manager.RegisterAllAsync(Token);

        Assert.Equal(HotkeyStatus.Unavailable, State(manager, "region").Status);
        Assert.Equal(HotkeyStatus.Active, State(manager, "window").Status);
    }

    [Fact]
    public async Task PressingAHotkey_RunsItsBindingThroughDispatch()
    {
        using var manager = Create();
        var dispatched = 0;
        manager.Dispatch = work => { dispatched++; return work(); };
        await manager.RegisterAllAsync(Token);

        await _os.PressAsync("Ctrl+PrintScreen");

        Assert.Equal(["region"], _executed);
        Assert.Equal(1, dispatched);
    }

    [Fact]
    public async Task Rebind_Succeeds_IsSaved_AndSurvivesRestart()
    {
        using (var manager = Create())
        {
            await manager.RegisterAllAsync(Token);

            Assert.Equal(HotkeyStatus.Active, await manager.RebindAsync("region", "Ctrl+Shift+F9", Token));
            Assert.Equal("Ctrl+Shift+F9", State(manager, "region").Gesture);
            Assert.DoesNotContain("Ctrl+PrintScreen", _os.Active.Keys);
        }

        _os.Active.Clear();
        using var restarted = Create();
        await restarted.RegisterAllAsync(Token);

        Assert.Contains("Ctrl+Shift+F9", _os.Active.Keys);
        Assert.False(State(restarted, "region").IsDefault);
    }

    [Fact]
    public async Task Rebind_ToATakenHotkey_KeepsThePreviousOne()
    {
        using var manager = Create();
        await manager.RegisterAllAsync(Token);
        _os.Taken.Add("Ctrl+F1");

        var status = await manager.RebindAsync("region", "Ctrl+F1", Token);

        Assert.Equal(HotkeyStatus.Unavailable, status);
        Assert.Equal(new[] { "Ctrl+PrtSc", "Active" }, new[] { State(manager, "region").Gesture, State(manager, "region").Status.ToString() });
        Assert.Contains("Ctrl+PrintScreen", _os.Active.Keys);
    }

    [Fact]
    public async Task Rebind_InvalidText_ReturnsInvalid_AndChangesNothing()
    {
        using var manager = Create();
        await manager.RegisterAllAsync(Token);

        Assert.Equal(HotkeyStatus.Invalid, await manager.RebindAsync("region", "Ctrl+Banana", Token));
        Assert.Equal("Ctrl+PrtSc", State(manager, "region").Gesture);
    }

    [Fact]
    public async Task RebindBackToDefault_RemovesTheOverride()
    {
        using var manager = Create();
        await manager.RegisterAllAsync(Token);
        await manager.RebindAsync("region", "Ctrl+F2", Token);

        await manager.RebindAsync("region", "Ctrl+PrtSc", Token);

        Assert.True(State(manager, "region").IsDefault);
    }

    [Fact]
    public async Task SuspendThenResume_UnregistersAndRestores()
    {
        using var manager = Create();
        await manager.RegisterAllAsync(Token);

        await manager.SuspendAsync(Token);
        Assert.Empty(_os.Active);
        Assert.All(manager.States, s => Assert.Equal(HotkeyStatus.Suspended, s.Status));

        await manager.ResumeAsync(Token);
        Assert.Equal(2, _os.Active.Count);
    }

    [Fact]
    public async Task ResetAll_RestoresDefaults()
    {
        using var manager = Create();
        await manager.RegisterAllAsync(Token);
        await manager.RebindAsync("region", "Ctrl+F3", Token);

        await manager.ResetAllAsync(Token);

        Assert.All(manager.States, s => Assert.True(s.IsDefault));
        Assert.Contains("Ctrl+PrintScreen", _os.Active.Keys);
    }

    [Fact]
    public async Task Changed_IsRaisedAfterRegistration()
    {
        using var manager = Create();
        var raised = 0;
        manager.Changed += () => raised++;

        await manager.RegisterAllAsync(Token);

        Assert.Equal(1, raised);
    }

    private sealed class HotkeyModule(List<string> executed) : ISnapricotModule
    {
        public string Id => "test.hotkeys";

        public string DisplayName => "Hotkeys";

        public void Register(IModuleBuilder builder) => builder
            .AddHotkey(new HotkeyBinding("region", "Region", "Ctrl+PrtSc", (_, _) => { executed.Add("region"); return Task.CompletedTask; }))
            .AddHotkey(new HotkeyBinding("window", "Window", "Alt+PrtSc", (_, _) => { executed.Add("window"); return Task.CompletedTask; }));
    }

    private sealed class FakeHotkeys : IHotkeyService
    {
        public HashSet<string> Taken { get; } = [];

        public Dictionary<string, Func<CancellationToken, Task>> Active { get; } = [];

        public Task<IDisposable> RegisterAsync(HotkeyGesture gesture, Func<CancellationToken, Task> onPressed, CancellationToken cancellationToken)
        {
            var key = gesture.ToString();
            if (Taken.Contains(key) || Active.ContainsKey(key))
            {
                throw new HotkeyUnavailableException(gesture);
            }

            Active[key] = onPressed;
            return Task.FromResult<IDisposable>(new Registration(() => Active.Remove(key)));
        }

        public Task PressAsync(string gesture) => Active[gesture](CancellationToken.None);

        private sealed class Registration(Action dispose) : IDisposable
        {
            public void Dispose() => dispose();
        }
    }
}
