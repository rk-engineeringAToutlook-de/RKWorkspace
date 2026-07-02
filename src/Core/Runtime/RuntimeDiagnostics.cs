namespace RKWorkspace.Core.Runtime;

/// <summary>
/// Describes platform-neutral runtime diagnostics.
/// </summary>
public sealed record RuntimeDiagnostics
{
    /// <summary>
    /// Gets the runtime version.
    /// </summary>
    public required string RuntimeVersion { get; init; }

    /// <summary>
    /// Gets the runtime start timestamp.
    /// </summary>
    public DateTimeOffset? StartTime { get; init; }

    /// <summary>
    /// Gets the runtime uptime.
    /// </summary>
    public required TimeSpan Uptime { get; init; }

    /// <summary>
    /// Gets the registered plugin count.
    /// </summary>
    public required int PluginCount { get; init; }

    /// <summary>
    /// Gets the registered workspace count.
    /// </summary>
    public required int WorkspaceCount { get; init; }

    /// <summary>
    /// Gets the registered transfer object count.
    /// </summary>
    public required int TransferObjectCount { get; init; }

    /// <summary>
    /// Gets the runtime state.
    /// </summary>
    public required RuntimeState RuntimeState { get; init; }

    /// <summary>
    /// Gets runtime error messages.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets runtime warning messages.
    /// </summary>
    public IReadOnlyCollection<string> Warnings { get; init; } = Array.Empty<string>();
}
