namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Defines the platform-neutral contract every RK Workspace plugin must implement.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the stable plugin id.
    /// </summary>
    string PluginId { get; }

    /// <summary>
    /// Gets the user-visible plugin name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the plugin version string.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the plugin type.
    /// </summary>
    PluginType Type { get; }

    /// <summary>
    /// Gets the current plugin lifecycle state.
    /// </summary>
    PluginState State { get; }

    /// <summary>
    /// Gets capabilities provided by the plugin.
    /// </summary>
    IReadOnlyCollection<string> Capabilities { get; }

    /// <summary>
    /// Initializes the plugin.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Activates the plugin.
    /// </summary>
    void Activate();

    /// <summary>
    /// Deactivates the plugin.
    /// </summary>
    void Deactivate();

    /// <summary>
    /// Shuts the plugin down.
    /// </summary>
    void Shutdown();
}
