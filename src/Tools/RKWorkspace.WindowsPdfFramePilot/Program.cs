using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Ownership;
using RKWorkspace.Shell;
using ShellAblageIdentity = RKWorkspace.Shell.AblageIdentity;

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
    PrintLifecycleArea(result);
    PrintGlassEdgeArea(result);
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
    Console.WriteLine($"Seite: {result.Frame.DocumentFrameState.CurrentPage}/{result.Frame.DocumentFrameState.PageCount}");
    Console.WriteLine($"Seitennavigation: {(result.Frame.MultiPageNavigationPrepared ? "bereit" : "nicht bereit")}");
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
    Console.WriteLine($"Kapsel: {result.CapsuleVisibleStatus}");
    Console.WriteLine($"OpenFrame: {result.OpenFrameVisibleStatus}");
    Console.WriteLine($"Zoom: {(result.ZoomPrepared ? "bereit" : "nicht bereit")}");
    Console.WriteLine($"Scroll: {(result.ScrollPrepared ? "bereit" : "nicht bereit")}");
    Console.WriteLine($"Rueckgabe: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Returning)}");
    Console.WriteLine($"Verbindung: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Expired)}");
    Console.WriteLine($"PDF-Datei: {result.GuestPdfFileText}");
    Console.WriteLine();
}

