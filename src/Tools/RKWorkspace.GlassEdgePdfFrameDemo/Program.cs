using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Ownership;
using RKWorkspace.Shell;

var options = GlassEdgePdfFrameDemoOptions.Parse(args, FindRoot());
var demo = new GlassEdgePdfFrameDemo();
var result = demo.Run(options);

Print(result);
return result.IsSuccessful ? 0 : 1;

static void Print(GlassEdgePdfFrameDemoResult result)
{
    Console.WriteLine("RK Workspace Glass Edge PDF Frame Demo");
    Console.WriteLine("--------------------------------------");
    Console.WriteLine($"PDF: {result.Document.FileName}");
    Console.WriteLine($"OriginalThing: {result.Document.ThingId}");
    Console.WriteLine($"OriginalAblage: {result.OwnerDisplayName}");
    Console.WriteLine($"NearestAblage: {result.Nearest.TargetDisplayName}");
    Console.WriteLine($"EdgeDirection: {result.GlassEdge.Direction}");
    Console.WriteLine($"GlassEdge: {(result.GlassEdge.IsActive ? "Active" : "Inactive")}");
    Console.WriteLine($"StateFlow: {string.Join(" -> ", result.StateFlow)}");
    Console.WriteLine($"CarryLease: {result.Lease.State}");
    Console.WriteLine($"FrameSession: {result.FrameSession.State}");
    Console.WriteLine($"OwnerLocked: {(result.OwnerLocked ? "OK" : "FAILED")}");
    Console.WriteLine("PDF liegt hier im Frame.");
    Console.WriteLine($"GuestHasPdfFile: {(result.GuestHasPdfFile ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasOriginalPath: {(result.GuestHasOriginalPath ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasCopiedPdfBytes: {(result.GuestHasCopiedPdfBytes ? "YES" : "NO")}");
    Console.WriteLine($"NoFileIngress: {(result.NoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"Returned: {(result.Returned ? "SUCCESS" : "FAILED")}");
    Console.WriteLine("Zurueckgegeben.");
    Console.WriteLine($"Recovery: {(result.RecoverySuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"Audit: {(result.AuditSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine("Renderer: PDF-Inhalt noch nicht gerendert, nur Frame- und Ownership-Mechanik getestet.");
    Console.WriteLine($"GlassEdgePdfFrameDemo: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"RESULT: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
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

public sealed record GlassEdgePdfFrameDemoOptions(string PdfPath, bool SmokeTest)
{
    public static GlassEdgePdfFrameDemoOptions Parse(string[] args, string root)
    {
        var pdfPath = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
        var smokeTest = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase))
            {
                smokeTest = true;
                continue;
            }

            if (string.Equals(arg, "--pdf-path", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                pdfPath = args[++index];
            }
        }

        return new GlassEdgePdfFrameDemoOptions(Path.GetFullPath(pdfPath), smokeTest);
    }
}

public sealed class GlassEdgePdfFrameDemo
{
    public GlassEdgePdfFrameDemoResult Run(GlassEdgePdfFrameDemoOptions options)
    {
        var now = DateTimeOffset.UtcNow;
        var currentAblage = SimulatedAblageProximityProvider.WindowsAblageId;
        var proximity = new SimulatedAblageProximityProvider();
        var selector = new NearestAblageSelector();
        var nearest = selector.Select(proximity.GetSnapshot(currentAblage));
        var glassEdge = GlassEdge.FromNearest(nearest, GlassEdgeState.Opening, activation: 1.0, absorption: 0.35);

        if (!nearest.HasTarget || nearest.TargetAblageId is null)
        {
            throw new InvalidOperationException("No target ablage available for Glass Edge PDF Frame Demo.");
        }

        var targetAblageId = nearest.TargetAblageId.Value.Value;
        var document = PdfFrameDocument.Load(options.PdfPath);
        var session = RkwpSession.CreateDevelopment(currentAblage.Value, targetAblageId);
        var audit = new InMemoryRkwpAuditSink();
        audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.SessionStarted, session.SessionId, null, null, "Session started.", now));

        var ownership = ThingOwnership.Original(document.ThingId, session.OwnerAblageId);
        var lockedOwnership = ownership.LeaseToGuest(session.GuestAblageId, OwnershipMode.FrameOnly);
        var lease = CarryLease.Grant(document.ThingId, session.OwnerAblageId, session.GuestAblageId, CarryLeasePolicy.FrameOnlyDefault, now, session.SessionId);
        audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.LeaseGranted, session.SessionId, lease.LeaseId, document.ThingId, "Lease granted.", now));

        var frame = FrameSession
            .Open(lease, FrameMode.ViewOnly, now.AddMilliseconds(20))
            .Ready(now.AddMilliseconds(40))
            .Activate(now.AddMilliseconds(60));
        audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.FrameOpened, session.SessionId, lease.LeaseId, document.ThingId, "Frame opened.", now.AddMilliseconds(60)));

        var firstUpdate = new FrameUpdate(
            frame.FrameSessionId,
            document.ThingId,
            PageNumber: 1,
            Representation: $"PDF frame representation; name={document.FileName}; pages={document.PageCount}; sha256={document.Sha256[..16]}",
            ContentHash: document.Sha256,
            ContainsOriginalFileBytes: false,
            Timestamp: now.AddMilliseconds(80));

        var guestFrame = new PdfGuestFrame(
            frame.FrameSessionId,
            document.ThingId,
            document.FileName,
            document.PageCount,
            document.Sha256,
            PdfFrameRepresentationKind.MetadataPreview,
            DisplayText: $"FrameOnly view of {document.FileName}",
            RendererStatus: "RendererBlocked",
            RendererName: "MetadataPreviewDevRenderer",
            IsPlaceholder: true,
            FrameFormat: FrameFormat.Placeholder,
            SupportsScroll: true,
            SupportsZoom: true,
            ContainsOriginalFileBytes: false,
            HasOriginalFilePath: false);

        var returnedLease = lease.Return(now.AddSeconds(1));
        var returnedFrame = frame.Close(now.AddSeconds(1));
        audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.FrameReturned, session.SessionId, lease.LeaseId, document.ThingId, "Frame returned.", now.AddSeconds(1)));

        var heartbeatLost = (lease with { LastHeartbeat = now.AddSeconds(-30) }).Advance(now);
        var recovery = heartbeatLost.Recover();
        audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.RecoveredByOwner, session.SessionId, lease.LeaseId, document.ThingId, "Owner recovered thing.", now.AddSeconds(2)));

        return new GlassEdgePdfFrameDemoResult(
            document,
            "Ablage Windows",
            nearest,
            glassEdge,
            lockedOwnership,
            lease,
            frame,
            firstUpdate,
            guestFrame,
            returnedLease,
            returnedFrame,
            recovery,
            audit.Events,
            [
                "RestingOnOwnerAblage",
                "Picked",
                "Carried",
                "GlassEdgeActive",
                "ObjectEnteringEdge",
                "FrameSessionOpen",
                "PresentedOnGuest",
                "ViewOnly",
                "ReturningToOwner",
                "ReturnedToOwner"
            ]);
    }
}

