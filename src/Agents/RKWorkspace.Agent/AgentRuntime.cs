using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Runtime;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Agent;

internal sealed class AgentRuntime
{
    private readonly AgentConfiguration _configuration;
    private readonly TextWriter _output;
    private readonly List<string> _errors = new();
    private RuntimeEngine? _runtime;
    private WorkspaceId? _workspaceId;
    private AgentState _state = AgentState.Created;

    public AgentRuntime(AgentConfiguration configuration, TextWriter? output = null)
    {
        _configuration = configuration.Validate();
        _output = output ?? Console.Out;
    }

    public AgentState State => _state;

    public void Start()
    {
        if (_state == AgentState.Running)
        {
            return;
        }

        if (_state is AgentState.Starting or AgentState.Stopping)
        {
            throw new AgentException($"Agent cannot start from state {_state}.");
        }

        try
        {
            _state = AgentState.Starting;
            _runtime = new RuntimeEngine(new RuntimeConfiguration
            {
                LoggingEnabled = true,
                SimulationEnabled = _configuration.EnableDemoWorkspace,
                DiagnosticsEnabled = true
            });
            _runtime.Start();

            if (_configuration.EnableDemoWorkspace)
            {
                RegisterLocalWorkspace();
            }

            _state = AgentState.Running;
            if (_configuration.EnableConsoleStatus)
            {
                PrintStatus();
            }
        }
        catch (Exception ex)
        {
            _state = AgentState.Failed;
            _errors.Add(ex.Message);
            _output.WriteLine($"RK Workspace Agent failed: {ex.Message}");
            throw new AgentException("Agent failed to start.", ex);
        }
    }

    public void Stop()
    {
        if (_state is AgentState.Stopped or AgentState.Created)
        {
            _state = AgentState.Stopped;
            return;
        }

        try
        {
            _state = AgentState.Stopping;
            if (_runtime?.GetStatus() is RuntimeState.Running or RuntimeState.Paused)
            {
                _runtime.Shutdown();
            }

            _state = AgentState.Stopped;
            _output.WriteLine("RK Workspace Agent stopped cleanly");
        }
        catch (Exception ex)
        {
            _state = AgentState.Failed;
            _errors.Add(ex.Message);
            _output.WriteLine($"RK Workspace Agent failed: {ex.Message}");
            throw new AgentException("Agent failed to stop.", ex);
        }
    }

    public AgentDiagnostics GetDiagnostics()
    {
        var runtimeDiagnostics = _runtime?.GetDiagnostics();
        return new AgentDiagnostics
        {
            AgentId = _configuration.AgentId,
            DisplayName = _configuration.DisplayName,
            State = _state,
            WorkspaceId = _workspaceId?.ToString() ?? string.Empty,
            WorkspaceName = _configuration.WorkspaceName,
            Mode = _configuration.RunMode.ToString(),
            RuntimeState = runtimeDiagnostics?.RuntimeState ?? RuntimeState.Created,
            WorkspaceCount = runtimeDiagnostics?.WorkspaceCount ?? 0,
            PluginCount = runtimeDiagnostics?.PluginCount ?? 0,
            TransferObjectCount = runtimeDiagnostics?.TransferObjectCount ?? 0,
            Capabilities = GetCapabilitiesText(),
            Errors = _errors
                .Concat(runtimeDiagnostics?.Errors ?? Array.Empty<string>())
                .ToArray()
        };
    }

    public int RunOnce()
    {
        try
        {
            Start();
            Stop();
            return 0;
        }
        catch
        {
            return 1;
        }
    }

    public int RunUntilCancelled()
    {
        using var cancellation = new CancellationTokenSource();
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
                cancellation.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(_configuration.HeartbeatIntervalSeconds));
                if (!cancellation.IsCancellationRequested && _configuration.EnableConsoleStatus)
                {
                    _output.WriteLine($"Heartbeat: {DateTimeOffset.Now:HH:mm:ss} State: {_state}");
                }
            }

            Stop();
            return 0;
        }
        catch
        {
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    public void PrintStatus()
    {
        var diagnostics = GetDiagnostics();
        _output.WriteLine("RK Workspace Agent");
        _output.WriteLine($"AgentId: {diagnostics.AgentId}");
        _output.WriteLine($"State: {diagnostics.State}");
        _output.WriteLine($"Workspace: {diagnostics.WorkspaceName}");
        _output.WriteLine($"Runtime: {diagnostics.RuntimeState}");
        _output.WriteLine($"Capabilities: {diagnostics.Capabilities}");
        _output.WriteLine($"Mode: {diagnostics.Mode}");
    }

    private void RegisterLocalWorkspace()
    {
        if (_runtime is null)
        {
            throw new AgentException("Runtime is not initialized.");
        }

        _workspaceId = WorkspaceId.Create($"workspace-{_configuration.AgentId}");
        var capabilities = CapabilitySet.FromIds(
            CapabilityId.Display,
            CapabilityId.Keyboard,
            CapabilityId.Mouse,
            CapabilityId.Clipboard,
            CapabilityId.Encryption,
            CapabilityId.Pairing,
            CapabilityId.OfflineMode,
            CapabilityId.Logging);
        var descriptor = new WorkspaceDescriptor
        {
            WorkspaceId = _workspaceId,
            DisplayName = _configuration.WorkspaceName,
            WorkspaceType = _configuration.WorkspaceType,
            WorkspaceState = WorkspaceState.Available,
            Position = _configuration.WorkspacePosition,
            Capabilities = capabilities,
            IsTrusted = true,
            Priority = 10,
            LastSeen = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["agentId"] = _configuration.AgentId,
                ["runMode"] = _configuration.RunMode.ToString()
            }
        };

        _runtime.WorkspaceRegistry.RegisterWorkspace(Workspace.FromDescriptor(descriptor));
        _runtime.CapabilityManager.RegisterProvider(new AgentCapabilityProvider(descriptor));
    }

    private string GetCapabilitiesText()
    {
        if (_runtime is null || _runtime.GetStatus() == RuntimeState.Created)
        {
            return string.Empty;
        }

        var present = _runtime.CapabilityManager.GetCombinedCapabilities()
            .Capabilities
            .Select(capability => capability.CapabilityId)
            .ToHashSet();
        var preferredOrder = new[]
        {
            CapabilityId.Display,
            CapabilityId.Keyboard,
            CapabilityId.Mouse,
            CapabilityId.Clipboard,
            CapabilityId.Encryption,
            CapabilityId.Pairing,
            CapabilityId.OfflineMode,
            CapabilityId.Logging
        };
        var values = preferredOrder
            .Where(present.Contains)
            .Select(capability => capability.ToString())
            .ToArray();

        return values.Length == 0
            ? "None"
            : string.Join(", ", values);
    }

    private sealed class AgentCapabilityProvider : ICapabilityProvider
    {
        private readonly WorkspaceDescriptor _descriptor;

        public AgentCapabilityProvider(WorkspaceDescriptor descriptor)
        {
            _descriptor = descriptor.Snapshot();
        }

        public string ProviderId => $"agent:{_descriptor.WorkspaceId}";

        public string DisplayName => _descriptor.DisplayName;

        public CapabilitySet GetCapabilities()
        {
            return _descriptor.Capabilities.Snapshot();
        }
    }
}
