namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Describes the result of matching a capability set against a requirement.
/// </summary>
public sealed record CapabilityMatchResult
{
    /// <summary>
    /// Gets whether the capability set matches the requirement.
    /// </summary>
    public required bool IsMatch { get; init; }

    /// <summary>
    /// Gets required capabilities that were missing.
    /// </summary>
    public IReadOnlyCollection<CapabilityId> MissingRequiredCapabilities { get; init; } = Array.Empty<CapabilityId>();

    /// <summary>
    /// Gets optional capabilities that were present.
    /// </summary>
    public IReadOnlyCollection<CapabilityId> PresentOptionalCapabilities { get; init; } = Array.Empty<CapabilityId>();

    /// <summary>
    /// Gets forbidden capabilities that were present.
    /// </summary>
    public IReadOnlyCollection<CapabilityId> PresentForbiddenCapabilities { get; init; } = Array.Empty<CapabilityId>();

    /// <summary>
    /// Gets a simple score for ranking matching providers.
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Gets a human-readable reason.
    /// </summary>
    public required string Reason { get; init; }
}
