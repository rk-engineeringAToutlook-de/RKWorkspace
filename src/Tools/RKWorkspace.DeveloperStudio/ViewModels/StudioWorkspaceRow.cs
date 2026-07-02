namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record StudioWorkspaceRow
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public required string Position { get; init; }

    public required string State { get; init; }

    public required bool Trusted { get; init; }

    public required int Priority { get; init; }
}
