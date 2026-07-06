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
Console.WriteLine($"GuestVisibleStatus: {result.GuestVisibleStatus}");
Console.WriteLine($"OwnerVisibleStatus: {result.OwnerVisibleStatus}");
Console.WriteLine($"GuestStateFlow: {FormatFlow(result.VisibleStates, "Guest")}");
Console.WriteLine($"VisibleStateLanguage: {(result.VisibleStateLanguageIsValid ? "SUCCESS" : "FAILED")}");
Console.WriteLine($"FrameRepresentation: {(result.GuestShowsFrameRepresentation ? "OK" : "FAILED")}");
Console.WriteLine($"PreviewKind: {result.GuestFrame.RepresentationKind}");
Console.WriteLine($"RendererStatus: {result.GuestFrame.RendererStatus}");
Console.WriteLine($"ScrollPrepared: {(result.GuestFrame.SupportsScroll ? "OK" : "FAILED")}");
Console.WriteLine($"ZoomPrepared: {(result.GuestFrame.SupportsZoom ? "OK" : "FAILED")}");
Console.WriteLine($"GuestHasPdfFile: {(result.GuestFrame.HasOriginalFilePath ? "YES" : "NO")}");
Console.WriteLine($"OriginalFileBytes: {(result.GuestFrame.ContainsOriginalFileBytes ? "YES" : "NO")}");
Console.WriteLine($"FrameOnly: {(result.GuestHasNoFileIngress ? "OK" : "FAILED")}");
Console.WriteLine("Zurueckgegeben.");
var isSuccessful = result.GuestHasNoFileIngress && result.VisibleStateLanguageIsValid;
Console.WriteLine(isSuccessful ? "RESULT: SUCCESS" : "RESULT: FAILED");
return isSuccessful ? 0 : 1;

static string FormatFlow(IReadOnlyList<VisibleFrameState> states, string scope)
{
    return string.Join(" -> ", states
        .Where(state => string.Equals(state.Scope, scope, StringComparison.Ordinal))
        .Select(state => state.Text));
}

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
