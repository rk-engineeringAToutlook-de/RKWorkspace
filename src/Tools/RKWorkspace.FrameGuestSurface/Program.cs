using RKWorkspace.Frame.Pdf;
using RKWorkspace.Surface.Abstractions;

var pdfPath = FindSamplePdf();
var result = new PdfFrameOwnerService().OpenFrameOnlySession(pdfPath);
var identity = new SurfaceIdentity("ablage-tablet-guest", SurfacePlatform.IPadOS, "Tablet Ablage", "desk");

Console.WriteLine("RK Workspace Frame Guest Surface");
Console.WriteLine($"Surface: {identity.DisplayName}");
Console.WriteLine($"Platform: {identity.Platform}");
Console.WriteLine($"FrameSession: {result.GuestFrame.FrameSessionId}");
Console.WriteLine($"DisplayName: {result.GuestFrame.DisplayName}");
Console.WriteLine("Frame geoeffnet.");
Console.WriteLine("Liegt hier im Frame.");
Console.WriteLine($"GuestHasPdfFile: {(result.GuestFrame.HasOriginalFilePath ? "YES" : "NO")}");
Console.WriteLine($"OriginalFileBytes: {(result.GuestFrame.ContainsOriginalFileBytes ? "YES" : "NO")}");
Console.WriteLine($"FrameOnly: {(result.GuestHasNoFileIngress ? "OK" : "FAILED")}");
Console.WriteLine("Zurueckgegeben.");
Console.WriteLine(result.GuestHasNoFileIngress ? "RESULT: SUCCESS" : "RESULT: FAILED");
return result.GuestHasNoFileIngress ? 0 : 1;

static string FindSamplePdf()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        throw new InvalidOperationException("Repository root could not be located.");
    }

    return Path.Combine(directory.FullName, "samples", "Objects", "Rechnung.pdf");
}
