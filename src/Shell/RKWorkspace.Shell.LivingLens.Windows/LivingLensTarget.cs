namespace RKWorkspace.Shell.LivingLens.Windows;

public sealed record LivingLensTarget
{
    public required string LensId { get; init; }

    public required string Label { get; init; }

    public required LivingLensEdge Edge { get; init; }

    public required float X { get; init; }

    public required float Y { get; init; }
}
