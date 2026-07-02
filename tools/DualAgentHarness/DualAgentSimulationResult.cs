using RKWorkspace.Agent;
using RKWorkspace.Core.Runtime;
using RKWorkspace.Core.TransferObjects;

namespace RKWorkspace.DualAgentHarness;

internal sealed record DualAgentSimulationResult
{
    public required AgentDiagnostics AgentARunning { get; init; }

    public required AgentDiagnostics AgentBRunning { get; init; }

    public required AgentDiagnostics AgentAStopped { get; init; }

    public required AgentDiagnostics AgentBStopped { get; init; }

    public required string RequestId { get; init; }

    public required string TransferObjectId { get; init; }

    public required bool TransferSuccess { get; init; }

    public required string SourceWorkspaceId { get; init; }

    public required string TargetWorkspaceId { get; init; }

    public required TransferObjectState FinalState { get; init; }

    public required IReadOnlyCollection<string> History { get; init; }

    public required int AgentATransferObjectCount { get; init; }

    public required int AgentBTransferObjectCount { get; init; }

    public required RuntimeState AgentARuntimeAfterTransfer { get; init; }

    public required RuntimeState AgentBRuntimeAfterTransfer { get; init; }

    public required IReadOnlyCollection<string> Checks { get; init; }
}
