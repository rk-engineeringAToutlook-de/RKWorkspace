using RKWorkspace.Core.Models;
using RKWorkspace.Core.Services;

namespace RKWorkspace.Core.Simulation;

public sealed class LocalTransferSimulation
{
    public LocalTransferSimulationResult RunTextTransferFromAToB()
    {
        var log = new List<SimulationLogEntry>();
        var sequence = 0;

        void AddLog(string stage, string message)
        {
            log.Add(new SimulationLogEntry(++sequence, DateTimeOffset.UtcNow, stage, message));
        }

        AddLog("SIMULATION_START", "Starting local RK Workspace text transfer simulation.");

        var sourceIdentity = DeviceIdentity.Create(
            "RK Windows Laptop",
            WorkspacePlatform.Windows,
            "sha256:source-demo-fingerprint");

        AddLog("SOURCE_IDENTITY_CREATED", $"Source device identity created with id {sourceIdentity.DeviceId}.");

        var targetIdentity = DeviceIdentity.Create(
            "RK MacBook",
            WorkspacePlatform.MacOS,
            "sha256:target-demo-fingerprint");

        AddLog("TARGET_IDENTITY_CREATED", $"Target device identity created with id {targetIdentity.DeviceId}.");

        var commonCapabilities =
            WorkspaceCapability.TextTransfer
            | WorkspaceCapability.FileTransfer
            | WorkspaceCapability.PdfTransfer
            | WorkspaceCapability.ImageTransfer
            | WorkspaceCapability.LinkTransfer
            | WorkspaceCapability.DirectionalTransfer
            | WorkspaceCapability.Pairing
            | WorkspaceCapability.Discovery;

        var workspaceA = Workspace.Create(
            "workspace-a",
            "Arbeitsflaeche A",
            sourceIdentity.DeviceId,
            WorkspacePlatform.Windows,
            commonCapabilities,
            WorkspacePosition.Center,
            TrustState.Trusted);

        AddLog("SOURCE_WORKSPACE_REGISTERED", "Arbeitsflaeche A registered at Center with Trusted state.");

        var workspaceB = Workspace.Create(
            "workspace-b",
            "Arbeitsflaeche B",
            targetIdentity.DeviceId,
            WorkspacePlatform.MacOS,
            commonCapabilities,
            WorkspacePosition.Right,
            TrustState.Trusted);

        AddLog("TARGET_WORKSPACE_REGISTERED", "Arbeitsflaeche B registered at Right with Trusted state.");

        var workspaceMap = new WorkspaceMap(new[] { workspaceA, workspaceB });
        AddLog("ROOM_MAP_CREATED", "Manual room map created with A at Center and B at Right.");

        var direction = WorkspacePosition.Right;
        AddLog("DIRECTION_REQUESTED", $"User intent simulated as direction {direction}.");

        var resolvedTarget = workspaceMap.ResolveDirectionalTarget(direction);
        AddLog("TARGET_RESOLVED", $"Direction {direction} resolved to {resolvedTarget.DisplayName}.");

        var planner = new TransferPlanner();
        var transfer = planner.PlanTextTransfer(
            workspaceA,
            workspaceMap,
            direction,
            "Hallo von RK Workspace A.",
            "DemoText.txt");

        AddLog("TRANSFER_OBJECT_CREATED", $"Transfer object {transfer.ObjectId} created for {transfer.ObjectType}.");
        AddLog("TRANSFER_VALIDATED", "Trust state, object capability and directional target validation passed.");
        AddLog("SIMULATION_COMPLETE", "Text transfer from A to B was planned successfully.");

        return new LocalTransferSimulationResult(
            workspaceA,
            workspaceB,
            direction,
            transfer,
            log);
    }
}
