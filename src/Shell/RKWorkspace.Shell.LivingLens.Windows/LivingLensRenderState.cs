using System.Drawing;

namespace RKWorkspace.Shell.LivingLens.Windows;

public sealed record LivingLensRenderState
{
    public required LivingLensVariantKind Variant { get; init; }

    public float Phase { get; init; }

    public float EmergenceProgress { get; init; }

    public float OpenProgress { get; init; }

    public float AbsorptionProgress { get; init; }

    public float TargetEmergenceProgress { get; init; }

    public float TiltX { get; init; }

    public float TiltY { get; init; }

    public float ShadowX { get; init; }

    public float ShadowY { get; init; } = 18f;

    public bool ThingCompact { get; init; }

    public bool ThingPartiallyOccluded { get; init; }

    public bool GripShadowVisible { get; init; }

    public bool DrawTestBackground { get; init; }

    public bool DrawThing { get; init; } = true;

    public bool DrawLens { get; init; } = true;

    public bool DrawGhost { get; init; }

    public PointF ThingCenter { get; init; }

    public PointF LensCenter { get; init; }

    public string LensLabel { get; init; } = "Ablage";

    public LivingLensNameVisibility NameVisibility { get; init; } = LivingLensNameVisibility.Hidden;
}
