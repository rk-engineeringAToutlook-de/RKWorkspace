using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

internal static class NativeGlassCaptureExclusion
{
    private const uint WdaExcludeFromCapture = 0x00000011;

    public static bool TryEnable(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        return handle != IntPtr.Zero && SetWindowDisplayAffinity(handle, WdaExcludeFromCapture);
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint affinity);
}
