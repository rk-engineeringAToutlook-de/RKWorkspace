namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialPortalTransition
{
    public required string TransitionId { get; init; }

    public required string ThingId { get; init; }

    public required string SourceAblageId { get; init; }

    public required string TargetAblageId { get; init; }

    public required SpatialPortalTransitionState State { get; init; }

    public double Progress { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public double SourceVisualProgress { get; init; }

    public double TargetVisualProgress { get; init; }
}
