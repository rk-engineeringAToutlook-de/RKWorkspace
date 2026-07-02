using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Models;
using RKWorkspace.Core.Plugins;
using RKWorkspace.Core.Services;
using RKWorkspace.Core.Simulation;
using RegistryIWorkspace = RKWorkspace.Core.Workspaces.IWorkspace;
using RegistryIWorkspaceRegistry = RKWorkspace.Core.Workspaces.IWorkspaceRegistry;
using RegistryWorkspace = RKWorkspace.Core.Workspaces.Workspace;
using RegistryWorkspaceDescriptor = RKWorkspace.Core.Workspaces.WorkspaceDescriptor;
using RegistryWorkspaceErrorCode = RKWorkspace.Core.Workspaces.WorkspaceErrorCode;
using RegistryWorkspaceException = RKWorkspace.Core.Workspaces.WorkspaceException;
using RegistryWorkspaceId = RKWorkspace.Core.Workspaces.WorkspaceId;
using RegistryWorkspacePosition = RKWorkspace.Core.Workspaces.WorkspacePosition;
using RegistryWorkspaceQuery = RKWorkspace.Core.Workspaces.WorkspaceQuery;
using RegistryWorkspaceRegistry = RKWorkspace.Core.Workspaces.WorkspaceRegistry;
using RegistryWorkspaceState = RKWorkspace.Core.Workspaces.WorkspaceState;
using RegistryWorkspaceType = RKWorkspace.Core.Workspaces.WorkspaceType;

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
    ("Plugin core assembly has no platform dependencies", PluginCoreAssemblyHasNoPlatformDependencies),
    ("Capability ids and categories cover the architecture baseline", CapabilityIdsAndCategoriesCoverArchitectureBaseline),
    ("Capability creates default category and metadata", CapabilityCreatesDefaultCategoryAndMetadata),
    ("CapabilitySet adds capabilities and prevents duplicates", CapabilitySetAddsCapabilitiesAndPreventsDuplicates),
    ("CapabilitySet removes capabilities", CapabilitySetRemovesCapabilities),
    ("CapabilitySet checks all and any requirements", CapabilitySetChecksAllAndAnyRequirements),
    ("CapabilitySet intersects, differs and snapshots", CapabilitySetIntersectsDiffersAndSnapshots),
    ("CapabilityRequirement validates invalid ids", CapabilityRequirementValidatesInvalidIds),
    ("CapabilityManager matches requirements positively", CapabilityManagerMatchesRequirementsPositively),
    ("CapabilityManager reports missing required capabilities", CapabilityManagerReportsMissingRequiredCapabilities),
    ("CapabilityManager detects forbidden capabilities", CapabilityManagerDetectsForbiddenCapabilities),
    ("CapabilityManager registers providers", CapabilityManagerRegistersProviders),
    ("CapabilityManager prevents duplicate providers", CapabilityManagerPreventsDuplicateProviders),
    ("CapabilityManager unregisters providers", CapabilityManagerUnregistersProviders),
    ("CapabilityManager returns provider capability snapshots", CapabilityManagerReturnsProviderCapabilitySnapshots),
    ("CapabilityManager combines capabilities", CapabilityManagerCombinesCapabilities),
    ("CapabilityManager finds matching providers", CapabilityManagerFindsMatchingProviders),
    ("CapabilityManager reports capability availability", CapabilityManagerReportsCapabilityAvailability),
    ("CapabilityManager throws capability errors", CapabilityManagerThrowsCapabilityErrors),
    ("Capability core assembly has no platform dependencies", CapabilityCoreAssemblyHasNoPlatformDependencies),
    ("Workspace registry enums cover the architecture baseline", WorkspaceRegistryEnumsCoverArchitectureBaseline),
    ("WorkspaceId validates and compares ids", WorkspaceIdValidatesAndComparesIds),
    ("WorkspaceRegistry registers a workspace", WorkspaceRegistryRegistersWorkspace),
    ("WorkspaceRegistry prevents duplicate workspace ids", WorkspaceRegistryPreventsDuplicateWorkspaceIds),
    ("WorkspaceRegistry unregisters a workspace", WorkspaceRegistryUnregistersWorkspace),
    ("WorkspaceRegistry rejects unknown workspace removal", WorkspaceRegistryRejectsUnknownWorkspaceRemoval),
    ("WorkspaceRegistry updates a workspace", WorkspaceRegistryUpdatesWorkspace),
    ("WorkspaceRegistry gets workspaces", WorkspaceRegistryGetsWorkspaces),
    ("WorkspaceRegistry gets all workspaces", WorkspaceRegistryGetsAllWorkspaces),
    ("WorkspaceRegistry finds by position", WorkspaceRegistryFindsByPosition),
    ("WorkspaceRegistry finds by capability", WorkspaceRegistryFindsByCapability),
    ("WorkspaceRegistry finds matching workspaces", WorkspaceRegistryFindsMatchingWorkspaces),
    ("WorkspaceRegistry best target uses only target", WorkspaceRegistryBestTargetUsesOnlyTarget),
    ("WorkspaceRegistry best target prefers position", WorkspaceRegistryBestTargetPrefersPosition),
    ("WorkspaceRegistry best target prefers capabilities", WorkspaceRegistryBestTargetPrefersCapabilities),
    ("WorkspaceRegistry best target prefers trust", WorkspaceRegistryBestTargetPrefersTrust),
    ("WorkspaceRegistry best target prefers priority", WorkspaceRegistryBestTargetPrefersPriority),
    ("WorkspaceRegistry best target prefers last seen", WorkspaceRegistryBestTargetPrefersLastSeen),
    ("WorkspaceRegistry creates snapshots", WorkspaceRegistryCreatesSnapshots),
    ("WorkspaceRegistry throws workspace errors", WorkspaceRegistryThrowsWorkspaceErrors),
    ("Workspace registry core assembly has no platform dependencies", WorkspaceRegistryCoreAssemblyHasNoPlatformDependencies)
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

