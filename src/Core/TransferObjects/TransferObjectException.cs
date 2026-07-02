namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Represents a transfer object manager error.
/// </summary>
public sealed class TransferObjectException : Exception
{
    /// <summary>
    /// Initializes a transfer object exception.
    /// </summary>
    /// <param name="code">The transfer object error code.</param>
    /// <param name="message">A human-readable message.</param>
    /// <param name="objectId">The related object id, if any.</param>
    public TransferObjectException(
        TransferObjectErrorCode code,
        string message,
        string? objectId = null)
        : base(message)
    {
        Code = code;
        ObjectId = objectId;
    }

    /// <summary>
    /// Gets the transfer object error code.
    /// </summary>
    public TransferObjectErrorCode Code { get; }

    /// <summary>
    /// Gets the related transfer object id, if any.
    /// </summary>
    public string? ObjectId { get; }
}
