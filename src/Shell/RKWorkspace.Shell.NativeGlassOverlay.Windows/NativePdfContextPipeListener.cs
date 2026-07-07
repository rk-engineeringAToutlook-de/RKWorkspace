using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Windows.Threading;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativePdfContextPipeListener : IDisposable
{
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
                using var server = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(_cancellation.Token).ConfigureAwait(false);

                using var reader = new StreamReader(
                    server,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 1024,
                    leaveOpen: true);
                using var writer = new StreamWriter(
                    server,
                    Encoding.UTF8,
                    bufferSize: 1024,
                    leaveOpen: true) { AutoFlush = true };
                var line = await reader.ReadLineAsync(_cancellation.Token).ConfigureAwait(false);
                var pdfPath = ReadPdfPath(line);
                var result = await _dispatcher.InvokeAsync(() => _pickPdf(pdfPath)).Task.ConfigureAwait(false);
                await writer.WriteLineAsync(JsonSerializer.Serialize(new
                {
                    ok = result.Success,
                    message = result.Message
                })).ConfigureAwait(false);
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
}

public sealed record NativePdfContextPickResult(bool Success, string Message)
{
    public static NativePdfContextPickResult Ok(string message) => new(true, message);

    public static NativePdfContextPickResult Failed(string message) => new(false, message);
}
