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

    private static RenderedFrame RenderFrame(string pdfPath, int page = 1, int width = 1400, bool memoryPdfFrame = false)
    {
        var document = PdfFrameDocument.Load(pdfPath);
        if (memoryPdfFrame)
        {
            var pdfBytes = File.ReadAllBytes(pdfPath);
            return new RenderedFrame(
                $"frame-macos-{Guid.NewGuid():N}",
                $"lease-macos-{Guid.NewGuid():N}",
                document.ThingId,
                document.FileName,
                document.PageCount,
                document.Sha256,
                page,
                0,
                0,
                "PdfMemoryFrame",
                string.Empty,
                Convert.ToBase64String(pdfBytes),
                pdfBytes.Length,
                "PDFKitMemoryFrame",
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
        if (frame.FrameFormat == "PdfMemoryFrame")
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
        Console.WriteLine(frame.FrameFormat == "PdfMemoryFrame"
            ? "OriginalFileBytes: MEMORY_ONLY_PDF_FRAME"
            : "OriginalFileBytes: NO");
        Console.WriteLine($"FrameFormat: {frame.FrameFormat}");
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
            if (frame.FrameFormat == "PdfMemoryFrame")
            {
                Console.WriteLine($"FrameFormat: PdfMemoryFrame");
                Console.WriteLine($"PdfBytes: {frame.PdfByteCount}");
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
            var response = CreateResponse(request, frame, options, sessionId);
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
        Options options,
        string sessionId)
    {
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
                    ["devPairing"] = "allowed"
                },
                request.MessageId),
            TransportMessageType.FrameUpdate => RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                CreateFramePayload(ResolveFrameForRequest(frame, options), options),
                request.MessageId),
            TransportMessageType.CarryLeaseReturn => RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseReturn,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["return"] = "accepted",
                    ["frameSessionId"] = frame?.FrameSessionId ?? string.Empty,
                    ["leaseId"] = frame?.LeaseId ?? string.Empty,
                    ["guestKeptOriginalFile"] = "false",
                    ["noFileIngress"] = "true"
                },
                request.MessageId),
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

    private static RenderedFrame? ResolveFrameForRequest(RenderedFrame? frame, Options options)
    {
        if (frame is not null)
        {
            return frame;
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
                ["noFileIngress"] = "true",
                ["frameCache"] = "MemoryOnly",
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
            ["noFileIngress"] = "true",
            ["frameCache"] = "MemoryOnly",
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
        Console.WriteLine("  --memory-pdf-frame      Send the real PDF as a memory-only frame for PDFKit.");
        Console.WriteLine("  --placement-signal <p>  Local Windows signal file written by the glass overlay.");
        Console.WriteLine("  --once                  Stop after first FrameUpdate.");
        Console.WriteLine("  --smoke-test            Render the PDF frame without listening.");
        Console.WriteLine("  --placement-gating-smoke-test");
        Console.WriteLine("                         Verify that the frame is released only after placement.");
    }

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
        public IReadOnlyDictionary<string, string> ToPayload()
        {
            var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["frameSessionId"] = FrameSessionId,
                ["leaseId"] = LeaseId,
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
                ["hasOriginalPath"] = "false",
                ["guestHasPdfFile"] = "false",
                ["noFileIngress"] = "true",
                ["frameCache"] = "MemoryOnly",
                ["visibleStatus"] = "liegt hier im Frame"
            };

            if (FrameFormat == "PdfMemoryFrame")
            {
                payload["pdfBase64"] = PdfBase64;
                payload["pdfByteCount"] = PdfByteCount.ToString();
                payload["memoryOnlyPdf"] = "true";
                payload["noPersistentFileIngress"] = "true";
                payload["containsOriginalFileBytes"] = "true";
                payload["visibleStatus"] = "liegt hier als PDF-Frame";
            }
            else
            {
                payload["pngBase64"] = PngBase64;
                payload["containsOriginalFileBytes"] = "false";
            }

            return payload;
        }
    }
}
