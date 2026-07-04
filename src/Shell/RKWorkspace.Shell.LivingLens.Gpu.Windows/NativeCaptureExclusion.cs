using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

internal static class NativeCaptureExclusion
{
    private const uint WdaExcludeFromCapture = 0x00000011;

    public static bool TryEnable(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return false;
        }

        return SetWindowDisplayAffinity(handle, WdaExcludeFromCapture);
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint affinity);
}
