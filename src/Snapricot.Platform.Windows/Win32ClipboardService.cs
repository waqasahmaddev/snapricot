using System.ComponentModel;
using System.Runtime.InteropServices;
using Snapricot.Core.Capture;
using Snapricot.Core.Platform;
using Snapricot.Platform.Windows.Interop;
using static Snapricot.Platform.Windows.Interop.NativeMethods;

namespace Snapricot.Platform.Windows;

/// <summary>
/// Puts images on the clipboard as CF_DIB (32-bit, bottom-up), which every Windows app that accepts pasted images reads.
/// Runs on the <see cref="Win32MessageThread"/>, whose hidden window owns the clipboard data.
/// </summary>
internal sealed unsafe class Win32ClipboardService(Win32MessageThread thread) : IClipboardService
{
    private const int OpenAttempts = 10;

    public Task SetImageAsync(CapturedImage image, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image);
        cancellationToken.ThrowIfCancellationRequested();

        return thread.InvokeAsync(() =>
        {
            SetDib(image, thread.Window);
            return true;
        });
    }

    private static void SetDib(CapturedImage image, nint owner)
    {
        var headerSize = sizeof(BITMAPINFOHEADER);
        var rowBytes = image.Width * 4;
        var total = headerSize + (rowBytes * image.Height);

        var memory = GlobalAlloc(GMEM_MOVEABLE, (nuint)total);
        if (memory == 0)
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "GlobalAlloc failed.");
        }

        var handedToClipboard = false;
        try
        {
            var destination = (byte*)GlobalLock(memory);
            *(BITMAPINFOHEADER*)destination = new BITMAPINFOHEADER
            {
                Size = (uint)headerSize,
                Width = image.Width,
                Height = image.Height, // positive = bottom-up, the most compatible DIB layout
                Planes = 1,
                BitCount = 32,
                Compression = BI_RGB,
                SizeImage = (uint)(rowBytes * image.Height),
            };

            var source = image.Pixels.Span;
            var pixels = destination + headerSize;
            for (var row = 0; row < image.Height; row++)
            {
                source.Slice(row * image.Stride, rowBytes)
                    .CopyTo(new Span<byte>(pixels + ((image.Height - 1 - row) * rowBytes), rowBytes));
            }

            GlobalUnlock(memory);

            OpenWithRetry(owner);
            try
            {
                EmptyClipboard();
                if (SetClipboardData(CF_DIB, memory) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "SetClipboardData failed.");
                }

                handedToClipboard = true; // the system owns the memory now
            }
            finally
            {
                CloseClipboard();
            }
        }
        finally
        {
            if (!handedToClipboard)
            {
                GlobalFree(memory);
            }
        }
    }

    private static void OpenWithRetry(nint owner)
    {
        // Another app may hold the clipboard for a moment (clipboard managers, RDP).
        for (var attempt = 1; ; attempt++)
        {
            if (OpenClipboard(owner))
            {
                return;
            }

            if (attempt == OpenAttempts)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "The clipboard is busy.");
            }

            Thread.Sleep(15);
        }
    }
}
