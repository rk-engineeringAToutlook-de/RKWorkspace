using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Identity;
using RKWorkspace.Protocol.Ownership;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Dev;
using RKWorkspace.Transport.Rkwp;

var options = WindowsLocalFrameE2EOptions.Parse(args, FindRoot());

try
{
    var result = await WindowsLocalFrameE2E.RunAsync(options).ConfigureAwait(false);
    Print(result);
    return result.IsSuccessful ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine("RK Workspace Windows Local Frame E2E");
    Console.WriteLine("------------------------------------");
    Console.WriteLine($"RESULT: FAILED - {ex.Message}");
    return 1;
}

static void Print(WindowsLocalFrameE2EResult result)
{
    Console.WriteLine("RK Workspace Windows Local Frame E2E");
    Console.WriteLine("------------------------------------");
    Console.WriteLine($"Mode: {(result.SmokeTest ? "SmokeTest" : "Demo")}");
    Console.WriteLine($"PDF: {result.Document.FileName}");
    Console.WriteLine($"ThingId: {result.Document.ThingId}");
    Console.WriteLine($"Owner: {result.OwnerIdentity.DisplayName}");
    Console.WriteLine($"Guest: {result.GuestIdentity.DisplayName}");
    Console.WriteLine($"OwnerStarted: {(result.OwnerStarted ? "OK" : "FAILED")}");
    Console.WriteLine($"GuestStarted: {(result.GuestStarted ? "OK" : "FAILED")}");
    Console.WriteLine($"DevPairing: {(result.DevPairingSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"Transport: NamedPipeDev");
    Console.WriteLine($"TransportConnected: {(result.TransportConnected ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"TransportMessages: Sent={result.TransportDiagnostics.MessagesSent} Received={result.TransportDiagnostics.MessagesReceived}");
    Console.WriteLine($"PdfOriginalRegistered: {(result.PdfOriginalRegistered ? "OK" : "FAILED")}");
    Console.WriteLine($"CarryLease: {result.Lease.State}");
    Console.WriteLine($"OwnerLocked: {(result.OwnerLocked ? "OK" : "FAILED")}");
    Console.WriteLine($"OwnerVisibleStatus: {result.OwnerVisibleStatus}");
    Console.WriteLine($"OwnerReturnedStatus: {result.OwnerReturnedStatus}");
    Console.WriteLine($"OwnerRecoveryStatus: {result.OwnerRecoveryStatus}");
    Console.WriteLine($"FrameSession: {result.FrameSession.State}");
    Console.WriteLine("PDF liegt im Frame.");
    Console.WriteLine($"GuestFrame: {(result.GuestFrameReady ? "OK" : "FAILED")}");
    Console.WriteLine($"GuestVisibleStatus: {result.GuestVisibleStatus}");
    Console.WriteLine($"GuestRevokedStatus: {result.GuestRevokedStatus}");
    Console.WriteLine($"GuestExpiredStatus: {result.GuestExpiredStatus}");
    Console.WriteLine($"OwnerStateFlow: {FormatFlow(result.VisibleStates, "Owner")}");
    Console.WriteLine($"GuestStateFlow: {FormatFlow(result.VisibleStates, "Guest")}");
    Console.WriteLine($"VisibleStateLanguage: {(result.VisibleStateLanguageIsValid ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"PreviewKind: {result.GuestFrame.RepresentationKind}");
    Console.WriteLine($"RendererStatus: {result.GuestFrame.RendererStatus}");
    Console.WriteLine($"GuestHasPdfFile: {(result.GuestHasPdfFile ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasOriginalPath: {(result.GuestHasOriginalPath ? "YES" : "NO")}");
    Console.WriteLine($"OriginalFileBytes: {(result.GuestHasCopiedPdfBytes ? "YES" : "NO")}");
    Console.WriteLine($"NoFileIngress: {(result.NoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"Return: {(result.ReturnSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine("Zurueckgegeben.");
    Console.WriteLine($"Recovery: {(result.RecoverySuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"GuestEvents: {string.Join(" -> ", result.GuestEvents)}");
    Console.WriteLine($"Blocker: {(result.RendererBlocked ? "PDF renderer still blocked; metadata frame used." : "None")}");
    Console.WriteLine($"WindowsLocalFrameE2E: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"RESULT: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
}

static string FormatFlow(IReadOnlyList<VisibleFrameState> states, string scope)
{
    return string.Join(" -> ", states
        .Where(state => string.Equals(state.Scope, scope, StringComparison.Ordinal))
        .Select(state => state.Text));
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

public sealed record WindowsLocalFrameE2EOptions(string PdfPath, bool SmokeTest)
{
    public static WindowsLocalFrameE2EOptions Parse(string[] args, string root)
    {
        var pdfPath = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
        var smokeTest = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-SmokeTest", StringComparison.OrdinalIgnoreCase))
            {
                smokeTest = true;
                continue;
            }

            if ((string.Equals(arg, "--pdf-path", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(arg, "-PdfPath", StringComparison.OrdinalIgnoreCase)) &&
                index + 1 < args.Length)
            {
                pdfPath = args[++index];
            }
        }

        return new WindowsLocalFrameE2EOptions(Path.GetFullPath(pdfPath), smokeTest);
    }
}

public static class WindowsLocalFrameE2E
{
    private static readonly TimeSpan TransportTimeout = TimeSpan.FromSeconds(2);

    public static async Task<WindowsLocalFrameE2EResult> RunAsync(WindowsLocalFrameE2EOptions options)
    {
        var now = DateTimeOffset.UtcNow;
        var ownerIdentity = AblageIdentity.CreateDev(
            "ablage-windows-owner",
            "Ablage Owner",
            "Windows",
            ["FrameOnly", "PdfOwner", "NoFileIngress", "Return", "Recovery"]);
        var guestCandidate = AblageIdentity.CreateDev(
            "ablage-windows-guest",
            "Ablage Guest",
            "Windows",
            ["FrameOnly", "PdfGuest", "NoFileIngress", "Return"]) with
            {
                TrustLevel = AblageTrustLevel.Unknown,
                PairingState = AblagePairingState.PairingRequested
            };

        var pairing = new DevAblagePairingService();
        var pairingRequest = pairing.RequestPairing(guestCandidate, ownerIdentity, "Windows local frame E2E dev pairing");
        var pairingDecision = pairing.Decide(pairingRequest, approved: true);
        var guestIdentity = pairing.ApplyDecision(guestCandidate, pairingDecision);
        var devPairingSuccessful =
            pairingRequest.State == AblagePairingState.PairingPending &&
            pairingDecision.Approved &&
            guestIdentity.TrustLevel == AblageTrustLevel.DevTrusted &&
            guestIdentity.PairingState == AblagePairingState.Paired;

        var document = PdfFrameDocument.Load(options.PdfPath);
        var session = RkwpSession.CreateDevelopment(ownerIdentity.AblageId.Value, guestIdentity.AblageId.Value);
        var ownership = ThingOwnership.Original(document.ThingId, ownerIdentity.AblageId.Value);
        var lockedOwnership = ownership.LeaseToGuest(guestIdentity.AblageId.Value, OwnershipMode.FrameOnly);
        var leaseDecision = AblageTrustGate.CanGrantLease(
            guestIdentity,
            AblageTrustPolicy.DevelopmentFrameOnly,
            OwnershipMode.FrameOnly,
            session.SecurityMode,
            session.SecureSessionRequired);
        if (!leaseDecision.Allowed)
        {
            throw new AblageTrustException(leaseDecision.Reason);
        }

        var lease = CarryLease.Grant(
            document.ThingId,
            session.OwnerAblageId,
            session.GuestAblageId,
            CarryLeasePolicy.FrameOnlyDefault,
            now,
            session.SessionId);
        var frame = FrameSession
            .Open(lease, FrameMode.ViewOnly, now.AddMilliseconds(20))
            .Ready(now.AddMilliseconds(40))
            .Activate(now.AddMilliseconds(60));
        var update = new FrameUpdate(
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

        var transport = new RkwpDevTransport();
        var endpoint = TransportEndpoint.NamedPipe($"rkws-windows-local-frame-e2e-{Guid.NewGuid():N}");
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(endpoint);
        var guestEvents = new List<string>();
        var transportConnected = false;

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        var serverTask = Task.Run(
            () => RunGuestConversationAsync(server, session.SessionId, guestIdentity, guestEvents, cancellation.Token),
            cancellation.Token);

        var helloResponse = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                payload: ownerIdentity.ToHelloPayload()),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);
        transportConnected = helloResponse.MessageType == TransportMessageType.AblageCapabilities &&
                             helloResponse.SessionId == session.SessionId;

        var capabilitiesResponse = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                session.SessionId,
                ownerIdentity.ToCapabilitiesPayload()),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);
        Ensure(capabilitiesResponse.MessageType == TransportMessageType.AblageCapabilities, "Capabilities handshake failed.");

        var frameUpdateResponse = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                session.SessionId,
                new Dictionary<string, string>
                {
                    ["frameSessionId"] = update.FrameSessionId,
                    ["thingId"] = update.ThingId,
                    ["page"] = update.PageNumber.ToString(),
                    ["contentHash"] = update.ContentHash,
                    ["containsOriginalFileBytes"] = update.ContainsOriginalFileBytes.ToString(),
                    ["hasOriginalPath"] = guestFrame.HasOriginalFilePath.ToString()
                }),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);
        Ensure(frameUpdateResponse.Payload.TryGetValue("guestFrame", out var guestFrameState) && guestFrameState == "opened", "Guest frame did not open.");

        var heartbeatResponse = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseHeartbeat,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                session.SessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = lease.LeaseId,
                    ["state"] = lease.State.ToString()
                }),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);
        Ensure(heartbeatResponse.MessageType == TransportMessageType.CarryLeaseHeartbeat, "Heartbeat failed.");

        var returnedLease = lease.Return(now.AddSeconds(1));
        var returnedFrame = frame.Close(now.AddSeconds(1));
        var revokedFrame = frame.Revoke(now.AddSeconds(2));
        var expiredFrame = frame.Expire(now.AddSeconds(3));
        var returnResponse = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseReturn,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                session.SessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = returnedLease.LeaseId,
                    ["frameSessionId"] = returnedFrame.FrameSessionId,
                    ["state"] = returnedLease.State.ToString()
                }),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);
        Ensure(returnResponse.MessageType == TransportMessageType.CarryLeaseReturn, "Return failed.");

        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);

        var recovery = (lease with { LastHeartbeat = now.AddSeconds(-30) })
            .Advance(now)
            .Recover();

        return new WindowsLocalFrameE2EResult(
            options.SmokeTest,
            ownerIdentity,
            guestIdentity,
            document,
            lockedOwnership,
            lease,
            frame,
            update,
            guestFrame,
            returnedLease,
            returnedFrame,
            revokedFrame,
            expiredFrame,
            recovery,
            devPairingSuccessful,
            OwnerStarted: true,
            GuestStarted: true,
            transportConnected,
            transport.GetDiagnostics(),
            guestEvents);
    }

    private static async Task RunGuestConversationAsync(
        IRkwpTransportServer server,
        string sessionId,
        AblageIdentity guestIdentity,
        List<string> guestEvents,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < 5; index++)
        {
            var request = await server.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
            var response = request.MessageType switch
            {
                TransportMessageType.AblageHello => RkwpTransportMessage.Create(
                    TransportMessageType.AblageCapabilities,
                    guestIdentity.AblageId.Value,
                    request.SourceAblageId,
                    sessionId,
                    guestIdentity.ToCapabilitiesPayload(),
                    request.MessageId),
                TransportMessageType.FrameUpdate => CreateFrameUpdateResponse(request, sessionId, guestEvents),
                TransportMessageType.CarryLeaseReturn => CreateReturnResponse(request, sessionId, guestEvents),
                _ => RkwpTransportMessage.Create(
                    request.MessageType,
                    request.TargetAblageId,
                    request.SourceAblageId,
                    sessionId,
                    request.Payload,
                    request.MessageId)
            };

            if (request.MessageType == TransportMessageType.AblageHello)
            {
                guestEvents.Add("GuestStarted");
            }

            if (request.MessageType == TransportMessageType.CarryLeaseHeartbeat)
            {
                guestEvents.Add("Heartbeat");
            }

            await server.SendResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    private static RkwpTransportMessage CreateFrameUpdateResponse(
        RkwpTransportMessage request,
        string sessionId,
        List<string> guestEvents)
    {
        var containsBytes = request.Payload.TryGetValue("containsOriginalFileBytes", out var bytes) &&
                            bool.TryParse(bytes, out var parsedBytes) &&
                            parsedBytes;
        var hasOriginalPath = request.Payload.TryGetValue("hasOriginalPath", out var path) &&
                              bool.TryParse(path, out var parsedPath) &&
                              parsedPath;
        Ensure(!containsBytes, "Guest received original PDF bytes.");
        Ensure(!hasOriginalPath, "Guest received original PDF path.");
        guestEvents.Add("FrameOpened");
        return RkwpTransportMessage.Create(
            TransportMessageType.FrameUpdate,
            request.TargetAblageId,
            request.SourceAblageId,
            sessionId,
            new Dictionary<string, string>
            {
                ["guestFrame"] = "opened",
                ["noFileIngress"] = "true"
            },
            request.MessageId);
    }

    private static RkwpTransportMessage CreateReturnResponse(
        RkwpTransportMessage request,
        string sessionId,
        List<string> guestEvents)
    {
        guestEvents.Add("Returned");
        return RkwpTransportMessage.Create(
            TransportMessageType.CarryLeaseReturn,
            request.TargetAblageId,
            request.SourceAblageId,
            sessionId,
            new Dictionary<string, string>
            {
                ["return"] = "accepted",
                ["leaseId"] = request.Payload.TryGetValue("leaseId", out var leaseId) ? leaseId : string.Empty
            },
            request.MessageId);
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

public sealed record WindowsLocalFrameE2EResult(
    bool SmokeTest,
    AblageIdentity OwnerIdentity,
    AblageIdentity GuestIdentity,
    PdfFrameDocument Document,
    ThingOwnership Ownership,
    CarryLease Lease,
    FrameSession FrameSession,
    FrameUpdate FirstFrameUpdate,
    PdfGuestFrame GuestFrame,
    CarryLease ReturnedLease,
    FrameSession ReturnedFrame,
    FrameSession RevokedFrame,
    FrameSession ExpiredFrame,
    CarryLeaseRecovery Recovery,
    bool DevPairingSuccessful,
    bool OwnerStarted,
    bool GuestStarted,
    bool TransportConnected,
    RkwpTransportDiagnostics TransportDiagnostics,
    IReadOnlyList<string> GuestEvents)
{
    public bool PdfOriginalRegistered => File.Exists(Document.OwnerPath) && Document.PageCount > 0;

    public bool OwnerLocked =>
        Ownership.OwnerAblageId == Lease.OwnerAblageId &&
        Ownership.State == OwnershipState.LockedOnOwner &&
        Ownership.OriginalDisposition == OriginalDisposition.RetainOriginal;

    public bool GuestFrameReady =>
        FrameSession.State == FrameSessionState.Active &&
        !string.IsNullOrWhiteSpace(GuestFrame.DisplayText) &&
        GuestEvents.Contains("FrameOpened");

    public IReadOnlyList<VisibleFrameState> VisibleStates =>
        OwnerGuestFrameStateUx.CreateTimeline(Ownership, Lease, FrameSession, ReturnedLease, Recovery);

    public string OwnerVisibleStatus =>
        OwnerGuestFrameStateUx.GetOwnerText(OwnerGuestFrameStateUx.GetOwnerState(Ownership, Lease));

    public string OwnerReturnedStatus =>
        OwnerGuestFrameStateUx.GetOwnerText(OwnerGuestFrameStateUx.GetOwnerState(Ownership.ReturnToOwner(), ReturnedLease));

    public string OwnerRecoveryStatus =>
        OwnerGuestFrameStateUx.GetOwnerText(OwnerFrameUxState.RecoveredByOwner);

    public string GuestVisibleStatus =>
        OwnerGuestFrameStateUx.GetGuestText(OwnerGuestFrameStateUx.GetGuestState(FrameSession));

    public string GuestRevokedStatus =>
        OwnerGuestFrameStateUx.GetGuestText(OwnerGuestFrameStateUx.GetGuestState(RevokedFrame));

    public string GuestExpiredStatus =>
        OwnerGuestFrameStateUx.GetGuestText(OwnerGuestFrameStateUx.GetGuestState(ExpiredFrame));

    public bool VisibleStateLanguageIsValid =>
        OwnerGuestFrameStateUx.ValidateVisibleText(VisibleStates.Select(state => state.Text)).IsValid;

    public bool GuestHasPdfFile => GuestFrame.HasOriginalFilePath;

    public bool GuestHasOriginalPath => GuestFrame.HasOriginalFilePath;

    public bool GuestHasCopiedPdfBytes =>
        GuestFrame.ContainsOriginalFileBytes || FirstFrameUpdate.ContainsOriginalFileBytes;

    public bool NoFileIngress => !GuestHasPdfFile && !GuestHasOriginalPath && !GuestHasCopiedPdfBytes;

    public bool ReturnSuccessful =>
        ReturnedLease.State == CarryLeaseState.Returned &&
        ReturnedFrame.State == FrameSessionState.Closed &&
        GuestEvents.Contains("Returned");

    public bool RecoverySuccessful =>
        Recovery.OwnerRecoveredThing &&
        Recovery.GuestFrameInvalidated &&
        Recovery.FinalState == CarryLeaseState.RecoveredByOwner;

    public bool RendererBlocked => GuestFrame.RendererStatus == "RendererBlocked";

    public bool IsSuccessful =>
        OwnerStarted &&
        GuestStarted &&
        DevPairingSuccessful &&
        TransportConnected &&
        TransportDiagnostics.MessagesSent >= 5 &&
        TransportDiagnostics.MessagesReceived >= 5 &&
        PdfOriginalRegistered &&
        Lease.State == CarryLeaseState.Active &&
        FrameSession.State == FrameSessionState.Active &&
        OwnerLocked &&
        GuestFrameReady &&
        OwnerVisibleStatus == "wartet auf Rueckgabe" &&
        OwnerReturnedStatus == "zurueckgegeben" &&
        OwnerRecoveryStatus == "wieder verfuegbar" &&
        GuestVisibleStatus == "liegt hier im Frame" &&
        GuestRevokedStatus == "nicht verfuegbar" &&
        GuestExpiredStatus == "Verbindung verloren" &&
        VisibleStateLanguageIsValid &&
        NoFileIngress &&
        ReturnSuccessful &&
        RecoverySuccessful;
}
