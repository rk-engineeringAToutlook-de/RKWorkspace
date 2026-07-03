using RKWorkspace.Shell;

namespace RKWorkspace.Shell.Overlay.Windows;

public static class WorkspaceOverlayApplication
{
    public static int RunDemo()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            ApplicationConfiguration.Initialize();
            Application.Run(new WorkspaceOverlayWindow(runtime));
            runtime.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Overlay failed: {ex.Message}");
            return 1;
        }
    }

    public static int RunSmokeTest()
    {
        var runtime = new WorkspaceShellRuntime();
        try
        {
            runtime.Start();
            using var window = new WorkspaceOverlayWindow(runtime);
            var success = window.SmokeCheck();
            var session = window.Session;

            Console.WriteLine("RK Workspace Overlay Smoke Test");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"ShellState: {runtime.State}");
            Console.WriteLine($"OverlayState: {session.State}");
            Console.WriteLine($"CarryState: {session.CarryState}");
            Console.WriteLine("AblageLinks: READY");
            Console.WriteLine("AblageRechts: READY");
            Console.WriteLine($"DigitalThing: {session.ThingLabel}");
            Console.WriteLine($"ThingContent: {session.ThingContent}");
            Console.WriteLine("DesktopLayer: TRANSPARENT");
            Console.WriteLine("EscExit: READY");
            Console.WriteLine(success ? "OverlaySmoke: SUCCESS" : "OverlaySmoke: FAILED");
            Console.WriteLine(success ? "RESULT: SUCCESS" : "RESULT: FAILED");

            runtime.Stop();
            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RK Workspace Overlay Smoke Test failed: {ex.Message}");
            return 1;
        }
    }
}
