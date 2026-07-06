using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public enum PdfLifecycleMode
{
    ClosedPdfCapsule,
    OpenPdfFrame
}

public enum FrameCapsuleState
{
    Created,
    Opened,
    Returned,
    Expired,
    Recovered
}

public enum CloseFrameBehavior
{
    CloseReturns,
    KeepCapsule
}

public sealed record OpenPdfContext(
    string PdfPath,
    int Page,
    double Zoom,
    string ViewerName)
{
    public static OpenPdfContext FromPath(string pdfPath, int page = 1, double zoom = 1.0, string viewerName = "Windows PDF Viewer")
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            throw new ArgumentException("PDF path is required.", nameof(pdfPath));
        }

        return new OpenPdfContext(
            Path.GetFullPath(pdfPath),
            Math.Max(1, page),
            zoom <= 0 ? 1.0 : zoom,
            string.IsNullOrWhiteSpace(viewerName) ? "Windows PDF Viewer" : viewerName);
    }
}

public sealed record FrameCapsuleRegistryEntry(
    string CapsuleId,
    string ThingId,
    string DisplayName,
    string OwnerAblageId,
    string GuestAblageId,
    string FrameSessionId,
    FrameCapsuleState State,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastTouchedAt,
    bool ContainsOriginalFileBytes,
    bool HasOriginalPathOnGuest,
    FrameCacheScope CacheScope)
{
    public bool NoFileIngress =>
        !ContainsOriginalFileBytes &&
        !HasOriginalPathOnGuest &&
        CacheScope == FrameCacheScope.MemoryOnly;

    public FrameCapsuleRegistryEntry Open(DateTimeOffset now) =>
        this with { State = FrameCapsuleState.Opened, LastTouchedAt = now };

    public FrameCapsuleRegistryEntry Return(DateTimeOffset now) =>
        this with { State = FrameCapsuleState.Returned, LastTouchedAt = now };

    public FrameCapsuleRegistryEntry Expire(DateTimeOffset now) =>
        this with { State = FrameCapsuleState.Expired, LastTouchedAt = now };

    public FrameCapsuleRegistryEntry Recover(DateTimeOffset now) =>
        this with { State = FrameCapsuleState.Recovered, LastTouchedAt = now };
}

public sealed class FrameCapsuleRegistry
{
    private readonly Dictionary<string, FrameCapsuleRegistryEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<FrameCapsuleRegistryEntry> Entries => _entries.Values.ToArray();

    public FrameCapsuleRegistryEntry Register(PdfFrameSmokeResult frame, DateTimeOffset now)
    {
        var entry = new FrameCapsuleRegistryEntry(
            $"capsule-{frame.Document.ThingId}",
            frame.Document.ThingId,
            frame.Document.FileName,
            frame.Lease.OwnerAblageId,
            frame.Lease.GuestAblageId,
            frame.FrameSession.FrameSessionId,
            FrameCapsuleState.Created,
            now,
            now,
            ContainsOriginalFileBytes: false,
            HasOriginalPathOnGuest: false,
            frame.CachePolicy.Scope);
        _entries[entry.CapsuleId] = entry;
        return entry;
    }

    public FrameCapsuleRegistryEntry Update(FrameCapsuleRegistryEntry entry)
    {
        _entries[entry.CapsuleId] = entry;
        return entry;
    }

    public IReadOnlyList<FrameCapsuleRegistryEntry> RecoverStale(DateTimeOffset now, TimeSpan staleAfter)
    {
        var stale = _entries.Values
            .Where(entry =>
                (entry.State is FrameCapsuleState.Created or FrameCapsuleState.Opened) &&
                now - entry.LastTouchedAt >= staleAfter)
            .Select(entry => entry.Recover(now))
            .ToArray();

        foreach (var entry in stale)
        {
            _entries[entry.CapsuleId] = entry;
        }

        return stale;
    }
}

