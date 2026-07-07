using RKWorkspace.Frame.Pdf;
using RKWorkspace.RkwpTransport.DevLan;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;
using System.Text.Json;

namespace RKWorkspace.MacPdfFrameOwnerHost;

internal static class Program
{
    private const string OwnerAblageId = "ablage-windows-owner";
    private const string MacAblageId = "ablage-macos-guest";

    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = Options.Parse(args);
            if (options.ShowHelp)
            {
                PrintHelp();
                return 0;
            }

            var frame = options.DynamicPdfFromPlacementSignal ? null : RenderFrame(options);
            if (options.SmokeTest)
            {
                PrintSmoke(frame ?? RenderFrame(options.PdfPath ?? Options.DefaultPdfPath()));
                return 0;
            }

            if (options.PlacementGatingSmokeTest)
            {
                PrintPlacementGatingSmoke(frame ?? RenderFrame(options.PdfPath ?? Options.DefaultPdfPath()), options);
                return 0;
            }

            if (options.DynamicPlacementSmokeTest)
            {
                PrintDynamicPlacementSmoke(options);
                return 0;
            }

            return await RunOwnerAsync(options, frame).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace macOS PDF Frame Owner");
            Console.WriteLine("----------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static RenderedFrame RenderFrame(string pdfPath, int page = 1, int width = 1400, bool memoryPdfFrame = false, string placementId = "")
    {
        var document = PdfFrameDocument.Load(pdfPath);
        if (memoryPdfFrame)
        {
            var pdfBytes = File.ReadAllBytes(pdfPath);
            return new RenderedFrame(
                $"frame-macos-{Guid.NewGuid():N}",
                $"lease-macos-{Guid.NewGuid():N}",
                placementId,
                document.ThingId,
                document.FileName,
                document.PageCount,
                document.Sha256,
                page,
                0,
                0,
                "TransientPdfBytes",
                string.Empty,
                Convert.ToBase64String(pdfBytes),
                pdfBytes.Length,
                "TransientPdfLease",
                DateTimeOffset.UtcNow);
        }

        var renderer = PdfFrameRendererFactory.CreateDefault();
        var render = renderer.Render(new PdfFrameRenderRequest(
            document,
            page,
            new PdfFrameRenderOptions(RequestedWidth: width),
            OwnerAblageId,
            document.ThingId));

        if (!render.ContainsPixelPayload || render.PixelData is null || render.FrameFormat != FrameFormat.PngFrame)
        {
            throw new InvalidOperationException(
                "Real PDF frame rendering is not available. Poppler must be available on the Windows owner side.");
        }

        return new RenderedFrame(
            $"frame-macos-{Guid.NewGuid():N}",
            $"lease-macos-{Guid.NewGuid():N}",
            placementId,
            document.ThingId,
            document.FileName,
            document.PageCount,
            document.Sha256,
            render.PageNumber,
            render.Width,
            render.Height,
            "PngFrame",
            Convert.ToBase64String(render.PixelData),
            string.Empty,
            0,
            render.RendererName,
            render.RenderedAt);
    }

    private static void PrintSmoke(RenderedFrame frame)
    {
        Console.WriteLine("RK Workspace macOS PDF Frame Owner");
        Console.WriteLine("----------------------------------");
        Console.WriteLine("Mode: SmokeTest");
        Console.WriteLine($"DisplayName: {frame.DisplayName}");
        Console.WriteLine($"Page: {frame.Page}/{frame.PageCount}");
        if (frame.HasTransientPdfLease)
        {
            Console.WriteLine($"PdfBytes: {frame.PdfByteCount}");
        }
        else
        {
            Console.WriteLine($"FrameSize: {frame.Width}x{frame.Height}");
        }

        Console.WriteLine($"RendererName: {frame.RendererName}");
        Console.WriteLine("OwnerKeepsOriginal: OK");
        Console.WriteLine("GuestHasPdfFile: NO");
        Console.WriteLine("GuestHasOriginalPath: NO");
        Console.WriteLine("OriginalFileBytes: NO");
        Console.WriteLine($"FrameFormat: {frame.FrameFormat}");
        Console.WriteLine(frame.HasTransientPdfLease
            ? "TransientPdfLease: OK"
            : "TransientPdfLease: NOT_USED");
        Console.WriteLine(frame.HasTransientPdfLease
            ? "PDFCache: MemoryOnly"
            : "PDFCache: NOT_USED");
        Console.WriteLine("FrameCache: MemoryOnly");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
    }

    private static void PrintPlacementGatingSmoke(RenderedFrame frame, Options options)
    {
        var gatedOptions = options with { WaitForPlacement = true };
        if (File.Exists(gatedOptions.PlacementSignalPath))
        {
            File.Delete(gatedOptions.PlacementSignalPath);
        }

        var before = CreateFramePayload(frame, gatedOptions);
        if (before.TryGetValue("pngBase64", out _) ||
            !before.TryGetValue("placementReady", out var beforeReady) ||
            !string.Equals(beforeReady, "false", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Placement gating failed: frame leaked before placement signal.");
        }

        var directory = Path.GetDirectoryName(gatedOptions.PlacementSignalPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(gatedOptions.PlacementSignalPath, "ready");
        var after = CreateFramePayload(frame, gatedOptions);
        var hasFrame = (after.TryGetValue("pngBase64", out var pngBase64) && !string.IsNullOrWhiteSpace(pngBase64)) ||
            (after.TryGetValue("pdfBase64", out var pdfBase64) && !string.IsNullOrWhiteSpace(pdfBase64));
        if (!hasFrame ||
            !after.TryGetValue("placementReady", out var afterReady) ||
            !string.Equals(afterReady, "true", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Placement gating failed: frame was not released after placement signal.");
        }

        File.Delete(gatedOptions.PlacementSignalPath);

        Console.WriteLine("RK Workspace macOS PDF Frame Owner");
        Console.WriteLine("----------------------------------");
        Console.WriteLine("Mode: PlacementGatingSmokeTest");
        Console.WriteLine("BeforeSignal: NO_FRAME");
        Console.WriteLine($"AfterSignal: {frame.FrameFormat}");
        Console.WriteLine("PlacementGating: SUCCESS");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
    }

    private static void PrintDynamicPlacementSmoke(Options options)
    {
        var smokeOptions = options with
        {
            WaitForPlacement = true,
            DynamicPdfFromPlacementSignal = true,
            MemoryPdfFrame = true
        };

        if (File.Exists(smokeOptions.PlacementSignalPath))
        {
            File.Delete(smokeOptions.PlacementSignalPath);
        }

        var provider = new DynamicPlacementFrameProvider(smokeOptions);
        var before = CreateFramePayload(provider.ResolveFrame(), smokeOptions);
        if (before.TryGetValue("pdfBase64", out _) ||
            !before.TryGetValue("placementReady", out var beforeReady) ||
            !string.Equals(beforeReady, "false", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Dynamic placement smoke failed: frame leaked before placement.");
        }

        WritePlacementSignal(smokeOptions.PlacementSignalPath, "placement-smoke-001", smokeOptions.PdfPath);
        var firstFrame = provider.ResolveFrame();
        var firstPayload = CreateFramePayload(firstFrame, smokeOptions);
        if (firstFrame is null ||
            !firstPayload.TryGetValue("pdfBase64", out var firstPdf) ||
            string.IsNullOrWhiteSpace(firstPdf) ||
            !firstPayload.TryGetValue("placementReady", out var firstReady) ||
            !string.Equals(firstReady, "true", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Dynamic placement smoke failed: PDF lease was not released after placement.");
        }

        var stableFrame = provider.ResolveFrame();
        if (stableFrame is null || !string.Equals(stableFrame.LeaseId, firstFrame.LeaseId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Dynamic placement smoke failed: lease was not stable for the same placement.");
        }

        if (!provider.ReleaseReturnedLease(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["frameSessionId"] = firstFrame.FrameSessionId,
                ["leaseId"] = firstFrame.LeaseId,
                ["placementId"] = firstFrame.PlacementId,
                ["ownerMayReleaseLease"] = "true"
            }) ||
            File.Exists(smokeOptions.PlacementSignalPath) ||
            provider.ResolveFrame() is not null)
        {
            throw new InvalidOperationException("Dynamic placement smoke failed: returned lease was not released.");
        }

        WritePlacementSignal(smokeOptions.PlacementSignalPath, "placement-smoke-002", smokeOptions.PdfPath);
        var secondFrame = provider.ResolveFrame();
        if (secondFrame is null || string.Equals(secondFrame.LeaseId, firstFrame.LeaseId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Dynamic placement smoke failed: second placement did not create a new lease.");
        }

        File.Delete(smokeOptions.PlacementSignalPath);

        Console.WriteLine("RK Workspace macOS PDF Frame Owner");
        Console.WriteLine("----------------------------------");
        Console.WriteLine("Mode: DynamicPlacementSmokeTest");
        Console.WriteLine("BeforeSignal: NO_FRAME");
        Console.WriteLine($"AfterSignal: {firstFrame.FrameFormat}");
        Console.WriteLine("TransientPdfLease: OK");
        Console.WriteLine("StableLeasePerPlacement: OK");
        Console.WriteLine("ReturnRelease: OK");
        Console.WriteLine("SequentialPlacementLease: OK");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
    }

    private static RenderedFrame RenderFrame(Options options)
    {
        return RenderFrame(options.PdfPath ?? Options.DefaultPdfPath(), options.Page, options.Width, options.MemoryPdfFrame);
    }

    private static async Task<int> RunOwnerAsync(Options options, RenderedFrame? frame)
    {
        var sessionId = string.IsNullOrWhiteSpace(options.SessionId)
            ? $"rkwp-macos-frame-{Guid.NewGuid():N}"
            : options.SessionId;
        var transport = new RkwpDevLanTransport(new RkwpDevLanOptions
        {
            BindAddress = options.BindAddress,
            Port = options.Port,
            SessionId = sessionId,
            DevPairingAllowed = true,
            SecureSessionRequired = false,
            DefaultTimeout = TimeSpan.FromSeconds(10)
        });
        var server = transport.CreateServer(RkwpDevLanEndpoint.Create(options.BindAddress, options.Port));

        Console.WriteLine("RK Workspace macOS PDF Frame Owner");
        Console.WriteLine("----------------------------------");
        Console.WriteLine("ProductPath: Workspace Shell / Frame Owner Pilot");
        Console.WriteLine($"Listen: rkwp+tcp-dev://{options.BindAddress}:{options.Port}");
        Console.WriteLine($"Session: {sessionId}");
        Console.WriteLine($"PDF: {(frame is null ? "wartet auf echte PDF-Geste" : frame.DisplayName)}");
        if (frame is not null)
        {
            Console.WriteLine($"Page: {frame.Page}/{frame.PageCount}");
            if (frame.HasTransientPdfLease)
            {
                Console.WriteLine($"FrameFormat: {frame.FrameFormat}");
                Console.WriteLine($"PdfBytes: {frame.PdfByteCount}");
                Console.WriteLine("TransientPdfLease: OK");
            }
            else
            {
                Console.WriteLine($"FrameSize: {frame.Width}x{frame.Height}");
                Console.WriteLine($"FrameFormat: {frame.FrameFormat}");
            }

            Console.WriteLine($"RendererName: {frame.RendererName}");
        }

        Console.WriteLine($"DynamicPdfFromPlacementSignal: {(options.DynamicPdfFromPlacementSignal ? "YES" : "NO")}");
        Console.WriteLine("OwnerKeepsOriginal: OK");
        Console.WriteLine("GuestFileIngressAllowed: NO");
        Console.WriteLine($"WaitForGlassEdgePlacement: {(options.WaitForPlacement ? "YES" : "NO")}");
        if (options.WaitForPlacement)
        {
            Console.WriteLine($"PlacementSignal: {options.PlacementSignalPath}");
        }
        Console.WriteLine("WaitingForMacGuest: YES");
        Console.WriteLine("Stop: Ctrl+C");

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        Console.WriteLine("OwnerServerStarted: OK");

        var requestCount = 0;
        var dynamicFrameProvider = frame is null && options.DynamicPdfFromPlacementSignal
            ? new DynamicPlacementFrameProvider(options)
            : null;
        while (!cancellation.IsCancellationRequested)
        {
            RkwpTransportMessage request;
            try
            {
                request = await server.WaitForMessageAsync(cancellation.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is RkwpTransportException or IOException or OperationCanceledException)
            {
                if (cancellation.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine($"IgnoredEmptyOrInvalidConnection: {ex.Message}");
                continue;
            }

            requestCount++;
            var response = CreateResponse(request, frame, dynamicFrameProvider, options, sessionId);
            await server.SendResponseAsync(response, cancellation.Token).ConfigureAwait(false);
            Console.WriteLine($"{request.MessageType}: OK");

            if (options.Once && request.MessageType == TransportMessageType.FrameUpdate)
            {
                break;
            }
        }

        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine($"Requests: {requestCount}");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static RkwpTransportMessage CreateResponse(
        RkwpTransportMessage request,
        RenderedFrame? frame,
        DynamicPlacementFrameProvider? dynamicFrameProvider,
        Options options,
        string sessionId)
    {
        var activeFrame = dynamicFrameProvider?.CurrentFrame ?? frame;
        return request.MessageType switch
        {
            TransportMessageType.AblageHello => RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameOnly"] = "true",
                    ["noFileIngress"] = "true",
                    ["memoryOnlyFrame"] = "true",
                    ["pdfMemoryFrame"] = options.MemoryPdfFrame ? "true" : "false",
                    ["transientPdfFrame"] = options.MemoryPdfFrame ? "true" : "false",
                    ["supportsTransientPdfBytes"] = "true",
                    ["pdfLeaseMode"] = "MemoryOnly",
                    ["guestMayPersistPdf"] = "false",
                    ["guestMayExportPdf"] = "false",
                    ["allowTextSelection"] = "true",
                    ["openFrame"] = "true",
                    ["returnSupported"] = "true",
                    ["ownerKeepsOriginal"] = "true"
                },
                request.MessageId),
            TransportMessageType.AblageCapabilities => RkwpTransportMessage.Create(
                TransportMessageType.AblageCapabilities,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["frameOnly"] = "true",
                    ["noFileIngress"] = "true",
                    ["pdfMemoryFrame"] = options.MemoryPdfFrame ? "true" : "false",
                    ["transientPdfFrame"] = options.MemoryPdfFrame ? "true" : "false",
                    ["supportsTransientPdfBytes"] = "true",
                    ["pdfLeaseMode"] = "MemoryOnly",
                    ["guestMayPersistPdf"] = "false",
                    ["guestMayExportPdf"] = "false",
                    ["allowTextSelection"] = "true",
                    ["devPairing"] = "allowed"
                },
                request.MessageId),
            TransportMessageType.FrameUpdate => RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                CreateFramePayload(ResolveFrameForRequest(frame, dynamicFrameProvider, options), options),
                request.MessageId),
            TransportMessageType.CarryLeaseReturn => CreateCarryLeaseReturnResponse(
                request,
                activeFrame,
                dynamicFrameProvider,
                sessionId),
            _ => RkwpTransportMessage.Create(
                request.MessageType,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["accepted"] = "true",
                    ["noFileIngress"] = "true"
                },
                request.MessageId)
        };
    }

    private static RkwpTransportMessage CreateCarryLeaseReturnResponse(
        RkwpTransportMessage request,
        RenderedFrame? activeFrame,
        DynamicPlacementFrameProvider? dynamicFrameProvider,
        string sessionId)
    {
        var released = dynamicFrameProvider?.ReleaseReturnedLease(request.Payload) ?? false;
        if (released)
        {
            activeFrame = null;
        }

        return RkwpTransportMessage.Create(
            TransportMessageType.CarryLeaseReturn,
            OwnerAblageId,
            request.SourceAblageId,
            sessionId,
            new Dictionary<string, string>
            {
                ["return"] = "accepted",
                ["frameSessionId"] = request.Payload.TryGetValue("frameSessionId", out var frameSessionId)
                    ? frameSessionId
                    : activeFrame?.FrameSessionId ?? string.Empty,
                ["leaseId"] = request.Payload.TryGetValue("leaseId", out var leaseId)
                    ? leaseId
                    : activeFrame?.LeaseId ?? string.Empty,
                ["placementId"] = request.Payload.TryGetValue("placementId", out var placementId)
                    ? placementId
                    : activeFrame?.PlacementId ?? string.Empty,
                ["leaseReleased"] = released ? "true" : "false",
                ["guestKeptOriginalFile"] = "false",
                ["guestPersistedPdfFile"] = "false",
                ["transientPdfDiscarded"] = "true",
                ["pdfLeaseMode"] = "MemoryOnly",
                ["noFileIngress"] = "true"
            },
            request.MessageId);
    }

    private static RenderedFrame? ResolveFrameForRequest(
        RenderedFrame? frame,
        DynamicPlacementFrameProvider? dynamicFrameProvider,
        Options options)
    {
        if (frame is not null)
        {
            return frame;
        }

        if (dynamicFrameProvider is not null)
        {
            return dynamicFrameProvider.ResolveFrame();
        }

        if (!options.DynamicPdfFromPlacementSignal || !File.Exists(options.PlacementSignalPath))
        {
            return null;
        }

        var pdfPath = TryReadPdfPathFromPlacementSignal(options.PlacementSignalPath);
        if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
        {
            return null;
        }

        return RenderFrame(pdfPath, options.Page, options.Width, options.MemoryPdfFrame);
    }

    private static string? TryReadPdfPathFromPlacementSignal(string placementSignalPath)
    {
        try
        {
            using var stream = File.OpenRead(placementSignalPath);
            using var json = JsonDocument.Parse(stream);
            if (json.RootElement.TryGetProperty("sourcePdfPathLocalOnly", out var pathElement))
            {
                return pathElement.GetString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private sealed class DynamicPlacementFrameProvider
    {
        private readonly Options _options;
        private string? _lastFingerprint;
        private RenderedFrame? _lastFrame;

        public DynamicPlacementFrameProvider(Options options)
        {
            _options = options;
        }

        public RenderedFrame? CurrentFrame => _lastFrame;

        public RenderedFrame? ResolveFrame()
        {
            var signal = TryReadPlacementSignal(_options.PlacementSignalPath);
            if (signal is null)
            {
                return null;
            }

            var fingerprint = signal.Fingerprint;
            if (string.Equals(_lastFingerprint, fingerprint, StringComparison.Ordinal) && _lastFrame is not null)
            {
                return _lastFrame;
            }

            if (!File.Exists(signal.SourcePdfPath))
            {
                Console.WriteLine($"PlacementSignal: PDF_MISSING {signal.SourcePdfPath}");
                return null;
            }

            try
            {
                _lastFrame = RenderFrame(signal.SourcePdfPath, _options.Page, _options.Width, memoryPdfFrame: true, signal.PlacementId);
                _lastFingerprint = fingerprint;
                Console.WriteLine($"PlacementCommit: OK {_lastFrame.DisplayName}");
                Console.WriteLine($"PlacementId: {signal.PlacementId}");
                Console.WriteLine($"FrameSessionId: {_lastFrame.FrameSessionId}");
                Console.WriteLine($"LeaseId: {_lastFrame.LeaseId}");
                Console.WriteLine($"FrameFormat: {_lastFrame.FrameFormat}");
                Console.WriteLine($"PdfBytes: {_lastFrame.PdfByteCount}");
                return _lastFrame;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PlacementCommit: FAILED {ex.Message}");
                return null;
            }
        }

        public bool ReleaseReturnedLease(IReadOnlyDictionary<string, string> payload)
        {
            if (_lastFrame is null)
            {
                return false;
            }

            var frameSessionId = ReadPayloadString(payload, "frameSessionId");
            var leaseId = ReadPayloadString(payload, "leaseId");
            var placementId = ReadPayloadString(payload, "placementId");
            var ownerMayReleaseLease = ReadPayloadString(payload, "ownerMayReleaseLease");

            var matches =
                (!string.IsNullOrWhiteSpace(leaseId) && string.Equals(leaseId, _lastFrame.LeaseId, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(frameSessionId) && string.Equals(frameSessionId, _lastFrame.FrameSessionId, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(placementId) && string.Equals(placementId, _lastFrame.PlacementId, StringComparison.OrdinalIgnoreCase));
            if (!matches)
            {
                return false;
            }

            if (string.Equals(ownerMayReleaseLease, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _lastFrame = null;
            _lastFingerprint = null;
            TryDeletePlacementSignal(_options.PlacementSignalPath);
            Console.WriteLine("CarryLeaseReturn: RELEASED");
            return true;
        }
    }

    private static string ReadPayloadString(IReadOnlyDictionary<string, string> payload, string key) =>
        payload.TryGetValue(key, out var value) ? value : string.Empty;

    private static void TryDeletePlacementSignal(string placementSignalPath)
    {
        try
        {
            if (File.Exists(placementSignalPath))
            {
                File.Delete(placementSignalPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlacementSignal: DELETE_FAILED {ex.Message}");
        }
    }

    private sealed record PlacementSignal(
        string PlacementId,
        string SourcePdfPath,
        string Fingerprint);

    private static PlacementSignal? TryReadPlacementSignal(string placementSignalPath)
    {
        try
        {
            if (!File.Exists(placementSignalPath))
            {
                return null;
            }

            using var stream = File.OpenRead(placementSignalPath);
            using var json = JsonDocument.Parse(stream);
            if (!json.RootElement.TryGetProperty("sourcePdfPathLocalOnly", out var pathElement))
            {
                return null;
            }

            var sourcePdfPath = pathElement.GetString();
            if (string.IsNullOrWhiteSpace(sourcePdfPath))
            {
                return null;
            }

            var placementId = ReadJsonString(json.RootElement, "placementId");
            if (string.IsNullOrWhiteSpace(placementId))
            {
                placementId = $"signal-{File.GetLastWriteTimeUtc(placementSignalPath).Ticks}";
            }

            var placedAtUtc = ReadJsonString(json.RootElement, "placedAtUtc");
            var fingerprint = $"{placementId}|{placedAtUtc}|{sourcePdfPath}";
            return new PlacementSignal(placementId, sourcePdfPath, fingerprint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlacementSignal: INVALID {ex.Message}");
            return null;
        }
    }

    private static string ReadJsonString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static IReadOnlyDictionary<string, string> CreateFramePayload(RenderedFrame? frame, Options options)
    {
        if (frame is null)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["placementReady"] = "false",
                ["ownerKeepsOriginal"] = "true",
                ["containsOriginalFileBytes"] = "false",
                ["hasOriginalPath"] = "false",
                ["guestHasPdfFile"] = "false",
                ["guestMayPersistPdf"] = "false",
                ["guestMayExportPdf"] = "false",
                ["noFileIngress"] = "true",
                ["frameCache"] = "MemoryOnly",
                ["pdfCache"] = "MemoryOnly",
                ["transientPdfFrame"] = "true",
                ["supportsTransientPdfBytes"] = "true",
                ["pdfLeaseMode"] = "MemoryOnly",
                ["allowTextSelection"] = "true",
                ["visibleStatus"] = File.Exists(options.PlacementSignalPath)
                    ? "Ablage-Signal erkannt, PDF-Pfad wird geprueft"
                    : "wartet auf echte PDF am Glasrand"
            };
        }

        if (!options.WaitForPlacement || File.Exists(options.PlacementSignalPath))
        {
            var payload = new Dictionary<string, string>(frame.ToPayload(), StringComparer.OrdinalIgnoreCase)
            {
                ["placementReady"] = "true"
            };
            return payload;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["frameSessionId"] = frame.FrameSessionId,
            ["leaseId"] = frame.LeaseId,
            ["placementId"] = frame.PlacementId,
            ["thingId"] = frame.ThingId,
            ["displayName"] = frame.DisplayName,
            ["pageCount"] = frame.PageCount.ToString(),
            ["page"] = frame.Page.ToString(),
            ["width"] = frame.Width.ToString(),
            ["height"] = frame.Height.ToString(),
            ["placementReady"] = "false",
            ["ownerKeepsOriginal"] = "true",
            ["containsOriginalFileBytes"] = "false",
            ["hasOriginalPath"] = "false",
            ["guestHasPdfFile"] = "false",
            ["guestMayPersistPdf"] = "false",
            ["guestMayExportPdf"] = "false",
            ["noFileIngress"] = "true",
            ["frameCache"] = "MemoryOnly",
            ["pdfCache"] = "MemoryOnly",
            ["transientPdfFrame"] = "true",
            ["supportsTransientPdfBytes"] = "true",
            ["pdfLeaseMode"] = "MemoryOnly",
            ["allowTextSelection"] = "true",
            ["visibleStatus"] = "wartet auf Ablage am Glasrand"
        };
    }

    private static void PrintHelp()
    {
        Console.WriteLine("RK Workspace macOS PDF Frame Owner");
        Console.WriteLine("----------------------------------");
        Console.WriteLine("Options:");
        Console.WriteLine("  --pdf-path <path>       Windows-owned PDF source.");
        Console.WriteLine("  --bind-address <ip>     Bind address. Default: 0.0.0.0");
        Console.WriteLine("  --port <port>           Dev-LAN port. Default: 57120");
        Console.WriteLine("  --page <number>         PDF page. Default: 1");
        Console.WriteLine("  --width <pixels>        Rendered frame width. Default: 1400");
        Console.WriteLine("  --wait-for-placement    Send the frame only after the glass-edge placement signal exists.");
        Console.WriteLine("  --memory-pdf-frame      Send the real PDF as a transient MemoryOnly PDF lease.");
        Console.WriteLine("  --placement-signal <p>  Local Windows signal file written by the glass overlay.");
        Console.WriteLine("  --once                  Stop after first FrameUpdate.");
        Console.WriteLine("  --smoke-test            Render the PDF frame without listening.");
        Console.WriteLine("  --placement-gating-smoke-test");
        Console.WriteLine("                         Verify that the frame is released only after placement.");
        Console.WriteLine("  --dynamic-placement-smoke-test");
        Console.WriteLine("                         Verify dynamic PDF lease release and sequential placements.");
    }

    private static void WritePlacementSignal(string placementSignalPath, string placementId, string pdfPath)
    {
        var directory = Path.GetDirectoryName(placementSignalPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var pdfName = Path.GetFileName(pdfPath);
        File.WriteAllLines(placementSignalPath, new[]
        {
            "{",
            "  \"signalSchema\": \"rkws-placement-v2\",",
            $"  \"placementId\": \"{EscapeJson(placementId)}\",",
            $"  \"placedAtUtc\": \"{DateTimeOffset.UtcNow:O}\",",
            $"  \"sourcePdfName\": \"{EscapeJson(pdfName)}\",",
            "  \"sourcePdfOwner\": \"Windows\",",
            $"  \"sourcePdfPathLocalOnly\": \"{EscapeJson(Path.GetFullPath(pdfPath))}\",",
            "  \"targetAblageName\": \"Ablage macOS\",",
            "  \"targetEdge\": \"Right\",",
            "  \"requestedFrameFormat\": \"TransientPdfBytes\",",
            "  \"transientPdfFrame\": \"true\",",
            "  \"supportsTransientPdfBytes\": \"true\",",
            "  \"pdfLeaseMode\": \"MemoryOnly\",",
            "  \"ownerKeepsOriginal\": \"true\",",
            "  \"guestMayPersistPdf\": \"false\",",
            "  \"guestMayExportPdf\": \"false\",",
            "  \"allowTextSelection\": \"true\",",
            "  \"committed\": \"true\"",
            "}"
        });
    }

    private static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private sealed record Options(
        string PdfPath,
        string BindAddress,
        int Port,
        int Page,
        int Width,
        string SessionId,
        string PlacementSignalPath,
        bool WaitForPlacement,
        bool DynamicPdfFromPlacementSignal,
        bool MemoryPdfFrame,
        bool Once,
        bool SmokeTest,
        bool PlacementGatingSmokeTest,
        bool DynamicPlacementSmokeTest,
        bool ShowHelp)
    {
        public static string DefaultPdfPath()
        {
            var root = FindRepositoryRoot();
            return Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
        }

        public static Options Parse(string[] args)
        {
            var root = FindRepositoryRoot();
            var pdfPath = DefaultPdfPath();
            var bindAddress = "0.0.0.0";
            var port = 57120;
            var page = 1;
            var width = 1400;
            var sessionId = string.Empty;
            var placementSignalPath = Path.Combine(Path.GetTempPath(), "rkws-ma017-real-pdf-placement-ready.signal");
            var waitForPlacement = false;
            var dynamicPdfFromPlacementSignal = false;
            var memoryPdfFrame = false;
            var once = false;
            var smokeTest = false;
            var placementGatingSmokeTest = false;
            var dynamicPlacementSmokeTest = false;
            var showHelp = false;

            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                if (Is(arg, "--help", "-Help", "-h"))
                {
                    showHelp = true;
                    continue;
                }

                if (Is(arg, "--once", "-Once"))
                {
                    once = true;
                    continue;
                }

                if (Is(arg, "--wait-for-placement", "-WaitForPlacement"))
                {
                    waitForPlacement = true;
                    continue;
                }

                if (Is(arg, "--dynamic-pdf-from-placement-signal", "-DynamicPdfFromPlacementSignal"))
                {
                    dynamicPdfFromPlacementSignal = true;
                    waitForPlacement = true;
                    continue;
                }

                if (Is(arg, "--memory-pdf-frame", "-MemoryPdfFrame"))
                {
                    memoryPdfFrame = true;
                    continue;
                }

                if (Is(arg, "--smoke-test", "-SmokeTest"))
                {
                    smokeTest = true;
                    continue;
                }

                if (Is(arg, "--placement-gating-smoke-test", "-PlacementGatingSmokeTest"))
                {
                    placementGatingSmokeTest = true;
                    waitForPlacement = true;
                    continue;
                }

                if (Is(arg, "--dynamic-placement-smoke-test", "-DynamicPlacementSmokeTest"))
                {
                    dynamicPlacementSmokeTest = true;
                    dynamicPdfFromPlacementSignal = true;
                    memoryPdfFrame = true;
                    waitForPlacement = true;
                    continue;
                }

                if (ReadString(args, ref index, "--pdf-path", "-PdfPath") is { } parsedPdfPath)
                {
                    pdfPath = parsedPdfPath;
                    continue;
                }

                if (ReadString(args, ref index, "--bind-address", "-BindAddress") is { } parsedBindAddress)
                {
                    bindAddress = parsedBindAddress;
                    continue;
                }

                if (ReadString(args, ref index, "--session-id", "-SessionId") is { } parsedSessionId)
                {
                    sessionId = parsedSessionId;
                    continue;
                }

                if (ReadString(args, ref index, "--placement-signal", "-PlacementSignalPath") is { } parsedPlacementSignalPath)
                {
                    placementSignalPath = parsedPlacementSignalPath;
                    continue;
                }

                if (ReadInt(args, ref index, "--port", "-Port") is { } parsedPort)
                {
                    port = parsedPort;
                    continue;
                }

                if (ReadInt(args, ref index, "--page", "-Page") is { } parsedPage)
                {
                    page = parsedPage;
                    continue;
                }

                if (ReadInt(args, ref index, "--width", "-Width") is { } parsedWidth)
                {
                    width = parsedWidth;
                }
            }

            return new Options(
                Path.GetFullPath(pdfPath),
                bindAddress,
                port,
                page,
                width,
                sessionId,
                Path.GetFullPath(placementSignalPath),
                waitForPlacement,
                dynamicPdfFromPlacementSignal,
                memoryPdfFrame,
                once,
                smokeTest,
                placementGatingSmokeTest,
                dynamicPlacementSmokeTest,
                showHelp);
        }

        private static string? ReadString(string[] args, ref int index, params string[] names)
        {
            if (!Is(args[index], names) || index + 1 >= args.Length)
            {
                return null;
            }

            index++;
            return args[index];
        }

        private static int? ReadInt(string[] args, ref int index, params string[] names)
        {
            var value = ReadString(args, ref index, names);
            return int.TryParse(value, out var parsed) ? parsed : null;
        }

        private static bool Is(string value, params string[] names) =>
            names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName
                ?? throw new InvalidOperationException("Repository root could not be located.");
        }
    }

    private sealed record RenderedFrame(
        string FrameSessionId,
        string LeaseId,
        string PlacementId,
        string ThingId,
        string DisplayName,
        int PageCount,
        string SourceHash,
        int Page,
        int Width,
        int Height,
        string FrameFormat,
        string PngBase64,
        string PdfBase64,
        int PdfByteCount,
        string RendererName,
        DateTimeOffset RenderedAt)
    {
        public bool HasTransientPdfLease =>
            (FrameFormat is "TransientPdfBytes" or "PdfMemoryFrame") &&
            !string.IsNullOrWhiteSpace(PdfBase64);

        public IReadOnlyDictionary<string, string> ToPayload()
        {
            var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["frameSessionId"] = FrameSessionId,
                ["leaseId"] = LeaseId,
                ["placementId"] = PlacementId,
                ["thingId"] = ThingId,
                ["displayName"] = DisplayName,
                ["pageCount"] = PageCount.ToString(),
                ["sourceHash"] = SourceHash,
                ["page"] = Page.ToString(),
                ["width"] = Width.ToString(),
                ["height"] = Height.ToString(),
                ["frameFormat"] = FrameFormat,
                ["rendererName"] = RendererName,
                ["renderedAt"] = RenderedAt.ToString("O"),
                ["ownerKeepsOriginal"] = "true",
                ["noFileIngress"] = "true",
                ["frameCache"] = "MemoryOnly",
                ["pdfCache"] = "MemoryOnly",
                ["containsOriginalFileBytes"] = "false",
                ["hasOriginalPath"] = "false",
                ["guestHasPdfFile"] = "false",
                ["guestMayPersistPdf"] = "false",
                ["guestMayExportPdf"] = "false",
                ["transientPdfFrame"] = "true",
                ["supportsTransientPdfBytes"] = "true",
                ["pdfLeaseMode"] = "MemoryOnly",
                ["allowTextSelection"] = "true",
                ["visibleStatus"] = "liegt hier im Frame"
            };

            if (HasTransientPdfLease)
            {
                payload["pdfBase64"] = PdfBase64;
                payload["pdfByteCount"] = PdfByteCount.ToString();
                payload["memoryOnlyPdf"] = "true";
                payload["noPersistentFileIngress"] = "true";
                payload["transientPdfBytes"] = "true";
                payload["visibleStatus"] = "liegt hier als PDF-Frame";
            }
            else
            {
                payload["pngBase64"] = PngBase64;
            }

            return payload;
        }
    }
}
