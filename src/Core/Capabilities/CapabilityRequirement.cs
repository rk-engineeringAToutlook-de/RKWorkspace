namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Describes required, optional and forbidden capabilities.
/// </summary>
public sealed record CapabilityRequirement
{
    /// <summary>
    /// Gets capabilities that must be present.
    /// </summary>
    public IReadOnlyCollection<CapabilityId> RequiredCapabilities { get; init; } = Array.Empty<CapabilityId>();

    /// <summary>
    /// Gets capabilities that improve matching when present.
    /// </summary>
    public IReadOnlyCollection<CapabilityId> OptionalCapabilities { get; init; } = Array.Empty<CapabilityId>();

    /// <summary>
    /// Gets capabilities that must not be present.
    /// </summary>
    public IReadOnlyCollection<CapabilityId> ForbiddenCapabilities { get; init; } = Array.Empty<CapabilityId>();

    /// <summary>
    /// Gets an empty requirement.
    /// </summary>
    public static CapabilityRequirement Empty { get; } = new();

    /// <summary>
    /// Creates a requirement with required capabilities.
    /// </summary>
    /// <param name="capabilityIds">The required capability ids.</param>
    /// <returns>A capability requirement.</returns>
    public static CapabilityRequirement Require(params CapabilityId[] capabilityIds)
    {
        return new CapabilityRequirement
        {
            RequiredCapabilities = capabilityIds
        };
    }

    /// <summary>
    /// Validates that the requirement does not contain invalid capability ids.
    /// </summary>
    public void Validate()
    {
        var allIds = RequiredCapabilities
            .Concat(OptionalCapabilities)
            .Concat(ForbiddenCapabilities)
            .ToArray();

        if (allIds.Any(id => id == CapabilityId.Unknown))
        {
            throw new CapabilityException(
                CapabilityErrorCode.InvalidRequirement,
                "Capability requirements must not contain Unknown.");
        }
    }
}
