namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Identifies a plugin lifecycle or registration error.
/// </summary>
public enum PluginErrorCode
{
    /// <summary>
    /// The plugin id is missing or empty.
    /// </summary>
    MissingPluginId,

    /// <summary>
    /// A plugin with the same id is already registered.
    /// </summary>
    PluginAlreadyRegistered,

    /// <summary>
    /// The requested plugin is not registered.
    /// </summary>
    PluginNotRegistered,

    /// <summary>
    /// The plugin is in a state that does not allow the requested operation.
    /// </summary>
    InvalidState,

    /// <summary>
    /// Plugin initialization failed.
    /// </summary>
    InitializationFailed,

    /// <summary>
    /// Plugin activation failed.
    /// </summary>
    ActivationFailed,

    /// <summary>
    /// Plugin deactivation failed.
    /// </summary>
    DeactivationFailed,

    /// <summary>
    /// A required plugin dependency is missing.
    /// </summary>
    DependencyMissing,

    /// <summary>
    /// Plugin shutdown failed.
    /// </summary>
    ShutdownFailed,

    /// <summary>
    /// The plugin descriptor is not valid.
    /// </summary>
    InvalidDescriptor
}