static void CapabilityIdsAndCategoriesCoverArchitectureBaseline()
{
    var expectedIds = new[]
    {
        CapabilityId.Touch,
        CapabilityId.Touchpad,
        CapabilityId.Mouse,
        CapabilityId.Keyboard,
        CapabilityId.Clipboard,
        CapabilityId.DragDrop,
        CapabilityId.BLE,
        CapabilityId.WiFi,
        CapabilityId.LAN,
        CapabilityId.USB,
        CapabilityId.USBC,
        CapabilityId.UWB,
        CapabilityId.Display,
        CapabilityId.MultipleDisplays,
        CapabilityId.HapticFeedback,
        CapabilityId.Animation,
        CapabilityId.Overlay,
        CapabilityId.Notification,
        CapabilityId.Logging,
        CapabilityId.Encryption,
        CapabilityId.Pairing,
        CapabilityId.OfflineMode,
        CapabilityId.CloudMode,
        CapabilityId.FirmwareUpdate,
        CapabilityId.HardwareNode,
        CapabilityId.DisplayNode,
        CapabilityId.ContextTransfer,
        CapabilityId.Simulation,
        CapabilityId.Testing,
        CapabilityId.Unknown
    };

    var expectedCategories = new[]
    {
        CapabilityCategory.Input,
        CapabilityCategory.Display,
        CapabilityCategory.Communication,
        CapabilityCategory.Transfer,
        CapabilityCategory.Security,
        CapabilityCategory.Hardware,
        CapabilityCategory.Firmware,
        CapabilityCategory.Platform,
        CapabilityCategory.UserExperience,
        CapabilityCategory.System,
        CapabilityCategory.Unknown
    };

    Assert.Equal(expectedIds.Length, Enum.GetValues<CapabilityId>().Length);
    Assert.Equal(expectedCategories.Length, Enum.GetValues<CapabilityCategory>().Length);
    Assert.True(expectedIds.All(id => Enum.IsDefined(id)));
    Assert.True(expectedCategories.All(category => Enum.IsDefined(category)));
}

static void CapabilityCreatesDefaultCategoryAndMetadata()
{
    var capability = Capability.Create(CapabilityId.Touchpad) with
    {
        Description = "Trackpad gestures",
        IsRequired = true,
        IsExperimental = true,
        Version = "1.2.3"
    };

    Assert.Equal(CapabilityId.Touchpad, capability.CapabilityId);
    Assert.Equal("Touchpad", capability.DisplayName);
    Assert.Equal(CapabilityCategory.Input, capability.Category);
    Assert.Equal("Trackpad gestures", capability.Description);
    Assert.True(capability.IsRequired);
    Assert.True(capability.IsExperimental);
    Assert.Equal("1.2.3", capability.Version);
    Assert.Equal(CapabilityCategory.Display, Capability.GetDefaultCategory(CapabilityId.MultipleDisplays));
    Assert.Equal(CapabilityCategory.Communication, Capability.GetDefaultCategory(CapabilityId.BLE));
    Assert.Equal(CapabilityCategory.Transfer, Capability.GetDefaultCategory(CapabilityId.ContextTransfer));
    Assert.Equal(CapabilityCategory.Security, Capability.GetDefaultCategory(CapabilityId.Encryption));
    Assert.Equal(CapabilityCategory.Hardware, Capability.GetDefaultCategory(CapabilityId.DisplayNode));
    Assert.Equal(CapabilityCategory.Firmware, Capability.GetDefaultCategory(CapabilityId.FirmwareUpdate));
    Assert.Equal(CapabilityCategory.UserExperience, Capability.GetDefaultCategory(CapabilityId.Overlay));
    Assert.Equal(CapabilityCategory.System, Capability.GetDefaultCategory(CapabilityId.Testing));
    Assert.Equal(CapabilityCategory.Unknown, Capability.GetDefaultCategory(CapabilityId.Unknown));
}

