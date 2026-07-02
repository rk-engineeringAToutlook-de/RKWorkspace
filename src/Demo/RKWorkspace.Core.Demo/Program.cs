using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Plugins;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Workspaces;

try
{
    Console.WriteLine("RK Workspace Core Demo");
    Console.WriteLine("======================");

    var pluginManager = new PluginManager();
    Console.WriteLine("[OK] Plugin Manager initialized");

    var capabilityManager = new CapabilityManager();
    Console.WriteLine("[OK] Capability Manager initialized");

    var workspaceRegistry = new WorkspaceRegistry();
    Console.WriteLine("[OK] Workspace Registry initialized");

    var transferObjectManager = new TransferObjectManager();
    Console.WriteLine("[OK] Transfer Object Manager initialized");

    var demoPlugin = new DemoPlugin(
        "demo.core-flow",
        "Core demo flow",
        new[]
        {
            CapabilityId.Display.ToString(),
            CapabilityId.Keyboard.ToString(),
            CapabilityId.Clipboard.ToString(),
            CapabilityId.Encryption.ToString(),
            CapabilityId.Pairing.ToString()
        });
    var pluginResult = pluginManager.RegisterPlugin(demoPlugin);
    Ensure(pluginResult.Success, $"Plugin registration failed: {pluginResult.Error?.Code}");
    var activationResult = pluginManager.ActivatePlugin(demoPlugin.PluginId);
    Ensure(activationResult.Success, $"Plugin activation failed: {activationResult.Error?.Code}");
    Console.WriteLine("[OK] Demo plugin registered and activated");

    var source = Workspace(
        "RKWS-Demo-Laptop",
        WorkspacePosition.Center,
        priority: 10,
        CapabilityId.Display,
        CapabilityId.Keyboard,
        CapabilityId.Clipboard,
        CapabilityId.Encryption,
        CapabilityId.Pairing);
    Console.WriteLine("[OK] Workspace A created: RKWS-Demo-Laptop");

    var target = Workspace(
        "RKWS-Demo-Display-Right",
        WorkspacePosition.Right,
        priority: 5,
        CapabilityId.Display,
        CapabilityId.Clipboard,
        CapabilityId.Encryption,
        CapabilityId.Pairing);
    Console.WriteLine("[OK] Workspace B created: RKWS-Demo-Display-Right");

    Console.WriteLine("[OK] Capabilities assigned");

    RegisterWorkspace(workspaceRegistry, capabilityManager, source);
    Console.WriteLine("[OK] Workspace A registered: RKWS-Demo-Laptop");

    RegisterWorkspace(workspaceRegistry, capabilityManager, target);
    Console.WriteLine("[OK] Workspace B registered: RKWS-Demo-Display-Right");

    var sourceRequirement = CapabilityRequirement.Require(
        CapabilityId.Display,
        CapabilityId.Keyboard,
        CapabilityId.Clipboard,
        CapabilityId.Encryption,
        CapabilityId.Pairing);
    var sourceMatch = capabilityManager.MatchRequirement(source.Capabilities, sourceRequirement);
    Ensure(sourceMatch.IsMatch, "Source capabilities do not match demo requirements.");
    Console.WriteLine("[OK] Source capabilities checked");

    var transferObject = transferObjectManager.Create(
        TransferObjectType.Text,
        Metadata(source.WorkspaceId.ToString()));
    Console.WriteLine("[OK] Transfer object created: Text");

    transferObjectManager.Validate(transferObject.Id);
    Console.WriteLine("[OK] Transfer object validated");

    var targetRequirement = new CapabilityRequirement
    {
        RequiredCapabilities = new[]
        {
            CapabilityId.Display,
            CapabilityId.Clipboard,
            CapabilityId.Encryption,
            CapabilityId.Pairing
        }
    };
    var selectedTarget = workspaceRegistry.GetBestTarget(new WorkspaceQuery
    {
        Position = WorkspacePosition.Right,
        TrustedOnly = true,
        AvailableOnly = true,
        RequiredCapabilities = CapabilitySet.FromIds(targetRequirement.RequiredCapabilities)
    });
    var targetMatch = capabilityManager.MatchRequirement(selectedTarget.Capabilities, targetRequirement);
    Ensure(targetMatch.IsMatch, "Selected target capabilities do not match demo requirements.");
    Ensure(
        selectedTarget.Descriptor.WorkspaceId == target.WorkspaceId,
        $"Unexpected target selected: {selectedTarget.Descriptor.WorkspaceId}");
    Console.WriteLine("[OK] Target resolved: RKWS-Demo-Display-Right");

    transferObjectManager.UpdateMetadata(
        transferObject.Id,
        transferObject.Metadata with
        {
            TargetWorkspace = selectedTarget.Descriptor.WorkspaceId.ToString(),
            ModifiedAt = new DateTimeOffset(2026, 7, 2, 13, 1, 0, TimeSpan.Zero)
        });
    Console.WriteLine("[OK] Target workspace written to transfer object");

    transferObjectManager.UpdateState(transferObject.Id, TransferObjectState.Prepared);
    Console.WriteLine("[OK] Transfer prepared");

    transferObjectManager.UpdateState(transferObject.Id, TransferObjectState.Completed);
    Console.WriteLine("[OK] Transfer completed");

    var finalObject = transferObjectManager.Get(transferObject.Id);
    if (finalObject is null)
    {
        throw new InvalidOperationException("Transfer object not found after completion.");
    }

    Ensure(finalObject.State == TransferObjectState.Completed, $"Unexpected final state: {finalObject.State}");

    Console.WriteLine();
    Console.WriteLine("Transfer History:");
    var history = FriendlyHistory(finalObject.History).ToArray();
    for (var index = 0; index < history.Length; index++)
    {
        Console.WriteLine($"{index + 1:00} {history[index]}");
    }

    Console.WriteLine();
    Console.WriteLine($"Source: {source.WorkspaceId}");
    Console.WriteLine($"Target: {selectedTarget.Descriptor.WorkspaceId}");
    Console.WriteLine("Direction: Right");
    Console.WriteLine("ObjectType: Text");
    Console.WriteLine("Text: \"Hallo von RK Workspace\"");
    Console.WriteLine($"FinalState: {finalObject.State}");
    Console.WriteLine();
    Console.WriteLine("RESULT: SUCCESS");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("RESULT: FAILED");
    Console.WriteLine($"Reason: {ex.Message}");
    return 1;
}

