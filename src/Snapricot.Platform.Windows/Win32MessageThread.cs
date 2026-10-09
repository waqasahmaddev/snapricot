using System.Collections.Concurrent;
using Snapricot.Platform.Windows.Interop;
using static Snapricot.Platform.Windows.Interop.NativeMethods;

namespace Snapricot.Platform.Windows;

/// <summary>
/// A dedicated STA thread with its own Win32 message loop and a hidden message-only window.
/// Global hotkeys (WM_HOTKEY) and clipboard ownership live here, so they never depend on, or block, the UI thread.
/// </summary>
internal sealed class Win32MessageThread : IDisposable
{
    private readonly Thread _thread;
    private readonly ConcurrentQueue<Action> _work = new();
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private uint _threadId;
    private int _disposed;

    public Win32MessageThread()
    {
        _thread = new Thread(Run) { IsBackground = true, Name = "Snapricot.Win32" };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    /// <summary>Hidden message-only window owned by this thread (used as clipboard owner).</summary>
    public nint Window { get; private set; }

    /// <summary>Raised on the message thread with the hotkey id from <c>RegisterHotKey</c>.</summary>
    public event Action<int>? HotkeyPressed;

    /// <summary>Runs <paramref name="work"/> on the message thread (required for thread-affine Win32 calls).</summary>
    public async Task<T> InvokeAsync<T>(Func<T> work)
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
        await _ready.Task.ConfigureAwait(false);

        var result = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _work.Enqueue(() =>
        {
            try
            {
                result.SetResult(work());
            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }
        });
        PostThreadMessage(_threadId, WM_APP, 0, 0);
        return await result.Task.ConfigureAwait(false);
    }

    private unsafe void Run()
    {
        MSG msg;
        _threadId = GetCurrentThreadId();
        PeekMessage(&msg, 0, 0, 0, PM_NOREMOVE); // forces the thread's message queue to exist
        Window = CreateWindowEx(0, "STATIC", "Snapricot", 0, 0, 0, 0, 0, HWND_MESSAGE, 0, 0, 0);
        _ready.SetResult();

        while (GetMessage(&msg, 0, 0, 0) > 0)
        {
            switch (msg.Message)
            {
                case WM_HOTKEY:
                    HotkeyPressed?.Invoke((int)msg.WParam);
                    break;
                case WM_APP:
                    Drain();
                    break;
                default:
                    TranslateMessage(&msg);
                    DispatchMessage(&msg);
                    break;
            }
        }

        Drain();
        DestroyWindow(Window);
    }

    private void Drain()
    {
        while (_work.TryDequeue(out var work))
        {
            work();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0 && _ready.Task.IsCompleted)
        {
            PostThreadMessage(_threadId, WM_QUIT, 0, 0);
            _thread.Join(TimeSpan.FromSeconds(1));
        }
    }
}