static void CapabilitySetAddsCapabilitiesAndPreventsDuplicates()
{
    var set = new CapabilitySet();

    set.Add(Capability.Create(CapabilityId.Mouse));
    set.Add(Capability.Create(CapabilityId.Mouse) with { DisplayName = "Pointer" });

    Assert.Equal(1, set.Count);
    Assert.True(set.Contains(CapabilityId.Mouse));
    Assert.Equal("Pointer", set.Capabilities.Single().DisplayName);
    Assert.ThrowsWithCode(
        CapabilityErrorCode.InvalidCapabilityId,
        () => set.Add(Capability.Create(CapabilityId.Unknown)));
}

static void CapabilitySetRemovesCapabilities()
{
    var set = CapabilitySet.FromIds(CapabilityId.Keyboard, CapabilityId.Mouse);

    Assert.True(set.Remove(CapabilityId.Keyboard));
    Assert.False(set.Contains(CapabilityId.Keyboard));
    Assert.False(set.Remove(CapabilityId.Keyboard));
    Assert.Equal(1, set.Count);
}

static void CapabilitySetChecksAllAndAnyRequirements()
{
    var set = CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.Notification);

    Assert.True(set.ContainsAll(new[] { CapabilityId.Display, CapabilityId.Notification }));
    Assert.False(set.ContainsAll(new[] { CapabilityId.Display, CapabilityId.Touch }));
    Assert.True(set.ContainsAny(new[] { CapabilityId.Touch, CapabilityId.Notification }));
    Assert.False(set.ContainsAny(new[] { CapabilityId.Touch, CapabilityId.Keyboard }));
}

static void CapabilitySetIntersectsDiffersAndSnapshots()
{
    var left = CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.Notification, CapabilityId.Animation);
    var right = CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.Touch);

    var intersection = left.Intersect(right);
    var difference = left.Difference(right);
    var snapshot = left.Snapshot();

    left.Remove(CapabilityId.Display);

    Assert.Equal(1, intersection.Count);
    Assert.True(intersection.Contains(CapabilityId.Display));
    Assert.Equal(2, difference.Count);
    Assert.True(difference.Contains(CapabilityId.Notification));
    Assert.True(difference.Contains(CapabilityId.Animation));
    Assert.True(snapshot.Contains(CapabilityId.Display));
}

static void CapabilityRequirementValidatesInvalidIds()
{
    CapabilityRequirement.Empty.Validate();
    CapabilityRequirement.Require(CapabilityId.Pairing).Validate();

    var requirement = new CapabilityRequirement
    {
        RequiredCapabilities = new[] { CapabilityId.Unknown }
    };

    Assert.ThrowsWithCode(CapabilityErrorCode.InvalidRequirement, requirement.Validate);
}

static void CapabilityManagerMatchesRequirementsPositively()
{
    var manager = new CapabilityManager();
    var capabilities = CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.Notification, CapabilityId.Animation);
    var requirement = new CapabilityRequirement
    {
        RequiredCapabilities = new[] { CapabilityId.Display },
        OptionalCapabilities = new[] { CapabilityId.Notification, CapabilityId.Touch }
    };

    var result = manager.MatchRequirement(capabilities, requirement);

    Assert.True(result.IsMatch);
    Assert.Equal(11, result.Score);
    Assert.Equal(0, result.MissingRequiredCapabilities.Count);
    Assert.Equal(1, result.PresentOptionalCapabilities.Count);
    Assert.True(result.PresentOptionalCapabilities.Contains(CapabilityId.Notification));
    Assert.Equal("Requirement matched.", result.Reason);
}

static void CapabilityManagerReportsMissingRequiredCapabilities()
{
    var manager = new CapabilityManager();
    var result = manager.MatchRequirement(
        CapabilitySet.FromIds(CapabilityId.Display),
        CapabilityRequirement.Require(CapabilityId.Display, CapabilityId.Pairing));

    Assert.False(result.IsMatch);
    Assert.Equal(0, result.Score);
    Assert.True(result.MissingRequiredCapabilities.Contains(CapabilityId.Pairing));
    Assert.True(result.Reason.Contains("Missing required", StringComparison.Ordinal));
}

static void CapabilityManagerDetectsForbiddenCapabilities()
{
    var manager = new CapabilityManager();
    var requirement = new CapabilityRequirement
    {
        RequiredCapabilities = new[] { CapabilityId.Display },
        ForbiddenCapabilities = new[] { CapabilityId.CloudMode }
    };

    var result = manager.MatchRequirement(
        CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.CloudMode),
        requirement);

    Assert.False(result.IsMatch);
    Assert.Equal(0, result.Score);
    Assert.True(result.PresentForbiddenCapabilities.Contains(CapabilityId.CloudMode));
    Assert.True(result.Reason.Contains("Forbidden present", StringComparison.Ordinal));
}

