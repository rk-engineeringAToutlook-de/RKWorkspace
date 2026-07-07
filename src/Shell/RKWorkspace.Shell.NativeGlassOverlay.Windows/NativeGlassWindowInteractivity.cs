using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

internal static class NativeGlassWindowInteractivity
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;

    public static void SetClickThrough(Window window, bool enabled)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLong(handle, GwlExStyle);
        style |= WsExToolWindow;
        if (enabled)
        {
            style |= WsExTransparent;
        }
        else
        {
            style &= ~WsExTransparent;
        }

        SetWindowLong(handle, GwlExStyle, style);
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
