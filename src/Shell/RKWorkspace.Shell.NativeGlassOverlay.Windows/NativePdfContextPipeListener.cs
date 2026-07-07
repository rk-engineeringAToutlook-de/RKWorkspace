using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Windows.Threading;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativePdfContextPipeListener : IDisposable
{
    private const int MaxPipeInstances = 4;
    private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(2);

    private readonly string _pipeName;
    private readonly Dispatcher _dispatcher;
    private readonly Func<string, NativePdfContextPickResult> _pickPdf;
    private readonly NativeGlassOverlayDiagnostics _diagnostics;
    private readonly CancellationTokenSource _cancellation = new();
    private Task? _task;

    public NativePdfContextPipeListener(
        string pipeName,
        Dispatcher dispatcher,
        Func<string, NativePdfContextPickResult> pickPdf,
        NativeGlassOverlayDiagnostics diagnostics)
    {
        _pipeName = pipeName;
        _dispatcher = dispatcher;
        _pickPdf = pickPdf;
        _diagnostics = diagnostics;
    }

    public void Start()
    {
        if (_task is not null)
        {
            return;
        }

        _task = Task.Run(ListenAsync);
        _diagnostics.Set("Kontext", $"Listener aktiv: {_pipeName}");
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
    }

    private async Task ListenAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                var server = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    MaxPipeInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(_cancellation.Token).ConfigureAwait(false);
                _ = HandleClientAsync(server);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _ = _dispatcher.BeginInvoke(() =>
                    _diagnostics.Set("Kontext", $"Listener-Fehler: {ex.Message}"));
                await Task.Delay(250, _cancellation.Token).ConfigureAwait(false);
            }
        }
    }

    private async Task HandleClientAsync(NamedPipeServerStream server)
    {
        using (server)
        {
            using var reader = new StreamReader(
                server,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true);
            await using var writer = new StreamWriter(
                server,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                bufferSize: 1024,
                leaveOpen: true) { AutoFlush = true };

            try
            {
                using var readCancellation = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token);
                readCancellation.CancelAfter(ReadTimeout);
                var line = await reader.ReadLineAsync(readCancellation.Token).ConfigureAwait(false);
                var pdfPath = ReadPdfPath(line);
                var validation = ValidatePdfPath(pdfPath);
                if (validation is not null)
                {
                    await TryWriteResponseAsync(writer, false, validation).ConfigureAwait(false);
                    _ = _dispatcher.BeginInvoke(() => _diagnostics.Set("Kontext", validation));
                    return;
                }

                _ = _dispatcher.BeginInvoke(() =>
                {
                    var result = _pickPdf(pdfPath);
                    _diagnostics.Set("Kontext", result.Message);
                });
                await TryWriteResponseAsync(writer, true, $"PDF an Overlay uebergeben: {Path.GetFileName(pdfPath)}").ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!_cancellation.IsCancellationRequested)
            {
                await TryWriteResponseAsync(writer, false, "Kontextnachricht Timeout").ConfigureAwait(false);
                _ = _dispatcher.BeginInvoke(() => _diagnostics.Set("Kontext", "Client-Timeout; Listener bleibt aktiv"));
            }
            catch (Exception ex)
            {
                await TryWriteResponseAsync(writer, false, ex.Message).ConfigureAwait(false);
                _ = _dispatcher.BeginInvoke(() => _diagnostics.Set("Kontext", $"Client-Fehler: {ex.Message}"));
            }
        }
    }

    private static async Task TryWriteResponseAsync(StreamWriter writer, bool success, string message)
    {
        try
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(new
            {
                ok = success,
                message
            })).ConfigureAwait(false);
        }
        catch
        {
            // The context menu launcher is best-effort; diagnostics on the overlay remain authoritative.
        }
    }

    private static string ReadPdfPath(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new InvalidOperationException("Kontextnachricht war leer.");
        }

        using var document = JsonDocument.Parse(line);
        if (document.RootElement.TryGetProperty("pdfPath", out var pathElement))
        {
            return pathElement.GetString() ?? string.Empty;
        }

        throw new InvalidOperationException("Kontextnachricht enthaelt keinen pdfPath.");
    }

    private static string? ValidatePdfPath(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            return "Kontextnachricht ohne PDF-Pfad.";
        }

        if (!File.Exists(pdfPath))
        {
            return $"PDF existiert nicht: {pdfPath}";
        }

        if (!pdfPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return $"Datei ist keine PDF: {pdfPath}";
        }

        return null;
    }
}

public sealed record NativePdfContextPickResult(bool Success, string Message)
{
    public static NativePdfContextPickResult Ok(string message) => new(true, message);

    public static NativePdfContextPickResult Failed(string message) => new(false, message);
}
