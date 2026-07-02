namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Represents a plugin registration or lifecycle error.
/// </summary>
public sealed class PluginException : Exception
{
    /// <summary>
    /// Initializes a new plugin exception.
    /// </summary>
    /// <param name="code">The plugin error code.</param>
    /// <param name="pluginId">The related plugin id, if known.</param>
    /// <param name="message">A human-readable error message.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
    public PluginException(
        PluginErrorCode code,
        string? pluginId,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        PluginId = pluginId;
    }

    /// <summary>
    /// Gets the plugin error code.
    /// </summary>
    public PluginErrorCode Code { get; }

    /// <summary>
    /// Gets the related plugin id, if known.
    /// </summary>
    public string? PluginId { get; }
}