static void CapabilityManagerRegistersProviders()
{
    var manager = new CapabilityManager();
    var provider = FakeCapabilityProvider.Create(
        "provider.display",
        "Display provider",
        CapabilityId.Display,
        CapabilityId.Overlay);

    manager.RegisterProvider(provider);

    Assert.Same(provider, Assert.NotNull(manager.GetProvider(provider.ProviderId)));
    Assert.Equal(1, manager.GetAllProviders().Count);
}

static void CapabilityManagerPreventsDuplicateProviders()
{
    var manager = new CapabilityManager();
    var provider = FakeCapabilityProvider.Create("provider.input", "Input provider", CapabilityId.Keyboard);
    var duplicate = FakeCapabilityProvider.Create("PROVIDER.INPUT", "Duplicate provider", CapabilityId.Mouse);

    manager.RegisterProvider(provider);

    Assert.ThrowsWithCode(
        CapabilityErrorCode.ProviderAlreadyRegistered,
        () => manager.RegisterProvider(duplicate));
}

static void CapabilityManagerUnregistersProviders()
{
    var manager = new CapabilityManager();
    var provider = FakeCapabilityProvider.Create("provider.transfer", "Transfer provider", CapabilityId.Clipboard);

    manager.RegisterProvider(provider);
    manager.UnregisterProvider(provider.ProviderId);

    Assert.Null(manager.GetProvider(provider.ProviderId));
    Assert.Equal(0, manager.GetAllProviders().Count);
    Assert.ThrowsWithCode(
        CapabilityErrorCode.ProviderNotRegistered,
        () => manager.UnregisterProvider(provider.ProviderId));
}

static void CapabilityManagerReturnsProviderCapabilitySnapshots()
{
    var manager = new CapabilityManager();
    var provider = FakeCapabilityProvider.Create("provider.security", "Security provider", CapabilityId.Pairing);

    manager.RegisterProvider(provider);
    var snapshot = manager.GetCapabilitiesForProvider(provider.ProviderId);
    provider.SetCapabilities(CapabilitySet.FromIds(CapabilityId.Encryption));

    Assert.True(snapshot.Contains(CapabilityId.Pairing));
    Assert.False(snapshot.Contains(CapabilityId.Encryption));
    Assert.ThrowsWithCode(
        CapabilityErrorCode.ProviderNotRegistered,
        () => manager.GetCapabilitiesForProvider("missing"));
}

static void CapabilityManagerCombinesCapabilities()
{
    var manager = new CapabilityManager();
    manager.RegisterProvider(FakeCapabilityProvider.Create(
        "provider.display",
        "Display provider",
        CapabilityId.Display,
        CapabilityId.Notification));
    manager.RegisterProvider(FakeCapabilityProvider.Create(
        "provider.input",
        "Input provider",
        CapabilityId.Mouse,
        CapabilityId.Keyboard));

    var combined = manager.GetCombinedCapabilities();

    Assert.Equal(4, combined.Count);
    Assert.True(combined.ContainsAll(new[]
    {
        CapabilityId.Display,
        CapabilityId.Notification,
        CapabilityId.Mouse,
        CapabilityId.Keyboard
    }));
}

static void CapabilityManagerFindsMatchingProviders()
{
    var manager = new CapabilityManager();
    var display = FakeCapabilityProvider.Create("provider.display", "Display provider", CapabilityId.Display);
    var richDisplay = FakeCapabilityProvider.Create(
        "provider.rich-display",
        "Rich display provider",
        CapabilityId.Display,
        CapabilityId.Notification);
    var input = FakeCapabilityProvider.Create("provider.input", "Input provider", CapabilityId.Keyboard);
    var requirement = new CapabilityRequirement
    {
        RequiredCapabilities = new[] { CapabilityId.Display },
        OptionalCapabilities = new[] { CapabilityId.Notification }
    };

    manager.RegisterProvider(display);
    manager.RegisterProvider(richDisplay);
    manager.RegisterProvider(input);

    var matches = manager.FindProvidersMatching(requirement).ToArray();

    Assert.Equal(2, matches.Length);
    Assert.Same(richDisplay, matches[0]);
    Assert.Same(display, matches[1]);
}

static void CapabilityManagerReportsCapabilityAvailability()
{
    var manager = new CapabilityManager();
    manager.RegisterProvider(FakeCapabilityProvider.Create(
        "provider.comms",
        "Communication provider",
        CapabilityId.BLE,
        CapabilityId.WiFi));

    Assert.True(manager.IsCapabilityAvailable(CapabilityId.BLE));
    Assert.True(manager.IsCapabilityAvailable(CapabilityId.WiFi));
    Assert.False(manager.IsCapabilityAvailable(CapabilityId.LAN));
    Assert.False(manager.IsCapabilityAvailable(CapabilityId.Unknown));
}

