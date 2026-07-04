namespace RKWorkspace.Shell.VisualReality.Windows;

public sealed record VisualRealityLens
{
    public required string LensId { get; init; }

    public required string Label { get; init; }

    public required VisualRealityLensEdge Edge { get; init; }

    public required float X { get; init; }

    public required float Y { get; init; }
}
