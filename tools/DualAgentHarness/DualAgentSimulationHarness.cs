using RKWorkspace.Agent;
using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Runtime;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Transfers;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.DualAgentHarness;

internal sealed class DualAgentSimulationHarness
{
    public DualAgentSimulationResult Run()
    {
        var checks = new List<string>();
        var agentA = new AgentRuntime(
            AgentConfiguration.CreateLocalAgentA() with { EnableConsoleStatus = false },
            TextWriter.Null);
        var agentB = new AgentRuntime(
            AgentConfiguration.CreateLocalAgentB() with { EnableConsoleStatus = false },
            TextWriter.Null);

        TransferResult? transferResult = null;
        ITransferObject? sourceObject = null;
        AgentDiagnostics? runningA = null;
        AgentDiagnostics? runningB = null;
        RuntimeState runtimeAAfterTransfer = RuntimeState.Created;
        RuntimeState runtimeBAfterTransfer = RuntimeState.Created;

        try
        {
            StartInParallel(agentA, agentB);
            runningA = agentA.GetDiagnostics();
            runningB = agentB.GetDiagnostics();

            Ensure(runningA.State == AgentState.Running, "Agent A is running.", checks);
            Ensure(runningB.State == AgentState.Running, "Agent B is running.", checks);
            Ensure(runningA.RuntimeState == RuntimeState.Running, "Agent A runtime is running.", checks);
            Ensure(runningB.RuntimeState == RuntimeState.Running, "Agent B runtime is running.", checks);
            Ensure(!ReferenceEquals(agentA.Runtime, agentB.Runtime), "Agents use separate RuntimeEngine instances.", checks);
            Ensure(!ReferenceEquals(agentA.Runtime.WorkspaceRegistry, agentB.Runtime.WorkspaceRegistry), "Agents use separate WorkspaceRegistry instances.", checks);
            Ensure(!ReferenceEquals(agentA.Runtime.CapabilityManager, agentB.Runtime.CapabilityManager), "Agents use separate CapabilityManager instances.", checks);
            Ensure(!ReferenceEquals(agentA.Runtime.TransferObjectManager, agentB.Runtime.TransferObjectManager), "Agents use separate TransferObjectManager instances.", checks);
            Ensure(!string.Equals(runningA.AgentId, runningB.AgentId, StringComparison.OrdinalIgnoreCase), "AgentIds are distinct.", checks);
            Ensure(!string.Equals(runningA.WorkspaceId, runningB.WorkspaceId, StringComparison.OrdinalIgnoreCase), "WorkspaceIds are distinct.", checks);

            var sourceWorkspace = agentA.GetLocalWorkspaceDescriptor()
                ?? throw new InvalidOperationException("Agent A workspace is missing.");
            var targetWorkspace = agentB.GetLocalWorkspaceDescriptor()
                ?? throw new InvalidOperationException("Agent B workspace is missing.");
            Ensure(sourceWorkspace.Position == WorkspacePosition.Left, "Agent A workspace is positioned Left.", checks);
            Ensure(targetWorkspace.Position == WorkspacePosition.Right, "Agent B workspace is positioned Right.", checks);

            sourceObject = CreateTextTransferObject(agentA, sourceWorkspace);
            var request = CreateTransferRequest(agentA, sourceWorkspace, sourceObject);
            transferResult = ExecuteHarnessTransfer(agentA, sourceWorkspace, targetWorkspace, request);

            Ensure(transferResult.IsSuccess, "TransferResult is successful.", checks);
            Ensure(transferResult.SourceWorkspace?.Id == sourceWorkspace.WorkspaceId, "Transfer source is Agent A workspace.", checks);
            Ensure(transferResult.TargetWorkspace?.Id == targetWorkspace.WorkspaceId, "Transfer target is Agent B workspace.", checks);
            Ensure(transferResult.FinalState == TransferObjectState.Completed, "Transfer final state is Completed.", checks);
            Ensure(agentA.Runtime.TransferObjectManager.GetAll().Count == 1, "Agent A owns exactly one transfer object.", checks);
            Ensure(agentB.Runtime.TransferObjectManager.GetAll().Count == 0, "Agent B owns no duplicate transfer object.", checks);
            Ensure(agentA.Runtime.TransferObjectManager.Get(sourceObject.Id) is not null, "Transfer object exists in Agent A.", checks);
            Ensure(agentB.Runtime.TransferObjectManager.Get(sourceObject.Id) is null, "Transfer object is not duplicated into Agent B.", checks);
            EnsureHistory(transferResult.TransferObject?.History ?? Array.Empty<TransferHistoryEntry>(), checks);

            runtimeAAfterTransfer = agentA.Runtime.GetStatus();
            runtimeBAfterTransfer = agentB.Runtime.GetStatus();
            Ensure(runtimeAAfterTransfer == RuntimeState.Running, "Agent A runtime remains stable.", checks);
            Ensure(runtimeBAfterTransfer == RuntimeState.Running, "Agent B runtime remains stable.", checks);
        }
        finally
        {
            StopInParallel(agentA, agentB);
        }

        var stoppedA = agentA.GetDiagnostics();
        var stoppedB = agentB.GetDiagnostics();
        Ensure(stoppedA.State == AgentState.Stopped, "Agent A stopped cleanly.", checks);
        Ensure(stoppedB.State == AgentState.Stopped, "Agent B stopped cleanly.", checks);

        if (runningA is null || runningB is null || transferResult is null || sourceObject is null)
        {
            throw new InvalidOperationException("Dual agent simulation did not complete.");
        }

        return new DualAgentSimulationResult
        {
            AgentARunning = runningA,
            AgentBRunning = runningB,
            AgentAStopped = stoppedA,
            AgentBStopped = stoppedB,
            RequestId = transferResult.RequestId,
            TransferObjectId = sourceObject.Id.ToString(),
            TransferSuccess = transferResult.IsSuccess,
            SourceWorkspaceId = transferResult.SourceWorkspace?.Id.ToString() ?? string.Empty,
            TargetWorkspaceId = transferResult.TargetWorkspace?.Id.ToString() ?? string.Empty,
            FinalState = transferResult.FinalState ?? TransferObjectState.Failed,
            History = transferResult.TransferObject?.History.Select(entry => entry.Action).ToArray() ?? Array.Empty<string>(),
            AgentATransferObjectCount = agentA.Runtime.TransferObjectManager.GetAll().Count,
            AgentBTransferObjectCount = agentB.Runtime.TransferObjectManager.GetAll().Count,
            AgentARuntimeAfterTransfer = runtimeAAfterTransfer,
            AgentBRuntimeAfterTransfer = runtimeBAfterTransfer,
            Checks = checks.ToArray()
        };
    }