static void CapabilityManagerThrowsCapabilityErrors()
{
    var manager = new CapabilityManager();
    var missingIdProvider = FakeCapabilityProvider.Create(string.Empty, "Missing id provider", CapabilityId.Testing);

    Assert.ThrowsWithCode(
        CapabilityErrorCode.MissingProviderId,
        () => manager.RegisterProvider(missingIdProvider));
    Assert.ThrowsWithCode(
        CapabilityErrorCode.MissingProviderId,
        () => manager.UnregisterProvider(string.Empty));
    Assert.Null(manager.GetProvider(string.Empty));
    Assert.True(Enum.IsDefined(CapabilityErrorCode.CapabilityMissing));
    Assert.True(Enum.IsDefined(CapabilityErrorCode.InvalidOperation));
}

static void CapabilityCoreAssemblyHasNoPlatformDependencies()
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

    var references = typeof(CapabilityManager)
        .Assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => forbiddenFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

    Assert.Equal(0, references.Length);
}

static void WorkspaceRegistryEnumsCoverArchitectureBaseline()
{
    var expectedTypes = new[]
    {
        RegistryWorkspaceType.SmartDevice,
        RegistryWorkspaceType.DisplayNode,
        RegistryWorkspaceType.HeadlessNode,
        RegistryWorkspaceType.KvmNode,
        RegistryWorkspaceType.RemoteWorkspace,
        RegistryWorkspaceType.CloudWorkspace,
        RegistryWorkspaceType.HybridWorkspace,
        RegistryWorkspaceType.Unknown
    };
    var expectedStates = new[]
    {
        RegistryWorkspaceState.Discovered,
        RegistryWorkspaceState.Registered,
        RegistryWorkspaceState.Available,
        RegistryWorkspaceState.Unavailable,
        RegistryWorkspaceState.Trusted,
        RegistryWorkspaceState.Untrusted,
        RegistryWorkspaceState.Disabled,
        RegistryWorkspaceState.Failed,
        RegistryWorkspaceState.Unknown
    };
    var expectedPositions = new[]
    {
        RegistryWorkspacePosition.Center,
        RegistryWorkspacePosition.Left,
        RegistryWorkspacePosition.Right,
        RegistryWorkspacePosition.Above,
        RegistryWorkspacePosition.Below,
        RegistryWorkspacePosition.Front,
        RegistryWorkspacePosition.Back,
        RegistryWorkspacePosition.Unknown
    };
    var expectedErrors = new[]
    {
        RegistryWorkspaceErrorCode.MissingWorkspaceId,
        RegistryWorkspaceErrorCode.WorkspaceAlreadyRegistered,
        RegistryWorkspaceErrorCode.WorkspaceNotRegistered,
        RegistryWorkspaceErrorCode.InvalidDescriptor,
        RegistryWorkspaceErrorCode.InvalidQuery,
        RegistryWorkspaceErrorCode.TargetNotFound,
        RegistryWorkspaceErrorCode.InvalidOperation
    };

    Assert.Equal(expectedTypes.Length, Enum.GetValues<RegistryWorkspaceType>().Length);
    Assert.Equal(expectedStates.Length, Enum.GetValues<RegistryWorkspaceState>().Length);
    Assert.Equal(expectedPositions.Length, Enum.GetValues<RegistryWorkspacePosition>().Length);
    Assert.Equal(expectedErrors.Length, Enum.GetValues<RegistryWorkspaceErrorCode>().Length);
    Assert.True(expectedTypes.All(type => Enum.IsDefined(type)));
    Assert.True(expectedStates.All(state => Enum.IsDefined(state)));
    Assert.True(expectedPositions.All(position => Enum.IsDefined(position)));
    Assert.True(expectedErrors.All(error => Enum.IsDefined(error)));
}

static void WorkspaceIdValidatesAndComparesIds()
{
    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.MissingWorkspaceId,
        () => RegistryWorkspaceId.Create(" "));

    var id = RegistryWorkspaceId.Create(" Workspace-A ");
    var same = RegistryWorkspaceId.Create("workspace-a");
    string value = id;

    Assert.Equal("Workspace-A", id.ToString());
    Assert.Equal("Workspace-A", value);
    Assert.True(id.Equals(same));
    Assert.True(id == same);
    Assert.False(id != same);
    Assert.Equal(0, id.CompareTo(same));
}

static void WorkspaceRegistryRegistersWorkspace()
{
    RegistryIWorkspaceRegistry registry = new RegistryWorkspaceRegistry();
    var descriptor = RegistryDescriptor(
        "workspace-a",
        RegistryWorkspaceType.SmartDevice,
        RegistryWorkspaceState.Available,
        RegistryWorkspacePosition.Center,
        capabilityIds: new[] { CapabilityId.Display, CapabilityId.Pairing });
    var workspace = RegistryWorkspace.FromDescriptor(descriptor);

    registry.RegisterWorkspace(workspace);

    Assert.True(registry.ContainsWorkspace(descriptor.WorkspaceId));
    Assert.Same(workspace, Assert.NotNull(registry.GetWorkspace(descriptor.WorkspaceId)));
    Assert.Equal(RegistryWorkspaceState.Available, workspace.State);
    Assert.True(workspace.Capabilities.Contains(CapabilityId.Display));
}

