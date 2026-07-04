namespace RKWorkspace.Shell.LivingLens.Windows;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return LivingLensApplication.RunSmokeTest();
        }

        if (args.Any(argument => string.Equals(argument, "--export-frames", StringComparison.OrdinalIgnoreCase)))
        {
            return LivingLensApplication.ExportFrames();
        }

        return LivingLensApplication.RunDemo();
    }
}
