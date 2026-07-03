namespace RKWorkspace.Shell;

/// <summary>
/// Defines the invisible workspace shell foundation. It is not an application and performs no platform integration.
/// </summary>
public sealed class WorkspaceShell
{
    public static IReadOnlyList<string> HumanExperienceReferences { get; } =
        new[] { "HX-000", "HX-001", "HX-001A", "HX-002" };

    public WorkspaceShell(WorkspaceShellConfiguration? configuration = null)
    {
        Configuration = (configuration ?? WorkspaceShellConfiguration.Default).Snapshot();
        OverlayManager = new WorkspaceOverlayManager();
    }

    public WorkspaceShellConfiguration Configuration { get; }

    public WorkspaceShellState State { get; private set; } = WorkspaceShellState.Created;

    public WorkspaceSession? CurrentSession { get; private set; }

    public WorkspaceOverlayManager OverlayManager { get; }

    public void Start()
    {
        if (State is WorkspaceShellState.Stopped)
        {
            throw new InvalidOperationException("Workspace Shell cannot be restarted after it has stopped.");
        }

        State = WorkspaceShellState.Ready;
    }

    public WorkspaceSession BeginSession(string sessionId)
    {
        if (State == WorkspaceShellState.Created)
        {
            Start();
        }

        CurrentSession = new WorkspaceSession
        {
            SessionId = sessionId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        State = WorkspaceShellState.Observing;
        return CurrentSession;
    }

    public WorkspaceSession UpdateCarryState(WorkspaceCarryState carryState, string? humanExperienceReference = null)
    {
        if (CurrentSession is null)
        {
            throw new InvalidOperationException("Workspace Shell requires a session before carry state can change.");
        }

        CurrentSession = CurrentSession.WithCarryState(carryState);
        State = carryState is WorkspaceCarryState.Empty or WorkspaceCarryState.Placed or WorkspaceCarryState.Cancelled or WorkspaceCarryState.Lost
            ? WorkspaceShellState.Observing
            : WorkspaceShellState.Engaged;

        if (Configuration.OverlayEnabled && State == WorkspaceShellState.Engaged)
        {
            OverlayManager.ShowForHumanExperience(
                carryState,
                humanExperienceReference ?? "HX-002",
                "Human experience is active.");
        }
        else
        {
            OverlayManager.Hide();
        }

        return CurrentSession;
    }

    public void Suspend()
    {
        OverlayManager.Hide();
        State = WorkspaceShellState.Suspended;
    }

    public void Stop()
    {
        OverlayManager.Hide();
        CurrentSession = null;
        State = WorkspaceShellState.Stopped;
    }
}