static void WorkspaceRegistryPreventsDuplicateWorkspaceIds()
{
    var registry = new RegistryWorkspaceRegistry();

    registry.RegisterWorkspace(FakeWorkspace.Create("workspace-a", capabilityIds: CapabilityId.Display));

    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.WorkspaceAlreadyRegistered,
        () => registry.RegisterWorkspace(FakeWorkspace.Create("WORKSPACE-A", capabilityIds: CapabilityId.Keyboard)));
}

static void WorkspaceRegistryUnregistersWorkspace()
{
    var registry = new RegistryWorkspaceRegistry();
    var workspace = FakeWorkspace.Create("workspace-a", capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(workspace);
    registry.UnregisterWorkspace(workspace.Id);

    Assert.False(registry.ContainsWorkspace(workspace.Id));
    Assert.Null(registry.GetWorkspace(workspace.Id));
}

static void WorkspaceRegistryRejectsUnknownWorkspaceRemoval()
{
    var registry = new RegistryWorkspaceRegistry();

    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.WorkspaceNotRegistered,
        () => registry.UnregisterWorkspace(RegistryWorkspaceId.Create("missing")));
}

static void WorkspaceRegistryUpdatesWorkspace()
{
    var registry = new RegistryWorkspaceRegistry();
    var workspace = FakeWorkspace.Create("workspace-a", capabilityIds: CapabilityId.Display);
    registry.RegisterWorkspace(workspace);

    var updated = workspace.Descriptor with
    {
        DisplayName = "Updated workspace",
        WorkspaceState = RegistryWorkspaceState.Trusted,
        Capabilities = CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.Pairing),
        Priority = 10
    };

    registry.UpdateWorkspace(updated);
    var current = Assert.NotNull(registry.GetWorkspace(workspace.Id));

    Assert.Equal("Updated workspace", current.Descriptor.DisplayName);
    Assert.Equal(RegistryWorkspaceState.Trusted, current.State);
    Assert.True(current.Capabilities.Contains(CapabilityId.Pairing));
    Assert.Equal(10, current.Descriptor.Priority);
    Assert.Equal(1, workspace.UpdateCount);
}

