namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return NativeGlassOverlayApplication.RunSmokeTest();
        }

        return NativeGlassOverlayApplication.RunDemo(NativeGlassOverlayOptions.Parse(args));
    }
}
