using System.IO;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public static class GpuLivingLensShaderAvailability
{
    public static bool CompiledMaterialShaderExists()
    {
        var shaderPath = Path.Combine(AppContext.BaseDirectory, "Shaders", "LivingLensMaterial.ps");
        return File.Exists(shaderPath) && new FileInfo(shaderPath).Length > 0;
    }
}
