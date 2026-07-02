using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Describes a platform-neutral logical transfer result.
/// </summary>
public sealed record TransferResult
{
    /// <summary>
    /// Gets the transfer request id.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// Gets the transfer plan id.
    /// </summary>
    public string PlanId { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether the transfer completed successfully.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the source workspace, if resolved.
    /// </summary>
    public IWorkspace? SourceWorkspace { get; init; }

    /// <summary>
    /// Gets the target workspace, if resolved.
    /// </summary>
    public IWorkspace? TargetWorkspace { get; init; }

    /// <summary>
    /// Gets the transfer object, if resolved.
    /// </summary>
    public ITransferObject? TransferObject { get; init; }

    /// <summary>
    /// Gets the final transfer object state.
    /// </summary>
    public TransferObjectState? FinalState { get; init; }

    /// <summary>
    /// Gets the failure reason.
    /// </summary>
    public TransferFailureReason FailureReason { get; init; } = TransferFailureReason.None;

    /// <summary>
    /// Gets result messages.
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the completion timestamp.
    /// </summary>
    public DateTimeOffset CompletedAt { get; init; } = DateTimeOffset.UtcNow;
}
