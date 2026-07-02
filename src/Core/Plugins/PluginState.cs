namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Represents the lifecycle state of a plugin.
/// </summary>
public enum PluginState
{
    /// <summary>
    /// The plugin is known but not yet registered by the manager.
    /// </summary>
    Discovered,

    /// <summary>
    /// The plugin is registered with the manager.
    /// </summary>
    Registered,

    /// <summary>
    /// The plugin has been initialized and is ready to activate.
    /// </summary>
    Loaded,

    /// <summary>
    /// The plugin is active.
    /// </summary>
    Activated,

    /// <summary>
    /// The plugin was active and has been deactivated.
    /// </summary>
    Deactivated,

    /// <summary>
    /// The plugin has been shut down but remains known to the manager.
    /// </summary>
    Unloaded,

    /// <summary>
    /// The plugin failed during a lifecycle operation.
    /// </summary>
    Failed,

    /// <summary>
    /// The plugin is disabled by descriptor or policy.
    /// </summary>
    Disabled
}
