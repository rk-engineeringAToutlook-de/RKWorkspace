using RKWorkspace.Core.Models;
using RKWorkspace.Core.Plugins;
using RKWorkspace.Core.Services;
using RKWorkspace.Core.Simulation;

var tests = new (string Name, Action Body)[]
{
    ("Workspace capability checks match transfer object types", WorkspaceCapabilitiesMatchObjectTypes),
    ("WorkspaceMap resolves the right-side target", WorkspaceMapResolvesRightTarget),
    ("TransferPlanner creates a text transfer from A to B", TransferPlannerCreatesTextTransfer),
    ("TransferPlanner rejects untrusted targets", TransferPlannerRejectsUntrustedTarget),
    ("DeviceIdentity creates required identity fields", DeviceIdentityCreatesRequiredFields),
    ("Local simulation logs a complete text transfer from A to B", LocalSimulationLogsCompleteTextTransfer),
    ("PluginManager registers and initializes plugins", PluginManagerRegistersAndInitializesPlugins),
    ("PluginManager prevents duplicate plugin ids", PluginManagerPreventsDuplicatePluginIds),
    ("PluginManager gets plugins and returns null for unknown ids", PluginManagerGetsPluginsAndUnknownPluginReturnsNull),
    ("PluginManager filters plugins by type", PluginManagerFiltersPluginsByType),
    ("PluginManager activates and deactivates plugins", PluginManagerActivatesAndDeactivatesPlugins),
    ("PluginManager unloads plugins", PluginManagerUnloadsPlugins),
    ("PluginManager unregisters plugins", PluginManagerUnregistersPlugins),
    ("PluginManager rejects missing plugin ids", PluginManagerRejectsMissingPluginIds),
    ("PluginManager reports missing dependencies", PluginManagerReportsMissingDependencies),
    ("PluginManager rejects invalid lifecycle transitions", PluginManagerRejectsInvalidLifecycleTransitions),
    ("PluginManager preserves lifecycle exception details", PluginManagerPreservesLifecycleExceptionDetails),
    ("Plugin core assembly has no platform dependencies", PluginCoreAssemblyHasNoPlatformDependencies)
};

var failed = 0;

