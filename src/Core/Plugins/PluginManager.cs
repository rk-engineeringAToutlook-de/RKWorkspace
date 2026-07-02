namespace RKWorkspace.Core.Plugins;

/// <summary>
/// Provides platform-neutral plugin registration and lifecycle management.
/// </summary>
public sealed class PluginManager : IPluginManager
{
    private readonly Dictionary<string, PluginRegistration> _plugins = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public PluginLoadResult RegisterPlugin(IPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return RegisterPlugin(plugin, PluginDescriptor.FromPlugin(plugin));
    }

    /// <inheritdoc />
    public PluginLoadResult RegisterPlugin(IPlugin plugin, PluginDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(descriptor);

        var validation = ValidatePluginAndDescriptor(plugin, descriptor);
        if (validation is not null)
        {
            return PluginLoadResult.Failed(validation.PluginId, validation);
        }

        if (_plugins.ContainsKey(plugin.PluginId))
        {
            return Failure(
                plugin.PluginId,
                PluginErrorCode.PluginAlreadyRegistered,
                $"Plugin '{plugin.PluginId}' is already registered.");
        }

        var missingDependency = descriptor.Dependencies.FirstOrDefault(dependency => !IsRegistered(dependency));
        if (!string.IsNullOrWhiteSpace(missingDependency))
        {
            return Failure(
                plugin.PluginId,
                PluginErrorCode.DependencyMissing,
                $"Plugin '{plugin.PluginId}' requires missing dependency '{missingDependency}'.");
        }

        if (!descriptor.IsEnabled || plugin.State == PluginState.Disabled)
        {
            _plugins.Add(plugin.PluginId, new PluginRegistration(plugin, descriptor));
            return PluginLoadResult.Successful(plugin.PluginId, PluginState.Disabled);
        }

        try
        {
            plugin.Initialize();
            _plugins.Add(plugin.PluginId, new PluginRegistration(plugin, descriptor));
            return PluginLoadResult.Successful(plugin.PluginId, plugin.State);
        }
        catch (Exception ex)
        {
            return Failure(
                plugin.PluginId,
                PluginErrorCode.InitializationFailed,
                $"Plugin '{plugin.PluginId}' failed to initialize.",
                ex);
        }
    }

    /// <inheritdoc />
    public PluginLoadResult UnregisterPlugin(string pluginId)
    {
        if (!TryGetRegisteredPlugin(pluginId, out var registration, out var error))
        {
            return PluginLoadResult.Failed(pluginId, error);
        }

        if (registration.Plugin.State == PluginState.Activated)
        {
            var deactivate = DeactivatePlugin(pluginId);
            if (!deactivate.Success)
            {
                return deactivate;
            }
        }

        if (registration.Plugin.State != PluginState.Unloaded)
        {
            var unload = UnloadPlugin(pluginId);
            if (!unload.Success)
            {
                return unload;
            }
        }

        _plugins.Remove(registration.Plugin.PluginId);
        return PluginLoadResult.Successful(registration.Plugin.PluginId, PluginState.Unloaded);
    }

    /// <inheritdoc />
    public IPlugin? GetPlugin(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            return null;
        }

