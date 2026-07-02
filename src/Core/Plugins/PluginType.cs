namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Describes the architectural role of a plugin.
/// </summary>
public enum PluginType
{
    /// <summary>
    /// Provides workspace registration or workspace model integration.
    /// </summary>
    Workspace,

    /// <summary>
    /// Converts platform-specific gestures into neutral core intents.
    /// </summary>
    Gesture,

    /// <summary>
    /// Coordinates transfer planning or transfer state updates.
    /// </summary>
    Transfer,

    /// <summary>
    /// Discovers workspaces or workspace-capable nodes.
    /// </summary>
    Discovery,

    /// <summary>
    /// Provides protocol or transport integration through adapters.
    /// </summary>
    Communication,

    /// <summary>
    /// Provides trust, pairing, authentication or encryption capabilities.
    /// </summary>
    Security,

    /// <summary>
    /// Represents physical node or hardware integration.
    /// </summary>
    Hardware,

    /// <summary>
    /// Provides display or overlay capabilities.
    /// </summary>
    Display,

    /// <summary>
    /// Provides future context transfer capabilities.
    /// </summary>
    Context,

    /// <summary>
    /// Provides clipboard capabilities through platform adapters.
    /// </summary>
    Clipboard,

    /// <summary>
    /// Provides logging capabilities.
    /// </summary>
    Logging,

    /// <summary>
    /// Provides configuration or policy capabilities.
    /// </summary>
    Configuration,

    /// <summary>
    /// Provides testing capabilities.
    /// </summary>
    Testing,

    /// <summary>
    /// Provides simulation capabilities.
    /// </summary>
    Simulation,

    /// <summary>
    /// Provides firmware-related capabilities.
    /// </summary>
    Firmware,

    /// <summary>
    /// Provides software or firmware update capabilities.
    /// </summary>
    Update,

    /// <summary>
    /// The plugin type is not known.
    /// </summary>
    Unknown
}
