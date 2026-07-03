namespace RKWorkspace.Shell;

/// <summary>
/// Represents a Workspace Shell runtime lifecycle failure.
/// </summary>
public sealed class WorkspaceShellRuntimeException : Exception
{
    public WorkspaceShellRuntimeException(string message, WorkspaceShellRuntimeState runtimeState)
        : base(message)
    {
        RuntimeState = runtimeState;
    }

    public WorkspaceShellRuntimeException(
        string message,
        WorkspaceShellRuntimeState runtimeState,
        Exception innerException)
        : base(message, innerException)
    {
        RuntimeState = runtimeState;
    }

    public WorkspaceShellRuntimeState RuntimeState { get; }
}
