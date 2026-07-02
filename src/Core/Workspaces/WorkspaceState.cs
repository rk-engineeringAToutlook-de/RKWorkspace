namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Describes the neutral lifecycle state of a workspace.
/// </summary>
public enum WorkspaceState
{
    /// <summary>
    /// The workspace was discovered.
    /// </summary>
    Discovered,

    /// <summary>
    /// The workspace is registered.
    /// </summary>
    Registered,

    /// <summary>
    /// The workspace is available for selection.
    /// </summary>
    Available,

    /// <summary>
    /// The workspace is unavailable.
    /// </summary>
    Unavailable,

    /// <summary>
    /// The workspace is trusted.
    /// </summary>
    Trusted,

    /// <summary>
    /// The workspace is explicitly untrusted.
    /// </summary>
    Untrusted,

    /// <summary>
    /// The workspace is disabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// The workspace failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The workspace state is unknown.
    /// </summary>
    Unknown
}