static void WorkspaceRegistryGetsWorkspaces()
{
    var registry = new RegistryWorkspaceRegistry();
    var workspace = FakeWorkspace.Create("workspace-a", capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(workspace);

    Assert.Same(workspace, Assert.NotNull(registry.GetWorkspace(workspace.Id)));
    Assert.Null(registry.GetWorkspace(RegistryWorkspaceId.Create("missing")));
}

static void WorkspaceRegistryGetsAllWorkspaces()
{
    var registry = new RegistryWorkspaceRegistry();
    var workspaceB = FakeWorkspace.Create("workspace-b", capabilityIds: CapabilityId.Keyboard);
    var workspaceA = FakeWorkspace.Create("workspace-a", capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(workspaceB);
    registry.RegisterWorkspace(workspaceA);

    var all = registry.GetAllWorkspaces().ToArray();

    Assert.Equal(2, all.Length);
    Assert.Equal("workspace-a", all[0].Id.ToString());
    Assert.Equal("workspace-b", all[1].Id.ToString());
}

static void WorkspaceRegistryFindsByPosition()
{
    var registry = new RegistryWorkspaceRegistry();
    var right = FakeWorkspace.Create(
        "workspace-right",
        position: RegistryWorkspacePosition.Right,
        capabilityIds: CapabilityId.Display);
    var left = FakeWorkspace.Create(
        "workspace-left",
        position: RegistryWorkspacePosition.Left,
        capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(right);
    registry.RegisterWorkspace(left);

    var matches = registry.FindByPosition(RegistryWorkspacePosition.Right).ToArray();

    Assert.Equal(1, matches.Length);
    Assert.Same(right, matches[0]);
}

static void WorkspaceRegistryFindsByCapability()
{
    var registry = new RegistryWorkspaceRegistry();
    var display = FakeWorkspace.Create(
        "workspace-display",
        capabilityIds: new[] { CapabilityId.Display, CapabilityId.Overlay });
    var input = FakeWorkspace.Create("workspace-input", capabilityIds: CapabilityId.Keyboard);

    registry.RegisterWorkspace(display);
    registry.RegisterWorkspace(input);

    var matches = registry.FindByCapability(CapabilityId.Overlay).ToArray();

    Assert.Equal(1, matches.Length);
    Assert.Same(display, matches[0]);
    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.InvalidQuery,
        () => registry.FindByCapability(CapabilityId.Unknown));
}

static void WorkspaceRegistryFindsMatchingWorkspaces()
{
    var registry = new RegistryWorkspaceRegistry();
    var good = FakeWorkspace.Create(
        "workspace-good",
        type: RegistryWorkspaceType.SmartDevice,
        state: RegistryWorkspaceState.Available,
        isTrusted: true,
        priority: 5,
        capabilityIds: new[] { CapabilityId.Display, CapabilityId.Notification });
    var untrusted = FakeWorkspace.Create(
        "workspace-untrusted",
        isTrusted: false,
        priority: 5,
        capabilityIds: new[] { CapabilityId.Display, CapabilityId.Notification });
    var lowPriority = FakeWorkspace.Create(
        "workspace-low",
        priority: 1,
        capabilityIds: new[] { CapabilityId.Display, CapabilityId.Notification });
    var wrongType = FakeWorkspace.Create(
        "workspace-node",
        type: RegistryWorkspaceType.DisplayNode,
        priority: 5,
        capabilityIds: new[] { CapabilityId.Display, CapabilityId.Notification });
    var query = new RegistryWorkspaceQuery
    {
        WorkspaceType = RegistryWorkspaceType.SmartDevice,
        TrustedOnly = true,
        AvailableOnly = true,
        MinimumPriority = 2,
        RequiredCapabilities = CapabilitySet.FromIds(CapabilityId.Display),
        OptionalCapabilities = CapabilitySet.FromIds(CapabilityId.Notification)
    };

    registry.RegisterWorkspace(good);
    registry.RegisterWorkspace(untrusted);
    registry.RegisterWorkspace(lowPriority);
    registry.RegisterWorkspace(wrongType);

    var results = registry.FindMatching(query).ToArray();

    Assert.Equal(1, results.Length);
    Assert.True(results[0].IsMatch);
    Assert.Same(good, results[0].Workspace);
    Assert.True(results[0].Score > 0);
    Assert.Equal(0, results[0].MissingCapabilities.Count);
    Assert.True(results[0].Reasons.Any(reason => reason.Contains("matched", StringComparison.OrdinalIgnoreCase)));
    Assert.Equal(1, query.ToCapabilityRequirement().RequiredCapabilities.Count);
}

static void WorkspaceRegistryBestTargetUsesOnlyTarget()
{
    var registry = new RegistryWorkspaceRegistry();
    var only = FakeWorkspace.Create("workspace-only", capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(only);

    Assert.Same(only, registry.GetBestTarget(RegistryWorkspaceQuery.Empty));
}

static void WorkspaceRegistryBestTargetPrefersPosition()
{
    var registry = new RegistryWorkspaceRegistry();
    var left = FakeWorkspace.Create(
        "workspace-left",
        position: RegistryWorkspacePosition.Left,
        priority: 100,
        capabilityIds: CapabilityId.Display);
    var right = FakeWorkspace.Create(
        "workspace-right",
        position: RegistryWorkspacePosition.Right,
        priority: 1,
        capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(left);
    registry.RegisterWorkspace(right);

    var target = registry.GetBestTarget(new RegistryWorkspaceQuery
    {
        Position = RegistryWorkspacePosition.Right,
        RequiredCapabilities = CapabilitySet.FromIds(CapabilityId.Display)
    });

    Assert.Same(right, target);
}

static void WorkspaceRegistryBestTargetPrefersCapabilities()
{
    var registry = new RegistryWorkspaceRegistry();
    var basic = FakeWorkspace.Create("workspace-basic", capabilityIds: CapabilityId.Display);
    var rich = FakeWorkspace.Create(
        "workspace-rich",
        capabilityIds: new[] { CapabilityId.Display, CapabilityId.Notification });

    registry.RegisterWorkspace(basic);
    registry.RegisterWorkspace(rich);

    var target = registry.GetBestTarget(new RegistryWorkspaceQuery
    {
        RequiredCapabilities = CapabilitySet.FromIds(CapabilityId.Display),
        OptionalCapabilities = CapabilitySet.FromIds(CapabilityId.Notification)
    });

    Assert.Same(rich, target);
}

static void WorkspaceRegistryBestTargetPrefersTrust()
{
    var registry = new RegistryWorkspaceRegistry();
    var untrusted = FakeWorkspace.Create(
        "workspace-untrusted",
        isTrusted: false,
        priority: 100,
        capabilityIds: CapabilityId.Display);
    var trusted = FakeWorkspace.Create(
        "workspace-trusted",
        isTrusted: true,
        priority: 1,
        capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(untrusted);
    registry.RegisterWorkspace(trusted);

    var target = registry.GetBestTarget(new RegistryWorkspaceQuery
    {
        RequiredCapabilities = CapabilitySet.FromIds(CapabilityId.Display)
    });

    Assert.Same(trusted, target);
}

static void WorkspaceRegistryBestTargetPrefersPriority()
{
    var registry = new RegistryWorkspaceRegistry();
    var low = FakeWorkspace.Create("workspace-low", priority: 1, capabilityIds: CapabilityId.Display);
    var high = FakeWorkspace.Create("workspace-high", priority: 10, capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(low);
    registry.RegisterWorkspace(high);

    var target = registry.GetBestTarget(new RegistryWorkspaceQuery
    {
        RequiredCapabilities = CapabilitySet.FromIds(CapabilityId.Display)
    });

    Assert.Same(high, target);
}

static void WorkspaceRegistryBestTargetPrefersLastSeen()
{
    var registry = new RegistryWorkspaceRegistry();
    var older = FakeWorkspace.Create(
        "workspace-older",
        lastSeen: new DateTimeOffset(2026, 7, 2, 8, 0, 0, TimeSpan.Zero),
        capabilityIds: CapabilityId.Display);
    var newer = FakeWorkspace.Create(
        "workspace-newer",
        lastSeen: new DateTimeOffset(2026, 7, 2, 9, 0, 0, TimeSpan.Zero),
        capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(older);
    registry.RegisterWorkspace(newer);

    var target = registry.GetBestTarget(new RegistryWorkspaceQuery
    {
        RequiredCapabilities = CapabilitySet.FromIds(CapabilityId.Display)
    });

    Assert.Same(newer, target);
}

static void WorkspaceRegistryCreatesSnapshots()
{
    var registry = new RegistryWorkspaceRegistry();
    var workspace = FakeWorkspace.Create("workspace-a", displayName: "Original", capabilityIds: CapabilityId.Display);

    registry.RegisterWorkspace(workspace);
    var snapshot = registry.CreateSnapshot().ToArray();
    registry.UpdateWorkspace(workspace.Descriptor with { DisplayName = "Updated" });

    Assert.Equal(1, snapshot.Length);
    Assert.Equal("Original", snapshot[0].DisplayName);
    Assert.True(snapshot[0].Capabilities.Contains(CapabilityId.Display));
}

static void WorkspaceRegistryThrowsWorkspaceErrors()
{
    var registry = new RegistryWorkspaceRegistry();
    RegistryIWorkspace nullWorkspace = null!;
    var workspace = RegistryWorkspace.FromDescriptor(RegistryDescriptor("workspace-a", capabilityIds: CapabilityId.Display));

    registry.RegisterWorkspace(workspace);

    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.InvalidDescriptor,
        () => registry.RegisterWorkspace(nullWorkspace));
    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.InvalidDescriptor,
        () => RegistryWorkspace.FromDescriptor(workspace.Descriptor with { DisplayName = string.Empty }));
    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.WorkspaceNotRegistered,
        () => registry.UpdateWorkspace(RegistryDescriptor("missing", capabilityIds: CapabilityId.Display)));
    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.InvalidQuery,
        () => registry.FindMatching(new RegistryWorkspaceQuery { MinimumPriority = -1 }));
    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.TargetNotFound,
        () => new RegistryWorkspaceRegistry().GetBestTarget(new RegistryWorkspaceQuery
        {
            RequiredCapabilities = CapabilitySet.FromIds(CapabilityId.Display)
        }));
    Assert.ThrowsWithCode(
        RegistryWorkspaceErrorCode.InvalidOperation,
        () => workspace.UpdateDescriptor(RegistryDescriptor("other", capabilityIds: CapabilityId.Display)));
}

static void WorkspaceRegistryCoreAssemblyHasNoPlatformDependencies()
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

    var references = typeof(RegistryWorkspaceRegistry)
        .Assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => forbiddenFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

    Assert.Equal(0, references.Length);
}

static RegistryWorkspaceDescriptor RegistryDescriptor(
    string workspaceId,
    RegistryWorkspaceType type = RegistryWorkspaceType.SmartDevice,
    RegistryWorkspaceState state = RegistryWorkspaceState.Available,
    RegistryWorkspacePosition position = RegistryWorkspacePosition.Unknown,
    bool isTrusted = true,
    int priority = 0,
    DateTimeOffset? lastSeen = null,
    string? displayName = null,
    params CapabilityId[] capabilityIds)
{
    return new RegistryWorkspaceDescriptor
    {
        WorkspaceId = RegistryWorkspaceId.Create(workspaceId),
        DisplayName = displayName ?? workspaceId,
        WorkspaceType = type,
        WorkspaceState = state,
        Position = position,
        Capabilities = CapabilitySet.FromIds(capabilityIds),
        IsTrusted = isTrusted,
        Priority = priority,
        LastSeen = lastSeen ?? DateTimeOffset.UnixEpoch,
        Metadata = new Dictionary<string, string>
        {
            ["source"] = "test"
        }
    };
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

    public static void ThrowsWithCode(CapabilityErrorCode expectedCode, Action action)
    {
        try
        {
            action();
        }
        catch (CapabilityException ex)
        {
            Equal(expectedCode, ex.Code);
            return;
        }

        throw new InvalidOperationException($"Expected capability exception {expectedCode}.");
    }

    public static void ThrowsWithCode(RegistryWorkspaceErrorCode expectedCode, Action action)
    {
        try
        {
            action();
        }
        catch (RegistryWorkspaceException ex)
        {
            Equal(expectedCode, ex.Code);
            return;
        }

        throw new InvalidOperationException($"Expected workspace exception {expectedCode}.");
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
