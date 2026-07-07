using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace RKWorkspace.WindowsPdfContextPick;

internal static class Program
{
    private const string DefaultPipeName = "RKWorkspace.NativeGlassOverlay.PdfPick";
    private const int DefaultTimeoutMilliseconds = 1500;

    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "rkws-windows-pdf-context-pick.log");

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var options = Options.Parse(args);
            if (options.ShowHelp)
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(options.PdfPath))
            {
                WriteLog("FAILED: no PDF path received.");
                return 2;
            }

            var resolvedPdfPath = Path.GetFullPath(options.PdfPath);
            WriteLog($"START: PDF={resolvedPdfPath} Pipe={options.PipeName} TimeoutMs={options.TimeoutMilliseconds}");
            if (!File.Exists(resolvedPdfPath))
            {
                WriteLog($"FAILED: PDF does not exist: {resolvedPdfPath}");
                return 3;
            }

            if (!resolvedPdfPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                WriteLog($"FAILED: not a PDF: {resolvedPdfPath}");
                return 4;
            }

            using var client = new NamedPipeClientStream(
                ".",
                options.PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            using var connectCancellation = new CancellationTokenSource(options.TimeoutMilliseconds);
            WriteLog("CONNECTING: overlay pipe.");
            client.ConnectAsync(connectCancellation.Token).GetAwaiter().GetResult();
            WriteLog("CONNECTED: overlay pipe.");

            var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            using var writer = new StreamWriter(client, utf8NoBom, 1024, leaveOpen: true)
            {
                AutoFlush = true
            };
            using var reader = new StreamReader(client, utf8NoBom, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);

            var payload = JsonSerializer.Serialize(new { pdfPath = resolvedPdfPath });
            using var sendCancellation = new CancellationTokenSource(options.TimeoutMilliseconds);
            writer.WriteLineAsync(payload.AsMemory(), sendCancellation.Token).GetAwaiter().GetResult();
            WriteLog("SENT: context payload.");

            using var responseCancellation = new CancellationTokenSource(options.TimeoutMilliseconds);
            var response = reader.ReadLineAsync(responseCancellation.Token).GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(response))
            {
                WriteLog("FAILED: overlay returned an empty response.");
                return 5;
            }

            using var document = JsonDocument.Parse(response);
            var ok = document.RootElement.TryGetProperty("ok", out var okElement) &&
                okElement.ValueKind == JsonValueKind.True;
            var message = document.RootElement.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : string.Empty;

            WriteLog($"{(ok ? "OK" : "FAILED")}: {message} | PDF={resolvedPdfPath}");
            return ok ? 0 : 6;
        }
        catch (TimeoutException ex)
        {
            WriteLog($"FAILED: overlay pipe timeout. Is the native overlay running? {ex.Message}");
            return 7;
        }
        catch (Exception ex)
        {
            WriteLog($"FAILED: {ex.GetType().Name}: {ex.Message}");
            return 8;
        }
    }

    private static void WriteLog(string message)
    {
        try
        {
            File.AppendAllText(
                LogPath,
                $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}",
                Encoding.UTF8);
        }
        catch
        {
            // Explorer context activation must never interrupt the user with a secondary error.
        }
    }

    private sealed record Options(
        string PdfPath,
        string PipeName,
        int TimeoutMilliseconds,
        bool ShowHelp)
    {
        public static Options Parse(IReadOnlyList<string> args)
        {
            var pdfPath = string.Empty;
            var pipeName = DefaultPipeName;
            var timeoutMilliseconds = DefaultTimeoutMilliseconds;
            var showHelp = false;

            for (var index = 0; index < args.Count; index++)
            {
                var arg = args[index];
                if (Is(arg, "--help", "-h", "/?"))
                {
                    showHelp = true;
                    continue;
                }

                if (ReadString(args, ref index, "--pdf", "--pdf-path", "-PdfPath") is { } parsedPdfPath)
                {
                    pdfPath = parsedPdfPath;
                    continue;
                }

                if (ReadString(args, ref index, "--pipe", "--pipe-name", "-PipeName") is { } parsedPipeName)
                {
                    pipeName = parsedPipeName;
                    continue;
                }

                if (ReadString(args, ref index, "--timeout-ms", "-TimeoutMs") is { } parsedTimeout &&
                    int.TryParse(parsedTimeout, out var timeout) &&
                    timeout > 0)
                {
                    timeoutMilliseconds = timeout;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(pdfPath))
                {
                    pdfPath = arg;
                }
            }

            return new Options(pdfPath, pipeName, timeoutMilliseconds, showHelp);
        }

        private static bool Is(string value, params string[] names)
        {
            foreach (var name in names)
            {
                if (string.Equals(value, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string? ReadString(IReadOnlyList<string> args, ref int index, params string[] names)
        {
            var matches = false;
            foreach (var name in names)
            {
                if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    matches = true;
                    break;
                }
            }

            if (!matches || index + 1 >= args.Count)
            {
                return null;
            }

            index++;
            return args[index];
        }
    }
}
