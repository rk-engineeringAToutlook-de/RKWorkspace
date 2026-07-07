using System.IO;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed record NativeGlassOverlayOptions(
    string SourcePdfPath,
    string PlacementSignalPath)
{
    public static NativeGlassOverlayOptions Default()
    {
        var root = FindRepositoryRoot();
        return new NativeGlassOverlayOptions(
            Path.Combine(root, "samples", "Objects", "Rechnung.pdf"),
            Path.Combine(Path.GetTempPath(), "rkws-ma017-real-pdf-placement-ready.signal"));
    }

    public static NativeGlassOverlayOptions Parse(string[] args)
    {
        var options = Default();
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (ReadString(args, ref index, "--source-pdf", "-SourcePdfPath") is { } sourcePdfPath)
            {
                options = options with { SourcePdfPath = Path.GetFullPath(sourcePdfPath) };
                continue;
            }

            if (ReadString(args, ref index, "--placement-signal", "-PlacementSignalPath") is { } placementSignalPath)
            {
                options = options with { PlacementSignalPath = Path.GetFullPath(placementSignalPath) };
            }
        }

        return options;
    }

    private static string? ReadString(string[] args, ref int index, params string[] names)
    {
        var matches = false;
        foreach (var name in names)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                matches = true;
                break;
            }
        }

        if (!matches || index + 1 >= args.Length)
        {
            return null;
        }

        index++;
        return args[index];
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Repository root could not be located.");
    }
}