foreach (var test in tests)
{
    try
    {
        test.Body();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"FAIL {test.Name}");
        Console.WriteLine($"     {ex.GetType().Name}: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine(failed == 0
    ? $"All {tests.Length} tests passed."
    : $"{failed} of {tests.Length} tests failed.");

return failed == 0 ? 0 : 1;

static void WorkspaceCapabilitiesMatchObjectTypes()
{
    var workspace = TestWorkspace(
        "workspace-a",
        WorkspacePosition.Center,
        WorkspaceCapability.TextTransfer | WorkspaceCapability.PdfTransfer);

    Assert.True(workspace.Supports(TransferObjectType.Text));
    Assert.True(workspace.Supports(TransferObjectType.Pdf));
    Assert.False(workspace.Supports(TransferObjectType.Image));
}

static void WorkspaceMapResolvesRightTarget()
{
    var source = TestWorkspace("workspace-a", WorkspacePosition.Center);
    var target = TestWorkspace("workspace-b", WorkspacePosition.Right);

    var map = new WorkspaceMap(new[] { source, target });

    Assert.Equal(target.WorkspaceId, map.ResolveDirectionalTarget(WorkspacePosition.Right).WorkspaceId);
    Assert.Throws<ArgumentException>(() => map.ResolveDirectionalTarget(WorkspacePosition.Center));
}

static void TransferPlannerCreatesTextTransfer()
{
    var source = TestWorkspace("workspace-a", WorkspacePosition.Center);
    var target = TestWorkspace("workspace-b", WorkspacePosition.Right);
    var planner = new TransferPlanner();

    var transfer = planner.PlanTextTransfer(
        source,
        new WorkspaceMap(new[] { source, target }),
        WorkspacePosition.Right,
        "Hallo RKWS",
        "Greeting.txt");

    Assert.Equal(TransferObjectType.Text, transfer.ObjectType);
    Assert.Equal(source.WorkspaceId, transfer.SourceWorkspaceId);
    Assert.Equal(target.WorkspaceId, transfer.TargetWorkspaceId);
    Assert.Equal("Greeting.txt", transfer.DisplayName);
    Assert.Equal("text/plain; charset=utf-8", transfer.MimeType);
    Assert.Equal(TransferStatus.TransferPending, transfer.TransferStatus);
    Assert.Equal(10, transfer.Size);
    Assert.Equal(64, transfer.Checksum.Length);
    Assert.Equal(PayloadReferenceKind.InlineText, transfer.PayloadReference.Kind);
}

static void TransferPlannerRejectsUntrustedTarget()
{
    var source = TestWorkspace("workspace-a", WorkspacePosition.Center);
    var target = TestWorkspace("workspace-b", WorkspacePosition.Right) with
    {
        TrustState = TrustState.Untrusted
    };

    var planner = new TransferPlanner();

    Assert.Throws<InvalidOperationException>(() => planner.PlanTextTransfer(
        source,
        new WorkspaceMap(new[] { source, target }),
        WorkspacePosition.Right,
        "No transfer"));
}

static void DeviceIdentityCreatesRequiredFields()
{
    var identity = DeviceIdentity.Create(
        "RK Windows Laptop",
        WorkspacePlatform.Windows,
        "sha256:test");

    Assert.StartsWith("rkws-dev-", identity.DeviceId);
    Assert.Equal("RK Windows Laptop", identity.DisplayName);
    Assert.Equal(WorkspacePlatform.Windows, identity.Platform);
    Assert.Equal("sha256:test", identity.PublicKeyFingerprint);
    Assert.Equal(6, identity.PairingId.Length);
    Assert.True(identity.CreatedAt <= identity.LastSeen);
}

static void LocalSimulationLogsCompleteTextTransfer()
{
    var simulation = new LocalTransferSimulation();

    var result = simulation.RunTextTransferFromAToB();

    Assert.True(result.Success);
    Assert.Equal("workspace-a", result.SourceWorkspace.WorkspaceId);
    Assert.Equal("workspace-b", result.TargetWorkspace.WorkspaceId);
    Assert.Equal("workspace-b", result.TransferObject.TargetWorkspaceId);
    Assert.Equal(TransferObjectType.Text, result.TransferObject.ObjectType);
    Assert.Equal(TransferStatus.TransferPending, result.TransferObject.TransferStatus);
    Assert.True(result.Log.Count >= 10);
    Assert.ContainsStage("SIMULATION_START", result.Log);
    Assert.ContainsStage("TARGET_RESOLVED", result.Log);
    Assert.ContainsStage("TRANSFER_OBJECT_CREATED", result.Log);
    Assert.ContainsStage("SIMULATION_COMPLETE", result.Log);

    for (var i = 0; i < result.Log.Count; i++)
    {
        Assert.Equal(i + 1, result.Log[i].Sequence);
    }
}

static void PluginManagerRegistersAndInitializesPlugins()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.workspace", PluginType.Workspace, "Workspace plugin");

    var result = manager.RegisterPlugin(plugin);

    Assert.Success(result);
    Assert.Equal(PluginState.Loaded, plugin.State);
    Assert.Equal(1, plugin.InitializeCount);
    Assert.True(manager.IsRegistered(plugin.PluginId));
    Assert.Same(plugin, Assert.NotNull(manager.GetPlugin(plugin.PluginId)));
}

static void PluginManagerPreventsDuplicatePluginIds()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.workspace", PluginType.Workspace, "Workspace plugin");
    var duplicate = FakePlugin.Create("plugin.workspace", PluginType.Transfer, "Duplicate plugin");

    Assert.Success(manager.RegisterPlugin(plugin));
    var result = manager.RegisterPlugin(duplicate);

    Assert.Failure(result, PluginErrorCode.PluginAlreadyRegistered);
}

