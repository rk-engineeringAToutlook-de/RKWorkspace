namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Defines platform-neutral capability provider registry and matching operations.
/// </summary>
public interface ICapabilityManager
{
    /// <summary>
    /// Registers a capability provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    void RegisterProvider(ICapabilityProvider provider);

    /// <summary>
    /// Unregisters a capability provider.
    /// </summary>
    /// <param name="providerId">The provider id.</param>
    void UnregisterProvider(string providerId);

    /// <summary>
    /// Gets a registered provider.
    /// </summary>
    /// <param name="providerId">The provider id.</param>
    /// <returns>The provider, or null when it is not registered.</returns>
    ICapabilityProvider? GetProvider(string providerId);

    /// <summary>
    /// Gets all registered providers.
    /// </summary>
    /// <returns>All providers.</returns>
    IReadOnlyCollection<ICapabilityProvider> GetAllProviders();

    /// <summary>
    /// Gets a snapshot of capabilities for a provider.
    /// </summary>
    /// <param name="providerId">The provider id.</param>
    /// <returns>The provider capability set.</returns>
    CapabilitySet GetCapabilitiesForProvider(string providerId);

    /// <summary>
    /// Gets a combined capability set across all registered providers.
    /// </summary>
    /// <returns>The combined capabilities.</returns>
    CapabilitySet GetCombinedCapabilities();

    /// <summary>
    /// Matches a capability set against a requirement.
    /// </summary>
    /// <param name="capabilities">The capability set.</param>
    /// <param name="requirement">The requirement.</param>
    /// <returns>The match result.</returns>
    CapabilityMatchResult MatchRequirement(CapabilitySet capabilities, CapabilityRequirement requirement);

    /// <summary>
    /// Finds providers matching a requirement.
    /// </summary>
    /// <param name="requirement">The requirement.</param>
    /// <returns>Matching providers.</returns>
    IReadOnlyCollection<ICapabilityProvider> FindProvidersMatching(CapabilityRequirement requirement);

    /// <summary>
    /// Gets whether a capability is available across registered providers.
    /// </summary>
    /// <param name="capabilityId">The capability id.</param>
    /// <returns>True when available; otherwise false.</returns>
    bool IsCapabilityAvailable(CapabilityId capabilityId);
}
