using RKWorkspace.Shell;

namespace RKWorkspace.Shell.NativeOverlay.Windows;

public static class NativeSpatialOverlayApplication
{
    public static int RunDemo()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            ApplicationConfiguration.Initialize();
            Application.Run(new NativeSpatialOverlayWindow(runtime));
            runtime.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Native Spatial Overlay failed: {ex.Message}");
            return 1;
        }
    }

    public static int RunSmokeTest()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            using var window = new NativeSpatialOverlayWindow(runtime);
            var success = window.SmokeCheck();
            var session = window.Session;

            Console.WriteLine("RK Workspace Native Spatial Overlay Smoke Test");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"ShellState: {runtime.State}");
            Console.WriteLine($"NativeOverlay: {(window.IsNativeOverlay ? "READY" : "FAILED")}");
            Console.WriteLine($"BrowserSurface: {(window.HasBrowserSurface ? "FAILED" : "NONE")}");
            Console.WriteLine($"Borderless: {(window.IsBorderless ? "OK" : "FAILED")}");
            Console.WriteLine($"TransparentDesktop: {(window.IsTransparentDesktopOverlay ? "OK" : "FAILED")}");
            Console.WriteLine($"TopMost: {(window.TopMost ? "OK" : "FAILED")}");
            Console.WriteLine($"DemoThing: {(session.DemoThingCreated ? "OK" : "FAILED")}");
            Console.WriteLine($"PickCarryState: {(session.CarryState == WorkspaceCarryState.Placed ? "OK" : "FAILED")}");
            Console.WriteLine($"DigitalHand: {(window.SmokeDigitalHandOk ? "OK" : "FAILED")}");
            Console.WriteLine($"VectorDiagonal: {(window.SmokeDiagonalVectorOk ? "OK" : "FAILED")}");
            Console.WriteLine($"BubblesOnlyOnCarry: {(window.SmokeBubblesOnlyOnCarryOk ? "OK" : "FAILED")}");
            Console.WriteLine($"BubbleLens: {(window.SmokeBubbleLensOk ? "OK" : "FAILED")}");
            Console.WriteLine($"PortalOpen: {(window.SmokePortalOk ? "OK" : "FAILED")}");
            Console.WriteLine($"MiniAblage: {(window.SmokeMiniAblageOk ? "OK" : "FAILED")}");
            Console.WriteLine($"GlideIntoPortal: {(window.SmokeGlideOk ? "OK" : "FAILED")}");
            Console.WriteLine($"TargetPosition: {(window.SmokeTargetPositionOk ? "OK" : "FAILED")}");
            Console.WriteLine($"EscExit: {(window.EscExitReady ? "OK" : "FAILED")}");
            Console.WriteLine(success ? "NativeOverlaySmoke: SUCCESS" : "NativeOverlaySmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            runtime.Stop();
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Native Spatial Overlay Smoke Test failed: {ex.Message}");
            return 1;
        }
    }
}
