using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol.Ownership;

var options = WindowsPdfFramePilotOptions.Parse(args, FindRoot());

try
{
    var result = WindowsPdfFramePilot.Run(options);
    Print(result);
    return result.IsSuccessful ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine("RK Workspace Windows PDF Frame Pilot");
    Console.WriteLine("------------------------------------");
    Console.WriteLine($"RESULT: FAILED - {ex.Message}");
    return 1;
}

static void Print(WindowsPdfFramePilotResult result)
{
    Console.WriteLine("RK Workspace Windows PDF Frame Pilot");
    Console.WriteLine("------------------------------------");
    Console.WriteLine($"Modus: {(result.Options.SmokeTest ? "Smoke-Test" : "Owner-Test")}");
    Console.WriteLine("Hinweis: Diese Anzeige ist ein Testwerkzeug, keine finale UX.");
    Console.WriteLine();

    PrintOwnerArea(result);
    PrintGuestArea(result);
    PrintSafetyArea(result);

    if (result.Options.OwnerVisible)
    {
        PrintTimeline("Originalablage Verlauf", result.OwnerTimeline);
    }

    if (result.Options.GuestVisible)
    {
        PrintTimeline("Gastablage Verlauf", result.GuestTimeline);
    }

    if (result.Options.Debug)
    {
        PrintDebug(result);
    }

    if (result.Options.SmokeTest)
    {
        PrintSmokeChecks(result);
    }

    Console.WriteLine();
    Console.WriteLine($"WindowsPdfFramePilot: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"RESULT: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
}

static void PrintOwnerArea(WindowsPdfFramePilotResult result)
{
    Console.WriteLine("Originalablage");
    Console.WriteLine("---------------");
    Console.WriteLine($"Ablage: {result.OwnerAblageName}");
    Console.WriteLine($"PDF-Name: {result.Frame.Document.FileName}");
    Console.WriteLine($"Zustand: {result.Frame.OwnerVisibleStatus}");
    Console.WriteLine($"Bearbeitung: {(result.OwnerLocked ? "gesperrt waehrend ausgeliehen" : "verfuegbar")}");
    Console.WriteLine($"Nach Rueckgabe: {result.OwnerReturnedStatus}");
    Console.WriteLine($"Recovery: {result.OwnerRecoveryStatus}");
    Console.WriteLine();
}

static void PrintGuestArea(WindowsPdfFramePilotResult result)
{
    Console.WriteLine("Gastablage");
    Console.WriteLine("----------");
    Console.WriteLine($"Ablage: {result.GuestAblageName}");
    Console.WriteLine($"Frame: {result.Frame.GuestVisibleStatus}");
    Console.WriteLine($"FrameStatus: {result.GuestFrameStatus}");
    Console.WriteLine($"Rueckgabe: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Returning)}");
    Console.WriteLine($"Verbindung: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Expired)}");
    Console.WriteLine($"PDF-Datei: {result.GuestPdfFileText}");
    Console.WriteLine();
}

static void PrintSafetyArea(WindowsPdfFramePilotResult result)
{
    Console.WriteLine("Sicherheitsstatus");
    Console.WriteLine("-----------------");
    Console.WriteLine($"No File Ingress: {(result.NoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"No File Ingress Status: {(result.NoFileIngress ? "sichtbar im Log" : "nicht bestaetigt")}");
    Console.WriteLine($"Sichtbare Sprache: {(result.VisibleTextLanguageIsValid ? "SUCCESS" : "FAILED")}");
}

static void PrintDebug(WindowsPdfFramePilotResult result)
{
    Console.WriteLine();
    Console.WriteLine("Debug");
    Console.WriteLine("-----");
    Console.WriteLine($"PdfPath: {result.Frame.Document.OwnerPath}");
    Console.WriteLine($"ThingId: {result.Frame.Document.ThingId}");
    Console.WriteLine($"Pages: {result.Frame.Document.PageCount}");
    Console.WriteLine($"OwnerAblageId: {result.OwnerAblageId}");
    Console.WriteLine($"GuestAblageId: {result.GuestAblageId}");
    Console.WriteLine($"LeaseId: {result.LeaseId}");
    Console.WriteLine($"FrameSessionId: {result.Frame.FrameSession.FrameSessionId}");
    Console.WriteLine($"PreviewKind: {result.Frame.GuestFrame.RepresentationKind}");
    Console.WriteLine($"RendererStatus: {result.Frame.GuestFrame.RendererStatus}");
    Console.WriteLine($"RendererName: {result.Frame.GuestFrame.RendererName}");
    Console.WriteLine($"FrameFormat: {result.Frame.GuestFrame.FrameFormat}");
    Console.WriteLine($"GuestHasPdfFile: {(result.GuestHasPdfFile ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasOriginalPath: {(result.GuestHasOriginalPath ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasCopiedPdfBytes: {(result.GuestHasCopiedPdfBytes ? "YES" : "NO")}");
}

static void PrintSmokeChecks(WindowsPdfFramePilotResult result)
{
    Console.WriteLine();
    Console.WriteLine("Smoke Checks");
    Console.WriteLine("------------");
    Console.WriteLine($"PDF: {result.Frame.Document.FileName}");
    Console.WriteLine($"SamplePdf: {(result.SamplePdfExists ? "OK" : "FAILED")}");
    Console.WriteLine($"OwnerSurface: {(result.OwnerSurfaceStarted ? "STARTED" : "FAILED")}");
    Console.WriteLine($"GuestSurface: {(result.GuestSurfaceStarted ? "STARTED" : "FAILED")}");
    Console.WriteLine($"PdfOriginalRegistered: {(result.PdfOriginalRegistered ? "OK" : "FAILED")}");
    Console.WriteLine($"CarryLease: {result.Frame.Lease.State}");
    Console.WriteLine($"OwnerLocked: {(result.OwnerLocked ? "OK" : "FAILED")}");
    Console.WriteLine($"FrameSession: {result.Frame.FrameSession.State}");
    Console.WriteLine($"GuestFrame: {(result.GuestFrameReady ? "OK" : "FAILED")}");
    Console.WriteLine($"OwnerInitialStatus: {OwnerGuestFrameStateUx.GetOwnerText(OwnerFrameUxState.OriginalOwned)}");
    Console.WriteLine($"OwnerLeasedStatus: {OwnerGuestFrameStateUx.GetOwnerText(OwnerFrameUxState.LeasedToGuest)}");
    Console.WriteLine($"OwnerLockedStatus: {result.Frame.OwnerVisibleStatus}");
    Console.WriteLine($"OwnerReturnedStatus: {result.OwnerReturnedStatus}");
    Console.WriteLine($"OwnerRecoveryStatus: {result.OwnerRecoveryStatus}");
    Console.WriteLine($"GuestOpeningStatus: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.FrameOpening)}");
    Console.WriteLine($"GuestFrameStatus: {result.Frame.GuestVisibleStatus}");
    Console.WriteLine($"GuestReturnStatus: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Returning)}");
    Console.WriteLine($"GuestRevokedStatus: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Revoked)}");
    Console.WriteLine($"GuestExpiredStatus: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Expired)}");
    Console.WriteLine($"PreviewKind: {result.Frame.GuestFrame.RepresentationKind}");
    Console.WriteLine($"RendererStatus: {result.Frame.GuestFrame.RendererStatus}");
    Console.WriteLine($"GuestHasPdfFile: {(result.GuestHasPdfFile ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasOriginalPath: {(result.GuestHasOriginalPath ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasCopiedPdfBytes: {(result.GuestHasCopiedPdfBytes ? "YES" : "NO")}");
    Console.WriteLine($"NoFileIngress: {(result.NoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"Rueckgabe: {(result.ReturnSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"Recovery: {(result.RecoverySuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"ReturnVisibleState: {(result.ReturnVisibleStateIsCorrect ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"RecoveryVisibleState: {(result.RecoveryVisibleStateIsCorrect ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"VisibleForbiddenTerms: {(result.VisibleTextLanguageIsValid ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"VisibleStateLanguage: {(result.Frame.VisibleStateLanguageIsValid ? "SUCCESS" : "FAILED")}");
}

static void PrintTimeline(string title, IReadOnlyList<string> timeline)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
    foreach (var item in timeline)
    {
        Console.WriteLine(item);
    }
}

static string FindRoot()
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

    return directory.FullName;
}

public sealed record WindowsPdfFramePilotOptions(
    string PdfPath,
    bool SmokeTest,
    bool OwnerVisible,
    bool GuestVisible,
    bool Debug)
{
    public static WindowsPdfFramePilotOptions Parse(string[] args, string root)
    {
        var pdfPath = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
        var smokeTest = false;
        var ownerVisible = false;
        var guestVisible = false;
        var debug = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Is(arg, "--smoke-test", "-SmokeTest"))
            {
                smokeTest = true;
                continue;
            }

            if (Is(arg, "--owner-visible", "-OwnerVisible"))
            {
                ownerVisible = true;
                continue;
            }

            if (Is(arg, "--guest-visible", "-GuestVisible"))
            {
                guestVisible = true;
                continue;
            }

            if (Is(arg, "--debug", "-Debug"))
            {
                debug = true;
                continue;
            }

            if (Is(arg, "--pdf-path", "-PdfPath") && index + 1 < args.Length)
            {
                pdfPath = args[++index];
            }
        }

        return new WindowsPdfFramePilotOptions(Path.GetFullPath(pdfPath), smokeTest, ownerVisible, guestVisible, debug);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}

public static class WindowsPdfFramePilot
{
    public static WindowsPdfFramePilotResult Run(WindowsPdfFramePilotOptions options)
    {
        var service = new PdfFrameOwnerService();
        var frame = service.OpenFrameOnlySession(
            options.PdfPath,
            ownerAblageId: "ablage-windows-owner",
            guestAblageId: "ablage-windows-guest");

        return new WindowsPdfFramePilotResult(
            options,
            "Ablage Windows Owner",
            "Ablage Windows Guest",
            frame,
            OwnerSurfaceStarted: true,
            GuestSurfaceStarted: true);
    }
}

public sealed record WindowsPdfFramePilotResult(
    WindowsPdfFramePilotOptions Options,
    string OwnerAblageName,
    string GuestAblageName,
    PdfFrameSmokeResult Frame,
    bool OwnerSurfaceStarted,
    bool GuestSurfaceStarted)
{
    public string OwnerAblageId => Frame.Lease.OwnerAblageId;

    public string GuestAblageId => Frame.Lease.GuestAblageId;

    public string LeaseId => Frame.Lease.LeaseId;

    public bool SamplePdfExists => File.Exists(Frame.Document.OwnerPath);

    public bool PdfOriginalRegistered =>
        SamplePdfExists &&
        Frame.Document.PageCount > 0 &&
        !string.IsNullOrWhiteSpace(Frame.Document.Sha256);

    public bool OwnerLocked => Frame.OwnerStillOwnsOriginal;

    public bool GuestFrameReady =>
        Frame.FrameSession.State == FrameSessionState.Active &&
        Frame.GuestShowsFrameRepresentation;

    public bool GuestHasPdfFile => Frame.GuestFrame.HasOriginalFilePath;

    public bool GuestHasOriginalPath => Frame.GuestFrame.HasOriginalFilePath;

    public bool GuestHasCopiedPdfBytes =>
        Frame.GuestFrame.ContainsOriginalFileBytes ||
        Frame.FirstFrameUpdate.ContainsOriginalFileBytes;

    public bool NoFileIngress =>
        !GuestHasPdfFile &&
        !GuestHasOriginalPath &&
        !GuestHasCopiedPdfBytes &&
        Frame.GuestHasNoFileIngress;

    public bool ReturnSuccessful => Frame.ReturnedLease.State == CarryLeaseState.Returned;

    public bool RecoverySuccessful =>
        Frame.Recovery.OwnerRecoveredThing &&
        Frame.Recovery.GuestFrameInvalidated &&
        Frame.Recovery.FinalState == CarryLeaseState.RecoveredByOwner;

    public string OwnerReturnedStatus => OwnerGuestFrameStateUx.GetOwnerText(OwnerFrameUxState.Returned);

    public string OwnerRecoveryStatus => OwnerGuestFrameStateUx.GetOwnerText(OwnerFrameUxState.RecoveredByOwner);

    public string GuestFrameStatus => GuestFrameReady ? "liegt hier im Frame" : "nicht verfuegbar";

    public string GuestPdfFileText => NoFileIngress ? "keine PDF-Datei vorhanden" : "nicht bestaetigt";

    public bool ReturnVisibleStateIsCorrect =>
        ReturnSuccessful &&
        OwnerReturnedStatus == "wieder verfuegbar";

    public bool RecoveryVisibleStateIsCorrect =>
        RecoverySuccessful &&
        OwnerRecoveryStatus == "wiederhergestellt";

    public IReadOnlyList<string> VisibleTexts =>
    [
        $"PDF-Name: {Frame.Document.FileName}",
        $"Zustand: {Frame.OwnerVisibleStatus}",
        $"Bearbeitung: {(OwnerLocked ? "gesperrt waehrend ausgeliehen" : "verfuegbar")}",
        $"Nach Rueckgabe: {OwnerReturnedStatus}",
        $"Recovery: {OwnerRecoveryStatus}",
        $"Frame: {Frame.GuestVisibleStatus}",
        $"FrameStatus: {GuestFrameStatus}",
        $"Rueckgabe: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Returning)}",
        $"Verbindung: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Expired)}",
        $"PDF-Datei: {GuestPdfFileText}"
    ];

    public bool VisibleTextLanguageIsValid =>
        OwnerGuestFrameStateUx.ValidateVisibleText(VisibleTexts).IsValid;

    public IReadOnlyList<string> OwnerTimeline =>
    [
        $"PDF liegt auf {OwnerAblageName}.",
        OwnerGuestFrameStateUx.GetOwnerText(OwnerFrameUxState.OriginalOwned),
        OwnerGuestFrameStateUx.GetOwnerText(OwnerFrameUxState.LeasedToGuest),
        Frame.OwnerVisibleStatus,
        OwnerReturnedStatus,
        OwnerRecoveryStatus
    ];

    public IReadOnlyList<string> GuestTimeline =>
    [
        OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.FrameOpening),
        Frame.GuestVisibleStatus,
        "PDF liegt hier im Frame.",
        OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Returning),
        OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Revoked),
        OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Expired)
    ];

    public bool IsSuccessful =>
        SamplePdfExists &&
        OwnerSurfaceStarted &&
        GuestSurfaceStarted &&
        PdfOriginalRegistered &&
        Frame.Lease.State == CarryLeaseState.Active &&
        OwnerLocked &&
        Frame.FrameSession.State == FrameSessionState.Active &&
        GuestFrameReady &&
        NoFileIngress &&
        ReturnSuccessful &&
        RecoverySuccessful &&
        ReturnVisibleStateIsCorrect &&
        RecoveryVisibleStateIsCorrect &&
        Frame.VisibleStateLanguageIsValid &&
        VisibleTextLanguageIsValid;
}
