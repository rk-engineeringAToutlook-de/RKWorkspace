using System.Windows;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public readonly record struct GpuLivingLensShaderSnapshot(
    bool IsVisible,
    Int32Rect CaptureRect,
    Rect RenderRect,
    double LensCenterX,
    double LensCenterY,
    double LensRadiusX,
    double LensRadiusY,
    double Open,
    double Intensity,
    double LookMode,
    double TimeSeconds);
