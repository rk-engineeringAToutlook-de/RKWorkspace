namespace RKWorkspace.Core.Runtime;

/// <summary>
/// Describes the platform-neutral lifecycle state of the core runtime.
/// </summary>
public enum RuntimeState
{
    /// <summary>
    /// The runtime was created but not initialized.
    /// </summary>
    Created,

    /// <summary>
    /// The runtime is initializing core components.
    /// </summary>
    Initializing,

    /// <summary>
    /// The runtime is running.
    /// </summary>
    Running,

    /// <summary>
    /// The runtime is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The runtime is stopping.
    /// </summary>
    Stopping,

    /// <summary>
    /// The runtime is stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The runtime failed during a lifecycle operation.
    /// </summary>
    Failed
}
