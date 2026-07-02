namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record WorkspaceIllusionSmokeResult
{
    public required bool GripStateSet { get; init; }

    public required bool EdgeCandidateCreated { get; init; }

    public required bool EdgeLockedReached { get; init; }

    public required bool ForwardTransferSuccess { get; init; }

    public required bool ReturnTransferSuccess { get; init; }

    public required bool CandidateCompleted { get; init; }

    public required MultiWindowUxDiagnosticsSnapshot Diagnostics { get; init; }

    public bool IsSuccess =>
        GripStateSet &&
        EdgeCandidateCreated &&
        EdgeLockedReached &&
        ForwardTransferSuccess &&
        ReturnTransferSuccess &&
        CandidateCompleted;
}
