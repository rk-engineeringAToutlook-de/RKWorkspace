namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Any(argument => string.Equals(argument, "--glass-edge-smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return GlassEdgeSmokeTest.Run(exportFrames: false);
        }

        if (args.Any(argument => string.Equals(argument, "--export-glass-edge-frames", StringComparison.OrdinalIgnoreCase)))
        {
            return GlassEdgeSmokeTest.ExportFrames();
        }

        if (args.Any(argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return GpuLivingLensApplication.RunSmokeTest();
        }

        return GpuLivingLensApplication.RunDemo();
    }
}