static void PluginManagerGetsPluginsAndUnknownPluginReturnsNull()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.transfer", PluginType.Transfer, "Transfer plugin");

    Assert.Success(manager.RegisterPlugin(plugin));

    Assert.Same(plugin, Assert.NotNull(manager.GetPlugin(plugin.PluginId)));
    Assert.NotNull(manager.GetDescriptor(plugin.PluginId));
    Assert.Null(manager.GetPlugin("missing"));
    Assert.Null(manager.GetDescriptor("missing"));
    Assert.False(manager.IsRegistered("missing"));
}

static void PluginManagerFiltersPluginsByType()
{
    var manager = new PluginManager();
    var workspacePlugin = FakePlugin.Create("plugin.workspace", PluginType.Workspace, "Workspace plugin");
    var transferPlugin = FakePlugin.Create("plugin.transfer", PluginType.Transfer, "Transfer plugin");

    Assert.Success(manager.RegisterPlugin(workspacePlugin));
    Assert.Success(manager.RegisterPlugin(transferPlugin));

    var workspacePlugins = manager.GetPluginsByType(PluginType.Workspace);

    Assert.Equal(1, workspacePlugins.Count);
    Assert.Same(workspacePlugin, workspacePlugins.Single());
    Assert.Equal(2, manager.GetPlugins().Count);
}

static void PluginManagerActivatesAndDeactivatesPlugins()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.transfer", PluginType.Transfer, "Transfer plugin");

    Assert.Success(manager.RegisterPlugin(plugin));
    Assert.Success(manager.ActivatePlugin(plugin.PluginId));
    Assert.Equal(PluginState.Activated, plugin.State);
    Assert.Equal(1, plugin.ActivateCount);

    Assert.Success(manager.DeactivatePlugin(plugin.PluginId));
    Assert.Equal(PluginState.Deactivated, plugin.State);
    Assert.Equal(1, plugin.DeactivateCount);
}

static void PluginManagerUnloadsPlugins()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.transfer", PluginType.Transfer, "Transfer plugin");

    Assert.Success(manager.RegisterPlugin(plugin));
    Assert.Success(manager.UnloadPlugin(plugin.PluginId));

    Assert.Equal(PluginState.Unloaded, plugin.State);
    Assert.Equal(1, plugin.ShutdownCount);
    Assert.True(manager.IsRegistered(plugin.PluginId));
}

static void PluginManagerUnregistersPlugins()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.transfer", PluginType.Transfer, "Transfer plugin");

    Assert.Success(manager.RegisterPlugin(plugin));
    Assert.Success(manager.ActivatePlugin(plugin.PluginId));
    Assert.Success(manager.UnregisterPlugin(plugin.PluginId));

    Assert.False(manager.IsRegistered(plugin.PluginId));
    Assert.Equal(PluginState.Unloaded, plugin.State);
    Assert.Equal(1, plugin.DeactivateCount);
    Assert.Equal(1, plugin.ShutdownCount);
}

static void PluginManagerRejectsMissingPluginIds()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create(string.Empty, PluginType.Workspace, "Missing id plugin");

    var result = manager.RegisterPlugin(plugin);

    Assert.Failure(result, PluginErrorCode.MissingPluginId);
}

static void PluginManagerReportsMissingDependencies()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.dependent", PluginType.Transfer, "Dependent plugin");
    var descriptor = new PluginDescriptor
    {
        PluginId = plugin.PluginId,
        Name = plugin.Name,
        Version = plugin.Version,
        Type = plugin.Type,
        Dependencies = new[] { "plugin.missing" }
    };

    var result = manager.RegisterPlugin(plugin, descriptor);

    Assert.Failure(result, PluginErrorCode.DependencyMissing);
    Assert.False(manager.IsRegistered(plugin.PluginId));
}

