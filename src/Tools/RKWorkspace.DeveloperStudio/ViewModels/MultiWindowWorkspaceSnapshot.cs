namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record MultiWindowWorkspaceSnapshot
{
    public required string WorkspaceId { get; init; }

    public required string WorkspaceName { get; init; }

    public required string WorkspaceType { get; init; }

    public required string WorkspacePosition { get; init; }

    public required string WorkspaceStatus { get; init; }

    public required bool IsDropTargetHighlighted { get; init; }

    public required IReadOnlyCollection<MultiWindowTransferObjectRow> TransferObjects { get; init; }

    public required IReadOnlyCollection<StudioTransferHistoryRow> History { get; init; }

    public required IReadOnlyCollection<MultiWindowLogRow> Log { get; init; }

    public required string Diagnostics { get; init; }

    public required string LastResult { get; init; }

    public required string LastError { get; init; }
}
