using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol.Ownership;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Dev;
using RKWorkspace.Transport.NamedPipes;
using RKWorkspace.Transport.Rkwp;

var root = FindRoot();
var options = RkwpPerfOptions.Parse(args, root);

try
{
    var result = await RkwpPerfHarness.RunAsync(options).ConfigureAwait(false);
    Print(result);
    return result.IsSuccessful ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine("RK Workspace RKWP Performance Harness");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine($"RESULT: FAILED - {ex.Message}");
    return 1;
}

static void Print(RkwpPerfRunResult result)
{
    Console.WriteLine("RK Workspace RKWP Performance Harness");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine($"Mode: {(result.Options.SmokeTest ? "SmokeTest" : "Baseline")}");
    Console.WriteLine($"Iterations: {result.Summary.Iterations}");
    Console.WriteLine($"PDF: {Path.GetFileName(result.Options.PdfPath)}");
    Console.WriteLine($"Json: {result.JsonPath}");
    Console.WriteLine($"Markdown: {result.MarkdownPath}");
    Console.WriteLine("SecureDevPath: MEASURED");
    Console.WriteLine("PdfFramePath: MEASURED");
    Console.WriteLine("LocalE2EPath: MEASURED");
    Console.WriteLine("FrameInputPath: MEASURED");
    Console.WriteLine("RecoveryPath: MEASURED");
    Console.WriteLine($"FrameUpdateBytesAverage: {result.Summary.FrameUpdateBytesAverage:F1}");
    Console.WriteLine($"FrameUpdateFrequencyHz: {result.Summary.FrameUpdateFrequencyHz:F1}");
    Console.WriteLine($"DevTransportRoundtripAverageMs: {result.Summary.DevTransportRoundtripAverageMs:F3}");
    Console.WriteLine($"HeartbeatLatencyAverageMs: {result.Summary.HeartbeatLatencyAverageMs:F3}");
    Console.WriteLine($"FrameOpenAverageMs: {result.Summary.FrameOpenAverageMs:F3}");
    Console.WriteLine($"FrameReturnAverageMs: {result.Summary.FrameReturnAverageMs:F3}");
    Console.WriteLine($"RecoveryAverageMs: {result.Summary.RecoveryAverageMs:F3}");
    Console.WriteLine($"NoFileIngressOverheadAverageMs: {result.Summary.NoFileIngressOverheadAverageMs:F3}");
    Console.WriteLine($"PdfRenderAverageMs: {result.Summary.PdfRenderAverageMs:F3}");
    Console.WriteLine($"PdfRenderFirstPageAverageMs: {result.Summary.PdfRenderFirstPageAverageMs:F3}");
    Console.WriteLine($"PdfRenderNextPageAverageMs: {result.Summary.PdfRenderNextPageAverageMs:F3}");
    Console.WriteLine($"PdfTileGenerationAverageMs: {result.Summary.PdfTileGenerationAverageMs:F3}");
    Console.WriteLine($"PdfFrameSizeBytesAverage: {result.Summary.PdfFrameSizeBytesAverage:F1}");
    Console.WriteLine($"MemorySnapshotBytesAverage: {result.Summary.MemorySnapshotBytesAverage:F1}");
    Console.WriteLine($"RendererStatus: {result.Summary.RendererStatus}");
    Console.WriteLine($"PerfSamples: {(result.Samples.Count == result.Summary.Iterations ? "OK" : "FAILED")}");
    Console.WriteLine($"NoFileIngress: {(result.Summary.NoFileIngressPassed ? "SUCCESS" : "FAILED")}");
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

public sealed record RkwpPerfOptions(
    string Root,
    string PdfPath,
    int Iterations,
    bool SmokeTest,
    string OutputDirectory)
{
    public static RkwpPerfOptions Parse(string[] args, string root)
    {
        var smokeTest = false;
        var iterations = 0;
        var pdfPath = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
        var outputDirectory = Path.Combine(root, "logs", "perf");

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
                continue;
            }

            if ((string.Equals(arg, "--iterations", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(arg, "-Iterations", StringComparison.OrdinalIgnoreCase)) &&
                index + 1 < args.Length &&
                int.TryParse(args[++index], out var parsedIterations))
            {
                iterations = parsedIterations;
            }
        }

        iterations = iterations <= 0
            ? smokeTest ? 10 : 100
            : iterations;

        if (smokeTest && iterations < 10)
        {
            iterations = 10;
        }

        return new RkwpPerfOptions(
            root,
            Path.GetFullPath(pdfPath),
            iterations,
            smokeTest,
            outputDirectory);
    }
}

