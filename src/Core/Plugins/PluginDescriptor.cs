namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Describes plugin metadata used by the plugin manager.
/// </summary>
public sealed record PluginDescriptor
{
    /// <summary>
    /// Gets the stable plugin id.
    /// </summary>
    public required string PluginId { get; init; }

    /// <summary>
    /// Gets the user-visible plugin name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the plugin version string.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets the plugin type.
    /// </summary>
    public required PluginType Type { get; init; }

    /// <summary>
    /// Gets the plugin description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plugin author.
    /// </summary>
    public string Author { get; init; } = string.Empty;

    /// <summary>
    /// Gets the capabilities required by this plugin.
    /// </summary>
    public IReadOnlyCollection<string> RequiredCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the capabilities provided by this plugin.
    /// </summary>
    public IReadOnlyCollection<string> ProvidedCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets plugin ids that must already be registered before this plugin can register.
    /// </summary>
    public IReadOnlyCollection<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets whether this plugin is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Creates a descriptor from an <see cref="IPlugin"/>.
    /// </summary>
    /// <param name="plugin">The plugin to describe.</param>
    /// <returns>A descriptor with core metadata and provided capabilities.</returns>
    public static PluginDescriptor FromPlugin(IPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        return new PluginDescriptor
        {
            PluginId = plugin.PluginId,
            Name = plugin.Name,
            Version = plugin.Version,
            Type = plugin.Type,
            ProvidedCapabilities = plugin.Capabilities,
            IsEnabled = plugin.State != PluginState.Disabled
        };
    }
}