        return _plugins.TryGetValue(pluginId, out var registration)
            ? registration.Plugin
            : null;
    }

    /// <inheritdoc />
    public PluginDescriptor? GetDescriptor(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            return null;
        }

        return _plugins.TryGetValue(pluginId, out var registration)
            ? registration.Descriptor
            : null;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IPlugin> GetPlugins()
    {
        return _plugins.Values
            .Select(registration => registration.Plugin)
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IPlugin> GetPluginsByType(PluginType type)
    {
        return _plugins.Values
            .Where(registration => registration.Plugin.Type == type)
            .Select(registration => registration.Plugin)
            .ToArray();
    }

    /// <inheritdoc />
    public PluginLoadResult ActivatePlugin(string pluginId)
    {
        if (!TryGetRegisteredPlugin(pluginId, out var registration, out var error))
        {
            return PluginLoadResult.Failed(pluginId, error);
        }

        if (registration.Plugin.State is not (PluginState.Loaded or PluginState.Deactivated))
        {
            return InvalidState(
                registration.Plugin,
                $"Plugin '{registration.Plugin.PluginId}' cannot activate from state {registration.Plugin.State}.");
        }

        try
        {
            registration.Plugin.Activate();
            return PluginLoadResult.Successful(registration.Plugin.PluginId, registration.Plugin.State);
        }
        catch (Exception ex)
        {
            return Failure(
                registration.Plugin.PluginId,
                PluginErrorCode.ActivationFailed,
                $"Plugin '{registration.Plugin.PluginId}' failed to activate.",
                ex);
        }
    }

    /// <inheritdoc />
    public PluginLoadResult DeactivatePlugin(string pluginId)
    {
        if (!TryGetRegisteredPlugin(pluginId, out var registration, out var error))
        {
            return PluginLoadResult.Failed(pluginId, error);
        }

        if (registration.Plugin.State != PluginState.Activated)
        {
            return InvalidState(
                registration.Plugin,
                $"Plugin '{registration.Plugin.PluginId}' cannot deactivate from state {registration.Plugin.State}.");
        }

        try
        {
            registration.Plugin.Deactivate();
            return PluginLoadResult.Successful(registration.Plugin.PluginId, registration.Plugin.State);
        }
        catch (Exception ex)
        {
            return Failure(
                registration.Plugin.PluginId,
                PluginErrorCode.DeactivationFailed,
                $"Plugin '{registration.Plugin.PluginId}' failed to deactivate.",
                ex);
        }
    }

    /// <inheritdoc />
    public PluginLoadResult UnloadPlugin(string pluginId)
    {
        if (!TryGetRegisteredPlugin(pluginId, out var registration, out var error))
        {
            return PluginLoadResult.Failed(pluginId, error);
        }

        if (registration.Plugin.State == PluginState.Activated)
        {
            return InvalidState(
                registration.Plugin,
                $"Plugin '{registration.Plugin.PluginId}' must be deactivated before unloading.");
        }

        if (registration.Plugin.State == PluginState.Unloaded)
        {
            return PluginLoadResult.Successful(registration.Plugin.PluginId, PluginState.Unloaded);
        }

        try
        {
            registration.Plugin.Shutdown();
            return PluginLoadResult.Successful(registration.Plugin.PluginId, registration.Plugin.State);
        }
        catch (Exception ex)
        {
            return Failure(
                registration.Plugin.PluginId,
                PluginErrorCode.ShutdownFailed,
                $"Plugin '{registration.Plugin.PluginId}' failed to shut down.",
                ex);
        }
    }

    /// <inheritdoc />
    public bool IsRegistered(string pluginId)
    {
        return !string.IsNullOrWhiteSpace(pluginId) && _plugins.ContainsKey(pluginId);
    }

    private static PluginException? ValidatePluginAndDescriptor(IPlugin plugin, PluginDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(plugin.PluginId))
        {
            return new PluginException(
                PluginErrorCode.MissingPluginId,
                plugin.PluginId,
                "Plugin id is required.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.PluginId))
        {
            return new PluginException(
                PluginErrorCode.MissingPluginId,
                descriptor.PluginId,
                "Descriptor plugin id is required.");
        }

        if (!string.Equals(plugin.PluginId, descriptor.PluginId, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(plugin.Name, descriptor.Name, StringComparison.Ordinal)
            || !string.Equals(plugin.Version, descriptor.Version, StringComparison.Ordinal)
            || plugin.Type != descriptor.Type)
        {
            return new PluginException(
                PluginErrorCode.InvalidDescriptor,
                plugin.PluginId,
                $"Descriptor metadata does not match plugin '{plugin.PluginId}'.");
        }

        return null;
    }

    private bool TryGetRegisteredPlugin(
        string pluginId,
        out PluginRegistration registration,
        out PluginException error)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            registration = default!;
            error = new PluginException(
                PluginErrorCode.MissingPluginId,
                pluginId,
                "Plugin id is required.");
            return false;
        }

        if (!_plugins.TryGetValue(pluginId, out registration!))
        {
            error = new PluginException(
                PluginErrorCode.PluginNotRegistered,
                pluginId,
                $"Plugin '{pluginId}' is not registered.");
            return false;
        }

        error = null!;
        return true;
    }

    private static PluginLoadResult InvalidState(IPlugin plugin, string message)
    {
        return Failure(plugin.PluginId, PluginErrorCode.InvalidState, message);
    }

    private static PluginLoadResult Failure(
        string? pluginId,
        PluginErrorCode code,
        string message,
        Exception? innerException = null)
    {
        return PluginLoadResult.Failed(
            pluginId,
            new PluginException(code, pluginId, message, innerException));
    }

    private sealed record PluginRegistration(IPlugin Plugin, PluginDescriptor Descriptor);
}
