namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Describes the lifecycle status of a transfer plan step.
/// </summary>
public enum TransferStepStatus
{
    /// <summary>
    /// The step is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// The step is running.
    /// </summary>
    Running,

    /// <summary>
    /// The step completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The step failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The step was skipped.
    /// </summary>
    Skipped
}

/// <summary>
/// Describes one logical transfer plan step.
/// </summary>
public sealed record TransferStep
{
    /// <summary>
    /// Gets the step number.
    /// </summary>
    public required int StepNumber { get; init; }

    /// <summary>
    /// Gets the stable step name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the human-readable step description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the step status.
    /// </summary>
    public TransferStepStatus Status { get; init; } = TransferStepStatus.Pending;

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the completion timestamp.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Gets the step error message.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Creates a pending transfer step.
    /// </summary>
    /// <param name="stepNumber">The step number.</param>
    /// <param name="name">The step name.</param>
    /// <param name="description">The step description.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The transfer step.</returns>
    public static TransferStep Pending(
        int stepNumber,
        string name,
        string description,
        DateTimeOffset createdAt)
    {
        return new TransferStep
        {
            StepNumber = stepNumber,
            Name = name,
            Description = description,
            CreatedAt = createdAt
        };
    }

    /// <summary>
    /// Returns the step marked as completed.
    /// </summary>
    /// <param name="completedAt">The completion timestamp.</param>
    /// <returns>The completed transfer step.</returns>
    public TransferStep Complete(DateTimeOffset completedAt)
    {
        return this with
        {
            Status = TransferStepStatus.Completed,
            CompletedAt = completedAt,
            ErrorMessage = string.Empty
        };
    }

    /// <summary>
    /// Returns the step marked as failed.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="completedAt">The completion timestamp.</param>
    /// <returns>The failed transfer step.</returns>
    public TransferStep Fail(string message, DateTimeOffset completedAt)
    {
        return this with
        {
            Status = TransferStepStatus.Failed,
            CompletedAt = completedAt,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Validates the transfer step.
    /// </summary>
    public void Validate()
    {
        if (StepNumber <= 0)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer step number must be positive.");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer step name is required.");
        }

        if (CreatedAt == default)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                "Transfer step creation timestamp is required.");
        }
    }
}
