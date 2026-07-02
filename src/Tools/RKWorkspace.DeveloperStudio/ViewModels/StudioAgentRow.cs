namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record StudioAgentRow
{
    public required string AgentId { get; init; }

    public required string DisplayName { get; init; }

    public required string Runtime { get; init; }

    public required string Workspace { get; init; }

    public required string WorkspaceId { get; init; }

    public required string Status { get; init; }
}
