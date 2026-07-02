using RKWorkspace.Core.Transfers;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record WorkspaceSessionCandidate
{
    public required string CandidateId { get; init; }

    public required string TransferObjectId { get; init; }

    public required string SourceWorkspaceId { get; init; }

    public required string TargetWorkspaceId { get; init; }

    public required TransferDirection Direction { get; init; }

    public required WorkspaceSessionCandidateStatus Status { get; init; }

    public required string Reason { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset? EdgeLockedAt { get; init; }

    public required DateTimeOffset? CompletedAt { get; init; }

    public required bool IsLiveWorkspacePrepared { get; init; }

    public static WorkspaceSessionCandidate Empty { get; } = new()
    {
        CandidateId = string.Empty,
        TransferObjectId = string.Empty,
        SourceWorkspaceId = string.Empty,
        TargetWorkspaceId = string.Empty,
        Direction = TransferDirection.Unknown,
        Status = WorkspaceSessionCandidateStatus.None,
        Reason = string.Empty,
        CreatedAt = DateTimeOffset.MinValue,
        EdgeLockedAt = null,
        CompletedAt = null,
        IsLiveWorkspacePrepared = false
    };
}
