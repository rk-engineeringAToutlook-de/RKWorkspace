using RKWorkspace.Shell;

namespace RKWorkspace.Shell.VisualReality.Windows;

public static class VisualRealityApplication
{
    public static int RunDemo()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            ApplicationConfiguration.Initialize();
            Application.Run(new VisualRealityOverlayWindow(runtime));
            runtime.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Visual Reality Lab failed: {ex.Message}");
            return 1;
        }
    }

    public static int RunSmokeTest()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            using var window = new VisualRealityOverlayWindow(runtime);
            var success = window.SmokeCheck();
            var session = window.Session;

            Console.WriteLine("RK Workspace Visual Reality Smoke Test");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"ShellState: {runtime.State}");
            Console.WriteLine($"NativeOverlay: {(window.IsNativeOverlay ? "READY" : "FAILED")}");
            Console.WriteLine($"BrowserSurface: {(window.HasBrowserSurface ? "FAILED" : "NONE")}");
            Console.WriteLine($"DesktopVisible: {(window.DesktopVisiblePrepared ? "OK" : "FAILED")}");
            Console.WriteLine($"LensVariants: {session.LensVariants.Count}");
            Console.WriteLine($"VariantSwitching: {(window.SmokeVariantSwitchingOk ? "OK" : "FAILED")}");
            Console.WriteLine($"SlowEmergence: {(window.SmokeSlowEmergenceOk ? "OK" : "FAILED")}");
            Console.WriteLine($"LensLiving: {(window.SmokeLensLivingOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DigitalHand: {(window.SmokeDigitalHandOk ? "OK" : "FAILED")}");
            Console.WriteLine($"VectorResponse: {(window.SmokeVectorResponseOk ? "OK" : "FAILED")}");
            Console.WriteLine($"DiagonalVector: {(window.SmokeDiagonalVectorOk ? "OK" : "FAILED")}");
            Console.WriteLine($"LensOpen: {(window.SmokeLensOpenOk ? "OK" : "FAILED")}");
            Console.WriteLine($"MiniAblage: {(window.SmokeMiniAblageOk ? "OK" : "FAILED")}");
            Console.WriteLine($"GlideIntoLens: {(window.SmokeGlideOk ? "OK" : "FAILED")}");
            Console.WriteLine($"TargetGhost: {(window.SmokeTargetGhostOk ? "OK" : "FAILED")}");
            Console.WriteLine($"EscExit: {(window.EscExitReady ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "VisualRealitySmoke: SUCCESS" : "VisualRealitySmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            runtime.Stop();
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Visual Reality Smoke Test failed: {ex.Message}");
            return 1;
        }
    }
}