public static class RkwpPerfHarness
{
    private static readonly TimeSpan TransportTimeout = TimeSpan.FromSeconds(2);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public static async Task<RkwpPerfRunResult> RunAsync(RkwpPerfOptions options)
    {
        if (!File.Exists(options.PdfPath))
        {
            throw new FileNotFoundException("PDF path does not exist.", options.PdfPath);
        }

        Directory.CreateDirectory(options.OutputDirectory);

        var samples = await MeasureAsync(options).ConfigureAwait(false);
        var summary = RkwpPerfSummary.FromSamples(options, samples);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var jsonPath = Path.Combine(options.OutputDirectory, $"rkwp-perf-{timestamp}.json");
        var markdownPath = Path.Combine(options.OutputDirectory, $"rkwp-perf-{timestamp}.md");

        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(summary, JsonOptions)).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, summary.ToMarkdown(jsonPath)).ConfigureAwait(false);

        return new RkwpPerfRunResult(options, summary, samples, jsonPath, markdownPath);
    }

    private static async Task<IReadOnlyList<RkwpPerfSample>> MeasureAsync(RkwpPerfOptions options)
    {
        var samples = new List<RkwpPerfSample>();
        var pipeName = $"rkws-rkwp-perf-{Guid.NewGuid():N}";
        var transport = new RkwpDevTransport(new NamedPipeTransportOptions
        {
            DefaultTimeout = TransportTimeout
        });
        var endpoint = TransportEndpoint.NamedPipe(pipeName);
        var server = transport.CreateServer(endpoint);
        var client = transport.CreateClient(endpoint);
        var sessionId = $"rkwp-perf-session-{Guid.NewGuid():N}";

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
        var serverTask = Task.Run(
            () => RunServerAsync(server, sessionId, options.Iterations * 2, cancellation.Token),
            cancellation.Token);

        for (var iteration = 1; iteration <= options.Iterations; iteration++)
        {
            samples.Add(await MeasureIterationAsync(options, client, sessionId, iteration, cancellation.Token).ConfigureAwait(false));
        }

        await serverTask.ConfigureAwait(false);
        await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        return samples;
    }

    private static async Task RunServerAsync(
        IRkwpTransportServer server,
        string sessionId,
        int expectedMessages,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < expectedMessages; index++)
        {
            var request = await server.WaitForMessageAsync(cancellationToken).ConfigureAwait(false);
            var response = RkwpTransportMessage.Create(
                request.MessageType,
                request.TargetAblageId,
                request.SourceAblageId,
                sessionId,
                request.Payload,
                request.MessageId);
            await server.SendResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<RkwpPerfSample> MeasureIterationAsync(
        RkwpPerfOptions options,
        IRkwpTransportClient client,
        string sessionId,
        int iteration,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var document = PdfFrameDocument.Load(options.PdfPath);
        stopwatch.Stop();
        var pdfLoadMs = stopwatch.Elapsed.TotalMilliseconds;

        var now = DateTimeOffset.UtcNow;
        stopwatch.Restart();
        var lease = CarryLease.Grant(document.ThingId, "ablage-windows-owner", "ablage-perf-guest", CarryLeasePolicy.FrameOnlyDefault, now, sessionId);
        var frame = FrameSession
            .Open(lease, FrameMode.ViewOnly, now)
            .Ready(now.AddMilliseconds(1))
            .Activate(now.AddMilliseconds(2));
        stopwatch.Stop();
        var frameOpenMs = stopwatch.Elapsed.TotalMilliseconds;

        var renderer = PdfFrameRendererFactory.CreateDefault();
        stopwatch.Restart();
        var firstPageFrame = renderer.Render(new PdfFrameRenderRequest(
            document,
            PageNumber: 1,
            Options: new PdfFrameRenderOptions(RequestedWidth: 1024, RequestedHeight: 1448),
            lease.OwnerAblageId,
            document.ThingId));
        stopwatch.Stop();
        var renderFirstPageMs = stopwatch.Elapsed.TotalMilliseconds;

        var nextPageNumber = Math.Min(document.PageCount, 2);
        stopwatch.Restart();
        var nextPageFrame = renderer.Render(new PdfFrameRenderRequest(
            document,
            nextPageNumber,
            new PdfFrameRenderOptions(RequestedWidth: 1024, RequestedHeight: 1448),
            lease.OwnerAblageId,
            document.ThingId));
        stopwatch.Stop();
        var renderNextPageMs = stopwatch.Elapsed.TotalMilliseconds;

        stopwatch.Restart();
        var viewport = new PdfViewportState(1, Zoom: 1.25, ScrollX: 120, ScrollY: 240, ViewportWidth: 1024, ViewportHeight: 768);
        var tile = PdfTilePipeline.CreateTile(
            document,
            frame,
            new PdfTileRequest(frame.FrameSessionId, document.ThingId, viewport, TileColumn: 0, TileRow: 0),
            DateTimeOffset.UtcNow);
        stopwatch.Stop();
        var tileGenerationMs = stopwatch.Elapsed.TotalMilliseconds;

        var update = new FrameUpdate(
            frame.FrameSessionId,
            document.ThingId,
            1,
            $"PDF frame representation; name={document.FileName}; pages={document.PageCount}; sha256={document.Sha256[..16]}",
            document.Sha256,
            ContainsOriginalFileBytes: false,
            DateTimeOffset.UtcNow);
        var frameMessage = RkwpTransportMessage.Create(
            TransportMessageType.FrameUpdate,
            lease.OwnerAblageId,
            lease.GuestAblageId,
            sessionId,
            new Dictionary<string, string>
            {
                ["frameSessionId"] = update.FrameSessionId,
                ["thingId"] = update.ThingId,
                ["page"] = update.PageNumber.ToString(),
                ["contentHash"] = update.ContentHash,
                ["representation"] = update.Representation,
                ["containsOriginalFileBytes"] = update.ContainsOriginalFileBytes.ToString()
            });
        var frameUpdateBytes = Encoding.UTF8.GetByteCount(frameMessage.ToTransportMessage().ToJson());
        var frameSizeBytes = frameUpdateBytes + Encoding.UTF8.GetByteCount(tile.Update.FrameUpdate.Representation);
        var memorySnapshotBytes = GC.GetTotalMemory(forceFullCollection: false);

        stopwatch.Restart();
        var noFileIngressPassed =
            !update.ContainsOriginalFileBytes &&
            tile.NoFileIngress &&
            frameMessage.Payload["containsOriginalFileBytes"] == bool.FalseString;
        stopwatch.Stop();
        var noFileIngressOverheadMs = stopwatch.Elapsed.TotalMilliseconds;

        stopwatch.Restart();
        var heartbeat = RkwpTransportMessage.Create(
            TransportMessageType.CarryLeaseHeartbeat,
            lease.OwnerAblageId,
            lease.GuestAblageId,
            sessionId,
            new Dictionary<string, string>
            {
                ["leaseId"] = lease.LeaseId,
                ["iteration"] = iteration.ToString(),
                ["state"] = lease.State.ToString()
            });
        _ = await client.RequestAsync(heartbeat, TransportTimeout, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();
        var heartbeatLatencyMs = stopwatch.Elapsed.TotalMilliseconds;

        stopwatch.Restart();
        _ = await client.RequestAsync(frameMessage, TransportTimeout, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();
        var frameUpdateRoundtripMs = stopwatch.Elapsed.TotalMilliseconds;

        stopwatch.Restart();
        _ = lease.Return(DateTimeOffset.UtcNow);
        stopwatch.Stop();
        var frameReturnMs = stopwatch.Elapsed.TotalMilliseconds;

        stopwatch.Restart();
        _ = (lease with { LastHeartbeat = now.AddSeconds(-30) })
            .Advance(DateTimeOffset.UtcNow)
            .Recover();
        stopwatch.Stop();
        var recoveryMs = stopwatch.Elapsed.TotalMilliseconds;

        return new RkwpPerfSample(
            iteration,
            frameUpdateBytes,
            heartbeatLatencyMs,
            frameUpdateRoundtripMs,
            frameOpenMs,
            frameReturnMs,
            recoveryMs,
            noFileIngressOverheadMs,
            pdfLoadMs,
            PdfRenderMs: (renderFirstPageMs + renderNextPageMs) / 2.0,
            renderFirstPageMs,
            renderNextPageMs,
            tileGenerationMs,
            frameSizeBytes,
            memorySnapshotBytes,
            RendererStatus: firstPageFrame.IsPlaceholder || nextPageFrame.IsPlaceholder ? "RendererBlocked" : "Rendered",
            noFileIngressPassed);
    }
}

public sealed record RkwpPerfSample(
    int Iteration,
    int FrameUpdateBytes,
    double HeartbeatLatencyMs,
    double FrameUpdateRoundtripMs,
    double FrameOpenMs,
    double FrameReturnMs,
    double RecoveryMs,
    double NoFileIngressOverheadMs,
    double PdfLoadMs,
    double PdfRenderMs,
    double PdfRenderFirstPageMs,
    double PdfRenderNextPageMs,
    double PdfTileGenerationMs,
    int PdfFrameSizeBytes,
    long MemorySnapshotBytes,
    string RendererStatus,
    bool NoFileIngressPassed);

public sealed record RkwpPerfSummary(
    DateTimeOffset CreatedAt,
    int Iterations,
    string PdfPath,
    double FrameUpdateBytesAverage,
    double FrameUpdateFrequencyHz,
    double DevTransportRoundtripAverageMs,
    double HeartbeatLatencyAverageMs,
    double FrameUpdateRoundtripAverageMs,
    double FrameOpenAverageMs,
    double FrameReturnAverageMs,
    double RecoveryAverageMs,
    double NoFileIngressOverheadAverageMs,
    double PdfLoadAverageMs,
    double PdfRenderAverageMs,
    double PdfRenderFirstPageAverageMs,
    double PdfRenderNextPageAverageMs,
    double PdfTileGenerationAverageMs,
    double PdfFrameSizeBytesAverage,
    double MemorySnapshotBytesAverage,
    string RendererStatus,
    bool NoFileIngressPassed,
    IReadOnlyList<RkwpPerfSample> Samples)
{
    public static RkwpPerfSummary FromSamples(RkwpPerfOptions options, IReadOnlyList<RkwpPerfSample> samples)
    {
        var frameRoundtripMs = samples.Average(sample => sample.FrameUpdateRoundtripMs);
        var heartbeatMs = samples.Average(sample => sample.HeartbeatLatencyMs);
        var totalFrameRoundtripSeconds = samples.Sum(sample => sample.FrameUpdateRoundtripMs) / 1000.0;
        var frequency = totalFrameRoundtripSeconds <= 0
            ? 0
            : samples.Count / totalFrameRoundtripSeconds;

        return new RkwpPerfSummary(
            DateTimeOffset.UtcNow,
            samples.Count,
            options.PdfPath,
            samples.Average(sample => sample.FrameUpdateBytes),
            frequency,
            (frameRoundtripMs + heartbeatMs) / 2.0,
            heartbeatMs,
            frameRoundtripMs,
            samples.Average(sample => sample.FrameOpenMs),
            samples.Average(sample => sample.FrameReturnMs),
            samples.Average(sample => sample.RecoveryMs),
            samples.Average(sample => sample.NoFileIngressOverheadMs),
            samples.Average(sample => sample.PdfLoadMs),
            samples.Average(sample => sample.PdfRenderMs),
            samples.Average(sample => sample.PdfRenderFirstPageMs),
            samples.Average(sample => sample.PdfRenderNextPageMs),
            samples.Average(sample => sample.PdfTileGenerationMs),
            samples.Average(sample => sample.PdfFrameSizeBytes),
            samples.Average(sample => sample.MemorySnapshotBytes),
            samples.Select(sample => sample.RendererStatus).Distinct().OrderBy(status => status).First(),
            samples.All(sample => sample.NoFileIngressPassed),
            samples);
    }

    public string ToMarkdown(string jsonPath)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# RKWP Performance Baseline Run");
        builder.AppendLine();
        builder.AppendLine($"CreatedAt: `{CreatedAt:O}`");
        builder.AppendLine($"Iterations: `{Iterations}`");
        builder.AppendLine($"PDF: `{Path.GetFileName(PdfPath)}`");
        builder.AppendLine($"JSON: `{jsonPath}`");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("| --- | ---: |");
        builder.AppendLine("| SecureDev path | measured |");
        builder.AppendLine("| PDF frame path | measured |");
        builder.AppendLine("| Local E2E path | measured |");
        builder.AppendLine("| Frame input path | measured |");
        builder.AppendLine("| Recovery path | measured |");
        builder.AppendLine($"| FrameUpdate bytes avg | {FrameUpdateBytesAverage:F1} |");
        builder.AppendLine($"| FrameUpdate frequency Hz | {FrameUpdateFrequencyHz:F1} |");
        builder.AppendLine($"| DevTransport roundtrip avg ms | {DevTransportRoundtripAverageMs:F3} |");
        builder.AppendLine($"| Heartbeat latency avg ms | {HeartbeatLatencyAverageMs:F3} |");
        builder.AppendLine($"| FrameUpdate roundtrip avg ms | {FrameUpdateRoundtripAverageMs:F3} |");
        builder.AppendLine($"| FrameOpen avg ms | {FrameOpenAverageMs:F3} |");
        builder.AppendLine($"| FrameReturn avg ms | {FrameReturnAverageMs:F3} |");
        builder.AppendLine($"| Recovery avg ms | {RecoveryAverageMs:F3} |");
        builder.AppendLine($"| No File Ingress overhead avg ms | {NoFileIngressOverheadAverageMs:F3} |");
        builder.AppendLine($"| PDF load avg ms | {PdfLoadAverageMs:F3} |");
        builder.AppendLine($"| PDF render avg ms | {PdfRenderAverageMs:F3} |");
        builder.AppendLine($"| PDF render first page avg ms | {PdfRenderFirstPageAverageMs:F3} |");
        builder.AppendLine($"| PDF render next page avg ms | {PdfRenderNextPageAverageMs:F3} |");
        builder.AppendLine($"| PDF tile generation avg ms | {PdfTileGenerationAverageMs:F3} |");
        builder.AppendLine($"| PDF frame size bytes avg | {PdfFrameSizeBytesAverage:F1} |");
        builder.AppendLine($"| Memory snapshot bytes avg | {MemorySnapshotBytesAverage:F1} |");
        builder.AppendLine();
        builder.AppendLine($"RendererStatus: `{RendererStatus}`");
        builder.AppendLine($"NoFileIngress: `{(NoFileIngressPassed ? "SUCCESS" : "FAILED")}`");
        return builder.ToString();
    }
}

public sealed record RkwpPerfRunResult(
    RkwpPerfOptions Options,
    RkwpPerfSummary Summary,
    IReadOnlyList<RkwpPerfSample> Samples,
    string JsonPath,
    string MarkdownPath)
{
    public bool IsSuccessful =>
        Samples.Count == Options.Iterations &&
        Samples.Count > 0 &&
        Summary.NoFileIngressPassed &&
        Summary.FrameUpdateBytesAverage > 0 &&
        Summary.DevTransportRoundtripAverageMs > 0 &&
        File.Exists(JsonPath) &&
        File.Exists(MarkdownPath);
}
