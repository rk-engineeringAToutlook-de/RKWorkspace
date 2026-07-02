namespace RKWorkspace.Core.Runtime;

/// <summary>
/// Describes platform-neutral runtime configuration flags.
/// </summary>
public sealed record RuntimeConfiguration
{
    /// <summary>
    /// Gets whether runtime logging is enabled.
    /// </summary>
    public bool LoggingEnabled { get; init; } = true;

    /// <summary>
    /// Gets whether simulation mode is enabled.
    /// </summary>
    public bool SimulationEnabled { get; init; }

    /// <summary>
    /// Gets whether test mode is enabled.
    /// </summary>
    public bool TestModeEnabled { get; init; }

    /// <summary>
    /// Gets whether diagnostics mode is enabled.
    /// </summary>
    public bool DiagnosticsEnabled { get; init; } = true;

    /// <summary>
    /// Gets whether debug mode is enabled.
    /// </summary>
    public bool DebugModeEnabled { get; init; }

    /// <summary>
    /// Gets the default runtime configuration.
    /// </summary>
    public static RuntimeConfiguration Default { get; } = new();

    /// <summary>
    /// Creates an immutable configuration snapshot.
    /// </summary>
    /// <returns>The runtime configuration snapshot.</returns>
    public RuntimeConfiguration Snapshot()
    {
        return this with { };
    }
}
