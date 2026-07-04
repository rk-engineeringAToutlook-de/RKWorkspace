namespace RKWorkspace.Shell.VisualReality.Windows;

public sealed record VisualRealityLensHypothesis
{
    public required VisualRealityLensKind Kind { get; init; }

    public required string Name { get; init; }

    public required string Intention { get; init; }

    public required bool UsesTransparency { get; init; }

    public required bool UsesLightRefraction { get; init; }

    public required bool UsesDepth { get; init; }

    public required bool UsesLivingMotion { get; init; }

    public bool UsesGlassMaterial { get; init; }

    public bool UsesGravityWell { get; init; }

    public bool UsesCalmPortal { get; init; }

    public bool PreservesRealDesktop { get; init; } = true;

    public bool AvoidsSpaceBackdrop { get; init; } = true;

    public required bool AvoidsGreenPointUi { get; init; }

    public required bool AvoidsButtonShape { get; init; }
}
