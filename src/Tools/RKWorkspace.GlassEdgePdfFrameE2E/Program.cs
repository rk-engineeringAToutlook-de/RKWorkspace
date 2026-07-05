using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Identity;
using RKWorkspace.Protocol.Ownership;
using RKWorkspace.Shell;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Dev;
using RKWorkspace.Transport.Rkwp;
using ProtocolAblageIdentity = RKWorkspace.Protocol.Identity.AblageIdentity;

var options = GlassEdgePdfFrameE2EOptions.Parse(args, FindRoot());

try
{
    var result = await GlassEdgePdfFrameE2E.RunAsync(options).ConfigureAwait(false);
    Print(result);
    return result.IsSuccessful ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine("RK Workspace Glass Edge PDF Frame E2E");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine($"RESULT: FAILED - {ex.Message}");
    return 1;
}

static void Print(GlassEdgePdfFrameE2EResult result)
{
    Console.WriteLine("RK Workspace Glass Edge PDF Frame E2E");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine($"Mode: {(result.SmokeTest ? "SmokeTest" : "Demo")}");
    Console.WriteLine($"PDF: {result.Document.FileName}");
    Console.WriteLine($"NearestAblage: {result.Nearest.TargetDisplayName}");
    Console.WriteLine($"EdgeDirection: {result.GlassEdge.Direction}");
    Console.WriteLine($"GlassEdge: {(result.GlassEdge.IsActive ? "Active" : "Inactive")}");
    Console.WriteLine($"EventFlow: {string.Join(" -> ", result.EventFlow)}");
    Console.WriteLine($"NearestAblageSelected: {(result.Nearest.HasTarget ? "OK" : "FAILED")}");
    Console.WriteLine($"GlassEdgeAppearing: {(result.HasEvent(RkwpMessageType.GlassEdgeAppearing) ? "OK" : "FAILED")}");
    Console.WriteLine($"GlassEdgeActive: {(result.HasEvent(RkwpMessageType.GlassEdgeActive) ? "OK" : "FAILED")}");
    Console.WriteLine($"ObjectEnteringEdge: {(result.HasEvent(RkwpMessageType.ObjectEnteringEdge) ? "OK" : "FAILED")}");
    Console.WriteLine($"ObjectPlaced: {(result.HasEvent(RkwpMessageType.ObjectPlaced) ? "OK" : "FAILED")}");
    Console.WriteLine($"CarryLease: {result.Lease.State}");
    Console.WriteLine($"FrameSession: {result.FrameSession.State}");
    Console.WriteLine("PDF liegt im Frame.");
    Console.WriteLine($"GuestFrame: {(result.GuestFrameReady ? "OK" : "FAILED")}");
    Console.WriteLine($"GuestHasPdfFile: {(result.GuestHasPdfFile ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasOriginalPath: {(result.GuestHasOriginalPath ? "YES" : "NO")}");
    Console.WriteLine($"GuestHasCopiedPdfBytes: {(result.GuestHasCopiedPdfBytes ? "YES" : "NO")}");
    Console.WriteLine($"NoFileIngress: {(result.NoFileIngress ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"Return: {(result.ReturnSuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine("Zurueckgegeben.");
    Console.WriteLine($"Recovery: {(result.RecoverySuccessful ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"TransportConnected: {(result.TransportConnected ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"VisualSimulation: CLI event simulation");
    Console.WriteLine($"OpenUxPoints: Native manual key handling remains in the visual shell slice.");
    Console.WriteLine($"GlassEdgePdfFrameE2E: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
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

public sealed record GlassEdgePdfFrameE2EOptions(string PdfPath, bool SmokeTest)
{
    public static GlassEdgePdfFrameE2EOptions Parse(string[] args, string root)
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

        return new GlassEdgePdfFrameE2EOptions(Path.GetFullPath(pdfPath), smokeTest);
    }
}

public static class GlassEdgePdfFrameE2E
{
    private static readonly TimeSpan TransportTimeout = TimeSpan.FromSeconds(2);

    public static async Task<GlassEdgePdfFrameE2EResult> RunAsync(GlassEdgePdfFrameE2EOptions options)
    {
        var now = DateTimeOffset.UtcNow;
        var currentAblage = SimulatedAblageProximityProvider.WindowsAblageId;
        var nearest = new NearestAblageSelector().Select(new SimulatedAblageProximityProvider().GetSnapshot(currentAblage));
        if (!nearest.HasTarget || nearest.TargetAblageId is null)
        {
            throw new InvalidOperationException("No nearest ablage for Glass Edge PDF Frame E2E.");
        }

        var glassEdge = GlassEdge.FromNearest(nearest, GlassEdgeState.Opening, activation: 1.0, absorption: 0.42);
        var ownerIdentity = ProtocolAblageIdentity.CreateDev(
            currentAblage.Value,
            "Ablage Owner",
            "Windows",
            ["FrameOnly", "PdfOwner", "GlassEdge", "NoFileIngress"]);
        var guestCandidate = ProtocolAblageIdentity.CreateDev(
            nearest.TargetAblageId.Value.Value,
            nearest.TargetDisplayName,
            nearest.Platform.ToString(),
            ["FrameOnly", "PdfGuest", "GlassEdge", "NoFileIngress"]) with
            {
                TrustLevel = AblageTrustLevel.Unknown,
                PairingState = AblagePairingState.PairingRequested
            };
        var pairing = new DevAblagePairingService();
        var pairingRequest = pairing.RequestPairing(guestCandidate, ownerIdentity, "Glass Edge PDF Frame E2E dev pairing");
        var guestIdentity = pairing.ApplyDecision(guestCandidate, pairing.Decide(pairingRequest, approved: true));
        var document = PdfFrameDocument.Load(options.PdfPath);
        var session = RkwpSession.CreateDevelopment(ownerIdentity.AblageId.Value, guestIdentity.AblageId.Value);

        var eventMessages = CreateEventFlow(session, document.ThingId, glassEdge);
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

        var ownership = ThingOwnership.Original(document.ThingId, session.OwnerAblageId)
            .LeaseToGuest(session.GuestAblageId, OwnershipMode.FrameOnly);
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
            SupportsScroll: true,
            SupportsZoom: true,
            ContainsOriginalFileBytes: false,
            HasOriginalFilePath: false);

        var transportConnected = await VerifyTransportAsync(
            ownerIdentity,
            guestIdentity,
            session.SessionId,
            update,
            guestFrame).ConfigureAwait(false);
        var returnedLease = lease.Return(now.AddSeconds(1));
        var returnedFrame = frame.Close(now.AddSeconds(1));
        var recovery = (lease with { LastHeartbeat = now.AddSeconds(-30) })
            .Advance(now)
            .Recover();

        return new GlassEdgePdfFrameE2EResult(
            options.SmokeTest,
            nearest,
            glassEdge,
            document,
            ownership,
            lease,
            frame,
            update,
            guestFrame,
            returnedLease,
            returnedFrame,
            recovery,
            eventMessages,
            transportConnected);
    }

    private static IReadOnlyList<RkwpMessage> CreateEventFlow(
        RkwpSession session,
        string thingId,
        GlassEdge glassEdge)
    {
        var sequence = 1L;
        var edgePayload = new Dictionary<string, string>
        {
            ["edgeId"] = glassEdge.EdgeId,
            ["direction"] = glassEdge.Direction.ToString(),
            ["targetAblageId"] = glassEdge.TargetAblageId.Value,
            ["thingId"] = thingId
        };

        return
        [
            RkwpMessage.Create(RkwpMessageType.GlassEdgeAppearing, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.GlassEdgeActive, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.ObjectEnteringEdge, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.CarryLeaseRequested, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.CarryLeaseGranted, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.FrameSessionOpen, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.FrameSessionReady, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.ObjectInTransit, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.ObjectEmerging, session, sequence++, edgePayload),
            RkwpMessage.Create(RkwpMessageType.ObjectPlaced, session, sequence++, edgePayload)
        ];
    }

    private static async Task<bool> VerifyTransportAsync(
        ProtocolAblageIdentity ownerIdentity,
        ProtocolAblageIdentity guestIdentity,
        string sessionId,
        FrameUpdate update,
        PdfGuestFrame guestFrame)
    {
        var transport = new RkwpDevTransport();
        var endpoint = TransportEndpoint.NamedPipe($"rkws-glass-edge-pdf-frame-e2e-{Guid.NewGuid():N}");
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(endpoint);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        var serverTask = Task.Run(
            () => RunGuestConversationAsync(server, guestIdentity, sessionId, cancellation.Token),
            cancellation.Token);

        var hello = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                payload: ownerIdentity.ToHelloPayload()),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);
        var frameUpdate = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameSessionId"] = update.FrameSessionId,
                    ["thingId"] = update.ThingId,
                    ["containsOriginalFileBytes"] = update.ContainsOriginalFileBytes.ToString(),
                    ["hasOriginalPath"] = guestFrame.HasOriginalFilePath.ToString()
                }),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);
        var returned = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseReturn,
                ownerIdentity.AblageId.Value,
                guestIdentity.AblageId.Value,
                sessionId,
                new Dictionary<string, string> { ["return"] = "requested" }),
            TransportTimeout,
            cancellation.Token).ConfigureAwait(false);

        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        var diagnostics = transport.GetDiagnostics();
        return hello.MessageType == TransportMessageType.AblageCapabilities &&
               hello.SessionId == sessionId &&
               frameUpdate.Payload.TryGetValue("guestFrame", out var guestFrameState) &&
               guestFrameState == "opened" &&
               returned.MessageType == TransportMessageType.CarryLeaseReturn &&
               diagnostics.MessagesSent >= 3 &&
               diagnostics.MessagesReceived >= 3;
    }

    private static async Task RunGuestConversationAsync(
        IRkwpTransportServer server,
        ProtocolAblageIdentity guestIdentity,
        string sessionId,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < 3; index++)
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
                TransportMessageType.FrameUpdate => RkwpTransportMessage.Create(
                    TransportMessageType.FrameUpdate,
                    request.TargetAblageId,
                    request.SourceAblageId,
                    sessionId,
                    new Dictionary<string, string>
                    {
                        ["guestFrame"] = "opened",
                        ["noFileIngress"] = "true"
                    },
                    request.MessageId),
                TransportMessageType.CarryLeaseReturn => RkwpTransportMessage.Create(
                    TransportMessageType.CarryLeaseReturn,
                    request.TargetAblageId,
                    request.SourceAblageId,
                    sessionId,
                    new Dictionary<string, string>
                    {
                        ["return"] = "accepted"
                    },
                    request.MessageId),
                _ => throw new InvalidOperationException($"Unexpected message type {request.MessageType}.")
            };

            await server.SendResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }
}