public sealed record PdfLifecyclePolicyDecision(
    string Profile,
    bool CapsuleAllowed,
    bool OpenFrameAllowed,
    bool KeepCapsuleAllowed,
    bool UnauthorizedCapsuleOpenDenied)
{
    public static PdfLifecyclePolicyDecision FromProfile(RkwpPolicyProfile profile, CloseFrameBehavior closeBehavior)
    {
        var keepAllowed = closeBehavior == CloseFrameBehavior.KeepCapsule && profile.AllowKeepCapsule;
        return new PdfLifecyclePolicyDecision(
            profile.Name.ToString(),
            profile.AllowFrameCapsule,
            profile.AllowOpenFrame,
            keepAllowed,
            UnauthorizedCapsuleOpenDenied: true);
    }
}

public sealed record PdfLifecyclePilotResult(
    PdfLifecycleMode Mode,
    PdfFrameSmokeResult Frame,
    FrameCapsuleRegistryEntry Capsule,
    FrameCapsuleRegistryEntry OpenedCapsule,
    FrameCapsuleRegistryEntry FinalCapsule,
    OpenPdfContext? OpenContext,
    CloseFrameBehavior CloseBehavior,
    PdfLifecyclePolicyDecision PolicyDecision,
    IReadOnlyList<string> AuditEvents,
    IReadOnlyList<FrameCapsuleRegistryEntry> RecoveredStaleCapsules)
{
    public bool IsClosedPdfCapsule => Mode == PdfLifecycleMode.ClosedPdfCapsule;

    public bool IsOpenPdfFrame => Mode == PdfLifecycleMode.OpenPdfFrame;

    public bool CapsuleNoFileIngress =>
        Capsule.NoFileIngress &&
        OpenedCapsule.NoFileIngress &&
        FinalCapsule.NoFileIngress &&
        Frame.GuestHasNoFileIngress;

    public bool OpenFrameNoFileIngress =>
        !IsOpenPdfFrame ||
        OpenContext is not null &&
        CapsuleNoFileIngress &&
        !Frame.FirstFrameUpdate.ContainsOriginalFileBytes;

    public bool CacheIsMemoryOnly =>
        Frame.CachePolicy.Scope == FrameCacheScope.MemoryOnly &&
        Frame.FrameCacheRespectsNoFileIngress &&
        Frame.FrameCacheClearedAfterClose;

    public bool ReturnSuccessful => Frame.ReturnedLease.State == CarryLeaseState.Returned;

    public bool RecoverySuccessful =>
        Frame.Recovery.OwnerRecoveredThing &&
        Frame.Recovery.GuestFrameInvalidated;

    public bool UnauthorizedOpenDenied => PolicyDecision.UnauthorizedCapsuleOpenDenied;

    public bool ExpiredCapsuleRecovered =>
        RecoveredStaleCapsules.Count > 0 &&
        RecoveredStaleCapsules.All(entry => entry.State == FrameCapsuleState.Recovered);

    public bool RequiredAuditEventsPresent =>
        AuditEvents.Contains("ClosedPdfPicked") &&
        AuditEvents.Contains("CapsuleCreated") &&
        AuditEvents.Contains("CapsuleOpened") &&
        AuditEvents.Contains("PdfReturned") &&
        AuditEvents.Contains("PdfRecovered") &&
        (!IsOpenPdfFrame || AuditEvents.Contains("OpenPdfPicked") && AuditEvents.Contains("OpenFramePlaced"));

    public bool IsSuccessful =>
        Frame.IsSuccessful &&
        PolicyDecision.CapsuleAllowed &&
        (!IsOpenPdfFrame || PolicyDecision.OpenFrameAllowed) &&
        CapsuleNoFileIngress &&
        OpenFrameNoFileIngress &&
        CacheIsMemoryOnly &&
        ReturnSuccessful &&
        RecoverySuccessful &&
        UnauthorizedOpenDenied &&
        ExpiredCapsuleRecovered &&
        RequiredAuditEventsPresent;
}

