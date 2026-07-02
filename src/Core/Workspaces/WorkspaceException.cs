namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Represents a workspace registry error.
/// </summary>
public sealed class WorkspaceException : Exception
{
    /// <summary>
    /// Initializes a new workspace exception.
    /// </summary>
    /// <param name="code">The workspace error code.</param>
    /// <param name="message">A human-readable message.</param>
    /// <param name="workspaceId">The related workspace id, if any.</param>
    public WorkspaceException(
        WorkspaceErrorCode code,
        string message,
        string? workspaceId = null)
        : base(message)
    {
        Code = code;
        WorkspaceId = workspaceId;
    }

    /// <summary>
    /// Gets the workspace error code.
    /// </summary>
    public WorkspaceErrorCode Code { get; }

    /// <summary>
    /// Gets the related workspace id, if any.
    /// </summary>
    public string? WorkspaceId { get; }
}
