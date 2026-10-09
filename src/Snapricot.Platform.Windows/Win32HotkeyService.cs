using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Snapricot.Core.Hotkeys;
using Snapricot.Core.Platform;
using static Snapricot.Platform.Windows.Interop.NativeMethods;

namespace Snapricot.Platform.Windows;

/// <summary>Global hotkeys via <c>RegisterHotKey</c> on the <see cref="Win32MessageThread"/>.</summary>
internal sealed partial class Win32HotkeyService : IHotkeyService
{
    private readonly Win32MessageThread _thread;
    private readonly ILogger<Win32HotkeyService> _logger;
    private readonly ConcurrentDictionary<int, Func<CancellationToken, Task>> _handlers = new();
    private int _nextId;

    public Win32HotkeyService(Win32MessageThread thread, ILogger<Win32HotkeyService> logger)
    {
        _thread = thread;
        _logger = logger;
        _thread.HotkeyPressed += OnHotkeyPressed;
    }

    public async Task<IDisposable> RegisterAsync(
        HotkeyGesture gesture,
        Func<CancellationToken, Task> onPressed,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gesture);
        ArgumentNullException.ThrowIfNull(onPressed);
        cancellationToken.ThrowIfCancellationRequested();

        var (modifiers, virtualKey) = Win32Keys.ToNative(gesture);
        var id = Interlocked.Increment(ref _nextId);
        _handlers[id] = onPressed;

        // Hotkeys registered with a null window post WM_HOTKEY to the registering thread's queue.
        var registered = await _thread.InvokeAsync(() => RegisterHotKey(0, id, modifiers | MOD_NOREPEAT, virtualKey)).ConfigureAwait(false);
        if (!registered)
        {
            _handlers.TryRemove(id, out _);
            throw new HotkeyUnavailableException(gesture);
        }

        return new Registration(this, id);
    }

    private void OnHotkeyPressed(int id)
    {
        if (!_handlers.TryGetValue(id, out var handler))
        {
            return;
        }

        // Never run user work on the message thread: it must keep pumping.
        _ = Task.Run(async () =>
        {
            try
            {
                await handler(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHandlerFailed(ex, id);
            }
        });
    }

    private void Unregister(int id)
    {
        if (_handlers.TryRemove(id, out _))
        {
            _ = _thread.InvokeAsync(() => UnregisterHotKey(0, id));
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Hotkey handler {HotkeyId} failed")]
    private partial void LogHandlerFailed(Exception exception, int hotkeyId);

    private sealed class Registration(Win32HotkeyService owner, int id) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                owner.Unregister(id);
            }
        }
    }
}
