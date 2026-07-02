using RKWorkspace.Core.Capabilities;

namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Describes filters for workspace search and target selection.
/// </summary>
public sealed record WorkspaceQuery
{
    /// <summary>
    /// Gets a workspace type filter.
    /// </summary>
    public WorkspaceType? WorkspaceType { get; init; }

    /// <summary>
    /// Gets a workspace state filter.
    /// </summary>
    public WorkspaceState? WorkspaceState { get; init; }

    /// <summary>
    /// Gets a logical position filter.
    /// </summary>
    public WorkspacePosition? Position { get; init; }

    /// <summary>
    /// Gets required capabilities.
    /// </summary>
    public CapabilitySet RequiredCapabilities { get; init; } = CapabilitySet.Empty;

    /// <summary>
    /// Gets optional capabilities used for ranking.
    /// </summary>
    public CapabilitySet OptionalCapabilities { get; init; } = CapabilitySet.Empty;

    /// <summary>
    /// Gets whether only trusted workspaces are accepted.
    /// </summary>
    public bool TrustedOnly { get; init; }

    /// <summary>
    /// Gets whether only available workspaces are accepted.
    /// </summary>
    public bool AvailableOnly { get; init; }

    /// <summary>
    /// Gets the minimum selection priority.
    /// </summary>
    public int? MinimumPriority { get; init; }

    /// <summary>
    /// Gets an empty query.
    /// </summary>
    public static WorkspaceQuery Empty { get; } = new();

    /// <summary>
    /// Converts this query to a capability requirement.
    /// </summary>
    /// <returns>A capability requirement.</returns>
    public CapabilityRequirement ToCapabilityRequirement()
    {
        return new CapabilityRequirement
        {
            RequiredCapabilities = RequiredCapabilities.Capabilities
                .Select(capability => capability.CapabilityId)
                .ToArray(),
            OptionalCapabilities = OptionalCapabilities.Capabilities
                .Select(capability => capability.CapabilityId)
                .ToArray()
        };
    }

    /// <summary>
    /// Validates the query.
    /// </summary>
    public void Validate()
    {
        if (RequiredCapabilities is null || OptionalCapabilities is null)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidQuery,
                "Workspace query capabilities are required.");
        }

        if (MinimumPriority < 0)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidQuery,
                "Minimum priority must not be negative.");
        }

        ToCapabilityRequirement().Validate();
    }
}
