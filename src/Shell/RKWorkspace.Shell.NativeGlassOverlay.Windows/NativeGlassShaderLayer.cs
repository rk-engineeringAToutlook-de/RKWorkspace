using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WImage = System.Windows.Controls.Image;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativeGlassShaderLayer : Canvas
{
    private readonly WImage _image;
    private readonly NativeGlassMaterialEffect _effect;
    private int _frame;

    public NativeGlassShaderLayer()
    {
        IsHitTestVisible = false;
        ClipToBounds = false;

        _effect = new NativeGlassMaterialEffect();
        _image = new WImage
        {
            IsHitTestVisible = false,
            Stretch = Stretch.Fill,
            SnapsToDevicePixels = false,
            Effect = _effect,
            Opacity = 1.0
        };

        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality);
        Children.Add(_image);
    }

    public void Tick(NativeGlassShaderSnapshot snapshot)
    {
        if (!snapshot.IsVisible)
        {
            Visibility = Visibility.Hidden;
            return;
        }

        Visibility = Visibility.Visible;
        SetLeft(_image, snapshot.RenderRect.X);
        SetTop(_image, snapshot.RenderRect.Y);
        _image.Width = snapshot.RenderRect.Width;
        _image.Height = snapshot.RenderRect.Height;
        _image.Clip = new EllipseGeometry(new Rect(0, 0, snapshot.RenderRect.Width, snapshot.RenderRect.Height));

        _effect.LensCenterX = snapshot.LensCenterX;
        _effect.LensCenterY = snapshot.LensCenterY;
        _effect.LensRadiusX = snapshot.LensRadiusX;
        _effect.LensRadiusY = snapshot.LensRadiusY;
        _effect.Open = snapshot.Open;
        _effect.Intensity = snapshot.Intensity;
        _effect.TimeSeconds = snapshot.TimeSeconds;

        _frame++;
        if (_image.Source is null || _frame % 2 == 0)
        {
            _image.Source = NativeDesktopSampler.Capture(snapshot.CaptureRect);
        }
    }
}
