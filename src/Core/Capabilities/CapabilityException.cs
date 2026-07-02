namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Represents a capability provider, set or requirement error.
/// </summary>
public sealed class CapabilityException : Exception
{
    /// <summary>
    /// Initializes a new capability exception.
    /// </summary>
    /// <param name="code">The capability error code.</param>
    /// <param name="message">A human-readable message.</param>
    /// <param name="providerId">The related provider id, if any.</param>
    /// <param name="capabilityId">The related capability id, if any.</param>
    public CapabilityException(
        CapabilityErrorCode code,
        string message,
        string? providerId = null,
        CapabilityId? capabilityId = null)
        : base(message)
    {
        Code = code;
        ProviderId = providerId;
        CapabilityId = capabilityId;
    }

    /// <summary>
    /// Gets the capability error code.
    /// </summary>
    public CapabilityErrorCode Code { get; }

    /// <summary>
    /// Gets the related provider id, if any.
    /// </summary>
    public string? ProviderId { get; }

    /// <summary>
    /// Gets the related capability id, if any.
    /// </summary>
    public CapabilityId? CapabilityId { get; }
}