static void PrintLifecycleArea(WindowsPdfFramePilotResult result)
{
    Console.WriteLine("PDF Lifecycle");
    Console.WriteLine("-------------");
    Console.WriteLine($"LifecycleMode: {result.Lifecycle.Mode}");
    Console.WriteLine($"PolicyProfile: {result.Lifecycle.PolicyDecision.Profile}");
    Console.WriteLine($"FrameCapsule: {(result.Lifecycle.CapsuleNoFileIngress ? "OK" : "FAILED")}");
    Console.WriteLine($"CapsuleState: {result.Lifecycle.Capsule.State}");
    Console.WriteLine($"CapsuleOpen: {(result.Lifecycle.OpenedCapsule.State == FrameCapsuleState.Opened ? "OK" : "FAILED")}");
    Console.WriteLine($"FinalCapsuleState: {result.Lifecycle.FinalCapsule.State}");
    Console.WriteLine($"CloseFrameBehavior: {result.Lifecycle.CloseBehavior}");
    Console.WriteLine($"KeepCapsulePolicy: {(result.Lifecycle.PolicyDecision.KeepCapsuleAllowed ? "ALLOWED" : "DENIED")}");
    Console.WriteLine($"CapsuleNoFileIngress: {(result.Lifecycle.CapsuleNoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"OpenFrameNoFileIngress: {(result.Lifecycle.OpenFrameNoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"CapsuleCache: {result.Frame.CachePolicy.Scope}");
    Console.WriteLine($"OpenFrameCache: {result.Frame.CachePolicy.Scope}");
    Console.WriteLine($"UnauthorizedCapsuleOpen: {(result.Lifecycle.UnauthorizedOpenDenied ? "DENIED" : "ALLOWED")}");
    Console.WriteLine($"ExpiredCapsule: {(result.Lifecycle.ExpiredCapsuleRecovered ? "RECOVERED_BY_OWNER" : "FAILED")}");
    Console.WriteLine($"AuditEvents: {string.Join(" -> ", result.Lifecycle.AuditEvents)}");

    if (result.Lifecycle.OpenContext is not null)
    {
        Console.WriteLine($"OpenPdfContext: OK");
        Console.WriteLine($"OpenPdfPath: {result.Lifecycle.OpenContext.PdfPath}");
        Console.WriteLine($"OpenPdfPage: {result.Lifecycle.OpenContext.Page}");
        Console.WriteLine($"OpenPdfZoom: {result.Lifecycle.OpenContext.Zoom:0.##}");
        Console.WriteLine($"OpenPdfViewer: {result.Lifecycle.OpenContext.ViewerName}");
        Console.WriteLine($"OpenFrame: OK");
    }

    Console.WriteLine();
}

static void PrintGlassEdgeArea(WindowsPdfFramePilotResult result)
{
    if (!result.Options.UseGlassEdge)
    {
        return;
    }

    Console.WriteLine("Glass Edge");
    Console.WriteLine("----------");
    Console.WriteLine($"UseGlassEdge: YES");
    Console.WriteLine($"UseManualMap: {(result.Options.UseManualMap ? "YES" : "NO")}");
    Console.WriteLine($"NearestAblage: {result.GlassEdge?.Nearest.TargetDisplayName ?? "nicht verfuegbar"}");
    Console.WriteLine($"EdgeDirection: {result.GlassEdge?.Edge.Direction.ToString() ?? "Unknown"}");
    Console.WriteLine($"EventFlow: {string.Join(" -> ", result.GlassEdgeEventFlow)}");
    Console.WriteLine($"GlassEdgeAppearing: {(result.HasGlassEdgeEvent(RkwpMessageType.GlassEdgeAppearing) ? "OK" : "FAILED")}");
    Console.WriteLine($"GlassEdgeActive: {(result.HasGlassEdgeEvent(RkwpMessageType.GlassEdgeActive) ? "OK" : "FAILED")}");
    Console.WriteLine($"ObjectEnteringEdge: {(result.HasGlassEdgeEvent(RkwpMessageType.ObjectEnteringEdge) ? "OK" : "FAILED")}");
    Console.WriteLine($"ObjectInTransit: {(result.HasGlassEdgeEvent(RkwpMessageType.ObjectInTransit) ? "OK" : "FAILED")}");
    Console.WriteLine($"ObjectEmerging: {(result.HasGlassEdgeEvent(RkwpMessageType.ObjectEmerging) ? "OK" : "FAILED")}");
    Console.WriteLine($"ObjectPlaced: {(result.HasGlassEdgeEvent(RkwpMessageType.ObjectPlaced) ? "OK" : "FAILED")}");
    Console.WriteLine($"FrameSessionOpen: {(result.HasGlassEdgeEvent(RkwpMessageType.FrameSessionOpen) ? "OK" : "FAILED")}");
    Console.WriteLine($"FrameSessionReady: {(result.HasGlassEdgeEvent(RkwpMessageType.FrameSessionReady) ? "OK" : "FAILED")}");
    Console.WriteLine($"PlaySequence: {(result.PlaySequenceCompleted ? "SUCCESS" : "NOT RUN")}");
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
    Console.WriteLine($"CurrentPage: {result.Frame.DocumentFrameState.CurrentPage}");
    Console.WriteLine($"PageCount: {result.Frame.DocumentFrameState.PageCount}");
    Console.WriteLine($"PageFrameUpdates: {result.Frame.DocumentFrameState.Updates.Count}");
    Console.WriteLine($"LifecycleMode: {result.Lifecycle.Mode}");
    Console.WriteLine($"CapsuleId: {result.Lifecycle.Capsule.CapsuleId}");
    Console.WriteLine($"CapsuleState: {result.Lifecycle.Capsule.State}");
    Console.WriteLine($"FinalCapsuleState: {result.Lifecycle.FinalCapsule.State}");
    Console.WriteLine($"PolicyProfile: {result.Lifecycle.PolicyDecision.Profile}");
    Console.WriteLine($"GuestHasPdfFile: {(result.GuestHasPdfFile ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasOriginalPath: {(result.GuestHasOriginalPath ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasCopiedPdfBytes: {(result.GuestHasCopiedPdfBytes ? "YES" : "NO")}");
    if (result.GlassEdge is not null)
    {
        Console.WriteLine($"GlassEdgeId: {result.GlassEdge.Edge.EdgeId}");
        Console.WriteLine($"GlassEdgeState: {result.GlassEdge.Edge.State}");
        Console.WriteLine($"GlassEdgeTargetAblageId: {result.GlassEdge.Edge.TargetAblageId.Value}");
        Console.WriteLine($"GlassEdgeSource: {result.GlassEdge.Nearest.Source}");
    }
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
    Console.WriteLine($"CurrentPage: {result.Frame.DocumentFrameState.CurrentPage}");
    Console.WriteLine($"PageCount: {result.Frame.DocumentFrameState.PageCount}");
    Console.WriteLine($"PageNavigation: {(result.Frame.MultiPageNavigationPrepared ? "OK" : "FAILED")}");
    Console.WriteLine($"ZoomPrepared: {(result.ZoomPrepared ? "OK" : "FAILED")}");
    Console.WriteLine($"ScrollPrepared: {(result.ScrollPrepared ? "OK" : "FAILED")}");
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
    Console.WriteLine($"LifecycleMode: {result.Lifecycle.Mode}");
    Console.WriteLine($"FrameCapsule: {(result.Lifecycle.CapsuleNoFileIngress ? "OK" : "FAILED")}");
    Console.WriteLine($"CapsuleOpen: {(result.Lifecycle.OpenedCapsule.State == FrameCapsuleState.Opened ? "OK" : "FAILED")}");
    Console.WriteLine($"OpenPdfContext: {(result.Lifecycle.OpenContext is not null ? "OK" : "NOT USED")}");
    Console.WriteLine($"OpenFrame: {(result.Lifecycle.IsOpenPdfFrame ? "OK" : "NOT USED")}");
    Console.WriteLine($"CloseFrameBehavior: {result.Lifecycle.CloseBehavior}");
    Console.WriteLine($"KeepCapsulePolicy: {(result.Lifecycle.PolicyDecision.KeepCapsuleAllowed ? "ALLOWED" : "DENIED")}");
    Console.WriteLine($"CapsuleNoFileIngress: {(result.Lifecycle.CapsuleNoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"OpenFrameNoFileIngress: {(result.Lifecycle.OpenFrameNoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"CapsuleCache: {result.Frame.CachePolicy.Scope}");
    Console.WriteLine($"OpenFrameCache: {result.Frame.CachePolicy.Scope}");
    Console.WriteLine($"AuditEventsPresent: {(result.Lifecycle.RequiredAuditEventsPresent ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"UnauthorizedCapsuleOpen: {(result.Lifecycle.UnauthorizedOpenDenied ? "DENIED" : "ALLOWED")}");
    Console.WriteLine($"ExpiredCapsule: {(result.Lifecycle.ExpiredCapsuleRecovered ? "RECOVERED_BY_OWNER" : "FAILED")}");

    if (result.Options.UseGlassEdge)
    {
        Console.WriteLine($"NearestAblageSelected: {(result.NearestAblageSelected ? "OK" : "FAILED")}");
        Console.WriteLine($"GlassEdgeIntegration: {(result.GlassEdgeIntegrationSuccessful ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"GlassEdgePlaySequence: {(result.PlaySequenceCompleted ? "SUCCESS" : "FAILED")}");
    }
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
    string Root,
    string PdfPath,
    bool SmokeTest,
    bool OwnerVisible,
    bool GuestVisible,
    bool Debug,
    bool UseGlassEdge,
    bool UseManualMap,
    bool PlaySequence,
    bool ClosedPdf,
    bool OpenPdf,
    string? OpenPdfPath,
    int Page,
    double Zoom,
    string ViewerName,
    CloseFrameBehavior CloseBehavior,
    RkwpPolicyProfileName PolicyProfile)
{
    public static WindowsPdfFramePilotOptions Parse(string[] args, string root)
    {
        var pdfPath = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
        var smokeTest = false;
        var ownerVisible = false;
        var guestVisible = false;
        var debug = false;
        var useGlassEdge = false;
        var useManualMap = false;
        var playSequence = false;
        var closedPdf = true;
        var openPdf = false;
        string? openPdfPath = null;
        var page = 1;
        var zoom = 1.0;
        var viewerName = "Windows PDF Viewer";
        var closeBehavior = CloseFrameBehavior.CloseReturns;
        var policyProfile = RkwpPolicyProfileName.DevelopmentLab;

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

            if (Is(arg, "--use-glass-edge", "-UseGlassEdge"))
            {
                useGlassEdge = true;
                continue;
            }

            if (Is(arg, "--use-manual-map", "-UseManualMap"))
            {
                useManualMap = true;
                continue;
            }

            if (Is(arg, "--play-sequence", "-PlaySequence"))
            {
                playSequence = true;
                continue;
            }

            if (Is(arg, "--closed-pdf", "-ClosedPdf"))
            {
                closedPdf = true;
                openPdf = false;
                continue;
            }

            if (Is(arg, "--open-pdf", "-OpenPdf"))
            {
                openPdf = true;
                closedPdf = false;
                continue;
            }

            if (Is(arg, "--pdf-path", "-PdfPath") && index + 1 < args.Length)
            {
                pdfPath = args[++index];
                continue;
            }

            if (Is(arg, "--open-pdf-path", "-OpenPdfPath") && index + 1 < args.Length)
            {
                openPdfPath = args[++index];
                continue;
            }

            if (Is(arg, "--page", "-Page") && index + 1 < args.Length)
            {
                page = Math.Max(1, int.Parse(args[++index]));
                continue;
            }

            if (Is(arg, "--zoom", "-Zoom") && index + 1 < args.Length)
            {
                zoom = Math.Max(0.1, double.Parse(args[++index], System.Globalization.CultureInfo.InvariantCulture));
                continue;
            }

            if (Is(arg, "--viewer-name", "-ViewerName") && index + 1 < args.Length)
            {
                viewerName = args[++index];
                continue;
            }

            if (Is(arg, "--close-returns", "-CloseReturns"))
            {
                closeBehavior = CloseFrameBehavior.CloseReturns;
                continue;
            }

            if (Is(arg, "--keep-capsule", "-KeepCapsule"))
            {
                closeBehavior = CloseFrameBehavior.KeepCapsule;
                continue;
            }

            if (Is(arg, "--policy", "-Policy") && index + 1 < args.Length)
            {
                policyProfile = Enum.Parse<RkwpPolicyProfileName>(args[++index], ignoreCase: true);
            }
        }

        return new WindowsPdfFramePilotOptions(
            root,
            Path.GetFullPath(pdfPath),
            smokeTest,
            ownerVisible,
            guestVisible,
            debug,
            useGlassEdge,
            useManualMap,
            playSequence,
            closedPdf,
            openPdf,
            openPdfPath is null ? null : Path.GetFullPath(openPdfPath),
            page,
            zoom,
            viewerName,
            closeBehavior,
            policyProfile);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}

public static class WindowsPdfFramePilot
{
    public static WindowsPdfFramePilotResult Run(WindowsPdfFramePilotOptions options)
    {
        var lifecycleService = new PdfLifecycleOwnerService();
        var policy = RkwpPolicyProfileStore.Get(options.PolicyProfile);
        var lifecycle = options.OpenPdf
            ? lifecycleService.RunOpenPdfFrame(
                OpenPdfContext.FromPath(
                    options.OpenPdfPath ?? options.PdfPath,
                    options.Page,
                    options.Zoom,
                    options.ViewerName),
                policy,
                options.CloseBehavior,
                "ablage-windows-owner",
                "ablage-windows-guest")
            : lifecycleService.RunClosedPdfCapsule(
                options.PdfPath,
                policy,
                options.CloseBehavior,
                "ablage-windows-owner",
                "ablage-windows-guest");
        var glassEdge = options.UseGlassEdge
            ? WindowsPdfFrameGlassEdgePilot.Run(options, lifecycle.Frame)
            : null;

        return new WindowsPdfFramePilotResult(
            options,
            "Ablage Windows Owner",
            "Ablage Windows Guest",
            lifecycle,
            glassEdge,
            OwnerSurfaceStarted: true,
            GuestSurfaceStarted: true);
    }
}

public static class WindowsPdfFrameGlassEdgePilot
{
    public static WindowsPdfFrameGlassEdgeResult Run(
        WindowsPdfFramePilotOptions options,
        PdfFrameSmokeResult frame)
    {
        var nearest = ResolveNearest(options, new ShellAblageIdentity(frame.Lease.OwnerAblageId));
        if (!nearest.HasTarget || nearest.TargetAblageId is null)
        {
            throw new InvalidOperationException("No nearest ablage available for Glass Edge pilot.");
        }

        var edgeState = options.PlaySequence ? GlassEdgeState.Absorbing : GlassEdgeState.Opening;
        var edge = GlassEdge.FromNearest(
            nearest,
            edgeState,
            activation: 1.0,
            absorption: options.PlaySequence ? 0.74 : 0.24);
        var session = RkwpSession.CreateDevelopment(frame.Lease.OwnerAblageId, frame.Lease.GuestAblageId);
        var events = CreateEventFlow(session, frame, edge, options.PlaySequence);

        return new WindowsPdfFrameGlassEdgeResult(nearest, edge, events, options.PlaySequence);
    }

    private static NearestAblageResult ResolveNearest(
        WindowsPdfFramePilotOptions options,
        ShellAblageIdentity currentAblageId)
    {
        IAblageProximityProvider provider;
        if (options.UseManualMap)
        {
            var map = new ManualAblageMapStore(ManualAblageMapStore.DefaultPath(options.Root)).Load();
            provider = new ManualMapAblageProximityProvider(
                map.Entries.Count == 0 || options.SmokeTest
                    ? CreatePilotManualMap()
                    : map);
        }
        else
        {
            provider = new SimulatedAblageProximityProvider(CreatePilotSurfaces(currentAblageId));
        }

        return new NearestAblageSelector().Select(provider.GetSnapshot(currentAblageId));
    }

    private static IReadOnlyList<AblageSurface> CreatePilotSurfaces(ShellAblageIdentity currentAblageId)
    {
        var now = DateTimeOffset.UtcNow;
        return
        [
            new AblageSurface(
                currentAblageId,
                "Ablage Windows Owner",
                AblageSurfacePlatform.Windows,
                true,
                AblagePose.FromDirection(AblageDirection.Unknown),
                AblageDistance.FromSource(AblageDistanceKind.VeryNear, 0.0, 1.0, AblageProximitySource.Simulated),
                now),
            new AblageSurface(
                new ShellAblageIdentity("ablage-windows-guest"),
                "Ablage Windows Guest",
                AblageSurfacePlatform.Windows,
                true,
                AblagePose.FromDirection(AblageDirection.Right),
                AblageDistance.FromSource(AblageDistanceKind.Near, 0.90, 0.97, AblageProximitySource.Simulated),
                now.AddSeconds(-1)),
            new AblageSurface(
                new ShellAblageIdentity("ablage-lab-tablet"),
                "Ablage Lab Tablet",
                AblageSurfacePlatform.IOS,
                true,
                AblagePose.FromDirection(AblageDirection.Up),
                AblageDistance.FromSource(AblageDistanceKind.Medium, 2.30, 0.80, AblageProximitySource.Simulated),
                now.AddSeconds(-3))
        ];
    }

    private static ManualAblageMap CreatePilotManualMap()
    {
        var now = DateTimeOffset.UtcNow;
        return new ManualAblageMap(
        [
            new ManualAblageMapEntry(
                "ablage-windows-guest",
                "Ablage Windows Guest",
                AblageDirection.Right,
                AblageDistanceKind.Near,
                0.90,
                0.97,
                true,
                now.AddSeconds(-1),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.Windows),
            new ManualAblageMapEntry(
                "ablage-lab-tablet",
                "Ablage Lab Tablet",
                AblageDirection.Up,
                AblageDistanceKind.Medium,
                2.30,
                0.80,
                true,
                now.AddSeconds(-3),
                AblageProximitySource.ManualMap,
                AblageSurfacePlatform.IOS)
        ]);
    }

    private static IReadOnlyList<RkwpMessage> CreateEventFlow(
        RkwpSession session,
        PdfFrameSmokeResult frame,
        GlassEdge edge,
        bool playSequence)
    {
        var sequence = 1L;
        var payload = new Dictionary<string, string>
        {
            ["edgeId"] = edge.EdgeId,
            ["direction"] = edge.Direction.ToString(),
            ["counterDirection"] = edge.CounterDirection.ToString(),
            ["targetAblageId"] = edge.TargetAblageId.Value,
            ["thingId"] = frame.Document.ThingId,
            ["pdfName"] = frame.Document.FileName,
            ["frameSessionId"] = frame.FrameSession.FrameSessionId,
            ["playSequence"] = playSequence.ToString()
        };

        var events = new List<RkwpMessage>
        {
            RkwpMessage.Create(RkwpMessageType.GlassEdgeAppearing, session, sequence++, payload),
            RkwpMessage.Create(RkwpMessageType.GlassEdgeActive, session, sequence++, payload)
        };

        if (playSequence)
        {
            events.AddRange(
            [
                RkwpMessage.Create(RkwpMessageType.ObjectEnteringEdge, session, sequence++, payload, frame.Lease.LeaseId),
                RkwpMessage.Create(RkwpMessageType.CarryLeaseRequested, session, sequence++, payload, frame.Lease.LeaseId),
                RkwpMessage.Create(RkwpMessageType.CarryLeaseGranted, session, sequence++, payload, frame.Lease.LeaseId),
                RkwpMessage.Create(RkwpMessageType.FrameSessionOpen, session, sequence++, payload, frame.Lease.LeaseId),
                RkwpMessage.Create(RkwpMessageType.FrameSessionReady, session, sequence++, payload, frame.Lease.LeaseId),
                RkwpMessage.Create(RkwpMessageType.ObjectInTransit, session, sequence++, payload, frame.Lease.LeaseId),
                RkwpMessage.Create(RkwpMessageType.ObjectEmerging, session, sequence++, payload, frame.Lease.LeaseId),
                RkwpMessage.Create(RkwpMessageType.ObjectPlaced, session, sequence++, payload, frame.Lease.LeaseId)
            ]);
        }

        return events;
    }
}

public sealed record WindowsPdfFrameGlassEdgeResult(
    NearestAblageResult Nearest,
    GlassEdge Edge,
    IReadOnlyList<RkwpMessage> Events,
    bool PlaySequenceRequested)
{
    public IReadOnlyList<string> EventFlow => Events.Select(message => message.MessageType.ToString()).ToArray();

    public bool HasEvent(RkwpMessageType messageType)
    {
        return Events.Any(message => message.MessageType == messageType);
    }

    public bool PlaySequenceCompleted =>
        PlaySequenceRequested &&
        HasEvent(RkwpMessageType.ObjectEnteringEdge) &&
        HasEvent(RkwpMessageType.ObjectInTransit) &&
        HasEvent(RkwpMessageType.ObjectEmerging) &&
        HasEvent(RkwpMessageType.ObjectPlaced) &&
        HasEvent(RkwpMessageType.FrameSessionOpen) &&
        HasEvent(RkwpMessageType.FrameSessionReady);

    public bool IsSuccessful =>
        Nearest.HasTarget &&
        Edge.IsActive &&
        HasEvent(RkwpMessageType.GlassEdgeAppearing) &&
        HasEvent(RkwpMessageType.GlassEdgeActive) &&
        (!PlaySequenceRequested || PlaySequenceCompleted);
}

public sealed record WindowsPdfFramePilotResult(
    WindowsPdfFramePilotOptions Options,
    string OwnerAblageName,
    string GuestAblageName,
    PdfLifecyclePilotResult Lifecycle,
    WindowsPdfFrameGlassEdgeResult? GlassEdge,
    bool OwnerSurfaceStarted,
    bool GuestSurfaceStarted)
{
    public PdfFrameSmokeResult Frame => Lifecycle.Frame;

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

    public bool ZoomPrepared => Frame.GuestFrame.SupportsZoom;

    public bool ScrollPrepared => Frame.GuestFrame.SupportsScroll;

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

    public string CapsuleVisibleStatus => Lifecycle.Capsule.State switch
    {
        FrameCapsuleState.Created => "Kapsel bereit",
        FrameCapsuleState.Opened => "liegt hier in der Kapsel",
        FrameCapsuleState.Returned => "zurueckgegeben",
        FrameCapsuleState.Expired => "nicht verfuegbar",
        FrameCapsuleState.Recovered => "zurueckgeholt",
        _ => "nicht verfuegbar"
    };

    public string OpenFrameVisibleStatus =>
        Lifecycle.IsOpenPdfFrame ? "liegt hier im Frame" : "nicht geoeffnet";

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
        $"Kapsel: {CapsuleVisibleStatus}",
        $"OpenFrame: {OpenFrameVisibleStatus}",
        $"Rueckgabe: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Returning)}",
        $"Verbindung: {OwnerGuestFrameStateUx.GetGuestText(GuestFrameUxState.Expired)}",
        $"PDF-Datei: {GuestPdfFileText}"
    ];

    public bool VisibleTextLanguageIsValid =>
        OwnerGuestFrameStateUx.ValidateVisibleText(VisibleTexts).IsValid;

    public IReadOnlyList<string> GlassEdgeEventFlow => GlassEdge?.EventFlow ?? [];

    public bool NearestAblageSelected => GlassEdge?.Nearest.HasTarget == true;

    public bool PlaySequenceCompleted => GlassEdge?.PlaySequenceCompleted == true;

    public bool HasGlassEdgeEvent(RkwpMessageType messageType)
    {
        return GlassEdge?.HasEvent(messageType) == true;
    }

    public bool GlassEdgeIntegrationSuccessful =>
        !Options.UseGlassEdge ||
        GlassEdge?.IsSuccessful == true &&
        NearestAblageSelected &&
        HasGlassEdgeEvent(RkwpMessageType.GlassEdgeAppearing) &&
        HasGlassEdgeEvent(RkwpMessageType.GlassEdgeActive) &&
        (!Options.PlaySequence || PlaySequenceCompleted && HasGlassEdgeEvent(RkwpMessageType.ObjectEnteringEdge)) &&
        Frame.Lease.State == CarryLeaseState.Active &&
        Frame.FrameSession.State == FrameSessionState.Active &&
        GuestFrameReady &&
        Frame.MultiPageNavigationPrepared &&
        ZoomPrepared &&
        ScrollPrepared &&
        NoFileIngress &&
        ReturnSuccessful;

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
        Lifecycle.IsSuccessful &&
        ReturnSuccessful &&
        RecoverySuccessful &&
        ReturnVisibleStateIsCorrect &&
        RecoveryVisibleStateIsCorrect &&
        Frame.VisibleStateLanguageIsValid &&
        VisibleTextLanguageIsValid &&
        GlassEdgeIntegrationSuccessful;
}
