namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Defines platform-neutral plugin registry and lifecycle operations.
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Registers and initializes a plugin using metadata from the plugin itself.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    /// <returns>The registration result.</returns>
    PluginLoadResult RegisterPlugin(IPlugin plugin);

    /// <summary>
    /// Registers and initializes a plugin with an explicit descriptor.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    /// <param name="descriptor">The plugin descriptor.</param>
    /// <returns>The registration result.</returns>
    PluginLoadResult RegisterPlugin(IPlugin plugin, PluginDescriptor descriptor);

    /// <summary>
    /// Unregisters a plugin and removes it from the registry.
    /// </summary>
    /// <param name="pluginId">The plugin id.</param>
    /// <returns>The unregister result.</returns>
    PluginLoadResult UnregisterPlugin(string pluginId);

    /// <summary>
    /// Gets a registered plugin by id.
    /// </summary>
    /// <param name="pluginId">The plugin id.</param>
    /// <returns>The plugin, or null when it is not registered.</returns>
    IPlugin? GetPlugin(string pluginId);

    /// <summary>
    /// Gets a descriptor for a registered plugin.
    /// </summary>
    /// <param name="pluginId">The plugin id.</param>
    /// <returns>The descriptor, or null when it is not registered.</returns>
    PluginDescriptor? GetDescriptor(string pluginId);

    /// <summary>
    /// Gets all registered plugins.
    /// </summary>
    /// <returns>All registered plugins.</returns>
    IReadOnlyCollection<IPlugin> GetPlugins();

    /// <summary>
    /// Gets all registered plugins of a given type.
    /// </summary>
    /// <param name="type">The plugin type.</param>
    /// <returns>Plugins matching the type.</returns>
    IReadOnlyCollection<IPlugin> GetPluginsByType(PluginType type);

    /// <summary>
    /// Activates a registered plugin.
    /// </summary>
    /// <param name="pluginId">The plugin id.</param>
    /// <returns>The activation result.</returns>
    PluginLoadResult ActivatePlugin(string pluginId);

    /// <summary>
    /// Deactivates a registered plugin.
    /// </summary>
    /// <param name="pluginId">The plugin id.</param>
    /// <returns>The deactivation result.</returns>
    PluginLoadResult DeactivatePlugin(string pluginId);

    /// <summary>
    /// Shuts a registered plugin down without removing it from the registry.
    /// </summary>
    /// <param name="pluginId">The plugin id.</param>
    /// <returns>The unload result.</returns>
    PluginLoadResult UnloadPlugin(string pluginId);

    /// <summary>
    /// Gets whether a plugin id is registered.
    /// </summary>
    /// <param name="pluginId">The plugin id.</param>
    /// <returns>True when the plugin is registered; otherwise false.</returns>
    bool IsRegistered(string pluginId);
}
