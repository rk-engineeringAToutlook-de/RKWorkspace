using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Plugins;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Core.Runtime;

/// <summary>
/// Provides the platform-neutral core runtime lifecycle orchestrator.
/// </summary>
public sealed class RuntimeEngine : IRuntimeEngine
{
    private const string CurrentRuntimeVersion = "0.1.0";

    private readonly Func<IPluginManager> _pluginManagerFactory;
    private readonly Func<ICapabilityManager> _capabilityManagerFactory;
    private readonly Func<IWorkspaceRegistry> _workspaceRegistryFactory;
    private readonly Func<ITransferObjectManager> _transferObjectManagerFactory;
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    private IPluginManager? _pluginManager;
    private ICapabilityManager? _capabilityManager;
    private IWorkspaceRegistry? _workspaceRegistry;
    private ITransferObjectManager? _transferObjectManager;
    private RuntimeState _state = RuntimeState.Created;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _stoppedAt;

    /// <summary>
    /// Initializes a runtime engine.
    /// </summary>
    /// <param name="configuration">The runtime configuration.</param>
    /// <param name="pluginManagerFactory">Optional plugin manager factory for tests.</param>
    /// <param name="capabilityManagerFactory">Optional capability manager factory for tests.</param>
    /// <param name="workspaceRegistryFactory">Optional workspace registry factory for tests.</param>
    /// <param name="transferObjectManagerFactory">Optional transfer object manager factory for tests.</param>
    public RuntimeEngine(
        RuntimeConfiguration? configuration = null,
        Func<IPluginManager>? pluginManagerFactory = null,
        Func<ICapabilityManager>? capabilityManagerFactory = null,
        Func<IWorkspaceRegistry>? workspaceRegistryFactory = null,
        Func<ITransferObjectManager>? transferObjectManagerFactory = null)
    {
        Configuration = (configuration ?? RuntimeConfiguration.Default).Snapshot();
        _pluginManagerFactory = pluginManagerFactory ?? (() => new PluginManager());
        _capabilityManagerFactory = capabilityManagerFactory ?? (() => new CapabilityManager());
        _workspaceRegistryFactory = workspaceRegistryFactory ?? (() => new WorkspaceRegistry());
        _transferObjectManagerFactory = transferObjectManagerFactory ?? (() => new TransferObjectManager());
    }

    /// <inheritdoc />
    public RuntimeConfiguration Configuration { get; }

    /// <inheritdoc />
    public IPluginManager PluginManager => _pluginManager ?? throw ComponentUnavailable(nameof(PluginManager));

    /// <inheritdoc />
    public ICapabilityManager CapabilityManager => _capabilityManager ?? throw ComponentUnavailable(nameof(CapabilityManager));

    /// <inheritdoc />
    public IWorkspaceRegistry WorkspaceRegistry => _workspaceRegistry ?? throw ComponentUnavailable(nameof(WorkspaceRegistry));

    /// <inheritdoc />
    public ITransferObjectManager TransferObjectManager =>
        _transferObjectManager ?? throw ComponentUnavailable(nameof(TransferObjectManager));

    /// <inheritdoc />
    public void Initialize()
    {
        if (_state is not (RuntimeState.Created or RuntimeState.Stopped))
        {
            ThrowInvalidTransition(nameof(Initialize), RuntimeState.Created, RuntimeState.Stopped);
        }

        if (ComponentsInitialized())
        {
            return;
        }

        try
        {
            _state = RuntimeState.Initializing;
            _pluginManager = RequireComponent(_pluginManagerFactory(), nameof(PluginManager));
            _capabilityManager = RequireComponent(_capabilityManagerFactory(), nameof(CapabilityManager));
            _workspaceRegistry = RequireComponent(_workspaceRegistryFactory(), nameof(WorkspaceRegistry));
            _transferObjectManager = RequireComponent(_transferObjectManagerFactory(), nameof(TransferObjectManager));
            _state = RuntimeState.Stopped;
        }
        catch (Exception ex) when (ex is not RuntimeException)
        {
            _state = RuntimeState.Failed;
            RecordError($"Runtime initialization failed: {ex.Message}");
            throw new RuntimeException(
                nameof(Initialize),
                _state,
                "Runtime initialization failed.",
                ex);
        }
    }

    /// <inheritdoc />
    public void Start()
    {
        if (_state is RuntimeState.Running)
        {
            ThrowInvalidTransition(nameof(Start), RuntimeState.Created, RuntimeState.Stopped);
        }

        if (_state is RuntimeState.Paused)
        {
            ThrowInvalidTransition(nameof(Start), RuntimeState.Stopped);
        }

        if (_state is RuntimeState.Failed or RuntimeState.Stopping or RuntimeState.Initializing)
        {
            ThrowInvalidTransition(nameof(Start), RuntimeState.Created, RuntimeState.Stopped);
        }

        if (!ComponentsInitialized())
        {
            Initialize();
        }

        _state = RuntimeState.Running;
        _startedAt = DateTimeOffset.UtcNow;
        _stoppedAt = null;
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (_state is not (RuntimeState.Running or RuntimeState.Paused))
        {
            ThrowInvalidTransition(nameof(Stop), RuntimeState.Running, RuntimeState.Paused);
        }

        try
        {
            _state = RuntimeState.Stopping;
            StopPlugins();
            _stoppedAt = DateTimeOffset.UtcNow;
            _state = RuntimeState.Stopped;
        }
        catch (Exception ex) when (ex is not RuntimeException)
        {
            _state = RuntimeState.Failed;
            RecordError($"Runtime stop failed: {ex.Message}");
            throw new RuntimeException(nameof(Stop), _state, "Runtime stop failed.", ex);
        }
    }