public sealed record GlassEdgePdfFrameDemoResult(
    PdfFrameDocument Document,
    string OwnerDisplayName,
    NearestAblageResult Nearest,
    GlassEdge GlassEdge,
    ThingOwnership Ownership,
    CarryLease Lease,
    FrameSession FrameSession,
    FrameUpdate FirstFrameUpdate,
    PdfGuestFrame GuestFrame,
    CarryLease ReturnedLease,
    FrameSession ReturnedFrame,
    CarryLeaseRecovery Recovery,
    IReadOnlyList<RkwpAuditEvent> AuditEvents,
    IReadOnlyList<string> StateFlow)
{
    public bool OwnerLocked =>
        Ownership.State == OwnershipState.LockedOnOwner &&
        Ownership.OwnerAblageId == Lease.OwnerAblageId &&
        Ownership.OriginalDisposition == OriginalDisposition.RetainOriginal;

    public bool GuestHasPdfFile => GuestFrame.HasOriginalFilePath;

    public bool GuestHasOriginalPath => GuestFrame.HasOriginalFilePath;

    public bool GuestHasCopiedPdfBytes => GuestFrame.ContainsOriginalFileBytes || FirstFrameUpdate.ContainsOriginalFileBytes;

    public bool NoFileIngress => !GuestHasPdfFile && !GuestHasOriginalPath && !GuestHasCopiedPdfBytes;

    public bool Returned => ReturnedLease.State == CarryLeaseState.Returned && ReturnedFrame.State == FrameSessionState.Closed;

    public bool RecoverySuccessful =>
        Recovery.OwnerRecoveredThing &&
        Recovery.GuestFrameInvalidated &&
        Recovery.FinalState == CarryLeaseState.RecoveredByOwner;

    public bool AuditSuccessful =>
        AuditEvents.Any(auditEvent => auditEvent.EventType == RkwpAuditEventType.SessionStarted) &&
        AuditEvents.Any(auditEvent => auditEvent.EventType == RkwpAuditEventType.LeaseGranted) &&
        AuditEvents.Any(auditEvent => auditEvent.EventType == RkwpAuditEventType.FrameOpened) &&
        AuditEvents.Any(auditEvent => auditEvent.EventType == RkwpAuditEventType.FrameReturned) &&
        AuditEvents.Any(auditEvent => auditEvent.EventType == RkwpAuditEventType.RecoveredByOwner);

    public bool IsSuccessful =>
        File.Exists(Document.OwnerPath) &&
        Nearest.HasTarget &&
        GlassEdge.IsActive &&
        Lease.State == CarryLeaseState.Active &&
        FrameSession.State == FrameSessionState.Active &&
        OwnerLocked &&
        NoFileIngress &&
        Returned &&
        RecoverySuccessful &&
        AuditSuccessful;
}
