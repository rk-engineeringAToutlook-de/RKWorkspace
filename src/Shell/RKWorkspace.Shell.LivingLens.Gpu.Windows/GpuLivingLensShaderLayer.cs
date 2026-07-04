using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WImage = System.Windows.Controls.Image;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public sealed class GpuLivingLensShaderLayer : Canvas
{
    private readonly GpuLivingLensSurface _surface;
    private readonly System.Drawing.Rectangle _screenBounds;
    private readonly WImage _image;
    private readonly LivingLensMaterialEffect _effect;
    private int _frame;

    public GpuLivingLensShaderLayer(GpuLivingLensSurface surface, System.Drawing.Rectangle screenBounds)
    {
        _surface = surface;
        _screenBounds = screenBounds;
        IsHitTestVisible = false;
        ClipToBounds = false;
        Width = screenBounds.Width;
        Height = screenBounds.Height;

        _effect = new LivingLensMaterialEffect();
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

    public void Tick()
    {
        var snapshot = _surface.CreateShaderSnapshot();
        if (!snapshot.IsVisible)
        {
            Visibility = Visibility.Hidden;
            return;
        }

        Visibility = Visibility.Visible;
        Canvas.SetLeft(_image, snapshot.RenderRect.X);
        Canvas.SetTop(_image, snapshot.RenderRect.Y);
        _image.Width = snapshot.RenderRect.Width;
        _image.Height = snapshot.RenderRect.Height;
        _image.Clip = new EllipseGeometry(new Rect(0, 0, snapshot.RenderRect.Width, snapshot.RenderRect.Height));

        _effect.LensCenterX = snapshot.LensCenterX;
        _effect.LensCenterY = snapshot.LensCenterY;
        _effect.LensRadiusX = snapshot.LensRadiusX;
        _effect.LensRadiusY = snapshot.LensRadiusY;
        _effect.Open = snapshot.Open;
        _effect.Intensity = snapshot.Intensity;
        _effect.LookMode = snapshot.LookMode;
        _effect.TimeSeconds = snapshot.TimeSeconds;

        _frame++;
        if (_image.Source is null || _frame % 3 == 0)
        {
            _image.Source = DesktopRefractionSampler.Capture(snapshot.CaptureRect);
        }
    }
}
