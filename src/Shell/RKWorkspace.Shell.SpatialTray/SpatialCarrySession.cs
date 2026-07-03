namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialCarrySession
{
    public required string CarryId { get; init; }

    public required string ThingId { get; init; }

    public required string SourceAblageId { get; init; }

    public required string CarrierAblageId { get; init; }

    public string? TargetCandidateAblageId { get; init; }

    public required SpatialCarrySessionState State { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public SpatialPoint? Position { get; init; }
}
