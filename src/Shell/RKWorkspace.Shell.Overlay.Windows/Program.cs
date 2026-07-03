namespace RKWorkspace.Shell.Overlay.Windows;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return WorkspaceOverlayApplication.RunSmokeTest();
        }

        return WorkspaceOverlayApplication.RunDemo();
    }
}
