using RKWorkspace.Shell;

namespace RKWorkspace.Shell.LivingLens.Windows;

public static class LivingLensApplication
{
    public static int RunDemo()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            ApplicationConfiguration.Initialize();
            Application.Run(new LivingLensOverlayWindow(runtime));
            runtime.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Living Lens failed: {ex.Message}");
            return 1;
        }
    }

    public static int RunSmokeTest()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            using var window = new LivingLensOverlayWindow(runtime);
            var success = window.SmokeCheck();
            var session = window.Session;

            Console.WriteLine("RK Workspace Living Lens Smoke Test");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"ShellState: {runtime.State}");
            Console.WriteLine($"NativeOverlay: {(window.IsNativeOverlay ? "READY" : "FAILED")}");
            Console.WriteLine($"BrowserSurface: {(window.HasBrowserSurface ? "FAILED" : "NONE")}");
            Console.WriteLine($"WebView: {(window.HasWebView ? "FAILED" : "NONE")}");
            Console.WriteLine($"PerPixelAlpha: {(window.UsesPerPixelAlphaOverlay ? "OK" : "FAILED")}");
            Console.WriteLine($"ColorKeyTransparency: {(window.UsesColorKeyTransparency ? "FAILED" : "NONE")}");
            Console.WriteLine($"DesktopVisible: {(window.DesktopVisiblePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"NoPurpleBlob: {(session.PurpleBlobRejected ? "OK" : "FAILED")}");
            Console.WriteLine($"NoGreenPoint: {(session.GreenPointRejected ? "OK" : "FAILED")}");
            Console.WriteLine($"NoUiCircle: {(session.UiCircleRejected ? "OK" : "FAILED")}");
            Console.WriteLine($"NoButtonShape: {(session.ButtonShapeRejected ? "OK" : "FAILED")}");
            Console.WriteLine($"NoTechnicalWords: {(session.TechnicalWordsRejected ? "OK" : "FAILED")}");
            Console.WriteLine($"ExtremeFxMode: {(session.ExtremeFxMode && session.SmokeExtremeFxObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"LensVariants: {session.Variants.Count}");
            Console.WriteLine($"ExtremePresets: {(session.Variants.Count == 5 ? "OK" : "FAILED")}");
            Console.WriteLine($"EffectIntensity: {session.EffectIntensity}");
            Console.WriteLine($"IntensitySwitching: {(session.SmokeIntensitySwitchObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"DebugDefaultHidden: {(session.SmokeDebugDefaultHiddenObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"DefaultGlassLens: {(session.SmokeDefaultGlassObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"RealBubbleLens: {(session.RealBubbleLensExists ? "OK" : "FAILED")}");
            Console.WriteLine($"EdgeAnchored: {(session.LensesAtEdges ? "OK" : "FAILED")}");
            Console.WriteLine($"PrimaryEdgeLens: {(session.PrimaryLensHugsScreenEdge ? "OK" : "FAILED")}");
            Console.WriteLine($"SlowEmergence: {(session.LensEmergenceDurationMs is >= 1000 and <= 2000 ? "OK" : "FAILED")}");
            Console.WriteLine($"SubtleLiving: {(session.LensesLivingSubtly ? "OK" : "FAILED")}");
            Console.WriteLine($"NoWhiteAblageFrame: {(session.UsesSoftPortalWithoutWhiteFrame ? "OK" : "FAILED")}");
            Console.WriteLine($"LensOpen: {(session.SmokeLensOpenObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"MiniAblage: {(session.SmokeMiniAblageObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"ThingNoContainer: {(!session.ThingHasDistractingContainer ? "OK" : "FAILED")}");
            Console.WriteLine($"ThingCompact: {(session.ThingCompact ? "OK" : "FAILED")}");
            Console.WriteLine($"PartialOcclusion: {(session.ThingPartiallyOccluded ? "OK" : "FAILED")}");
            Console.WriteLine($"VectorDiagonal: {(session.CheckDiagonalVectorResponses() ? "OK" : "FAILED")}");
            Console.WriteLine($"NoAutoAbsorption: {(session.SmokeNoAutoAbsorptionObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"LensRelaxAway: {(session.SmokeLensRelaxObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"LensAbsorption: {(session.SmokeAbsorptionStartedObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"AbsorptionScale: {(session.SmokeAbsorptionScaleObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"AbsorptionDistortion: {(session.SmokeAbsorptionDistortionObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"NotInstantGone: {(session.SmokeNotInstantGoneObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"TargetGhost: {(session.SmokeTargetGhostObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"GhostEmergence: {(session.SmokeGhostEmergenceObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"PullOutFromLens: {(session.SmokePullOutObserved ? "OK" : "FAILED")}");
            Console.WriteLine($"TimingVariants: {(session.TimingVariantsExist ? "OK" : "FAILED")}");
            Console.WriteLine($"Timing2400: {(session.SmokeTiming2400Observed ? "OK" : "FAILED")}");
            Console.WriteLine($"ExportFrames: {(window.SmokeExportOk ? "OK" : "FAILED")}");
            Console.WriteLine($"EscExit: {(window.EscExitReady ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "LivingLensSmoke: SUCCESS" : "LivingLensSmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            runtime.Stop();
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Living Lens Smoke Test failed: {ex.Message}");
            return 1;
        }
    }

    public static int ExportFrames()
    {
        try
        {
            var success = LivingLensFrameExporter.ExportDefaultFrames();
            Console.WriteLine("RK Workspace Living Lens Visual Target Export");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine($"Output: {LivingLensFrameExporter.DefaultOutputDirectory}");
            Console.WriteLine(success ? "VisualTargetExport: SUCCESS" : "VisualTargetExport: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Living Lens Visual Target Export failed: {ex.Message}");
            return 1;
        }
    }
}
