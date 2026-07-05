using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Ownership;
using RKWorkspace.Surface.Abstractions;

var root = FindRoot();
var samplePdf = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
var checks = new List<(string Name, Func<bool> Check)>
{
    ("ProtocolVersion", () => RkwpVersion.Current.Major == 0 && RkwpVersion.Current.Minor == 1),
    ("DevelopmentProtector", DevelopmentProtector),
    ("MessageValidation", MessageValidation),
    ("EnvelopeFields", EnvelopeFields),
    ("OriginalOwnedDefault", OriginalOwnedDefault),
    ("PlaceDoesNotTransferOwnership", PlaceDoesNotTransferOwnership),
    ("FrameOnlyLease", FrameOnlyLease),
    ("HeartbeatMissingStartsGrace", HeartbeatMissingStartsGrace),
    ("LeaseRecovery", LeaseRecovery),
    ("OwnershipTransferGuard", OwnershipTransferGuard),
    ("ObjectKindRules", ObjectKindRuleChecks),
    ("NoFileIngress", () => new PdfFrameOwnerService().OpenFrameOnlySession(samplePdf).GuestHasNoFileIngress),
    ("PdfFrameOnly", () => new PdfFrameOwnerService().OpenFrameOnlySession(samplePdf).IsSuccessful),
    ("SurfaceContracts", SurfaceContracts)
};

Console.WriteLine("RK Workspace RKWP Protocol Tests");
foreach (var (name, check) in checks)
{
    if (!check())
    {
        Console.WriteLine($"{name}: FAILED");
        return 1;
    }

    Console.WriteLine($"{name}: PASS");
}

Console.WriteLine("RkwpTests: SUCCESS");
Console.WriteLine("RESULT: SUCCESS");
return 0;

static bool DevelopmentProtector()
{
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var message = RkwpMessage.Create(RkwpMessageType.AblageHello, session, 1);
    var protector = new DevelopmentRkwpSessionProtector();
    var protectedMessage = protector.Protect(message, session);
    return protectedMessage.AuthTag is not null && protector.Verify(protectedMessage, session);
}

static bool MessageValidation()
{
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var message = RkwpMessage.Create(RkwpMessageType.CarryLeaseHeartbeat, session, 2, leaseId: "lease-1");
    return RkwpMessageValidator.Validate(message).Count == 0;
}

static bool EnvelopeFields()
{
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var message = RkwpMessage.Create(RkwpMessageType.FrameSessionOpen, session, 3, leaseId: "lease-2");
    var envelope = new RkwpEnvelope(message);
    return envelope.MessageId == message.MessageId &&
           envelope.MessageType == message.MessageType &&
           envelope.ProtocolVersion == RkwpVersion.Current &&
           envelope.LeaseId == "lease-2" &&
           envelope.Nonce == message.Nonce;
}

static bool OriginalOwnedDefault()
{
    var ownership = ThingOwnership.Original("thing-1", "ablage-owner");
    return ownership.State == OwnershipState.OriginalOwned &&
           ownership.OwnerAblageId == "ablage-owner" &&
           ownership.OriginalDisposition == OriginalDisposition.RetainOriginal;
}

static bool PlaceDoesNotTransferOwnership()
{
    var ownership = ThingOwnership.Original("thing-1", "ablage-owner").LeaseToGuest("ablage-guest", OwnershipMode.FrameOnly);
    return ownership.OwnerAblageId == "ablage-owner" &&
           ownership.GuestAblageId == "ablage-guest" &&
           ownership.State == OwnershipState.LockedOnOwner &&
           ownership.OriginalDisposition == OriginalDisposition.RetainOriginal;
}

static bool FrameOnlyLease()
{
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, DateTimeOffset.UtcNow);
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, DateTimeOffset.UtcNow).Ready(DateTimeOffset.UtcNow).Activate(DateTimeOffset.UtcNow);
    return lease.Mode == OwnershipMode.FrameOnly &&
           lease.AllowedActions.HasFlag(RkwpAllowedAction.View) &&
           !frame.EditAllowed &&
           frame.State == FrameSessionState.Active;
}

static bool HeartbeatMissingStartsGrace()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var missing = lease.Advance(now.AddSeconds(5));
    var grace = missing.Advance(now.AddSeconds(6));
    var heartbeat = grace.Heartbeat(now.AddSeconds(7));
    return missing.State == CarryLeaseState.HeartbeatMissing &&
           grace.State == CarryLeaseState.GracePeriod &&
           heartbeat.State == CarryLeaseState.Active;
}

static bool LeaseRecovery()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var recovered = (lease with { LastHeartbeat = now.AddSeconds(-30) }).Advance(now).Recover();
    return recovered.OwnerRecoveredThing && recovered.GuestFrameInvalidated;
}

static bool OwnershipTransferGuard()
{
    var request = new OwnershipTransferRequest("request-1", "thing-1", "owner", "guest", OwnershipMode.MoveOwnership, DateTimeOffset.UtcNow);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipPolicy.CriticalDefault);
    return decision.Decision == OwnershipTransferDecisionKind.RequiresUserConfirmation;
}

static bool ObjectKindRuleChecks()
{
    var pdf = ObjectKindRules.For(ObjectKind.PdfDocument);
    var settings = ObjectKindRules.For(ObjectKind.SettingsWindow);
    var remote = ObjectKindRules.For(ObjectKind.RemoteSession);
    return pdf.DefaultMode == OwnershipMode.FrameOnly &&
           pdf.AllowsOptionalMode(OwnershipMode.CopyOut) &&
           settings.DefaultMode == OwnershipMode.InteractiveFrame &&
           !settings.OwnershipTransferSupported &&
           settings.AllowsOptionalMode(OwnershipMode.SnapshotExport) &&
           remote.AllowsOptionalMode(OwnershipMode.SessionHandoff);
}

static bool SurfaceContracts()
{
    var placement = new SurfaceFramePlacement("tablet", "right", 0.4, true);
    var identity = new SurfaceIdentity("tablet", WorkspaceSurfacePlatform.IPadOS, "Tablet", "desk");
    return placement.IsNearest && identity.Platform == WorkspaceSurfacePlatform.IPadOS;
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