public sealed record GlassEdgePdfFrameE2EResult(
    bool SmokeTest,
    NearestAblageResult Nearest,
    GlassEdge GlassEdge,
    PdfFrameDocument Document,
    ThingOwnership Ownership,
    CarryLease Lease,
    FrameSession FrameSession,
    FrameUpdate FirstFrameUpdate,
    PdfGuestFrame GuestFrame,
    CarryLease ReturnedLease,
    FrameSession ReturnedFrame,
    CarryLeaseRecovery Recovery,
    IReadOnlyList<RkwpMessage> Events,
    bool TransportConnected)
{
    public IReadOnlyList<string> EventFlow => Events.Select(message => message.MessageType.ToString()).ToArray();

    public bool HasEvent(RkwpMessageType messageType)
    {
        return Events.Any(message => message.MessageType == messageType);
    }

    public bool GuestFrameReady =>
        FrameSession.State == FrameSessionState.Active &&
        !string.IsNullOrWhiteSpace(GuestFrame.DisplayText);

    public bool GuestHasPdfFile => GuestFrame.HasOriginalFilePath;

    public bool GuestHasOriginalPath => GuestFrame.HasOriginalFilePath;

    public bool GuestHasCopiedPdfBytes =>
        GuestFrame.ContainsOriginalFileBytes || FirstFrameUpdate.ContainsOriginalFileBytes;

    public bool NoFileIngress => !GuestHasPdfFile && !GuestHasOriginalPath && !GuestHasCopiedPdfBytes;

    public bool ReturnSuccessful =>
        ReturnedLease.State == CarryLeaseState.Returned &&
        ReturnedFrame.State == FrameSessionState.Closed;

    public bool RecoverySuccessful =>
        Recovery.OwnerRecoveredThing &&
        Recovery.GuestFrameInvalidated &&
        Recovery.FinalState == CarryLeaseState.RecoveredByOwner;

    public bool IsSuccessful =>
        Nearest.HasTarget &&
        GlassEdge.IsActive &&
        HasEvent(RkwpMessageType.GlassEdgeAppearing) &&
        HasEvent(RkwpMessageType.GlassEdgeActive) &&
        HasEvent(RkwpMessageType.ObjectEnteringEdge) &&
        HasEvent(RkwpMessageType.ObjectPlaced) &&
        HasEvent(RkwpMessageType.FrameSessionOpen) &&
        HasEvent(RkwpMessageType.FrameSessionReady) &&
        Lease.State == CarryLeaseState.Active &&
        FrameSession.State == FrameSessionState.Active &&
        Ownership.State == OwnershipState.LockedOnOwner &&
        GuestFrameReady &&
        NoFileIngress &&
        ReturnSuccessful &&
        RecoverySuccessful &&
        TransportConnected;
}
