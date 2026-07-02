namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record MultiWindowWorkspaceSnapshot
{
    public required string WorkspaceId { get; init; }

    public required string WorkspaceName { get; init; }

    public required string WorkspaceType { get; init; }

    public required string WorkspacePosition { get; init; }

    public required string WorkspaceStatus { get; init; }

    public required bool IsDropTargetHighlighted { get; init; }

    public required bool IsObjectGrabbed { get; init; }

    public required bool IsEdgeCandidateActive { get; init; }

    public required bool IsEdgeLocked { get; init; }

    public required string ActiveDragObjectId { get; init; }

    public required string ActiveDragSourceWorkspaceId { get; init; }

    public required string SuggestedWorkspaceId { get; init; }

    public required string SuggestedWorkspaceName { get; init; }

    public required string SuggestedWorkspacePreview { get; init; }

    public required MultiWindowEdge ActiveEdge { get; init; }

    public required string EdgeHotZoneHint { get; init; }

    public required string EdgeTransitionHint { get; init; }

    public required string EdgeGhostObjectName { get; init; }

    public required string StatusHint { get; init; }

    public required string SuccessHint { get; init; }

    public required bool IsSuccessPulseActive { get; init; }

    public required IReadOnlyCollection<MultiWindowTransferObjectRow> TransferObjects { get; init; }

    public required IReadOnlyCollection<StudioTransferHistoryRow> History { get; init; }

    public required IReadOnlyCollection<MultiWindowLogRow> Log { get; init; }

    public required string Diagnostics { get; init; }

    public required string LastResult { get; init; }

    public required string LastError { get; init; }

    public required MultiWindowUxDiagnosticsSnapshot UxDiagnostics { get; init; }
}
