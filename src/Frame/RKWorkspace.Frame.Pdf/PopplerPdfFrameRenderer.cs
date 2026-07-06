using System.Diagnostics;
using System.Buffers.Binary;

namespace RKWorkspace.Frame.Pdf;

public sealed class PopplerPdfFrameRenderer : IPdfFrameRenderer
{
    public const string Name = "PopplerPdfFrameRenderer";

    private readonly string pdfInfoPath;
    private readonly string pdfToPpmPath;
    private readonly PdfFrameRendererCapabilities capabilities;
    private DateTimeOffset lastRenderAt = DateTimeOffset.MinValue;
    private int renderCount;

    public PopplerPdfFrameRenderer(string pdfInfoPath, string pdfToPpmPath)
    {
        if (!File.Exists(pdfInfoPath))
        {
            throw new PdfFrameRendererException($"pdfinfo was not found: {pdfInfoPath}");
        }

        if (!File.Exists(pdfToPpmPath))
        {
            throw new PdfFrameRendererException($"pdftoppm was not found: {pdfToPpmPath}");
        }

        this.pdfInfoPath = pdfInfoPath;
        this.pdfToPpmPath = pdfToPpmPath;
        capabilities = new PdfFrameRendererCapabilities(
            Name,
            SupportsMetadata: true,
            SupportsPageCount: true,
            SupportsFirstPageRendering: true,
            SupportsPngFrame: true,
            Headless: true,
            CrossPlatformCandidate: true,
            OwnerSideOnly: true,
            GuestFileIngressAllowed: false,
            LicenseNote: "Poppler utilities are used as an external renderer dependency for development.",
            DependencyNote: "Requires pdfinfo and pdftoppm on the owner side.");
    }

    public static bool TryCreate(out PopplerPdfFrameRenderer renderer, out string blocker)
    {
        if (TryResolveExecutable("pdfinfo", out var pdfInfoPath) &&
            TryResolveExecutable("pdftoppm", out var pdfToPpmPath))
        {
            renderer = new PopplerPdfFrameRenderer(pdfInfoPath, pdfToPpmPath);
            blocker = string.Empty;
            return true;
        }

        renderer = null!;
        blocker = "Poppler pdfinfo/pdftoppm not found. Install Poppler or provide RKWS_POPPLER_BIN.";
        return false;
    }

    public PdfFrameRenderResult Render(PdfFrameRenderRequest request)
    {
        request.Validate();
        var document = request.PdfReference;
        if (!File.Exists(document.OwnerPath))
        {
            throw new PdfFrameRendererException($"PDF source is missing on owner side: {document.OwnerPath}");
        }

        var pdfInfo = ReadPdfInfo(document.OwnerPath);
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"rkws-pdf-frame-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var outputPrefix = Path.Combine(tempDirectory, "page");
            RunProcess(pdfToPpmPath, new[]
            {
                "-png",
                "-f",
                request.PageNumber.ToString(),
                "-l",
                request.PageNumber.ToString(),
                "-singlefile",
                "-scale-to-x",
                request.Options.RequestedWidth.ToString(),
                "-scale-to-y",
                "-1",
                document.OwnerPath,
                outputPrefix
            });

            var pngPath = $"{outputPrefix}.png";
            if (!File.Exists(pngPath))
            {
                throw new PdfFrameRendererException("Poppler did not produce a PNG frame.");
            }

            var pngBytes = File.ReadAllBytes(pngPath);
            var (width, height) = ReadPngDimensions(pngBytes);
            lastRenderAt = DateTimeOffset.UtcNow;
            renderCount++;

            return new PdfFrameRenderResult(
                request.PageNumber,
                width,
                height,
                FrameFormat.PngFrame,
                pngBytes,
                ImagePath: string.Empty,
                Metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["fileName"] = document.FileName,
                    ["thingId"] = document.ThingId,
                    ["ownerAblageId"] = request.OwnerAblageId,
                    ["sha256"] = document.Sha256,
                    ["pageCount"] = GetPdfInfoValue(pdfInfo, "Pages", document.PageCount.ToString()),
                    ["pdfVersion"] = GetPdfInfoValue(pdfInfo, "PDF version", "unknown"),
                    ["frameKind"] = "PngFrame",
                    ["noFileIngress"] = "true",
                    ["containsOriginalFileBytes"] = "false",
                    ["hasOriginalPathForGuest"] = "false",
                    ["rendererStatus"] = PdfFrameRendererStatus.Rendered.ToString()
                },
                lastRenderAt,
                Name,
                IsPlaceholder: false);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    public PdfFrameRendererDiagnostics GetDiagnostics()
    {
        return new PdfFrameRendererDiagnostics(
            Name,
            SupportsRealRendering: true,
            OwnerSideOnly: capabilities.OwnerSideOnly,
            GuestFileIngressAllowed: capabilities.GuestFileIngressAllowed,
            Status: $"{PdfFrameRendererStatus.Ready}: Poppler headless PNG rendering is available.",
            lastRenderAt,
            renderCount);
    }

    public PdfFrameRendererCapabilities GetCapabilities() => capabilities;

    private Dictionary<string, string> ReadPdfInfo(string pdfPath)
    {
        var output = RunProcess(pdfInfoPath, new[] { pdfPath });
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        return values;
    }

    private static string GetPdfInfoValue(
        IReadOnlyDictionary<string, string> pdfInfo,
        string key,
        string fallback)
    {
        return pdfInfo.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static string RunProcess(string fileName, IReadOnlyList<string> arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        if (!process.WaitForExit(15000))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            throw new PdfFrameRendererException($"Renderer process timed out: {Path.GetFileName(fileName)}");
        }

        if (process.ExitCode != 0)
        {
            throw new PdfFrameRendererException(
                $"Renderer process failed: {Path.GetFileName(fileName)} {standardError}".Trim());
        }

        return standardOutput;
    }

    private static (int Width, int Height) ReadPngDimensions(byte[] pngBytes)
    {
        if (pngBytes.Length < 24 ||
            pngBytes[0] != 0x89 ||
            pngBytes[1] != 0x50 ||
            pngBytes[2] != 0x4E ||
            pngBytes[3] != 0x47)
        {
            throw new PdfFrameRendererException("Rendered frame is not a PNG image.");
        }

        var width = BinaryPrimitives.ReadInt32BigEndian(pngBytes.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(pngBytes.AsSpan(20, 4));
        return (width, height);
    }

    private static bool TryResolveExecutable(string toolName, out string path)
    {
        foreach (var directory in GetCandidateDirectories())
        {
            foreach (var extension in new[] { ".exe", ".cmd", string.Empty })
            {
                var candidate = Path.Combine(directory, $"{toolName}{extension}");
                if (File.Exists(candidate))
                {
                    path = candidate;
                    return true;
                }
            }
        }

        path = string.Empty;
        return false;
    }

    private static IEnumerable<string> GetCandidateDirectories()
    {
        var configured = Environment.GetEnvironmentVariable("RKWS_POPPLER_BIN");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            yield return configured;
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            yield return Path.Combine(
                userProfile,
                ".cache",
                "codex-runtimes",
                "codex-primary-runtime",
                "dependencies",
                "native",
                "poppler",
                "Library",
                "bin");
            yield return Path.Combine(
                userProfile,
                ".cache",
                "codex-runtimes",
                "codex-primary-runtime",
                "dependencies",
                "native",
                "poppler",
                "bin");
        }

        var path = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(path))
        {
            foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return directory;
            }
        }
    }
}
