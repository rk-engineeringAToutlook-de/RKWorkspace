namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Identifies capability manager errors.
/// </summary>
public enum CapabilityErrorCode
{
    /// <summary>
    /// Provider id is missing.
    /// </summary>
    MissingProviderId,

    /// <summary>
    /// Provider is already registered.
    /// </summary>
    ProviderAlreadyRegistered,

    /// <summary>
    /// Provider is not registered.
    /// </summary>
    ProviderNotRegistered,

    /// <summary>
    /// Capability is missing.
    /// </summary>
    CapabilityMissing,

    /// <summary>
    /// Requirement is invalid.
    /// </summary>
    InvalidRequirement,

    /// <summary>
    /// Capability id is unknown or invalid for the operation.
    /// </summary>
    InvalidCapabilityId,

    /// <summary>
    /// Operation is invalid for the current state.
    /// </summary>
    InvalidOperation
}
