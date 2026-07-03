namespace RKWorkspace.Shell;

/// <summary>
/// Describes the lifecycle of the invisible Workspace Shell runtime process.
/// </summary>
public enum WorkspaceShellRuntimeState
{
    Created = 0,
    Starting = 1,
    Running = 2,
    Paused = 3,
    Stopping = 4,
    Stopped = 5,
    Failed = 6
}
