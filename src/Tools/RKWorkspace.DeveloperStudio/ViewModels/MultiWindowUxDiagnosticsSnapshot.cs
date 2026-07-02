namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record MultiWindowUxDiagnosticsSnapshot
{
    public required int DragStartCount { get; init; }

    public required int TargetDetectedCount { get; init; }

    public required int DropCount { get; init; }

    public required int SuccessfulTransfers { get; init; }

    public required int FailedTransfers { get; init; }

    public required double LastDragDurationMs { get; init; }

    public required double LastTransferDurationMs { get; init; }

    public required double SuccessRatePercent { get; init; }

    public required WorkspaceSessionCandidate SessionCandidate { get; init; }
}
