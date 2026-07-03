using System.Reflection;

namespace RKWorkspace.Shell;

/// <summary>
/// Starts the invisible Workspace Shell without platform hooks, overlay windows, networking or adapters.
/// </summary>
public sealed class WorkspaceShellRuntime : IWorkspaceShellRuntime
{
    private const string SessionId = "workspace-shell-session";
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();
    private DateTimeOffset? _startedAt;
    private WorkspaceShellRuntimeState _state = WorkspaceShellRuntimeState.Created;

    public WorkspaceShellRuntime(WorkspaceShell? shell = null)
    {
        Shell = shell ?? new WorkspaceShell(new WorkspaceShellConfiguration
        {
            DiagnosticsEnabled = true,
            OverlayEnabled = true
        });
    }

    public WorkspaceShellRuntimeState State => _state;

    public WorkspaceShell Shell { get; }

    public void Start()
    {
        if (_state == WorkspaceShellRuntimeState.Running)
        {
            return;
        }

        if (_state is WorkspaceShellRuntimeState.Starting or WorkspaceShellRuntimeState.Stopping)
        {
            throw new WorkspaceShellRuntimeException(
                $"Workspace Shell cannot start from state {_state}.",
                _state);
        }

        try
        {
            _state = WorkspaceShellRuntimeState.Starting;
            Shell.Start();

            if (Shell.CurrentSession is null)
            {
                Shell.BeginSession(SessionId);
            }

            Shell.UpdateCarryState(WorkspaceCarryState.Empty, "HX-000");
            _startedAt ??= DateTimeOffset.UtcNow;
            _state = WorkspaceShellRuntimeState.Running;
        }
        catch (Exception ex) when (ex is not WorkspaceShellRuntimeException)
        {
            _state = WorkspaceShellRuntimeState.Failed;
            _errors.Add(ex.Message);
            throw new WorkspaceShellRuntimeException(
                "Workspace Shell failed to start.",
                _state,
                ex);
        }
    }

    public void Stop()
    {
        if (_state is WorkspaceShellRuntimeState.Stopped or WorkspaceShellRuntimeState.Created)
        {
            _state = WorkspaceShellRuntimeState.Stopped;
            return;
        }

        try
        {
            _state = WorkspaceShellRuntimeState.Stopping;
            Shell.Stop();
            _state = WorkspaceShellRuntimeState.Stopped;
        }
        catch (Exception ex)
        {
            _state = WorkspaceShellRuntimeState.Failed;
            _errors.Add(ex.Message);
            throw new WorkspaceShellRuntimeException(
                "Workspace Shell failed to stop.",
                _state,
                ex);
        }
    }

    public WorkspaceShellDiagnostics GetDiagnostics()
    {
        var session = Shell.CurrentSession;
        var overlay = Shell.OverlayManager.Current;
        var startedAt = _startedAt;

        return new WorkspaceShellDiagnostics
        {
            ProductMode = "Shell",
            State = _state,
            Session = session is null ? "Inactive" : "Active",
            CarryState = session?.CarryState ?? WorkspaceCarryState.Empty,
            Overlay = overlay.IsVisible ? "Active" : "Inactive",
            RuntimeVersion = GetRuntimeVersion(),
            StartedAt = startedAt,
            Uptime = startedAt is null ? TimeSpan.Zero : DateTimeOffset.UtcNow - startedAt.Value,
            HumanExperienceReferences = WorkspaceShell.HumanExperienceReferences,
            Errors = _errors.ToArray(),
            Warnings = _warnings.ToArray()
        };
    }

    public int RunOnce()
    {
        try
        {
            Start();
            return 0;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<int> RunUntilCancelled(CancellationToken cancellationToken = default)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ConsoleCancelEventHandler handler = (_, args) =>
        {
            args.Cancel = true;
            cancellation.Cancel();
        };

        Console.CancelKeyPress += handler;
        try
        {
            Start();
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellation.Token);
            }

            Stop();
            return 0;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            Stop();
            return 0;
        }
        catch (Exception ex)
        {
            _state = WorkspaceShellRuntimeState.Failed;
            _errors.Add(ex.Message);
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    private static string GetRuntimeVersion()
    {
        return typeof(WorkspaceShellRuntime).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";
    }
}
