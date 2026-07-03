namespace RKWorkspace.Shell.NativeOverlay.Windows;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return NativeSpatialOverlayApplication.RunSmokeTest();
        }

        return NativeSpatialOverlayApplication.RunDemo();
    }
}
