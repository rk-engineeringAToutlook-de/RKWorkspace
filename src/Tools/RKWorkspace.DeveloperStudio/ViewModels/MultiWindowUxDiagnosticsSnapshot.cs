namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record MultiWindowUxDiagnosticsSnapshot
{
    public required int DragStartCount { get; init; }

    public required int TargetDetectedCount { get; init; }

    public required int DropCount { get; init; }

    public required int SuccessfulTransfers { get; init; }

    public required int FailedTransfers { get; init; }

    public required int ReturnTransferCount { get; init; }

    public required int FailedAttempts { get; init; }

    public required double LastDragDurationMs { get; init; }

    public required double LastTransferDurationMs { get; init; }

    public required double LastTransitionDurationMs { get; init; }

    public required double SuccessRatePercent { get; init; }

    public required DateTimeOffset? LastGrabbedAt { get; init; }

    public required DateTimeOffset? LastEdgeLockedAt { get; init; }

    public required string ActiveDirection { get; init; }

    public required WorkspaceSessionCandidateStatus CandidateStatus { get; init; }

    public required WorkspaceSessionCandidate SessionCandidate { get; init; }
}
