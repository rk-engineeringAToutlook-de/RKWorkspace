namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Any(argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return GpuLivingLensApplication.RunSmokeTest();
        }

        return GpuLivingLensApplication.RunDemo();
    }
}
