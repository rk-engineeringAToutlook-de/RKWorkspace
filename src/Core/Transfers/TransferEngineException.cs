namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Represents a platform-neutral transfer engine error.
/// </summary>
public sealed class TransferEngineException : Exception
{
    /// <summary>
    /// Initializes a transfer engine exception.
    /// </summary>
    /// <param name="reason">The transfer failure reason.</param>
    /// <param name="message">A human-readable message.</param>
    /// <param name="requestId">The related request id, if any.</param>
    /// <param name="innerException">The inner exception, if any.</param>
    public TransferEngineException(
        TransferFailureReason reason,
        string message,
        string? requestId = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Reason = reason;
        RequestId = requestId;
    }

    /// <summary>
    /// Gets the transfer failure reason.
    /// </summary>
    public TransferFailureReason Reason { get; }

    /// <summary>
    /// Gets the related request id, if any.
    /// </summary>
    public string? RequestId { get; }
}
