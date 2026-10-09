using System.ComponentModel;
using System.Runtime.InteropServices;
using Snapricot.Core.Capture;
using Snapricot.Core.Platform;
using Snapricot.Platform.Windows.Interop;
using static Snapricot.Platform.Windows.Interop.NativeMethods;

namespace Snapricot.Platform.Windows;

/// <summary>
/// Screen capture with GDI <c>BitBlt</c> from the desktop DC (ADR-0008). With DWM composition this includes
/// GPU-rendered content, and it's fast enough for stills (a 4K frame copies in a few milliseconds).
/// </summary>
internal sealed unsafe class GdiScreenCapture : IScreenCapture
{
    public IReadOnlyList<DisplayInfo> GetDisplays()
    {
        var displays = new List<DisplayInfo>();
        var handle = GCHandle.Alloc(displays);
        try
        {
            EnumDisplayMonitors(0, 0, &OnMonitor, GCHandle.ToIntPtr(handle));
        }
        finally
        {
            handle.Free();
        }

        return displays;
    }

    public Task<CapturedImage> CaptureAsync(PixelBounds bounds, CancellationToken cancellationToken)
    {
        if (bounds.IsEmpty)
        {
            throw new ArgumentOutOfRangeException(nameof(bounds), bounds, "Capture bounds must not be empty.");
        }

        return Task.Run(() => Capture(bounds), cancellationToken);
    }

    [UnmanagedCallersOnly]
    private static int OnMonitor(nint monitor, nint hdc, RECT* rect, nint data)
    {
        var displays = (List<DisplayInfo>)GCHandle.FromIntPtr(data).Target!;

        var info = new MONITORINFOEXW { Size = (uint)sizeof(MONITORINFOEXW) };
        if (!GetMonitorInfo(monitor, &info))
        {
            return 1;
        }

        uint dpiX = 96, dpiY = 96;
        _ = GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, &dpiX, &dpiY);

        var r = info.Monitor;
        displays.Add(new DisplayInfo(
            Id: new string(info.Device),
            Bounds: new PixelBounds(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top),
            Scale: dpiX / 96d,
            IsPrimary: (info.Flags & MONITORINFOF_PRIMARY) != 0));
        return 1; // continue enumeration
    }

    private static CapturedImage Capture(PixelBounds bounds)
    {
        var capturedAt = DateTimeOffset.Now;
        var screenDc = GetDC(0);
        if (screenDc == 0)
        {
            throw new Win32Exception("GetDC failed.");
        }

        nint memoryDc = 0;
        nint bitmap = 0;
        try
        {
            memoryDc = CreateCompatibleDC(screenDc);
            var header = new BITMAPINFOHEADER
            {
                Size = (uint)sizeof(BITMAPINFOHEADER),
                Width = bounds.Width,
                Height = -bounds.Height, // negative = top-down rows
                Planes = 1,
                BitCount = 32,
                Compression = BI_RGB,
            };

            void* bits;
            bitmap = CreateDIBSection(screenDc, &header, DIB_RGB_COLORS, &bits, 0, 0);
            if (bitmap == 0 || bits == null)
            {
                throw new Win32Exception("CreateDIBSection failed.");
            }

            var previous = SelectObject(memoryDc, bitmap);
            var copied = BitBlt(memoryDc, 0, 0, bounds.Width, bounds.Height, screenDc, bounds.X, bounds.Y, SRCCOPY | CAPTUREBLT);
            SelectObject(memoryDc, previous);
            if (!copied)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "BitBlt failed.");
            }

            var stride = bounds.Width * 4;
            var pixels = new byte[stride * bounds.Height];
            new ReadOnlySpan<byte>(bits, pixels.Length).CopyTo(pixels);

            // GDI leaves the alpha channel undefined; screenshots are always opaque.
            foreach (ref var pixel in MemoryMarshal.Cast<byte, uint>(pixels.AsSpan()))
            {
                pixel |= 0xFF000000;
            }

            return new CapturedImage(bounds.Width, bounds.Height, stride, pixels, capturedAt);
        }
        finally
        {
            if (bitmap != 0)
            {
                DeleteObject(bitmap);
            }

            if (memoryDc != 0)
            {
                DeleteDC(memoryDc);
            }

            ReleaseDC(0, screenDc);
        }
    }
}
