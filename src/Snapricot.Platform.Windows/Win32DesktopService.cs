using Snapricot.Core.Capture;
using Snapricot.Core.Platform;
using Snapricot.Platform.Windows.Interop;
using static Snapricot.Platform.Windows.Interop.NativeMethods;

namespace Snapricot.Platform.Windows;

internal sealed unsafe class Win32DesktopService : IDesktopService
{
    public PixelPoint GetCursorPosition()
    {
        POINT point;
        return GetCursorPos(&point) ? new PixelPoint(point.X, point.Y) : default;
    }

    public PixelBounds? GetForegroundWindowBounds()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == 0)
        {
            return null;
        }

        // The DWM frame excludes the invisible resize borders that GetWindowRect includes on Windows 10/11.
        RECT rect;
        if (DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, &rect, sizeof(RECT)) != 0 && !GetWindowRect(hwnd, &rect))
        {
            return null;
        }

        var bounds = new PixelBounds(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        return bounds.IsEmpty ? null : bounds;
    }
}
