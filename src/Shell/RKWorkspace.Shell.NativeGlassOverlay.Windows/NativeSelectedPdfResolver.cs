using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public static class NativeSelectedPdfResolver
{
    private const byte VirtualKeyControl = 0x11;
    private const byte VirtualKeyC = 0x43;
    private const uint KeyEventKeyUp = 0x0002;

    public static string? TryResolveSelectedPdf(Dispatcher dispatcher, NativeGlassOverlayDiagnostics diagnostics)
    {
        if (!dispatcher.CheckAccess())
        {
            return dispatcher.Invoke(() => TryResolveSelectedPdf(dispatcher, diagnostics));
        }

        System.Windows.IDataObject? previousData = null;
        try
        {
            previousData = System.Windows.Clipboard.GetDataObject();
        }
        catch
        {
            diagnostics.Set("Geste", "Zwischenablage konnte nicht gesichert werden");
        }

        try
        {
            SendCopyGesture();
            Thread.Sleep(180);

            if (!System.Windows.Clipboard.ContainsFileDropList())
            {
                diagnostics.Set("PDF", "keine markierte Datei gefunden");
                return null;
            }

            var files = System.Windows.Clipboard.GetFileDropList()
                .Cast<string>()
                .Where(path => path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (files.Length == 0)
            {
                diagnostics.Set("PDF", "markierte Datei ist keine PDF");
                return null;
            }

            var selected = files[0];
            diagnostics.Set("PDF", Path.GetFileName(selected));
            return selected;
        }
        catch (Exception ex)
        {
            diagnostics.Set("PDF", $"nicht erkannt: {ex.Message}");
            return null;
        }
        finally
        {
            TryRestoreClipboard(previousData);
        }
    }

    private static void SendCopyGesture()
    {
        keybd_event(VirtualKeyControl, 0, 0, UIntPtr.Zero);
        keybd_event(VirtualKeyC, 0, 0, UIntPtr.Zero);
        keybd_event(VirtualKeyC, 0, KeyEventKeyUp, UIntPtr.Zero);
        keybd_event(VirtualKeyControl, 0, KeyEventKeyUp, UIntPtr.Zero);
    }

    private static void TryRestoreClipboard(System.Windows.IDataObject? data)
    {
        if (data is null)
        {
            return;
        }

        try
        {
            System.Windows.Clipboard.SetDataObject(data, true);
        }
        catch
        {
            // Best effort only. The selected PDF path has already been resolved.
        }
    }

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}
