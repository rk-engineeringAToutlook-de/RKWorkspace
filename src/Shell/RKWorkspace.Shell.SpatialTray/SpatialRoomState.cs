namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialRoomState
{
    public required string RoomId { get; init; }

    public required string Version { get; init; }

    public required IReadOnlyList<SpatialAblage> Ablagen { get; init; }

    public required IReadOnlyList<SpatialThing> Things { get; init; }

    public SpatialCarrySession? ActiveCarry { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}