static void PluginManagerRejectsInvalidLifecycleTransitions()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.transfer", PluginType.Transfer, "Transfer plugin");

    Assert.Success(manager.RegisterPlugin(plugin));
    Assert.Failure(manager.DeactivatePlugin(plugin.PluginId), PluginErrorCode.InvalidState);
    Assert.Success(manager.ActivatePlugin(plugin.PluginId));
    Assert.Failure(manager.UnloadPlugin(plugin.PluginId), PluginErrorCode.InvalidState);
}

static void PluginManagerPreservesLifecycleExceptionDetails()
{
    var manager = new PluginManager();
    var plugin = FakePlugin.Create("plugin.transfer", PluginType.Transfer, "Transfer plugin");
    plugin.ThrowOnActivate = true;

    Assert.Success(manager.RegisterPlugin(plugin));

    var result = manager.ActivatePlugin(plugin.PluginId);

    Assert.Failure(result, PluginErrorCode.ActivationFailed);
    Assert.NotNull(result.Error?.InnerException);
    Assert.Equal(PluginState.Failed, plugin.State);
}

static void PluginCoreAssemblyHasNoPlatformDependencies()
{
    var forbiddenFragments = new[]
    {
        "Windows",
        "Presentation",
        "WinForms",
        "Wpf",
        "UIKit",
        "AppKit",
        "Android"
    };

    var references = typeof(PluginManager)
        .Assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => forbiddenFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

    Assert.Equal(0, references.Length);
}

static Workspace TestWorkspace(
    string workspaceId,
    WorkspacePosition position,
    WorkspaceCapability capabilities =
        WorkspaceCapability.TextTransfer
        | WorkspaceCapability.FileTransfer
        | WorkspaceCapability.PdfTransfer
        | WorkspaceCapability.DirectionalTransfer
        | WorkspaceCapability.Pairing
        | WorkspaceCapability.Discovery)
{
    return Workspace.Create(
        workspaceId,
        workspaceId,
        $"device-{workspaceId}",
        WorkspacePlatform.Windows,
        capabilities,
        position,
        TrustState.Trusted);
}

internal static class Assert
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
        }
    }

    public static void True(bool value)
    {
        if (!value)
        {
            throw new InvalidOperationException("Expected true, got false.");
        }
    }

    public static void False(bool value)
    {
        if (value)
        {
            throw new InvalidOperationException("Expected false, got true.");
        }
    }

    public static void Null(object? value)
    {
        if (value is not null)
        {
            throw new InvalidOperationException($"Expected null, got {value}.");
        }
    }

    public static T NotNull<T>(T? value)
        where T : class
    {
        if (value is null)
        {
            throw new InvalidOperationException("Expected non-null value, got null.");
        }

        return value;
    }

    public static void Same(object expected, object actual)
    {
        if (!ReferenceEquals(expected, actual))
        {
            throw new InvalidOperationException("Expected references to be the same instance.");
        }
    }

    public static void Success(PluginLoadResult result)
    {
        if (!result.Success)
        {
            throw new InvalidOperationException($"Expected success, got {result.Error?.Code}: {result.Error?.Message}");
        }
    }

    public static void Failure(PluginLoadResult result, PluginErrorCode expectedCode)
    {
        if (result.Success)
        {
            throw new InvalidOperationException("Expected failure, got success.");
        }

        if (result.Error is null)
        {
            throw new InvalidOperationException("Expected plugin error, got null.");
        }

        Equal(expectedCode, result.Error.Code);
    }

    public static void StartsWith(string expectedPrefix, string actual)
    {
        if (!actual.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected prefix {expectedPrefix}, got {actual}.");
        }
    }

    public static void Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Expected exception {typeof(TException).Name}.");
    }

    public static void ContainsStage(string stage, IEnumerable<SimulationLogEntry> log)
    {
        if (!log.Any(entry => entry.Stage == stage))
        {
            throw new InvalidOperationException($"Expected log stage {stage}.");
        }
    }
}