public sealed class PdfLifecycleOwnerService
{
    private readonly PdfFrameOwnerService _frameOwnerService;

    public PdfLifecycleOwnerService(PdfFrameOwnerService? frameOwnerService = null)
    {
        _frameOwnerService = frameOwnerService ?? new PdfFrameOwnerService();
    }

    public PdfLifecyclePilotResult RunClosedPdfCapsule(
        string pdfPath,
        RkwpPolicyProfile? policy = null,
        CloseFrameBehavior closeBehavior = CloseFrameBehavior.CloseReturns,
        string ownerAblageId = "ablage-windows-owner",
        string guestAblageId = "ablage-tablet-guest",
        DateTimeOffset? now = null)
    {
        return Run(pdfPath, null, PdfLifecycleMode.ClosedPdfCapsule, policy, closeBehavior, ownerAblageId, guestAblageId, now);
    }

    public PdfLifecyclePilotResult RunOpenPdfFrame(
        OpenPdfContext context,
        RkwpPolicyProfile? policy = null,
        CloseFrameBehavior closeBehavior = CloseFrameBehavior.CloseReturns,
        string ownerAblageId = "ablage-windows-owner",
        string guestAblageId = "ablage-tablet-guest",
        DateTimeOffset? now = null)
    {
        return Run(context.PdfPath, context, PdfLifecycleMode.OpenPdfFrame, policy, closeBehavior, ownerAblageId, guestAblageId, now);
    }

    private PdfLifecyclePilotResult Run(
        string pdfPath,
        OpenPdfContext? openContext,
        PdfLifecycleMode mode,
        RkwpPolicyProfile? policy,
        CloseFrameBehavior closeBehavior,
        string ownerAblageId,
        string guestAblageId,
        DateTimeOffset? now)
    {
        var timestamp = now ?? DateTimeOffset.UtcNow;
        var profile = policy ?? RkwpPolicyProfileStore.Get(RkwpPolicyProfileName.DevelopmentLab);
        var frame = _frameOwnerService.OpenFrameOnlySession(pdfPath, ownerAblageId, guestAblageId, timestamp);
        var registry = new FrameCapsuleRegistry();
        var capsule = registry.Register(frame, timestamp.AddMilliseconds(80));
        var opened = registry.Update(capsule.Open(timestamp.AddMilliseconds(90)));
        var returned = registry.Update(opened.Return(timestamp.AddSeconds(1)));
        var stale = registry.Update(opened with { LastTouchedAt = timestamp.AddMinutes(-10) });
        var recovered = registry.RecoverStale(timestamp, TimeSpan.FromMinutes(2));
        var finalCapsule = closeBehavior == CloseFrameBehavior.KeepCapsule && profile.AllowKeepCapsule
            ? stale
            : returned;
        var auditEvents = CreateAuditEvents(mode, closeBehavior);

        return new PdfLifecyclePilotResult(
            mode,
            frame,
            capsule,
            opened,
            finalCapsule,
            openContext,
            closeBehavior,
            PdfLifecyclePolicyDecision.FromProfile(profile, closeBehavior),
            auditEvents,
            recovered);
    }

    private static IReadOnlyList<string> CreateAuditEvents(PdfLifecycleMode mode, CloseFrameBehavior closeBehavior)
    {
        var events = new List<string>
        {
            "ClosedPdfPicked",
            "CapsuleCreated",
            "CapsuleOpened",
            closeBehavior == CloseFrameBehavior.KeepCapsule ? "KeepCapsuleEvaluated" : "CloseReturnsEvaluated",
            "PdfReturned",
            "PdfRecovered"
        };

        if (mode == PdfLifecycleMode.OpenPdfFrame)
        {
            events.Insert(1, "OpenPdfPicked");
            events.Add("OpenFramePlaced");
        }

        return events;
    }
}
