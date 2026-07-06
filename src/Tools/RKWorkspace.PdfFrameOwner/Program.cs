using RKWorkspace.Frame.Pdf;

var pdfPath = GetArgument(args, "--pdf-path") ?? GetArgument(args, "-PdfPath") ?? FindSamplePdf();
var service = new PdfFrameOwnerService();
var result = service.OpenFrameOnlySession(pdfPath);

Console.WriteLine("RK Workspace PDF Frame Owner");
Console.WriteLine($"PDF: {result.Document.FileName}");
Console.WriteLine($"ThingId: {result.Document.ThingId}");
Console.WriteLine($"Pages: {result.Document.PageCount}");
Console.WriteLine($"OwnerAblage: {result.Lease.OwnerAblageId}");
Console.WriteLine($"GuestAblage: {result.Lease.GuestAblageId}");
Console.WriteLine("Status: PDF ist als Frame ausgeliehen.");
Console.WriteLine($"LeaseState: {result.Lease.State}");
Console.WriteLine($"FrameState: {result.FrameSession.State}");
Console.WriteLine($"FrameRepresentation: {(result.GuestShowsFrameRepresentation ? "OK" : "FAILED")}");
Console.WriteLine($"PreviewKind: {result.GuestFrame.RepresentationKind}");
Console.WriteLine($"RendererStatus: {result.GuestFrame.RendererStatus}");
Console.WriteLine($"RendererName: {result.GuestFrame.RendererName}");
Console.WriteLine($"FrameFormat: {result.GuestFrame.FrameFormat}");
Console.WriteLine($"IsPlaceholder: {result.GuestFrame.IsPlaceholder}");
Console.WriteLine($"FrameCacheScope: {result.CachePolicy.Scope}");
Console.WriteLine($"FrameCachePolicy: {result.CachePolicy.PolicyProfile}");
Console.WriteLine($"FrameCacheEntriesBeforeClose: {result.CacheBeforeClose.Entries}");
Console.WriteLine($"FrameCacheEntriesAfterClose: {result.CacheAfterClose.Entries}");
Console.WriteLine($"FrameCacheFileWrites: {result.CacheBeforeClose.FileWrites + result.CacheAfterClose.FileWrites}");
Console.WriteLine($"FrameCacheOriginalBytes: {(result.FrameCacheRespectsNoFileIngress ? "NO" : "YES")}");
Console.WriteLine($"FrameCacheClose: {(result.FrameCacheClearedAfterClose ? "CLEARED" : "FAILED")}");
Console.WriteLine($"DevInspectableDevelopment: {(DevInspectableDevelopmentAllowed() ? "OK" : "FAILED")}");
Console.WriteLine($"CriticalBlocksDevInspectable: {(CriticalBlocksDevInspectable() ? "OK" : "FAILED")}");
Console.WriteLine($"OwnerLocked: {(result.OwnerStillOwnsOriginal ? "OK" : "FAILED")}");
Console.WriteLine($"OwnerVisibleStatus: {result.OwnerVisibleStatus}");
Console.WriteLine($"GuestVisibleStatus: {result.GuestVisibleStatus}");
Console.WriteLine($"OwnerStateFlow: {FormatFlow(result.VisibleStates, "Owner")}");
Console.WriteLine($"GuestStateFlow: {FormatFlow(result.VisibleStates, "Guest")}");
Console.WriteLine($"VisibleStateLanguage: {(result.VisibleStateLanguageIsValid ? "SUCCESS" : "FAILED")}");
Console.WriteLine($"NoFileIngress: {(result.GuestHasNoFileIngress ? "OK" : "FAILED")}");
Console.WriteLine($"ReturnState: {result.ReturnedLease.State}");
Console.WriteLine("Rueckgabe: PDF auf Owner-Ablage logisch freigegeben.");
Console.WriteLine($"Recovery: {(result.Recovery.OwnerRecoveredThing ? "OK" : "FAILED")}");
Console.WriteLine(result.IsSuccessful ? "RESULT: SUCCESS" : "RESULT: FAILED");
return result.IsSuccessful ? 0 : 1;

static bool DevInspectableDevelopmentAllowed()
{
    _ = new FrameCache(new FrameCachePolicy(
        FrameCacheScope.DevInspectable,
        "DevelopmentLab",
        AllowDevInspectable: true,
        AllowTemporaryEncrypted: false));
    return true;
}

static bool CriticalBlocksDevInspectable()
{
    try
    {
        _ = new FrameCache(new FrameCachePolicy(
            FrameCacheScope.DevInspectable,
            "CriticalInfrastructure",
            AllowDevInspectable: false,
            AllowTemporaryEncrypted: false));
        return false;
    }
    catch (PdfFrameRendererException)
    {
        return true;
    }
}

static string? GetArgument(IReadOnlyList<string> args, string name)
{
    for (var index = 0; index < args.Count - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[index + 1];
        }
    }

    return null;
}

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
