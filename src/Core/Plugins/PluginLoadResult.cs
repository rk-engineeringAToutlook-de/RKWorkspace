namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Describes the result of a plugin manager operation.
/// </summary>
public sealed record PluginLoadResult
{
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets the related plugin id.
    /// </summary>
    public required string PluginId { get; init; }

    /// <summary>
    /// Gets the plugin state after the operation, if known.
    /// </summary>
    public PluginState? State { get; init; }

    /// <summary>
    /// Gets the plugin error if the operation failed.
    /// </summary>
    public PluginException? Error { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="pluginId">The related plugin id.</param>
    /// <param name="state">The plugin state after the operation.</param>
    /// <returns>A successful plugin load result.</returns>
    public static PluginLoadResult Successful(string pluginId, PluginState state)
    {
        return new PluginLoadResult
        {
            Success = true,
            PluginId = pluginId,
            State = state
        };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="pluginId">The related plugin id, if known.</param>
    /// <param name="error">The plugin error.</param>
    /// <returns>A failed plugin load result.</returns>
    public static PluginLoadResult Failed(string? pluginId, PluginException error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new PluginLoadResult
        {
            Success = false,
            PluginId = pluginId ?? string.Empty,
            State = null,
            Error = error
        };
    }
}
