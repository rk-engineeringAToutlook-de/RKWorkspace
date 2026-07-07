using System.IO;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed record NativeGlassOverlayOptions(
    string? SourcePdfPath,
    string PlacementSignalPath,
    bool RealPdfGestureMode,
    bool PickImmediately,
    bool ContextListenerEnabled,
    bool InstantPlacementSignal,
    string DiagnosticsPath,
    string TargetDisplayName,
    string TargetDirection,
    double TargetDistanceMeters,
    string TargetDistanceSource,
    string ContextPipeName)
{
    public static NativeGlassOverlayOptions Default()
    {
        var root = FindRepositoryRoot();
        return new NativeGlassOverlayOptions(
            Path.Combine(root, "samples", "Objects", "Rechnung.pdf"),
            Path.Combine(Path.GetTempPath(), "rkws-ma017-real-pdf-placement-ready.signal"),
            false,
            false,
            false,
            false,
            Path.Combine(Path.GetTempPath(), "rkws-native-glass-overlay-diagnostics.log"),
            "Ablage macOS",
            "Right",
            0.30,
            "ManualMap",
            "RKWorkspace.NativeGlassOverlay.PdfPick");
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
                continue;
            }

            if (ReadString(args, ref index, "--diagnostics", "-DiagnosticsPath") is { } diagnosticsPath)
            {
                options = options with { DiagnosticsPath = Path.GetFullPath(diagnosticsPath) };
                continue;
            }

            if (ReadString(args, ref index, "--target-name", "-TargetDisplayName") is { } targetName)
            {
                options = options with { TargetDisplayName = targetName };
                continue;
            }

            if (ReadString(args, ref index, "--target-direction", "-TargetDirection") is { } targetDirection)
            {
                options = options with { TargetDirection = targetDirection };
                continue;
            }

            if (Is(arg, "--real-pdf-gesture", "-RealPdfGesture"))
            {
                options = options with { RealPdfGestureMode = true, SourcePdfPath = null };
            }

            if (Is(arg, "--pick-immediately", "-PickImmediately"))
            {
                options = options with { PickImmediately = true };
            }

            if (Is(arg, "--context-listener", "-ContextListener"))
            {
                options = options with { ContextListenerEnabled = true };
                continue;
            }

            if (Is(arg, "--instant-placement-signal", "-InstantPlacementSignal"))
            {
                options = options with { InstantPlacementSignal = true };
                continue;
            }

            if (ReadString(args, ref index, "--target-distance-meters", "-TargetDistanceMeters") is { } targetDistance &&
                double.TryParse(targetDistance, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var targetDistanceMeters))
            {
                options = options with { TargetDistanceMeters = targetDistanceMeters };
                continue;
            }

            if (ReadString(args, ref index, "--target-distance-source", "-TargetDistanceSource") is { } targetDistanceSource)
            {
                options = options with { TargetDistanceSource = targetDistanceSource };
                continue;
            }

            if (ReadString(args, ref index, "--context-pipe", "-ContextPipeName") is { } contextPipe)
            {
                options = options with { ContextPipeName = contextPipe };
                continue;
            }
        }

        return options;
    }

    private static bool Is(string value, params string[] names)
    {
        foreach (var name in names)
        {
            if (string.Equals(value, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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
