namespace RKWorkspace.Shell;

/// <summary>
/// Controls the lifecycle of the prepared Workspace Shell product path.
/// </summary>
public interface IWorkspaceShellRuntime
{
    WorkspaceShellRuntimeState State { get; }

    WorkspaceShell Shell { get; }

    void Start();

    void Stop();

    WorkspaceShellDiagnostics GetDiagnostics();

    int RunOnce();

    Task<int> RunUntilCancelled(CancellationToken cancellationToken = default);
}
