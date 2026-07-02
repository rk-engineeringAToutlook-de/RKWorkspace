namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Defines a platform-neutral source of capabilities.
/// </summary>
public interface ICapabilityProvider
{
    /// <summary>
    /// Gets the stable provider id.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Gets the user-visible provider name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the current capability set for this provider.
    /// </summary>
    /// <returns>The provider capability set.</returns>
    CapabilitySet GetCapabilities();
}
