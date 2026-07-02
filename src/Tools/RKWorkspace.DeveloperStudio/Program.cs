using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        if (string.Equals(
            Environment.GetEnvironmentVariable("RKWS_STUDIO_SMOKE_TEST"),
            "1",
            StringComparison.Ordinal))
        {
            return RunSmokeTest();
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
        return 0;
    }

    private static int RunSmokeTest()
    {
        var viewModel = new StudioViewModel();
        var success = viewModel.RunFullInteractiveDemo();
        var fullDemoViewModel = new StudioViewModel();
        var fullDemoSuccess = fullDemoViewModel.RunFullDemo();
        var diagnostics = viewModel.Diagnostics;
        var interactive = viewModel.InteractiveWorkspace;
        var history = viewModel.TransferHistory.ToArray();
        var multiWindow = new MultiWindowWorkspaceContext();
        var multiWindowSuccess = multiWindow.RunFullDemo() &&
            multiWindow.HasTransferredObjectInTarget();
        var roundTripWindow = new MultiWindowWorkspaceContext();
        var roundTripSuccess = roundTripWindow.RunRoundTripDemo();
        var illusionWindow = new MultiWindowWorkspaceContext();
        var illusionResult = illusionWindow.RunWorkspaceIllusionDemo();
        var edgeLeft = multiWindow.DetectWindowEdge(5, 0, 200, 24);
        var edgeRight = multiWindow.DetectWindowEdge(195, 0, 200, 24);
        var edgeMiddle = multiWindow.DetectWindowEdge(100, 0, 200, 24);
        var edgeLeftSuggestion = multiWindow.SuggestTargetForEdge(edgeLeft);
        var edgeRightSuggestion = multiWindow.SuggestTargetForEdge(edgeRight);
        var edgeLogicSuccess =
            edgeLeft == MultiWindowEdge.Left &&
            edgeRight == MultiWindowEdge.Right &&
            edgeMiddle == MultiWindowEdge.None &&
            string.Equals(edgeLeftSuggestion.WorkspaceId, multiWindow.SourceWorkspaceId.ToString(), StringComparison.Ordinal) &&
            string.Equals(edgeRightSuggestion.WorkspaceId, multiWindow.TargetWorkspaceId.ToString(), StringComparison.Ordinal);
        var multiWindowSource = multiWindow.GetSnapshot(multiWindow.SourceWorkspaceId.ToString());
        var multiWindowTarget = multiWindow.GetSnapshot(multiWindow.TargetWorkspaceId.ToString());
        var roundTripSource = roundTripWindow.GetSnapshot(roundTripWindow.SourceWorkspaceId.ToString());
        var roundTripTarget = roundTripWindow.GetSnapshot(roundTripWindow.TargetWorkspaceId.ToString());
        var uxDiagnostics = roundTripSource.UxDiagnostics;
        var uxDiagnosticsSuccess =
            uxDiagnostics.DragStartCount >= 2 &&
            uxDiagnostics.TargetDetectedCount >= 2 &&
            uxDiagnostics.DropCount >= 2 &&
            uxDiagnostics.SuccessfulTransfers >= 2 &&
            uxDiagnostics.FailedTransfers == 0 &&
            uxDiagnostics.SuccessRatePercent >= 100;
        var sessionCandidateReady =
            !string.IsNullOrWhiteSpace(uxDiagnostics.SessionCandidate.CandidateId) &&
            !string.IsNullOrWhiteSpace(uxDiagnostics.SessionCandidate.TransferObjectId) &&
            string.Equals(
                uxDiagnostics.SessionCandidate.TargetWorkspaceId,
                roundTripWindow.SourceWorkspaceId.ToString(),
                StringComparison.Ordinal) &&
            !uxDiagnostics.SessionCandidate.IsLiveWorkspacePrepared;
        var returnTransferSuccess =
            roundTripSuccess &&
            roundTripSource.TransferObjects.Count == 5 &&
            roundTripTarget.TransferObjects.Count == 0;
        var interactiveSuccess = success &&
            fullDemoSuccess &&
            string.Equals(diagnostics.LastResult, "SUCCESS", StringComparison.Ordinal) &&
            string.Equals(interactive.ObjectLocation, "Workspace B", StringComparison.Ordinal) &&
            history.Any(entry => string.Equals(entry.Action, "State:Completed", StringComparison.Ordinal)) &&
            multiWindowSuccess &&
            edgeLogicSuccess &&
            returnTransferSuccess &&
            uxDiagnosticsSuccess &&
            sessionCandidateReady &&
            illusionResult.IsSuccess;

        Console.WriteLine("RK Workspace Developer Studio Smoke Test");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"RuntimeState: {diagnostics.RuntimeState}");
        Console.WriteLine($"PluginCount: {diagnostics.PluginCount}");
        Console.WriteLine($"WorkspaceCount: {diagnostics.WorkspaceCount}");
        Console.WriteLine($"TransferObjectCount: {diagnostics.TransferObjectCount}");
        Console.WriteLine($"AgentCount: {viewModel.Agents.Count}");
        Console.WriteLine($"InteractiveObjectLocation: {interactive.ObjectLocation}");
        Console.WriteLine($"InteractiveObjectState: {interactive.ObjectState}");
        Console.WriteLine($"InteractiveHistory: {string.Join(" -> ", history.Select(entry => entry.Action))}");
        Console.WriteLine($"FullDemo: {(fullDemoSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"MultiWindowSourceObjects: {multiWindowSource.TransferObjects.Count}");
        Console.WriteLine($"MultiWindowTargetObjects: {multiWindowTarget.TransferObjects.Count}");
        Console.WriteLine($"MultiWindowLastResult: {multiWindowTarget.LastResult}");
        Console.WriteLine($"RoundTripSourceObjects: {roundTripSource.TransferObjects.Count}");
        Console.WriteLine($"RoundTripTargetObjects: {roundTripTarget.TransferObjects.Count}");
        Console.WriteLine($"UxDragStart: {uxDiagnostics.DragStartCount}");
        Console.WriteLine($"UxTargetDetected: {uxDiagnostics.TargetDetectedCount}");
        Console.WriteLine($"UxDrop: {uxDiagnostics.DropCount}");
        Console.WriteLine($"UxSuccessRate: {uxDiagnostics.SuccessRatePercent}");
        Console.WriteLine($"WorkspaceSessionCandidate: {(sessionCandidateReady ? "READY" : "FAILED")}");
        Console.WriteLine($"IllusionGripState: {(illusionResult.GripStateSet ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"IllusionEdgeCandidate: {(illusionResult.EdgeCandidateCreated ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"IllusionEdgeLocked: {(illusionResult.EdgeLockedReached ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"IllusionForwardTransfer: {(illusionResult.ForwardTransferSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"IllusionReturnTransfer: {(illusionResult.ReturnTransferSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"IllusionCandidateCompleted: {(illusionResult.CandidateCompleted ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"IllusionCandidateStatus: {illusionResult.Diagnostics.CandidateStatus}");
        Console.WriteLine($"IllusionTransitionMs: {illusionResult.Diagnostics.LastTransitionDurationMs}");
        Console.WriteLine($"IllusionReturnCount: {illusionResult.Diagnostics.ReturnTransferCount}");
        Console.WriteLine($"EdgeTargetLeft: {edgeLeftSuggestion.WorkspaceId}");
        Console.WriteLine($"EdgeTargetRight: {edgeRightSuggestion.WorkspaceId}");
        Console.WriteLine($"EdgeTargetLogic: {(edgeLogicSuccess ? "SUCCESS" : "FAILED")}");
        foreach (var agent in viewModel.Agents)
        {
            Console.WriteLine($"Agent: {agent.AgentId} Runtime={agent.Runtime} Workspace={agent.Workspace} Status={agent.Status}");
        }

        Console.WriteLine($"LastResult: {diagnostics.LastResult}");
        Console.WriteLine($"LastError: {diagnostics.LastError}");
        Console.WriteLine(interactiveSuccess ? "InteractiveDemo: SUCCESS" : "InteractiveDemo: FAILED");
        Console.WriteLine(multiWindowSuccess ? "MultiWindow: SUCCESS" : "MultiWindow: FAILED");
        Console.WriteLine(returnTransferSuccess ? "RoundTrip: SUCCESS" : "RoundTrip: FAILED");
        Console.WriteLine(uxDiagnosticsSuccess ? "UxDiagnostics: SUCCESS" : "UxDiagnostics: FAILED");
        Console.WriteLine(illusionResult.IsSuccess ? "WorkspaceIllusion: SUCCESS" : "WorkspaceIllusion: FAILED");
        Console.WriteLine(interactiveSuccess ? "RESULT: SUCCESS" : "RESULT: FAILED");

        viewModel.StopDualAgents();
        fullDemoViewModel.StopDualAgents();

        return interactiveSuccess ? 0 : 1;
    }
}
