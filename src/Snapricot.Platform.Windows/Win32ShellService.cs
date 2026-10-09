using System.Diagnostics;
using Snapricot.Core.Platform;

namespace Snapricot.Platform.Windows;

internal sealed class Win32ShellService : IShellService
{
    public void RevealInFolder(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        using var _ = Process.Start(new ProcessStartInfo("explorer.exe") { ArgumentList = { $"/select,{filePath}" } });
    }

    public void OpenFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        using var _ = Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }
}
