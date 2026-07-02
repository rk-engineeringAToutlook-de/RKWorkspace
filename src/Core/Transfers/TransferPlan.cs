using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Describes a platform-neutral logical transfer plan.
/// </summary>
public sealed record TransferPlan
{
    /// <summary>
    /// Gets the plan id.
    /// </summary>
    public required string PlanId { get; init; }

    /// <summary>
    /// Gets the originating transfer request.
    /// </summary>
    public required TransferRequest Request { get; init; }

    /// <summary>
    /// Gets the source workspace.
    /// </summary>
    public required IWorkspace SourceWorkspace { get; init; }

    /// <summary>
    /// Gets the target workspace.
    /// </summary>
    public required IWorkspace TargetWorkspace { get; init; }

    /// <summary>
    /// Gets the transfer object.
    /// </summary>
    public required ITransferObject TransferObject { get; init; }

    /// <summary>
    /// Gets logical plan steps.
    /// </summary>
    public IReadOnlyCollection<TransferStep> Steps { get; init; } = Array.Empty<TransferStep>();

    /// <summary>
    /// Gets the plan creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets whether the plan is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets plan validation messages.
    /// </summary>
    public IReadOnlyCollection<string> ValidationMessages { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates an immutable plan snapshot.
    /// </summary>
    /// <returns>The transfer plan snapshot.</returns>
    public TransferPlan Snapshot()
    {
        ValidateShape();

        return this with
        {
            Request = Request.Snapshot(),
            Steps = Steps.ToArray(),
            ValidationMessages = ValidationMessages.ToArray()
        };
    }

    /// <summary>
    /// Validates plan shape.
    /// </summary>
    public void ValidateShape()
    {
        if (string.IsNullOrWhiteSpace(PlanId))
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer plan id is required.");
        }

        if (Request is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer plan request is required.",
                PlanId);
        }

        Request.Validate();

        if (SourceWorkspace is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.SourceWorkspaceMissing,
                "Transfer plan source workspace is required.",
                Request.RequestId);
        }

        if (TargetWorkspace is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.TargetWorkspaceMissing,
                "Transfer plan target workspace is required.",
                Request.RequestId);
        }

        if (TransferObject is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.TransferObjectMissing,
                "Transfer plan object is required.",
                Request.RequestId);
        }

        if (Steps is null || Steps.Count == 0)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer plan steps are required.",
                Request.RequestId);
        }

        if (CreatedAt == default)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer plan creation timestamp is required.",
                Request.RequestId);
        }

        if (ValidationMessages is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer plan validation messages are required.",
                Request.RequestId);
        }

        foreach (var step in Steps)
        {
            step.Validate();
        }
    }
}
