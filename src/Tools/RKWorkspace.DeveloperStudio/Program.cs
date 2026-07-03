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
        var firstContactResult = RunFirstContactSmokeTest();
        var labResult = RunWorkspaceExperienceLabSmokeTest();
        var humanExperienceLabResult = RunHumanExperienceLabSmokeTest();
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
            illusionResult.IsSuccess &&
            firstContactResult.IsSuccess &&
            labResult.IsSuccess &&
            humanExperienceLabResult.IsSuccess;

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
        Console.WriteLine($"FirstContactStarted: {(firstContactResult.StartedSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"FirstContactGrip: {(firstContactResult.GripSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"FirstContactPlace: {(firstContactResult.PlaceSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"FirstContactMetrics: {(firstContactResult.MetricsSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"FirstContactTimeToGripMs: {firstContactResult.TimeToGripMs}");
        Console.WriteLine($"FirstContactTimeToPlaceMs: {firstContactResult.TimeToPlaceMs}");
        Console.WriteLine($"LabGripVariants: {WorkspaceExperienceLabState.GripVariants.Count}");
        Console.WriteLine($"LabCarryVariants: {WorkspaceExperienceLabState.CarryVariants.Count}");
        Console.WriteLine($"LabEdgeVariants: {WorkspaceExperienceLabState.EdgeVariants.Count}");
        Console.WriteLine($"LabTransitionVariants: {WorkspaceExperienceLabState.TransitionVariants.Count}");
        Console.WriteLine($"LabDropVariants: {WorkspaceExperienceLabState.DropVariants.Count}");
        Console.WriteLine($"LabPreviewVariants: {WorkspaceExperienceLabState.PreviewVariants.Count}");
        Console.WriteLine($"LabLiveSwitch: {(labResult.LiveSwitchSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"LabRating: {(labResult.RatingSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"LabEvolution: {(labResult.EvolutionSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"LabAppliedToMultiWindow: {(labResult.MultiWindowAppliedSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"HumanExperienceLabStarted: {(humanExperienceLabResult.StartedSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"HumanExperienceLabExperiment: {(humanExperienceLabResult.ExperimentSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"HumanExperienceLabObservation: {(humanExperienceLabResult.ObservationSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"HumanExperienceLabEvolution: {(humanExperienceLabResult.EvolutionSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"HumanExperienceLabDashboard: {(humanExperienceLabResult.DashboardSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"HumanExperienceLabActive: {humanExperienceLabResult.ActiveHumanExperience}");
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
        Console.WriteLine(firstContactResult.IsSuccess ? "FirstContact: SUCCESS" : "FirstContact: FAILED");
        Console.WriteLine(labResult.IsSuccess ? "WorkspaceExperienceLab: SUCCESS" : "WorkspaceExperienceLab: FAILED");
        Console.WriteLine(humanExperienceLabResult.IsSuccess ? "HumanExperienceLab: SUCCESS" : "HumanExperienceLab: FAILED");
        Console.WriteLine(interactiveSuccess ? "RESULT: SUCCESS" : "RESULT: FAILED");

        viewModel.StopDualAgents();
        fullDemoViewModel.StopDualAgents();

        return interactiveSuccess ? 0 : 1;
    }

    private static FirstContactSmokeResult RunFirstContactSmokeTest()
    {
        var session = new FirstContactSession();
        var started = session.GetSnapshot();
        var startedSuccess =
            !started.IsCompleted &&
            !started.IsGrabbed &&
            string.Equals(started.Hint, "Nimm dieses Objekt.", StringComparison.Ordinal) &&
            started.FailedAttempts == 0 &&
            started.Cancellations == 0 &&
            started.UnnecessaryClicks == 0;

        session.RecordUnnecessaryClick();
        var gripSuccess = session.RecordGrip();
        var afterGrip = session.GetSnapshot();
        session.RecordHoverTarget(true);
        var placeSuccess = session.RecordPlace(overTarget: true);
        var completed = session.GetSnapshot();
        var metricsSuccess =
            afterGrip.TimeToGripMs is >= 0 &&
            completed.TimeToPlaceMs is >= 0 &&
            completed.IsSuccessWithinThirtySeconds &&
            completed.UnnecessaryClicks == 1 &&
            completed.FailedAttempts == 0 &&
            completed.Cancellations == 0 &&
            string.Equals(completed.ObjectLocation, "Right", StringComparison.Ordinal) &&
            string.Equals(completed.Hint, "Es liegt jetzt dort.", StringComparison.Ordinal);

        return new FirstContactSmokeResult
        {
            StartedSuccess = startedSuccess,
            GripSuccess = gripSuccess && afterGrip.IsGrabbed,
            PlaceSuccess = placeSuccess && completed.IsCompleted,
            MetricsSuccess = metricsSuccess,
            TimeToGripMs = afterGrip.TimeToGripMs ?? -1,
            TimeToPlaceMs = completed.TimeToPlaceMs ?? -1
        };
    }

    private static WorkspaceExperienceLabSmokeResult RunWorkspaceExperienceLabSmokeTest()
    {
        var lab = WorkspaceExperienceLabState.Load();
        var initial = lab.GetSnapshot();
        try
        {
            var countsSuccess = lab.SmokeCheck();
            lab.SetVariant("grip", "grip-generation-06");
            var firstSwitch = string.Equals(lab.GripVariantId, "grip-generation-06", StringComparison.Ordinal);
            lab.SetVariant("grip", "grip-generation-03");
            lab.SetVariant("carry", "carry-generation-04");
            var secondSwitch = string.Equals(lab.GripVariantId, "grip-generation-03", StringComparison.Ordinal);
            lab.SetRating("grip", "grip-generation-03", WorkspaceExperienceLabRating.Like);
            var ratingSuccess = lab.GetRating("grip", "grip-generation-03") == WorkspaceExperienceLabRating.Like;
            var evolutionSuccess = lab.EvolutionStep == initial.EvolutionStep + 1 &&
                string.Equals(lab.GripVariantId, "grip-generation-04", StringComparison.Ordinal) &&
                string.Equals(lab.CarryVariantId, "carry-generation-04", StringComparison.Ordinal);
            var context = new MultiWindowWorkspaceContext(lab);
            var snapshot = context.GetSnapshot(context.SourceWorkspaceId.ToString());
            var multiWindowApplied = string.Equals(
                snapshot.ExperienceLab.GripVariantId,
                "grip-generation-04",
                StringComparison.Ordinal);

            return new WorkspaceExperienceLabSmokeResult
            {
                CountsSuccess = countsSuccess,
                LiveSwitchSuccess = firstSwitch && secondSwitch,
                RatingSuccess = ratingSuccess,
                EvolutionSuccess = evolutionSuccess,
                MultiWindowAppliedSuccess = multiWindowApplied
            };
        }
        finally
        {
            lab.Restore(initial);
        }
    }

    private static HumanExperienceLabSmokeResult RunHumanExperienceLabSmokeTest()
    {
        var lab = HumanExperienceLabState.CreateTransient();
        var initial = lab.GetSnapshot();
        var startedSuccess = lab.SmokeCheck() &&
            lab.GetActiveHumanExperienceBlock().Contains("HX-000", StringComparison.Ordinal) &&
            lab.GetActiveHumanExperienceBlock().Contains("HX-003", StringComparison.Ordinal);

        lab.SetActiveHumanExperience("HX-001");
        var activeExperiment = lab.GetActiveExperiment();
        var experimentSuccess =
            string.Equals(lab.ActiveHumanExperienceId, "HX-001", StringComparison.Ordinal) &&
            string.Equals(activeExperiment.HumanExperienceId, "HX-001", StringComparison.Ordinal) &&
            activeExperiment.DisplayName.StartsWith("Experiment ", StringComparison.Ordinal);

        var observation = lab.RecordObservation(
            HumanExperienceLabRating.Right,
            "Owner bestaetigt: Das gehoert zu meiner Arbeit.",
            durationSeconds: 14,
            repetitions: 2);
        var snapshot = lab.GetSnapshot();
        var observationSuccess = snapshot.Observations.Any(item =>
            string.Equals(item.ExperimentId, activeExperiment.ExperimentId, StringComparison.Ordinal) &&
            item.Rating == HumanExperienceLabRating.Right &&
            item.DurationSeconds == 14 &&
            item.Repetitions == 2 &&
            string.Equals(item.Comment, observation.Comment, StringComparison.Ordinal));
        var evolvedExperiment = lab.GetActiveExperiment();
        var evolutionSuccess =
            lab.EvolutionStep == initial.EvolutionStep + 1 &&
            !string.Equals(evolvedExperiment.ExperimentId, activeExperiment.ExperimentId, StringComparison.Ordinal) &&
            string.Equals(evolvedExperiment.SourceExperimentId, activeExperiment.ExperimentId, StringComparison.Ordinal);
        var dashboard = lab.GetDashboardText();
        var dashboardSuccess =
            dashboard.Contains("Getestete HX: 1", StringComparison.Ordinal) &&
            dashboard.Contains("Bestaetigte HX: 1", StringComparison.Ordinal) &&
            lab.GetTimelineText().Contains("Status: bestaetigt", StringComparison.Ordinal);
        var ownershipSuccess = snapshot.Experiments.All(experiment =>
            HumanExperienceLabState.HumanExperiences.Any(hx =>
                string.Equals(hx.Id, experiment.HumanExperienceId, StringComparison.Ordinal)));

        return new HumanExperienceLabSmokeResult
        {
            StartedSuccess = startedSuccess,
            ExperimentSuccess = experimentSuccess && ownershipSuccess,
            ObservationSuccess = observationSuccess,
            EvolutionSuccess = evolutionSuccess,
            DashboardSuccess = dashboardSuccess,
            ActiveHumanExperience = lab.ActiveHumanExperienceId
        };
    }

    private sealed record WorkspaceExperienceLabSmokeResult
    {
        public required bool CountsSuccess { get; init; }

        public required bool LiveSwitchSuccess { get; init; }

        public required bool RatingSuccess { get; init; }

        public required bool EvolutionSuccess { get; init; }

        public required bool MultiWindowAppliedSuccess { get; init; }

        public bool IsSuccess => CountsSuccess &&
            LiveSwitchSuccess &&
            RatingSuccess &&
            EvolutionSuccess &&
            MultiWindowAppliedSuccess;
    }

    private sealed record HumanExperienceLabSmokeResult
    {
        public required bool StartedSuccess { get; init; }

        public required bool ExperimentSuccess { get; init; }

        public required bool ObservationSuccess { get; init; }

        public required bool EvolutionSuccess { get; init; }

        public required bool DashboardSuccess { get; init; }

        public required string ActiveHumanExperience { get; init; }

        public bool IsSuccess => StartedSuccess &&
            ExperimentSuccess &&
            ObservationSuccess &&
            EvolutionSuccess &&
            DashboardSuccess;
    }

    private sealed record FirstContactSmokeResult
    {
        public required bool StartedSuccess { get; init; }

        public required bool GripSuccess { get; init; }

        public required bool PlaceSuccess { get; init; }

        public required bool MetricsSuccess { get; init; }

        public required long TimeToGripMs { get; init; }

        public required long TimeToPlaceMs { get; init; }

        public bool IsSuccess => StartedSuccess &&
            GripSuccess &&
            PlaceSuccess &&
            MetricsSuccess;
    }
}
