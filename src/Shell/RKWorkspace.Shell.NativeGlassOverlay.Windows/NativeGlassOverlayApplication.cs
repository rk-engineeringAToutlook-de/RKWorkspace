using RKWorkspace.Shell;
using System.Windows;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public static class NativeGlassOverlayApplication
{
    public static int RunDemo(NativeGlassOverlayOptions options)
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            var application = new System.Windows.Application
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };

            application.Run(new NativeGlassOverlayWindow(runtime, options));
            runtime.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Native Glass Overlay failed: {ex.Message}");
            return 1;
        }
    }

    public static int RunSmokeTest()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            var session = new NativeGlassOverlaySession();
            session.RunSmokeScenario();
            var desktopSampleOk = NativeDesktopSampler.TryCheckSample();
            var shaderResourceOk = NativeGlassMaterialEffect.ShaderResourcePath.Contains("NativeGlassMaterial.ps", StringComparison.Ordinal);
            var success = runtime.State == WorkspaceShellRuntimeState.Running &&
                session.NativeTransparentOverlayPrepared &&
                session.NoBrowserSurfacePrepared &&
                session.NoSyntheticStagePrepared &&
                session.LiveDesktopRefractionPrepared &&
                desktopSampleOk &&
                shaderResourceOk &&
                session.PhysicalGlassLookPrepared &&
                session.SoftFresnelPrepared &&
                session.RealDesktopOnlyPrepared &&
                session.SmoothPickScalePrepared &&
                session.RectangularPaperPrepared &&
                session.ProgressiveDesktopGlassEdgePrepared &&
                session.GlassEdgeIntentGatePrepared &&
                session.GentleVectorTiltPrepared &&
                session.SoftPerspectiveShadowPrepared &&
                session.ReleaseRequiredForAbsorptionPrepared &&
                session.CenterLockedSuctionPrepared &&
                session.EdgeApexSqueezePrepared &&
                session.NoTwistPrepared &&
                session.ShadowSuctionPrepared &&
                session.TransitCountdownPrepared &&
                session.RetakeResetsTransitTimerPrepared &&
                session.TunnelAutoClosePrepared &&
                session.RemotePlacementPrepared &&
                session.RemoteGestureRequiredPrepared &&
                session.State == NativeGlassOverlayCarryState.Closed;

            Console.WriteLine("RK Workspace Native Glass Overlay Smoke Test");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"ShellState: {runtime.State}");
            Console.WriteLine("ProductPath: Workspace Shell");
            Console.WriteLine("NativeOverlay: READY");
            Console.WriteLine("BrowserSurface: NONE");
            Console.WriteLine("SyntheticStage: NONE");
            Console.WriteLine($"TransparentDesktop: {(session.NativeTransparentOverlayPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"DesktopSampling: {(desktopSampleOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DesktopRefraction: {(session.LiveDesktopRefractionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ShaderMaterial: {(shaderResourceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"PhysicalGlass: {(session.PhysicalGlassLookPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RealDesktopOnly: {(session.RealDesktopOnlyPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"SmoothPickScale: {(session.SmoothPickScalePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RectangularPaper: {(session.RectangularPaperPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ProgressiveDesktopGlassEdge: {(session.ProgressiveDesktopGlassEdgePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"GlassEdgeIntentGate: {(session.GlassEdgeIntentGatePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"GentleVectorTilt: {(session.GentleVectorTiltPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"SoftPerspectiveShadow: {(session.SoftPerspectiveShadowPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ReleaseRequiredForAbsorption: {(session.ReleaseRequiredForAbsorptionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"CenterLockedSuction: {(session.CenterLockedSuctionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"EdgeApexSqueeze: {(session.EdgeApexSqueezePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"NoTwist: {(session.NoTwistPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ShadowSuction: {(session.ShadowSuctionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TransitCountdown: {(session.TransitCountdownPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RetakeResetsTransitTimer: {(session.RetakeResetsTransitTimerPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TunnelAutoClose: {(session.TunnelAutoClosePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RemotePlacement: {(session.RemotePlacementPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RemoteGestureRequired: {(session.RemoteGestureRequiredPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"State: {session.State}");
            Console.WriteLine(success ? "NativeGlassOverlaySmoke: SUCCESS" : "NativeGlassOverlaySmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            runtime.Stop();
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Native Glass Overlay Smoke Test failed: {ex.Message}");
            return 1;
        }
    }
}