    /// <inheritdoc />
    public void Pause()
    {
        if (_state != RuntimeState.Running)
        {
            ThrowInvalidTransition(nameof(Pause), RuntimeState.Running);
        }

        _state = RuntimeState.Paused;
    }

    /// <inheritdoc />
    public void Resume()
    {
        if (_state != RuntimeState.Paused)
        {
            ThrowInvalidTransition(nameof(Resume), RuntimeState.Paused);
        }

        _state = RuntimeState.Running;
    }

    /// <inheritdoc />
    public void Shutdown()
    {
        if (_state is RuntimeState.Running or RuntimeState.Paused)
        {
            Stop();
            return;
        }

        if (_state == RuntimeState.Initializing)
        {
            ThrowInvalidTransition(nameof(Shutdown), RuntimeState.Created, RuntimeState.Stopped, RuntimeState.Failed);
        }

        if (_state == RuntimeState.Failed && ComponentsInitialized())
        {
            _state = RuntimeState.Stopping;
            StopPlugins();
        }

        _stoppedAt ??= DateTimeOffset.UtcNow;
        _state = RuntimeState.Stopped;
    }

    /// <inheritdoc />
    public RuntimeState GetStatus()
    {
        return _state;
    }

    /// <inheritdoc />
    public RuntimeDiagnostics GetDiagnostics()
    {
        return new RuntimeDiagnostics
        {
            RuntimeVersion = CurrentRuntimeVersion,
            StartTime = _startedAt,
            Uptime = GetUptime(),
            PluginCount = _pluginManager?.GetPlugins().Count ?? 0,
            WorkspaceCount = _workspaceRegistry?.GetAllWorkspaces().Count ?? 0,
            TransferObjectCount = _transferObjectManager?.GetAll().Count ?? 0,
            RuntimeState = _state,
            Errors = _errors.ToArray(),
            Warnings = _warnings.ToArray()
        };
    }

    private void StopPlugins()
    {
        if (_pluginManager is null)
        {
            return;
        }

        foreach (var plugin in _pluginManager.GetPlugins().ToArray())
        {
            if (plugin.State == PluginState.Activated)
            {
                var deactivate = _pluginManager.DeactivatePlugin(plugin.PluginId);
                EnsurePluginResult(nameof(Stop), deactivate);
            }

            if (plugin.State is PluginState.Unloaded or PluginState.Disabled)
            {
                continue;
            }

            var unload = _pluginManager.UnloadPlugin(plugin.PluginId);
            EnsurePluginResult(nameof(Stop), unload);
        }
    }

    private TimeSpan GetUptime()
    {
        if (_startedAt is null)
        {
            return TimeSpan.Zero;
        }

        var end = _state == RuntimeState.Stopped && _stoppedAt.HasValue
            ? _stoppedAt.Value
            : DateTimeOffset.UtcNow;

        return end >= _startedAt.Value
            ? end - _startedAt.Value
            : TimeSpan.Zero;
    }

    private bool ComponentsInitialized()
    {
        return _pluginManager is not null &&
            _capabilityManager is not null &&
            _workspaceRegistry is not null &&
            _transferObjectManager is not null;
    }

    private TComponent RequireComponent<TComponent>(TComponent? component, string name)
        where TComponent : class
    {
        if (component is not null)
        {
            return component;
        }

        throw new RuntimeException(
            nameof(Initialize),
            _state,
            $"Runtime component '{name}' factory returned null.");
    }

    private void EnsurePluginResult(string operation, PluginLoadResult result)
    {
        if (result.Success)
        {
            return;
        }

        var message = result.Error?.Message ?? $"Plugin operation failed for '{result.PluginId}'.";
        RecordError(message);
        throw new RuntimeException(operation, _state, message, result.Error);
    }

    private RuntimeException ComponentUnavailable(string componentName)
    {
        var message = $"Runtime component '{componentName}' is not initialized.";
        RecordError(message);
        return new RuntimeException(componentName, _state, message);
    }

    private void ThrowInvalidTransition(string operation, params RuntimeState[] expectedStates)
    {
        var expected = string.Join(", ", expectedStates);
        var message = $"Runtime operation '{operation}' is not valid from state {_state}. Expected: {expected}.";
        RecordError(message);
        throw new RuntimeException(operation, _state, message);
    }

    private void RecordError(string message)
    {
        if (!_errors.Contains(message, StringComparer.Ordinal))
        {
            _errors.Add(message);
        }
    }
}
