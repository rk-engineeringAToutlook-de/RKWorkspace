namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialAblage
{
    public required string AblageId { get; init; }

    public required string DisplayName { get; init; }

    public required SpatialSurfaceType SurfaceType { get; init; }

    public required string RelativePosition { get; init; }

    public required SpatialAblageDistance Distance { get; init; }

    public bool IsAvailable { get; init; } = true;

    public bool IsActive { get; init; }

    public bool CanReceive { get; init; } = true;

    public bool CanProvide { get; init; } = true;

    public DateTimeOffset LastSeen { get; init; } = DateTimeOffset.UtcNow;

    public SpatialPoint Position { get; init; } = new(0.5, 0.5);
}
