using System.Windows;
using System.Windows.Media.Effects;
using WBrush = System.Windows.Media.Brush;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public sealed class LivingLensMaterialEffect : ShaderEffect
{
    private static readonly PixelShader MaterialShader = new()
    {
        UriSource = new Uri("pack://application:,,,/RKWorkspace.Shell.LivingLens.Gpu.Windows;component/Shaders/LivingLensMaterial.ps")
    };

    public LivingLensMaterialEffect()
    {
        PixelShader = MaterialShader;
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(LensCenterXProperty);
        UpdateShaderValue(LensCenterYProperty);
        UpdateShaderValue(LensRadiusXProperty);
        UpdateShaderValue(LensRadiusYProperty);
        UpdateShaderValue(OpenProperty);
        UpdateShaderValue(IntensityProperty);
        UpdateShaderValue(LookModeProperty);
        UpdateShaderValue(TimeSecondsProperty);
    }

    public WBrush Input
    {
        get => (WBrush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public double LensCenterX
    {
        get => (double)GetValue(LensCenterXProperty);
        set => SetValue(LensCenterXProperty, value);
    }

    public double LensCenterY
    {
        get => (double)GetValue(LensCenterYProperty);
        set => SetValue(LensCenterYProperty, value);
    }

    public double LensRadiusX
    {
        get => (double)GetValue(LensRadiusXProperty);
        set => SetValue(LensRadiusXProperty, value);
    }

    public double LensRadiusY
    {
        get => (double)GetValue(LensRadiusYProperty);
        set => SetValue(LensRadiusYProperty, value);
    }

    public double Open
    {
        get => (double)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public double Intensity
    {
        get => (double)GetValue(IntensityProperty);
        set => SetValue(IntensityProperty, value);
    }

    public double LookMode
    {
        get => (double)GetValue(LookModeProperty);
        set => SetValue(LookModeProperty, value);
    }

    public double TimeSeconds
    {
        get => (double)GetValue(TimeSecondsProperty);
        set => SetValue(TimeSecondsProperty, value);
    }

    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(nameof(Input), typeof(LivingLensMaterialEffect), 0);

    public static readonly DependencyProperty LensCenterXProperty =
        DependencyProperty.Register(nameof(LensCenterX), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(0)));

    public static readonly DependencyProperty LensCenterYProperty =
        DependencyProperty.Register(nameof(LensCenterY), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(1)));

    public static readonly DependencyProperty LensRadiusXProperty =
        DependencyProperty.Register(nameof(LensRadiusX), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(2)));

    public static readonly DependencyProperty LensRadiusYProperty =
        DependencyProperty.Register(nameof(LensRadiusY), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(3)));

    public static readonly DependencyProperty OpenProperty =
        DependencyProperty.Register(nameof(Open), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(4)));

    public static readonly DependencyProperty IntensityProperty =
        DependencyProperty.Register(nameof(Intensity), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(5)));

    public static readonly DependencyProperty LookModeProperty =
        DependencyProperty.Register(nameof(LookMode), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(2.0, PixelShaderConstantCallback(6)));

    public static readonly DependencyProperty TimeSecondsProperty =
        DependencyProperty.Register(nameof(TimeSeconds), typeof(double), typeof(LivingLensMaterialEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(7)));
}
