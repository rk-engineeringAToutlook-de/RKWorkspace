namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialThing
{
    public required string ThingId { get; init; }

    public required string DisplayName { get; init; }

    public required string Kind { get; init; }

    public required SpatialThingState CurrentState { get; init; }

    public string? CurrentAblageId { get; init; }

    public string? CurrentCarryId { get; init; }

    public SpatialPoint? PositionOnAblage { get; init; }

    public string? PreviewAblageId { get; init; }

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
