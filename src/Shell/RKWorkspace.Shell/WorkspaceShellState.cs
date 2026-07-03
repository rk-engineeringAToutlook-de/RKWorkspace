namespace RKWorkspace.Shell;

/// <summary>
/// Describes the neutral shell lifecycle before any operating system integration exists.
/// </summary>
public enum WorkspaceShellState
{
    Created = 0,
    Ready = 1,
    Observing = 2,
    Engaged = 3,
    Suspended = 4,
    Stopped = 5
}
