namespace RKWorkspace.Shell.LivingLens.Windows;

public sealed record LivingLensVariant
{
    public required LivingLensVariantKind Kind { get; init; }

    public required string Name { get; init; }

    public required string Intention { get; init; }

    public required bool UsesMaterialTransparency { get; init; }

    public required bool UsesFineLightEdge { get; init; }

    public required bool UsesRefraction { get; init; }

    public required bool UsesDepth { get; init; }

    public required bool UsesSubtleLivingMotion { get; init; }

    public required bool SupportsAbsorption { get; init; }

    public bool AvoidsGreenPoint { get; init; } = true;

    public bool AvoidsPurpleBlob { get; init; } = true;

    public bool AvoidsUiCircle { get; init; } = true;

    public bool AvoidsButtonShape { get; init; } = true;

    public bool AvoidsTechnicalWords { get; init; } = true;
}
