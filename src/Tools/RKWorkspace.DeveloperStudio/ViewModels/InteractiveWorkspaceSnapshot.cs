namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record InteractiveWorkspaceSnapshot
{
    public required bool IsInitialized { get; init; }

    public required string SourceName { get; init; }

    public required string SourcePosition { get; init; }

    public required string SourceState { get; init; }

    public required string TargetName { get; init; }

    public required string TargetPosition { get; init; }

    public required string TargetState { get; init; }

    public required string ObjectTitle { get; init; }

    public required string ObjectText { get; init; }

    public required string ObjectState { get; init; }

    public required string ObjectLocation { get; init; }

    public required bool IsDragging { get; init; }

    public required bool IsTargetHighlighted { get; init; }
}
