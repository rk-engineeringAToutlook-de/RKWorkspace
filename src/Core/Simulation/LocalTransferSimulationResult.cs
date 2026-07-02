using RKWorkspace.Core.Models;

namespace RKWorkspace.Core.Simulation;

public sealed record LocalTransferSimulationResult(
    Workspace SourceWorkspace,
    Workspace TargetWorkspace,
    WorkspacePosition Direction,
    TransferObject TransferObject,
    IReadOnlyList<SimulationLogEntry> Log)
{
    public bool Success => TransferObject.TargetWorkspaceId == TargetWorkspace.WorkspaceId;
}