static void RegisterWorkspace(
    WorkspaceRegistry workspaceRegistry,
    CapabilityManager capabilityManager,
    WorkspaceDescriptor descriptor)
{
    workspaceRegistry.RegisterWorkspace(RKWorkspace.Core.Workspaces.Workspace.FromDescriptor(descriptor));
    capabilityManager.RegisterProvider(new WorkspaceCapabilityProvider(descriptor));
}

static WorkspaceDescriptor Workspace(
    string workspaceId,
    WorkspacePosition position,
    int priority,
    params CapabilityId[] capabilityIds)
{
    return new WorkspaceDescriptor
    {
        WorkspaceId = WorkspaceId.Create(workspaceId),
        DisplayName = workspaceId,
        WorkspaceType = WorkspaceType.SmartDevice,
        WorkspaceState = WorkspaceState.Available,
        Position = position,
        Capabilities = CapabilitySet.FromIds(capabilityIds),
        IsTrusted = true,
        Priority = priority,
        LastSeen = new DateTimeOffset(2026, 7, 2, 13, 0, 0, TimeSpan.Zero),
        Metadata = new Dictionary<string, string>
        {
            ["demo"] = "core-runner"
        }
    };
}

static TransferMetadata Metadata(string sourceWorkspace)
{
    return new TransferMetadata
    {
        ObjectId = TransferObjectId.NewId(),
        DisplayName = "Hallo von RK Workspace",
        MimeType = "text/plain; charset=utf-8",
        Size = "Hallo von RK Workspace".Length,
        Checksum = "sha256:demo-text",
        CreatedAt = new DateTimeOffset(2026, 7, 2, 13, 0, 30, TimeSpan.Zero),
        ModifiedAt = new DateTimeOffset(2026, 7, 2, 13, 0, 30, TimeSpan.Zero),
        SourceWorkspace = sourceWorkspace,
        TargetWorkspace = string.Empty,
        Owner = "demo",
        Priority = 1,
        Tags = new[] { "demo", "text" },
        Version = "1.0.0"
    };
}

static IEnumerable<string> FriendlyHistory(IEnumerable<TransferHistoryEntry> history)
{
    foreach (var entry in history)
    {
        yield return entry.Action switch
        {
            "State:Validated" => "Validated",
            "MetadataUpdated" => "TargetResolved",
            "State:Prepared" => "Prepared",
            "State:Completed" => "Completed",
            _ => entry.Action
        };
    }
}

static void Ensure(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

internal sealed class WorkspaceCapabilityProvider : ICapabilityProvider
{
    private readonly WorkspaceDescriptor _descriptor;

    public WorkspaceCapabilityProvider(WorkspaceDescriptor descriptor)
    {
        _descriptor = descriptor.Snapshot();
    }

    public string ProviderId => $"workspace:{_descriptor.WorkspaceId}";

    public string DisplayName => _descriptor.DisplayName;

    public CapabilitySet GetCapabilities()
    {
        return _descriptor.Capabilities.Snapshot();
    }
}

internal sealed class DemoPlugin : IPlugin
{
    private readonly string[] _capabilities;

    public DemoPlugin(string pluginId, string name, string[] capabilities)
    {
        PluginId = pluginId;
        Name = name;
        Version = "1.0.0";
        Type = PluginType.Simulation;
        State = PluginState.Discovered;
        _capabilities = capabilities;
    }

    public string PluginId { get; }

    public string Name { get; }

    public string Version { get; }

    public PluginType Type { get; }

    public PluginState State { get; private set; }

    public IReadOnlyCollection<string> Capabilities => _capabilities;

    public void Initialize()
    {
        State = PluginState.Loaded;
    }

    public void Activate()
    {
        State = PluginState.Activated;
    }

    public void Deactivate()
    {
        State = PluginState.Deactivated;
    }

    public void Shutdown()
    {
        State = PluginState.Unloaded;
    }
}
