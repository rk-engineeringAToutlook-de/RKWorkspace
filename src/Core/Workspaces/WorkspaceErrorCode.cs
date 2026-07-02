namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Identifies workspace registry error cases.
/// </summary>
public enum WorkspaceErrorCode
{
    /// <summary>
    /// A workspace id was missing.
    /// </summary>
    MissingWorkspaceId,

    /// <summary>
    /// A workspace is already registered.
    /// </summary>
    WorkspaceAlreadyRegistered,

    /// <summary>
    /// A workspace is not registered.
    /// </summary>
    WorkspaceNotRegistered,

    /// <summary>
    /// A workspace descriptor is invalid.
    /// </summary>
    InvalidDescriptor,

    /// <summary>
    /// A workspace query is invalid.
    /// </summary>
    InvalidQuery,

    /// <summary>
    /// No matching target workspace was found.
    /// </summary>
    TargetNotFound,

    /// <summary>
    /// A workspace operation is invalid.
    /// </summary>
    InvalidOperation
}
