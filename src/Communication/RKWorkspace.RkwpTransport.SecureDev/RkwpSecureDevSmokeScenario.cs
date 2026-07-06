using RKWorkspace.Protocol.Security;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransport.SecureDev;

public static class RkwpSecureDevSmokeScenario
{
    public static async Task<RkwpSecureDevTransportResult> RunAsync(
        RkwpSecureDevTransportOptions options,
        CancellationToken cancellationToken = default)
    {
        var events = new List<string>();
        var transport = new RkwpSecureDevTransport(options);
        var endpoint = TransportEndpoint.NamedPipe(options.EndpointName);
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(endpoint);
        RkwpSecureSession? secureSession = null;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));

        await server.StartAsync(timeout.Token).ConfigureAwait(false);
        events.Add("TransportStarted");

        var serverTask = Task.Run(async () =>
        {
            for (var index = 0; index < 5; index++)
            {
                var request = await server.WaitForMessageAsync(timeout.Token).ConfigureAwait(false);
                if (index == 0)
                {
                    Ensure(
                        request.MessageType == TransportMessageType.AblageHello &&
                        string.Equals(request.SourceAblageId, options.GuestIdentity.AblageId.Value, StringComparison.Ordinal) &&
                        string.Equals(request.TargetAblageId, options.OwnerIdentity.AblageId.Value, StringComparison.Ordinal),
                        "AblageHello did not carry the expected guest/owner identity.");
                    events.Add("AblageHello");
                    events.Add("IdentityExchange");
                    secureSession = transport.EstablishSession();
                    events.Add("SecureDevHandshake");
                }
                else
                {
                    var activeSession = secureSession ?? throw new RkwpSecureDevTransportException("Secure session was not established.");
                    Ensure(
                        string.Equals(request.SessionId, activeSession.Session.SessionId, StringComparison.Ordinal),
                        "SecureDev request used the wrong session id.");
                }

                await server.SendResponseAsync(CreateResponse(request, options, secureSession), timeout.Token)
                    .ConfigureAwait(false);
            }
        }, timeout.Token);

        var hello = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageHello,
                options.GuestIdentity.AblageId.Value,
                options.OwnerIdentity.AblageId.Value,
                payload: options.GuestIdentity.ToHelloPayload(),
                headers: CreateSecurityHeaders(options, transport)),
            options.DefaultTimeout,
            timeout.Token).ConfigureAwait(false);
        Ensure(hello.MessageType == TransportMessageType.AblageCapabilities, "AblageHello did not return capabilities.");
        Ensure(!string.IsNullOrWhiteSpace(hello.SessionId), "AblageHello did not return a secure session id.");
        events.Add("GuestConnected");

        var capabilities = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                options.GuestIdentity.AblageId.Value,
                options.OwnerIdentity.AblageId.Value,
                hello.SessionId,
                options.GuestIdentity.ToCapabilitiesPayload(),
                headers: CreateSecurityHeaders(options, transport)),
            options.DefaultTimeout,
            timeout.Token).ConfigureAwait(false);
        Ensure(capabilities.Payload.TryGetValue("identityExchange", out var identityExchange) && identityExchange == "OK", "Identity exchange was not confirmed.");

        var heartbeat = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseHeartbeat,
                options.GuestIdentity.AblageId.Value,
                options.OwnerIdentity.AblageId.Value,
                hello.SessionId,
                new Dictionary<string, string>
                {
                    ["leaseId"] = "lease-securedev-smoke",
                    ["state"] = "alive"
                },
                headers: CreateSecurityHeaders(options, transport)),
            options.DefaultTimeout,
            timeout.Token).ConfigureAwait(false);
        Ensure(heartbeat.MessageType == TransportMessageType.CarryLeaseHeartbeat, "Heartbeat failed.");
        events.Add("Heartbeat");

        var frameUpdate = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                options.OwnerIdentity.AblageId.Value,
                options.GuestIdentity.AblageId.Value,
                hello.SessionId,
                new Dictionary<string, string>
                {
                    ["frameId"] = "frame-securedev-smoke",
                    ["containsOriginalFileBytes"] = "false",
                    ["hasOriginalPath"] = "false"
                },
                headers: CreateSecurityHeaders(options, transport)),
            options.DefaultTimeout,
            timeout.Token).ConfigureAwait(false);
        Ensure(frameUpdate.Payload.TryGetValue("noFileIngress", out var noFileIngress) && noFileIngress == "true", "NoFileIngress was not preserved.");
        events.Add("FrameUpdate");

        var shutdown = await client.RequestAsync(
            RkwpTransportMessage.Create(
                TransportMessageType.ShutdownRequest,
                options.GuestIdentity.AblageId.Value,
                options.OwnerIdentity.AblageId.Value,
                hello.SessionId,
                new Dictionary<string, string>
                {
                    ["reason"] = "securedev-smoke-complete"
                },
                headers: CreateSecurityHeaders(options, transport)),
            options.DefaultTimeout,
            timeout.Token).ConfigureAwait(false);
        Ensure(shutdown.MessageType == TransportMessageType.ShutdownRequest, "Shutdown failed.");
        events.Add("Shutdown");

        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);

        var session = secureSession ?? throw new RkwpSecureDevTransportException("SecureDev session was not established.");
        events.Add("SessionActive");
        return new RkwpSecureDevTransportResult(
            Success: true,
            AblageId: options.OwnerIdentity.AblageId.Value,
            PeerAblageId: options.GuestIdentity.AblageId.Value,
            SecurityMode: transport.EffectiveSecurityMode,
            TransportProfile: transport.TransportProfile,
            SecureSessionRequired: options.SecureSessionRequired,
            TlsEnabled: transport.TlsEnabled,
            TlsStatus: transport.TlsStatus,
            HandshakeState: session.State,
            SessionId: session.Session.SessionId,
            Events: events,
            Diagnostics: transport.GetDiagnostics());
    }

    private static RkwpTransportMessage CreateResponse(
        RkwpTransportMessage request,
        RkwpSecureDevTransportOptions options,
        RkwpSecureSession? secureSession)
    {
        var sessionId = secureSession?.Session.SessionId ?? request.SessionId;
        return request.MessageType switch
        {
            TransportMessageType.AblageHello => RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                CreateOwnerCapabilitiesPayload(options, secureSession),
                request.MessageId,
                CreateSecurityHeaders(options, request.Headers)),
            TransportMessageType.AblageCapabilities => RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["identityExchange"] = "OK",
                    ["secureDevHandshake"] = secureSession?.State.ToString() ?? "Pending",
                    ["noFileIngress"] = "true"
                },
                request.MessageId,
                CreateSecurityHeaders(options, request.Headers)),
            TransportMessageType.FrameUpdate => RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["frame"] = "accepted",
                    ["noFileIngress"] = "true"
                },
                request.MessageId,
                CreateSecurityHeaders(options, request.Headers)),
            _ => RkwpTransportMessage.Create(
                request.MessageType,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                request.Payload,
                request.MessageId,
                CreateSecurityHeaders(options, request.Headers))
        };
    }

    private static Dictionary<string, string> CreateOwnerCapabilitiesPayload(
        RkwpSecureDevTransportOptions options,
        RkwpSecureSession? secureSession)
    {
        var payload = options.OwnerIdentity.ToCapabilitiesPayload().ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.OrdinalIgnoreCase);
        payload["identityExchange"] = "OK";
        payload["secureDevHandshake"] = secureSession?.State.ToString() ?? "Pending";
        payload["secureSessionRequired"] = options.SecureSessionRequired.ToString();
        payload["noFileIngress"] = "true";
        return payload;
    }

    private static Dictionary<string, string> CreateSecurityHeaders(
        RkwpSecureDevTransportOptions options,
        RkwpSecureDevTransport transport)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["security-mode"] = transport.EffectiveSecurityMode.ToString(),
            ["secure-session-required"] = options.SecureSessionRequired.ToString(),
            ["transport-profile"] = transport.TransportProfile,
            ["tls-enabled"] = transport.TlsEnabled.ToString(),
            ["tls-status"] = transport.TlsStatus
        };
    }

    private static Dictionary<string, string> CreateSecurityHeaders(
        RkwpSecureDevTransportOptions options,
        IReadOnlyDictionary<string, string> requestHeaders)
    {
        var headers = requestHeaders.ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.OrdinalIgnoreCase);
        headers["secure-session-required"] = options.SecureSessionRequired.ToString();
        return headers;
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new RkwpSecureDevTransportException(message);
        }
    }
}
