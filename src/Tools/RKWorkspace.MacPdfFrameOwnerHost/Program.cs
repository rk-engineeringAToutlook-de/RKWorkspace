using RKWorkspace.Frame.Pdf;
using RKWorkspace.RkwpTransport.DevLan;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Rkwp;

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

            var frame = RenderFrame(options);
            if (options.SmokeTest)
            {
                PrintSmoke(frame);
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

    private static RenderedFrame RenderFrame(Options options)
    {
        var document = PdfFrameDocument.Load(options.PdfPath);
        var renderer = PdfFrameRendererFactory.CreateDefault();
        var render = renderer.Render(new PdfFrameRenderRequest(
            document,
            options.Page,
            new PdfFrameRenderOptions(RequestedWidth: options.Width),
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
            Convert.ToBase64String(render.PixelData),
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
        Console.WriteLine($"FrameSize: {frame.Width}x{frame.Height}");
        Console.WriteLine($"RendererName: {frame.RendererName}");
        Console.WriteLine("OwnerKeepsOriginal: OK");
        Console.WriteLine("GuestHasPdfFile: NO");
        Console.WriteLine("GuestHasOriginalPath: NO");
        Console.WriteLine("OriginalFileBytes: NO");
        Console.WriteLine("FrameFormat: PngFrame");
        Console.WriteLine("FrameCache: MemoryOnly");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
    }

    private static async Task<int> RunOwnerAsync(Options options, RenderedFrame frame)
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
        Console.WriteLine($"PDF: {frame.DisplayName}");
        Console.WriteLine($"Page: {frame.Page}/{frame.PageCount}");
        Console.WriteLine($"FrameSize: {frame.Width}x{frame.Height}");
        Console.WriteLine($"RendererName: {frame.RendererName}");
        Console.WriteLine("OwnerKeepsOriginal: OK");
        Console.WriteLine("GuestFileIngressAllowed: NO");
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
            var response = CreateResponse(request, frame, sessionId);
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
        RenderedFrame frame,
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
                    ["devPairing"] = "allowed"
                },
                request.MessageId),
            TransportMessageType.FrameUpdate => RkwpTransportMessage.Create(
                TransportMessageType.FrameUpdate,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                frame.ToPayload(),
                request.MessageId),
            TransportMessageType.CarryLeaseReturn => RkwpTransportMessage.Create(
                TransportMessageType.CarryLeaseReturn,
                OwnerAblageId,
                request.SourceAblageId,
                sessionId,
                new Dictionary<string, string>
                {
                    ["return"] = "accepted",
                    ["frameSessionId"] = frame.FrameSessionId,
                    ["leaseId"] = frame.LeaseId,
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
        Console.WriteLine("  --once                  Stop after first FrameUpdate.");
        Console.WriteLine("  --smoke-test            Render the PDF frame without listening.");
    }

    private sealed record Options(
        string PdfPath,
        string BindAddress,
        int Port,
        int Page,
        int Width,
        string SessionId,
        bool Once,
        bool SmokeTest,
        bool ShowHelp)
    {
        public static Options Parse(string[] args)
        {
            var root = FindRepositoryRoot();
            var pdfPath = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
            var bindAddress = "0.0.0.0";
            var port = 57120;
            var page = 1;
            var width = 1400;
            var sessionId = string.Empty;
            var once = false;
            var smokeTest = false;
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

                if (Is(arg, "--smoke-test", "-SmokeTest"))
                {
                    smokeTest = true;
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
                once,
                smokeTest,
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
        string PngBase64,
        string RendererName,
        DateTimeOffset RenderedAt)
    {
        public IReadOnlyDictionary<string, string> ToPayload()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
                ["frameFormat"] = "PngFrame",
                ["pngBase64"] = PngBase64,
                ["rendererName"] = RendererName,
                ["renderedAt"] = RenderedAt.ToString("O"),
                ["ownerKeepsOriginal"] = "true",
                ["containsOriginalFileBytes"] = "false",
                ["hasOriginalPath"] = "false",
                ["guestHasPdfFile"] = "false",
                ["noFileIngress"] = "true",
                ["frameCache"] = "MemoryOnly",
                ["visibleStatus"] = "liegt hier im Frame"
            };
        }
    }
}
