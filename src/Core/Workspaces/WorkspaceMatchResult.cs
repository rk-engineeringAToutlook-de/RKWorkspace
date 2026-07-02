using RKWorkspace.Core.Capabilities;

namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Describes the result of matching a workspace against a query.
/// </summary>
public sealed record WorkspaceMatchResult
{
    /// <summary>
    /// Gets the matched workspace.
    /// </summary>
    public required IWorkspace Workspace { get; init; }

    /// <summary>
    /// Gets whether the workspace matches.
    /// </summary>
    public required bool IsMatch { get; init; }

    /// <summary>
    /// Gets the simple ranking score.
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Gets diagnostic reasons.
    /// </summary>
    public IReadOnlyCollection<string> Reasons { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets missing required capabilities.
    /// </summary>
    public IReadOnlyCollection<CapabilityId> MissingCapabilities { get; init; } =
        Array.Empty<CapabilityId>();
}
