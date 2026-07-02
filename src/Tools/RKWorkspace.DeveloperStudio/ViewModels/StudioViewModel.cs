using RKWorkspace.Agent;
using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Runtime;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Transfers;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class StudioViewModel
{
    private static readonly WorkspaceId SourceWorkspaceId = WorkspaceId.Create("RKWS-Demo-Laptop");
    private static readonly WorkspaceId TargetWorkspaceId = WorkspaceId.Create("RKWS-Demo-Display-Right");
    private readonly List<StudioLogEntry> _logEntries = new();
    private RuntimeEngine _runtime;
    private TransferEngine? _transferEngine;
    private AgentRuntime? _agentA;
    private AgentRuntime? _agentB;
    private TransferObjectId? _demoObjectId;
    private TransferResult? _lastTransferResult;
    private string _lastError = string.Empty;
    private string _lastResult = "Not run";

    public StudioViewModel()
    {
        _runtime = CreateRuntime();
        Refresh();
        AddLog("Studio Created", "Ready", string.Empty);
    }

    public IReadOnlyCollection<StudioWorkspaceRow> Workspaces { get; private set; } = Array.Empty<StudioWorkspaceRow>();

    public IReadOnlyCollection<StudioTransferObjectRow> TransferObjects { get; private set; } =
        Array.Empty<StudioTransferObjectRow>();

    public IReadOnlyCollection<StudioAgentRow> Agents { get; private set; } = Array.Empty<StudioAgentRow>();

    public IReadOnlyCollection<StudioLogEntry> LogEntries => _logEntries.ToArray();

    public StudioDiagnosticsSnapshot Diagnostics { get; private set; } = EmptyDiagnostics();

    public bool StartRuntime()
    {
        return Execute("Start Runtime", () =>
        {
            if (_runtime.GetStatus() == RuntimeState.Running)
            {
                return "Already running";
            }

            _runtime.Start();
            _transferEngine = new TransferEngine(
                _runtime.WorkspaceRegistry,
                _runtime.CapabilityManager,
                _runtime.TransferObjectManager);
            return "Runtime running";
        });
    }

    public bool AddDemoWorkspaces()
    {
        return Execute("Add Demo Workspaces", () =>
        {
            EnsureRuntime();
            RegisterWorkspace(DemoWorkspace(
                SourceWorkspaceId,
                WorkspacePosition.Center,
                priority: 10,
                CapabilityId.Display,
                CapabilityId.Keyboard,
                CapabilityId.Clipboard,
                CapabilityId.Encryption,
                CapabilityId.Pairing));
            RegisterWorkspace(DemoWorkspace(
                TargetWorkspaceId,
                WorkspacePosition.Right,
                priority: 5,
                CapabilityId.Display,
                CapabilityId.Clipboard,
                CapabilityId.Encryption,
                CapabilityId.Pairing));

            return "Demo workspaces available";
        });
    }

    public bool CreateTextObject()
    {
        return Execute("Create Text Object", () =>
        {
            EnsureRuntime();
            if (_demoObjectId is not null && _runtime.TransferObjectManager.Get(_demoObjectId) is not null)
            {
                return "Demo text object already exists";
            }

            var transferObject = _runtime.TransferObjectManager.Create(
                TransferObjectType.Text,
                DemoMetadata(SourceWorkspaceId.ToString()));
            _demoObjectId = transferObject.Id;

            return $"Created {transferObject.Metadata.DisplayName}";
        });
    }

    public bool TransferRight()
    {
        return Execute("Transfer Right", () =>
        {
            EnsureRuntime();
            EnsureDemoReady();

            var request = new TransferRequest
            {
                RequestId = $"studio-request-{Guid.NewGuid():N}",
                SourceWorkspaceId = SourceWorkspaceId,
                RequestedDirection = TransferDirection.Right,
                TransferObjectId = _demoObjectId!,
                RequiredCapabilities = CapabilitySet.FromIds(
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing),
                OptionalCapabilities = CapabilitySet.Empty,
                ForbiddenCapabilities = CapabilitySet.Empty,
                CreatedAt = DateTimeOffset.UtcNow,
                RequestedBy = "developer-studio",
                Metadata = new Dictionary<string, string>
                {
                    ["tool"] = "developer-studio"
                }
            };

            var result = GetTransferEngine().ExecuteLogicalTransfer(request);
            _lastTransferResult = result;
            _lastResult = result.IsSuccess ? "SUCCESS" : $"FAILED: {result.FailureReason}";
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(string.Join(" ", result.Messages));
            }

            return $"Transfer completed to {result.TargetWorkspace?.Descriptor.WorkspaceId}";
        });
    }

    public bool StartDualAgents()
    {
        return Execute("Start Dual Agents", () =>
        {
            _agentA ??= CreateAgent(AgentConfiguration.CreateLocalAgentA());
            _agentB ??= CreateAgent(AgentConfiguration.CreateLocalAgentB());

            if (_agentA.State == AgentState.Running && _agentB.State == AgentState.Running)
            {
                return "Dual agents already running";
            }

            Task.WaitAll(
                Task.Run(() => StartAgentIfNeeded(_agentA)),
                Task.Run(() => StartAgentIfNeeded(_agentB)));

            return "Dual agents running";
        });
    }

    public bool StopDualAgents()
    {
        return Execute("Stop Dual Agents", () =>
        {
            if (_agentA is null && _agentB is null)
            {
                return "No dual agents running";
            }

            Task.WaitAll(
                Task.Run(() => StopAgentIfNeeded(_agentA)),
                Task.Run(() => StopAgentIfNeeded(_agentB)));

            return "Dual agents stopped";
        });
    }

    public bool Reset()
    {
        try
        {
            StopAgentIfNeeded(_agentA);
            StopAgentIfNeeded(_agentB);
            _agentA = null;
            _agentB = null;

            if (_runtime.GetStatus() is RuntimeState.Running or RuntimeState.Paused)
            {
                _runtime.Shutdown();
            }

            _runtime = CreateRuntime();
            _transferEngine = null;
            _demoObjectId = null;
            _lastTransferResult = null;
            _lastError = string.Empty;
            _lastResult = "Not run";
            _logEntries.Clear();
            AddLog("Reset", "Runtime reset", string.Empty);
            Refresh();
            return true;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            AddLog("Reset", "FAILED", ex.Message);
            Refresh();
            return false;
        }
    }

    public bool RunFullDemo()
    {
        var started = StartRuntime();
        var workspaces = AddDemoWorkspaces();
        var textObject = CreateTextObject();
        var transfer = TransferRight();
        var dualAgents = StartDualAgents();

        return started && workspaces && textObject && transfer && dualAgents;
    }

    private bool Execute(string action, Func<string> operation)
    {
        try
        {
            var result = operation();
            _lastError = string.Empty;
            AddLog(action, result, string.Empty);
            Refresh();
            return true;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            _lastResult = "FAILED";
            AddLog(action, "FAILED", ex.Message);
            Refresh();
            return false;
        }
    }

    private void RegisterWorkspace(WorkspaceDescriptor descriptor)
    {
        if (!_runtime.WorkspaceRegistry.ContainsWorkspace(descriptor.WorkspaceId))
        {
            _runtime.WorkspaceRegistry.RegisterWorkspace(Workspace.FromDescriptor(descriptor));
        }

        var provider = new WorkspaceCapabilityProvider(descriptor);
        if (_runtime.CapabilityManager.GetProvider(provider.ProviderId) is null)
        {
            _runtime.CapabilityManager.RegisterProvider(provider);
        }
    }

    private static AgentRuntime CreateAgent(AgentConfiguration configuration)
    {
        return new AgentRuntime(
            configuration with { EnableConsoleStatus = false },
            TextWriter.Null);
    }

    private static void StartAgentIfNeeded(AgentRuntime agent)
    {
        if (agent.State != AgentState.Running)
        {
            agent.Start();
        }
    }

    private static void StopAgentIfNeeded(AgentRuntime? agent)
    {
        if (agent?.State == AgentState.Running)
        {
            agent.Stop();
        }
    }

    private IReadOnlyCollection<StudioAgentRow> GetAgentRows()
    {
        return new[] { _agentA, _agentB }
            .Where(agent => agent is not null)
            .Select(agent => agent!.GetDiagnostics())
            .Select(diagnostics => new StudioAgentRow
            {
                AgentId = diagnostics.AgentId,
                DisplayName = diagnostics.DisplayName,
                Runtime = diagnostics.RuntimeState.ToString(),
                Workspace = diagnostics.WorkspaceName,
                WorkspaceId = diagnostics.WorkspaceId,
                Status = diagnostics.State.ToString()
            })
            .ToArray();
    }

    private void EnsureRuntime()
    {
        if (_runtime.GetStatus() != RuntimeState.Running)
        {
            throw new InvalidOperationException("Runtime must be running.");
        }
    }

    private void EnsureDemoReady()
    {
        if (!_runtime.WorkspaceRegistry.ContainsWorkspace(SourceWorkspaceId) ||
            !_runtime.WorkspaceRegistry.ContainsWorkspace(TargetWorkspaceId))
        {
            throw new InvalidOperationException("Demo workspaces are required.");
        }

        if (_demoObjectId is null || _runtime.TransferObjectManager.Get(_demoObjectId) is null)
        {
            throw new InvalidOperationException("Demo text object is required.");
        }
    }

    private TransferEngine GetTransferEngine()
    {
        return _transferEngine ?? throw new InvalidOperationException("Transfer Engine is not initialized.");
    }

    private void Refresh()
    {
        Workspaces = RuntimeAvailable()
            ? _runtime.WorkspaceRegistry.GetAllWorkspaces()
                .Select(workspace => new StudioWorkspaceRow
                {
                    Name = workspace.Descriptor.DisplayName,
                    Type = workspace.Descriptor.WorkspaceType.ToString(),
                    Position = workspace.Descriptor.Position.ToString(),
                    State = workspace.Descriptor.WorkspaceState.ToString(),
                    Trusted = workspace.Descriptor.IsTrusted,
                    Priority = workspace.Descriptor.Priority
                })
                .ToArray()
            : Array.Empty<StudioWorkspaceRow>();

        TransferObjects = RuntimeAvailable()
            ? _runtime.TransferObjectManager.GetAll()
                .Select(transferObject => new StudioTransferObjectRow
                {
                    ObjectType = transferObject.ObjectType.ToString(),
                    DisplayName = transferObject.Metadata.DisplayName,
                    State = transferObject.State.ToString(),
                    Source = transferObject.Metadata.SourceWorkspace,
                    Target = transferObject.Metadata.TargetWorkspace
                })
                .ToArray()
            : Array.Empty<StudioTransferObjectRow>();

        Agents = GetAgentRows();

        var diagnostics = _runtime.GetDiagnostics();
        Diagnostics = new StudioDiagnosticsSnapshot
        {
            RuntimeVersion = diagnostics.RuntimeVersion,
            RuntimeState = diagnostics.RuntimeState.ToString(),
            PluginCount = diagnostics.PluginCount,
            WorkspaceCount = diagnostics.WorkspaceCount,
            TransferObjectCount = diagnostics.TransferObjectCount,
            Capabilities = GetCapabilitiesText(),
            LastResult = _lastResult,
            LastError = _lastError
        };
    }

    private string GetCapabilitiesText()
    {
        if (!RuntimeAvailable())
        {
            return string.Empty;
        }

        var capabilities = _runtime.CapabilityManager.GetCombinedCapabilities()
            .Capabilities
            .Select(capability => capability.CapabilityId.ToString())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return capabilities.Length == 0
            ? "None"
            : string.Join(", ", capabilities);
    }

    private bool RuntimeAvailable()
    {
        return _runtime.GetStatus() is RuntimeState.Running or RuntimeState.Paused or RuntimeState.Stopped;
    }

    private void AddLog(string action, string result, string error)
    {
        _logEntries.Add(new StudioLogEntry
        {
            Time = DateTimeOffset.Now.ToString("HH:mm:ss"),
            Action = action,
            Result = result,
            Error = error
        });
    }

    private static RuntimeEngine CreateRuntime()
    {
        return new RuntimeEngine(new RuntimeConfiguration
        {
            LoggingEnabled = true,
            SimulationEnabled = true,
            TestModeEnabled = true,
            DiagnosticsEnabled = true,
            DebugModeEnabled = false
        });
    }

    private static WorkspaceDescriptor DemoWorkspace(
        WorkspaceId workspaceId,
        WorkspacePosition position,
        int priority,
        params CapabilityId[] capabilityIds)
    {
        return new WorkspaceDescriptor
        {
            WorkspaceId = workspaceId,
            DisplayName = workspaceId.ToString(),
            WorkspaceType = WorkspaceType.SmartDevice,
            WorkspaceState = WorkspaceState.Available,
            Position = position,
            Capabilities = CapabilitySet.FromIds(capabilityIds),
            IsTrusted = true,
            Priority = priority,
            LastSeen = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["tool"] = "developer-studio"
            }
        };
    }

    private static TransferMetadata DemoMetadata(string sourceWorkspace)
    {
        return new TransferMetadata
        {
            ObjectId = TransferObjectId.NewId(),
            DisplayName = "Hallo von RK Workspace",
            MimeType = "text/plain; charset=utf-8",
            Size = "Hallo von RK Workspace".Length,
            Checksum = "sha256:developer-studio-demo-text",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            SourceWorkspace = sourceWorkspace,
            TargetWorkspace = string.Empty,
            Owner = "developer-studio",
            Priority = 1,
            Tags = new[] { "developer-studio", "demo", "text" },
            Version = "1.0.0"
        };
    }

    private static StudioDiagnosticsSnapshot EmptyDiagnostics()
    {
        return new StudioDiagnosticsSnapshot
        {
            RuntimeVersion = string.Empty,
            RuntimeState = RuntimeState.Created.ToString(),
            PluginCount = 0,
            WorkspaceCount = 0,
            TransferObjectCount = 0,
            Capabilities = string.Empty,
            LastResult = "Not run",
            LastError = string.Empty
        };
    }
}
