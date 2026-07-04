using RKWorkspace.Shell;
using System.Windows;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public static class GpuLivingLensApplication
{
    public static int RunDemo()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            var application = new System.Windows.Application
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            application.Run(new GpuLivingLensWindow(runtime));
            runtime.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace GPU Living Lens failed: {ex.Message}");
            return 1;
        }
    }

    public static int RunSmokeTest()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            var session = new GpuLivingLensSession();
            session.RunSmokeScenario();
            var desktopSampleOk = DesktopRefractionSampler.TryCheckSample();
            var success = runtime.State == WorkspaceShellRuntimeState.Running &&
                session.GpuCompositionPrepared &&
                session.DesktopRefractionPrepared &&
                session.RefractionMapPrepared &&
                session.HlslShaderContractPrepared &&
                session.NoPaperAxisSpinPrepared &&
                session.RectangularThingPrepared &&
                session.RectangularShadowPrepared &&
                session.GentleCarryTiltPrepared &&
                session.SoftShadowPrepared &&
                session.PortalEdgePullPrepared &&
                session.PortalEdgeSqueezePrepared &&
                session.PortalEdgeApexSqueezePrepared &&
                session.NoTwistPortalFunnelPrepared &&
                session.TiltDampingNearTunnelPrepared &&
                session.TunnelDepthPrepared &&
                session.PremiumTunnelVisualPrepared &&
                session.PremiumTunnelRefractionPrepared &&
                session.PremiumTunnelAperturePrepared &&
                session.PrimaryLensHugsScreenEdge &&
                session.EdgeContinuationPrepared &&
                session.LensAppearsOnPickPrepared &&
                session.NoWhiteBlock &&
                session.DropRequiresRelease &&
                session.PullOutPrepared &&
                session.VectorTiltPrepared &&
                session.PerspectiveTrapezoidPrepared &&
                session.ShadowPrepared &&
                session.ShadowSuctionPrepared &&
                session.ShadowTunnelSuctionPrepared &&
                session.CalmRestingObjectInTunnelPrepared &&
                session.CarryShadowOnlyPrepared &&
                session.TransitCountdownPrepared &&
                session.RetakeResetsTransitTimerPrepared &&
                session.RemotePlacementPrepared &&
                session.TunnelAutoClosePrepared &&
                session.TunnelClosedAfterTransitPrepared &&
                session.RemoteGestureRequiredPrepared &&
                session.TransitState == GpuLivingLensTransitState.Closed &&
                desktopSampleOk;

            Console.WriteLine("RK Workspace GPU Living Lens Smoke Test");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine($"ShellState: {runtime.State}");
            Console.WriteLine($"NativeOverlay: READY");
            Console.WriteLine($"BrowserSurface: NONE");
            Console.WriteLine($"WebView: NONE");
            Console.WriteLine($"GpuComposition: {(session.GpuCompositionPrepared ? "READY" : "FAILED")}");
            Console.WriteLine($"DesktopSampling: {(desktopSampleOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DesktopRefraction: {(session.DesktopRefractionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ShaderReadyMap: {(session.RefractionMapPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"HlslShaderContract: {(session.HlslShaderContractPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"NoPaperAxisSpin: {(session.NoPaperAxisSpinPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RectangularThing: {(session.RectangularThingPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RectangularShadow: {(session.RectangularShadowPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"GentleCarryTilt: {(session.GentleCarryTiltPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"SoftShadow: {(session.SoftShadowPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PortalEdgePull: {(session.PortalEdgePullPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PortalEdgeSqueeze: {(session.PortalEdgeSqueezePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PortalEdgeApexSqueeze: {(session.PortalEdgeApexSqueezePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"NoTwistPortalFunnel: {(session.NoTwistPortalFunnelPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TiltDampingNearTunnel: {(session.TiltDampingNearTunnelPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TunnelDepthLayers: {(session.TunnelDepthPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PremiumTunnelVisual: {(session.PremiumTunnelVisualPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PremiumTunnelRefraction: {(session.PremiumTunnelRefractionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PremiumTunnelAperture: {(session.PremiumTunnelAperturePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PrimaryEdgeLens: {(session.PrimaryLensHugsScreenEdge ? "OK" : "FAILED")}");
            Console.WriteLine($"EdgeContinuation: {(session.EdgeContinuationPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"LensAppearsOnPick: {(session.LensAppearsOnPickPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"NoWhiteBlock: {(session.NoWhiteBlock ? "OK" : "FAILED")}");
            Console.WriteLine($"DropRequiresRelease: {(session.DropRequiresRelease ? "OK" : "FAILED")}");
            Console.WriteLine($"PullOutFromLens: {(session.PullOutPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"VectorTilt: {(session.VectorTiltPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"PerspectiveTrapezoid: {(session.PerspectiveTrapezoidPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ShadowModel: {(session.ShadowPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ShadowSuction: {(session.ShadowSuctionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"ShadowTunnelSuction: {(session.ShadowTunnelSuctionPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"CalmRestingObjectInTunnel: {(session.CalmRestingObjectInTunnelPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"CarryShadowOnly: {(session.CarryShadowOnlyPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TransitTimeoutMs: {session.TransitTimeoutMilliseconds}");
            Console.WriteLine($"TransitCountdown: {(session.TransitCountdownPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RetakeResetsTransitTimer: {(session.RetakeResetsTransitTimerPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RemotePlacement: {(session.RemotePlacementPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TunnelAutoClose: {(session.TunnelAutoClosePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TunnelClosedAfterTransit: {(session.TunnelClosedAfterTransitPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"RemoteGestureRequired: {(session.RemoteGestureRequiredPrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"TransitState: {session.TransitState}");
            Console.WriteLine(success ? "GpuLivingLensSmoke: SUCCESS" : "GpuLivingLensSmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            runtime.Stop();
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace GPU Living Lens Smoke Test failed: {ex.Message}");
            return 1;
        }
    }
}
