namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialTrayDiagnostics
{
    public required int Port { get; init; }

    public required string TrayUrl { get; init; }

    public required string LocalTrayUrl { get; init; }

    public required string AblageUrl { get; init; }

    public required SpatialTrayState State { get; init; }

    public required string ThingName { get; init; }

    public required string DesktopAblageName { get; init; }

    public required DateTimeOffset StartedAt { get; init; }
}
