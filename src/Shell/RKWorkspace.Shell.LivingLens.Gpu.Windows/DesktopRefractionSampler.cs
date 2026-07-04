using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public static class DesktopRefractionSampler
{
    public static BitmapSource? Capture(Int32Rect screenRect)
    {
        var width = Math.Max(4, screenRect.Width);
        var height = Math.Max(4, screenRect.Height);
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
        }

        var handle = bitmap.GetHbitmap();
        try
        {
            var source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DeleteObject(handle);
        }
    }

    public static bool TryCheckSample()
    {
        try
        {
            var sample = Capture(new Int32Rect(0, 0, 16, 16));
            return sample is not null && sample.PixelWidth == 16 && sample.PixelHeight == 16;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr hObject);
}