    private static void StartInParallel(AgentRuntime agentA, AgentRuntime agentB)
    {
        Task.WaitAll(
            Task.Run(agentA.Start),
            Task.Run(agentB.Start));
    }

    private static void StopInParallel(AgentRuntime agentA, AgentRuntime agentB)
    {
        Task.WaitAll(
            Task.Run(agentA.Stop),
            Task.Run(agentB.Stop));
    }

    private static ITransferObject CreateTextTransferObject(
        AgentRuntime agent,
        WorkspaceDescriptor sourceWorkspace)
    {
        var now = DateTimeOffset.UtcNow;
        return agent.Runtime.TransferObjectManager.Create(
            TransferObjectType.Text,
            new TransferMetadata
            {
                ObjectId = TransferObjectId.NewId(),
                DisplayName = "Dual Agent Text",
                MimeType = "text/plain; charset=utf-8",
                Size = "Dual Agent Text".Length,
                Checksum = "sha256:dual-agent-text",
                CreatedAt = now,
                ModifiedAt = now,
                SourceWorkspace = sourceWorkspace.WorkspaceId.ToString(),
                TargetWorkspace = string.Empty,
                Owner = agent.Configuration.AgentId,
                Priority = 1,
                Tags = new[] { "dual-agent", "simulation", "text" },
                Version = "1.0.0"
            });
    }

    private static TransferRequest CreateTransferRequest(
        AgentRuntime agent,
        WorkspaceDescriptor sourceWorkspace,
        ITransferObject transferObject)
    {
        return new TransferRequest
        {
            RequestId = $"dual-agent-request-{Guid.NewGuid():N}",
            SourceWorkspaceId = sourceWorkspace.WorkspaceId,
            RequestedDirection = TransferDirection.Right,
            TransferObjectId = transferObject.Id,
            RequiredCapabilities = CapabilitySet.FromIds(
                CapabilityId.Display,
                CapabilityId.Clipboard,
                CapabilityId.Encryption,
                CapabilityId.Pairing),
            OptionalCapabilities = CapabilitySet.Empty,
            ForbiddenCapabilities = CapabilitySet.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
            RequestedBy = agent.Configuration.AgentId,
            Metadata = new Dictionary<string, string>
            {
                ["harness"] = "dual-local-agent",
                ["handoff"] = "agent-a-to-agent-b"
            }
        };
    }

    private static TransferResult ExecuteHarnessTransfer(
        AgentRuntime sourceAgent,
        WorkspaceDescriptor sourceWorkspace,
        WorkspaceDescriptor targetWorkspace,
        TransferRequest request)
    {
        var workspaceRegistry = new WorkspaceRegistry();
        workspaceRegistry.RegisterWorkspace(Workspace.FromDescriptor(sourceWorkspace));
        workspaceRegistry.RegisterWorkspace(Workspace.FromDescriptor(targetWorkspace));

        var capabilityManager = new CapabilityManager();
        capabilityManager.RegisterProvider(new StaticWorkspaceCapabilityProvider(sourceWorkspace));
        capabilityManager.RegisterProvider(new StaticWorkspaceCapabilityProvider(targetWorkspace));

        var transferEngine = new TransferEngine(
            workspaceRegistry,
            capabilityManager,
            sourceAgent.Runtime.TransferObjectManager);

        return transferEngine.ExecuteLogicalTransfer(request);
    }

    private static void EnsureHistory(
        IReadOnlyCollection<TransferHistoryEntry> history,
        ICollection<string> checks)
    {
        var actions = history.Select(entry => entry.Action).ToArray();
        Ensure(actions.Contains("Created", StringComparer.Ordinal), "History contains Created.", checks);
        Ensure(actions.Contains("State:Validated", StringComparer.Ordinal), "History contains State:Validated.", checks);
        Ensure(actions.Contains("MetadataUpdated", StringComparer.Ordinal), "History contains MetadataUpdated.", checks);
        Ensure(actions.Contains("State:Prepared", StringComparer.Ordinal), "History contains State:Prepared.", checks);
        Ensure(actions.Contains("State:Completed", StringComparer.Ordinal), "History contains State:Completed.", checks);
    }

    private static void Ensure(bool condition, string message, ICollection<string> checks)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }

        checks.Add(message);
    }

    private sealed class StaticWorkspaceCapabilityProvider : ICapabilityProvider
    {
        private readonly WorkspaceDescriptor _descriptor;

        public StaticWorkspaceCapabilityProvider(WorkspaceDescriptor descriptor)
        {
            _descriptor = descriptor.Snapshot();
        }

        public string ProviderId => $"dual-agent:{_descriptor.WorkspaceId}";

        public string DisplayName => _descriptor.DisplayName;

        public CapabilitySet GetCapabilities()
        {
            return _descriptor.Capabilities.Snapshot();
        }
    }
}
