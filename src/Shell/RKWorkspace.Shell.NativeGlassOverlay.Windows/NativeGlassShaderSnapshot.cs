using System.Windows;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public readonly record struct NativeGlassShaderSnapshot(
    bool IsVisible,
    Int32Rect CaptureRect,
    Rect RenderRect,
    double LensCenterX,
    double LensCenterY,
    double LensRadiusX,
    double LensRadiusY,
    double Open,
    double Intensity,
    double TimeSeconds);
