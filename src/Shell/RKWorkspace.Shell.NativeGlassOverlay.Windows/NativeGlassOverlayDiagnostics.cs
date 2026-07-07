using System.IO;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativeGlassOverlayDiagnostics
{
    private readonly string _path;
    private readonly Dictionary<string, string> _steps = new(StringComparer.OrdinalIgnoreCase);

    public NativeGlassOverlayDiagnostics(string path)
    {
        _path = path;
        Set("Start", "Overlay gestartet");
        Set("Kontext", "Listener aus");
        Set("Hotkeys", "werden registriert");
        Set("Geste", "wartet auf echte PDF-Geste: PDF markieren, Strg+Alt+Leertaste oder F9 druecken");
        Set("PDF", "noch keine PDF genommen");
        Set("Gegenseite", "noch nicht ausgewertet");
        Set("Glaskante", "noch nicht sichtbar");
        Set("Carry", "leer");
        Set("Drop", "wartet");
        Set("Signal", "nicht geschrieben");
        Set("macOS", "wartet auf FrameGuest");
    }

    public IReadOnlyDictionary<string, string> Steps => _steps;

    public void Set(string step, string value)
    {
        _steps[step] = value;
        WriteSnapshot();
    }

    private void WriteSnapshot()
    {
        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(_path, false);
            writer.WriteLine($"RK Workspace Native Glass Overlay Diagnostics {DateTimeOffset.Now:O}");
            foreach (var pair in _steps)
            {
                writer.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }
        catch
        {
            // Diagnostics must never destabilize the carry path.
        }
    }
}
