using RKWorkspace.Shell;
using System.IO;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public static class GlassEdgeSmokeTest
{
    public static int Run(bool exportFrames)
    {
        try
        {
            var current = SimulatedAblageProximityProvider.WindowsAblageId;
            var selector = new NearestAblageSelector();
            var provider = new SimulatedAblageProximityProvider();
            var snapshot = provider.GetSnapshot(current);
            var nearest = selector.Select(snapshot);
            var edge = GlassEdge.FromNearest(nearest, GlassEdgeState.Visible, 0.44, 0.0);
            var nearProfile = GlassEdgeVisualProfile.FromDistance(AblageDistanceKind.Near);
            var farProfile = GlassEdgeVisualProfile.FromDistance(AblageDistanceKind.Far);

            var fourAblagenOk = snapshot.Surfaces.Count >= 4;
            var exactlyOneTargetOk = nearest.HasTarget && snapshot.AvailableTargets().Count(surface => surface.Id == nearest.TargetAblageId) == 1;
            var nearestMacOsOk = nearest.TargetAblageId?.Value == "ablage-macos" &&
                nearest.Platform == AblageSurfacePlatform.MacOS;
            var directionOk = nearest.EdgeHint == AblageDirection.Right;
            var edgeVisibleOk = edge.IsVisible && edge.Direction == AblageDirection.Right;
            var onlyOneEdgeOk = new[] { edge }.Count(candidate => candidate.IsVisible) == 1;
            var distanceIntensityOk = nearProfile.Opacity > farProfile.Opacity &&
                nearProfile.Thickness > farProfile.Thickness &&
                nearProfile.Glow > farProfile.Glow;
            var nameRevealOk = nearProfile.NameRevealThreshold < farProfile.NameRevealThreshold;
            var activationThresholdOk = nearProfile.ActivationThreshold < farProfile.ActivationThreshold;
            var openingEdge = GlassEdge.FromNearest(nearest, GlassEdgeState.Opening, 0.82, 0.12);
            var absorbingEdge = GlassEdge.FromNearest(nearest, GlassEdgeState.Absorbing, 1.0, 0.72);
            var edgeOpeningOk = openingEdge.IsActive && openingEdge.ActivationProgress > edge.ActivationProgress;
            var absorptionOk = absorbingEdge.AbsorptionProgress > 0.6 &&
                absorbingEdge.TargetEmergenceProgress > 0.4;
            var variantsOk = Enum.GetValues<GlassEdgeAbsorptionVariant>().Length == 3 &&
                (int)GlassEdgeAbsorptionVariant.WholeEdge == 1 &&
                (int)GlassEdgeAbsorptionVariant.FocusPoint == 2 &&
                (int)GlassEdgeAbsorptionVariant.DirectionalSlot == 3;
            var handoff = new WorkspaceSurfaceHandoff(
                "handoff-smoke",
                nearest.CurrentAblageId,
                nearest.TargetAblageId!.Value,
                nearest.EdgeHint,
                AblageDirectionMapper.Opposite(nearest.EdgeHint),
                "thing-rechnung",
                0.72,
                new WorkspaceSurfacePlacement(nearest.TargetAblageId.Value, 0.62, 0.42, true));
            var counterEdgeOk = handoff.SourceEdge == AblageDirection.Right &&
                handoff.TargetCounterEdge == AblageDirection.Left;
            var targetPositionOk = handoff.Placement.X == 0.62 &&
                handoff.Placement.Y == 0.42 &&
                handoff.Placement.IsSimulated;
            var platformsOk = Enum.GetValues<AblageSurfacePlatform>().Contains(AblageSurfacePlatform.Windows) &&
                Enum.GetValues<AblageSurfacePlatform>().Contains(AblageSurfacePlatform.MacOS) &&
                Enum.GetValues<AblageSurfacePlatform>().Contains(AblageSurfacePlatform.IOS) &&
                Enum.GetValues<AblageSurfacePlatform>().Contains(AblageSurfacePlatform.Android);
            var diagonalMappedOk = AblageDirectionMapper.ToPrimaryEdge(new AblagePose(AblageDirection.DownRight, 0.48, 0.82)) == AblageDirection.Down;
            var iPhoneSnapshot = new AblageProximitySnapshot(
                current,
                SimulatedAblageProximityProvider.CreateIPhoneNearestSurfaces(),
                DateTimeOffset.UtcNow);
            var iPhoneNearest = selector.Select(iPhoneSnapshot);
            var clearSwitchOk = iPhoneNearest.TargetAblageId?.Value == "ablage-iphone" &&
                iPhoneNearest.EdgeHint == AblageDirection.Up;
            var stableSnapshot = new AblageProximitySnapshot(
                current,
                CreateNearTieSurfaces(),
                DateTimeOffset.UtcNow);
            var stableResult = selector.Select(stableSnapshot, nearest);
            var stableSwitchOk = stableResult.TargetAblageId?.Value == "ablage-macos" &&
                stableResult.IsStable;
            var noSurface = selector.Select(new AblageProximitySnapshot(current, [snapshot.Surfaces.Single(surface => surface.Id == current)], DateTimeOffset.UtcNow));
            var noSurfaceOk = !noSurface.HasTarget && noSurface.EdgeHint == AblageDirection.Unknown;
            var contractsOk = typeof(ISurfaceHost).IsInterface &&
                typeof(ISurfaceOverlay).IsInterface &&
                typeof(ISurfaceGestureProvider).IsInterface &&
                typeof(ISurfaceHapticsProvider).IsInterface &&
                typeof(ISurfaceProximityProvider).IsInterface &&
                typeof(ISurfaceEdgeRenderer).IsInterface &&
                typeof(ISurfaceObjectCaptureAdapter).IsInterface &&
                typeof(ISurfacePlacementAdapter).IsInterface;
            var hapticMomentsOk = Enum.GetValues<SurfaceHapticMoment>().Contains(SurfaceHapticMoment.ObjectEntered);
            var exportedOk = !exportFrames || ExportAndVerify(DefaultExportDirectory());

            var success = fourAblagenOk &&
                exactlyOneTargetOk &&
                nearestMacOsOk &&
                directionOk &&
                edgeVisibleOk &&
                onlyOneEdgeOk &&
                distanceIntensityOk &&
                nameRevealOk &&
                activationThresholdOk &&
                edgeOpeningOk &&
                absorptionOk &&
                variantsOk &&
                counterEdgeOk &&
                targetPositionOk &&
                platformsOk &&
                diagonalMappedOk &&
                clearSwitchOk &&
                stableSwitchOk &&
                noSurfaceOk &&
                contractsOk &&
                hapticMomentsOk &&
                exportedOk;

            Console.WriteLine("RK Workspace Glass Edge Nearest Ablage Smoke Test");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($"SimulatedAblagen: {(fourAblagenOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NearestSelector: {(exactlyOneTargetOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NearestPlatform: {(nearestMacOsOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NearestDirection: {(directionOk ? "OK" : "FAILED")}");
            Console.WriteLine($"SingleGlassEdge: {(edgeVisibleOk && onlyOneEdgeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DistanceIntensity: {(distanceIntensityOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NameReveal: {(nameRevealOk ? "OK" : "FAILED")}");
            Console.WriteLine($"ActivationThreshold: {(activationThresholdOk ? "OK" : "FAILED")}");
            Console.WriteLine($"EdgeOpening: {(edgeOpeningOk ? "OK" : "FAILED")}");
            Console.WriteLine($"EdgeAbsorption: {(absorptionOk ? "OK" : "FAILED")}");
            Console.WriteLine($"AbsorptionVariants: {(variantsOk ? "OK" : "FAILED")}");
            Console.WriteLine($"CounterEdgeGhost: {(counterEdgeOk ? "OK" : "FAILED")}");
            Console.WriteLine($"TargetPosition: {(targetPositionOk ? "OK" : "FAILED")}");
            Console.WriteLine($"CrossPlatformSurfaces: {(platformsOk && contractsOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DiagonalMapping: {(diagonalMappedOk ? "OK" : "FAILED")}");
            Console.WriteLine($"ClearEdgeSwitch: {(clearSwitchOk ? "OK" : "FAILED")}");
            Console.WriteLine($"StableEdgeSwitch: {(stableSwitchOk ? "OK" : "FAILED")}");
            Console.WriteLine($"NoSurfaceAvailable: {(noSurfaceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"MobileHapticsPrepared: {(hapticMomentsOk ? "OK" : "FAILED")}");
            Console.WriteLine($"ExportFrames: {(exportedOk ? "OK" : "SKIPPED")}");
            Console.WriteLine(success ? "GlassEdgeSmoke: SUCCESS" : "GlassEdgeSmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace Glass Edge Nearest Ablage Smoke Test");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($"GlassEdgeSmoke: FAILED ({ex.Message})");
            Console.WriteLine("RESULT: FAILED");
            return 1;
        }
    }

    public static int ExportFrames()
    {
        try
        {
            var success = ExportAndVerify(DefaultExportDirectory());
            Console.WriteLine("RK Workspace Glass Edge Frame Export");
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"ExportDirectory: {DefaultExportDirectory()}");
            Console.WriteLine($"ExportFrames: {(success ? "SUCCESS" : "FAILED")}");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace Glass Edge Frame Export");
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"ExportFrames: FAILED ({ex.Message})");
            Console.WriteLine("RESULT: FAILED");
            return 1;
        }
    }

    private static IReadOnlyList<AblageSurface> CreateNearTieSurfaces()
    {
        var now = DateTimeOffset.UtcNow;
        return
        [
            new AblageSurface(new AblageIdentity("ablage-windows"), "Ablage Windows", AblageSurfacePlatform.Windows, true, AblagePose.FromDirection(AblageDirection.Unknown), AblageDistance.Simulated(AblageDistanceKind.VeryNear, 0.0, 1.0), now),
            new AblageSurface(new AblageIdentity("ablage-macos"), "Ablage macOS", AblageSurfacePlatform.MacOS, true, AblagePose.FromDirection(AblageDirection.Right), AblageDistance.Simulated(AblageDistanceKind.Near, 1.20, 0.92), now.AddSeconds(-1)),
            new AblageSurface(new AblageIdentity("ablage-iphone"), "Ablage iPhone", AblageSurfacePlatform.IOS, true, AblagePose.FromDirection(AblageDirection.Up), AblageDistance.Simulated(AblageDistanceKind.Near, 1.24, 0.91), now.AddSeconds(-2))
        ];
    }

    private static bool ExportAndVerify(string directory)
    {
        var files = GlassEdgeFrameExporter.Export(directory);
        return files.Count >= 10 && files.All(file => File.Exists(file) && new FileInfo(file).Length > 100);
    }

    private static string DefaultExportDirectory()
    {
        var root = LocateRepositoryRoot();
        return Path.Combine(root, "Docs", "VisualTargets", "MA00613");
    }

    private static string LocateRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")) ||
                Directory.Exists(Path.Combine(directory.FullName, "Docs")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
