using RKWorkspace.Core.Plugins;

internal sealed class FakePlugin : IPlugin
{
    private readonly string[] _capabilities;

    private FakePlugin(string pluginId, PluginType type, string name, string[] capabilities)
    {
        PluginId = pluginId;
        Type = type;
        Name = name;
        Version = "1.0.0";
        State = PluginState.Discovered;
        _capabilities = capabilities;
    }

    public string PluginId { get; }

    public string Name { get; }

    public string Version { get; }

    public PluginType Type { get; }

    public PluginState State { get; private set; }

    public IReadOnlyCollection<string> Capabilities => _capabilities;

    public bool ThrowOnInitialize { get; init; }

    public bool ThrowOnActivate { get; set; }

    public bool ThrowOnDeactivate { get; init; }

    public bool ThrowOnShutdown { get; init; }

    public int InitializeCount { get; private set; }

    public int ActivateCount { get; private set; }

    public int DeactivateCount { get; private set; }

    public int ShutdownCount { get; private set; }

    public static FakePlugin Create(string pluginId, PluginType type, string name)
    {
        return new FakePlugin(pluginId, type, name, new[] { $"{type}.Capability" });
    }

    public void Initialize()
    {
        InitializeCount++;
        if (ThrowOnInitialize)
        {
            State = PluginState.Failed;
            throw new InvalidOperationException("Fake initialization failure.");
        }

        State = PluginState.Loaded;
    }

    public void Activate()
    {
        ActivateCount++;
        if (ThrowOnActivate)
        {
            State = PluginState.Failed;
            throw new InvalidOperationException("Fake activation failure.");
        }

        State = PluginState.Activated;
    }

    public void Deactivate()
    {
        DeactivateCount++;
        if (ThrowOnDeactivate)
        {
            State = PluginState.Failed;
            throw new InvalidOperationException("Fake deactivation failure.");
        }

        State = PluginState.Deactivated;
    }

    public void Shutdown()
    {
        ShutdownCount++;
        if (ThrowOnShutdown)
        {
            State = PluginState.Failed;
            throw new InvalidOperationException("Fake shutdown failure.");
        }

        State = PluginState.Unloaded;
    }
}
