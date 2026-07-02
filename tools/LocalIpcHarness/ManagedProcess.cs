using System.Diagnostics;

namespace RKWorkspace.LocalIpcHarness;

internal sealed class ManagedProcess
{
    private readonly Process _process;
    private readonly TaskCompletionSource _exit = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly List<string> _output = new();
    private readonly List<string> _error = new();

    private ManagedProcess(Process process)
    {
        _process = process;
        _process.EnableRaisingEvents = true;
        _process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (_output)
                {
                    _output.Add(args.Data);
                }
            }
        };
        _process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (_error)
                {
                    _error.Add(args.Data);
                }
            }
        };
        _process.Exited += (_, _) => _exit.TrySetResult();
    }

    public int? ExitCode => _process.HasExited ? _process.ExitCode : null;

    public IReadOnlyCollection<string> Output
    {
        get
        {
            lock (_output)
            {
                return _output.ToArray();
            }
        }
    }

    public static ManagedProcess Start(ProcessStartInfo startInfo)
    {
        var process = new Process
        {
            StartInfo = startInfo
        };
        var managed = new ManagedProcess(process);
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        if (process.HasExited)
        {
            managed._exit.TrySetResult();
        }

        return managed;
    }

    public async Task WaitForOutputAsync(string value, TimeSpan timeout)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);
        while (!timeoutSource.IsCancellationRequested)
        {
            if (Output.Any(line => line.Contains(value, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (_process.HasExited)
            {
                throw new InvalidOperationException(
                    $"Process exited before output '{value}' appeared. Output: {string.Join(" | ", Output)} Error: {string.Join(" | ", Error)}");
            }

            await Task.Delay(50, timeoutSource.Token).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timed out waiting for output '{value}'.");
    }

    public async Task WaitForExitAsync(TimeSpan timeout)
    {
        var completed = await Task.WhenAny(_exit.Task, Task.Delay(timeout)).ConfigureAwait(false);
        if (completed != _exit.Task)
        {
            KillIfRunning();
            throw new TimeoutException("Process did not stop before timeout.");
        }

        await _exit.Task.ConfigureAwait(false);
    }

    public void KillIfRunning()
    {
        if (_process.HasExited)
        {
            return;
        }

        try
        {
            _process.Kill(entireProcessTree: true);
        }
        catch
        {
        }
    }

    private IReadOnlyCollection<string> Error
    {
        get
        {
            lock (_error)
            {
                return _error.ToArray();
            }
        }
    }
}
