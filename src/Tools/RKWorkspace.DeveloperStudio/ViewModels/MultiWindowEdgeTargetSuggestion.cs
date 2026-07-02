namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record MultiWindowEdgeTargetSuggestion
{
    public required MultiWindowEdge Edge { get; init; }

    public required string WorkspaceId { get; init; }

    public required string WorkspaceName { get; init; }

    public required string Hint { get; init; }
}
