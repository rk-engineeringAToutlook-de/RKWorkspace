using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Plugins;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Core.Runtime;

/// <summary>
/// Defines platform-neutral core runtime lifecycle operations.
/// </summary>
public interface IRuntimeEngine
{
    /// <summary>
    /// Gets the runtime configuration.
    /// </summary>
    RuntimeConfiguration Configuration { get; }

    /// <summary>
    /// Gets the plugin manager.
    /// </summary>
    IPluginManager PluginManager { get; }

    /// <summary>
    /// Gets the capability manager.
    /// </summary>
    ICapabilityManager CapabilityManager { get; }

    /// <summary>
    /// Gets the workspace registry.
    /// </summary>
    IWorkspaceRegistry WorkspaceRegistry { get; }

    /// <summary>
    /// Gets the transfer object manager.
    /// </summary>
    ITransferObjectManager TransferObjectManager { get; }

    /// <summary>
    /// Initializes core runtime components.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Starts the core runtime.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the core runtime.
    /// </summary>
    void Stop();

    /// <summary>
    /// Pauses the core runtime.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes a paused core runtime.
    /// </summary>
    void Resume();

    /// <summary>
    /// Shuts the core runtime down.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Gets the current runtime state.
    /// </summary>
    /// <returns>The runtime state.</returns>
    RuntimeState GetStatus();

    /// <summary>
    /// Gets current runtime diagnostics.
    /// </summary>
    /// <returns>The runtime diagnostics.</returns>
    RuntimeDiagnostics GetDiagnostics();
}
