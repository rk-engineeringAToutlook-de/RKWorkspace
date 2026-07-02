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
using ManagedTransferHistoryEntry = RKWorkspace.Core.TransferObjects.TransferHistoryEntry;
using ManagedTransferMetadata = RKWorkspace.Core.TransferObjects.TransferMetadata;
using ManagedTransferObject = RKWorkspace.Core.TransferObjects.TransferObject;
using ManagedTransferObjectErrorCode = RKWorkspace.Core.TransferObjects.TransferObjectErrorCode;
using ManagedTransferObjectException = RKWorkspace.Core.TransferObjects.TransferObjectException;
using ManagedTransferObjectId = RKWorkspace.Core.TransferObjects.TransferObjectId;
using ManagedTransferObjectManager = RKWorkspace.Core.TransferObjects.TransferObjectManager;
using ManagedTransferObjectState = RKWorkspace.Core.TransferObjects.TransferObjectState;
using ManagedTransferObjectType = RKWorkspace.Core.TransferObjects.TransferObjectType;
using ManagedITransferObject = RKWorkspace.Core.TransferObjects.ITransferObject;
using ManagedITransferObjectManager = RKWorkspace.Core.TransferObjects.ITransferObjectManager;
using CoreITransferEngine = RKWorkspace.Core.Transfers.ITransferEngine;
using CoreTransferDirection = RKWorkspace.Core.Transfers.TransferDirection;
using CoreTransferEngine = RKWorkspace.Core.Transfers.TransferEngine;
using CoreTransferEngineException = RKWorkspace.Core.Transfers.TransferEngineException;
using CoreTransferFailureReason = RKWorkspace.Core.Transfers.TransferFailureReason;
using CoreTransferPlan = RKWorkspace.Core.Transfers.TransferPlan;
using CoreTransferRequest = RKWorkspace.Core.Transfers.TransferRequest;
using CoreTransferStep = RKWorkspace.Core.Transfers.TransferStep;
using CoreTransferStepStatus = RKWorkspace.Core.Transfers.TransferStepStatus;
using CoreRuntimeConfiguration = RKWorkspace.Core.Runtime.RuntimeConfiguration;
using CoreRuntimeEngine = RKWorkspace.Core.Runtime.RuntimeEngine;
using CoreRuntimeException = RKWorkspace.Core.Runtime.RuntimeException;
using CoreRuntimeState = RKWorkspace.Core.Runtime.RuntimeState;
using TalNamedPipeTransport = RKWorkspace.Transport.NamedPipes.NamedPipeTransport;
using TalNamedPipeTransportOptions = RKWorkspace.Transport.NamedPipes.NamedPipeTransportOptions;
using TalTransportEndpoint = RKWorkspace.Transport.TransportEndpoint;
using TalTransportException = RKWorkspace.Transport.TransportException;
using TalTransportMessage = RKWorkspace.Transport.TransportMessage;
using TalTransportMessageType = RKWorkspace.Transport.TransportMessageType;

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
    ("Workspace registry core assembly has no platform dependencies", WorkspaceRegistryCoreAssemblyHasNoPlatformDependencies),
    ("Transfer object enums cover the object model baseline", TransferObjectEnumsCoverObjectModelBaseline),
    ("TransferObjectId validates and compares ids", TransferObjectIdValidatesAndComparesIds),
    ("TransferObjectManager creates an object", TransferObjectManagerCreatesObject),
    ("TransferObjectManager deletes an object", TransferObjectManagerDeletesObject),
    ("TransferObjectManager archives an object", TransferObjectManagerArchivesObject),
    ("TransferObjectManager clones an object", TransferObjectManagerClonesObject),
    ("TransferObjectManager updates metadata", TransferObjectManagerUpdatesMetadata),
    ("TransferObjectManager updates state", TransferObjectManagerUpdatesState),
    ("TransferObjectManager records history", TransferObjectManagerRecordsHistory),
    ("TransferObjectManager finds objects", TransferObjectManagerFindsObjects),
    ("TransferObjectManager snapshots objects", TransferObjectManagerSnapshotsObjects),
    ("TransferObjectManager prevents duplicates", TransferObjectManagerPreventsDuplicates),
    ("TransferObjectManager validates objects", TransferObjectManagerValidatesObjects),
    ("TransferObjectManager reports missing objects", TransferObjectManagerReportsMissingObjects),
    ("TransferObjectManager rejects invalid metadata and states", TransferObjectManagerRejectsInvalidMetadataAndStates),
    ("FakeTransferObject implements transfer object contract", FakeTransferObjectImplementsTransferObjectContract),
    ("Transfer object core assembly has no platform dependencies", TransferObjectCoreAssemblyHasNoPlatformDependencies),
    ("TransferRequest creates and validates requests", TransferRequestCreatesAndValidatesRequests),
    ("TransferPlan creates logical plans", TransferPlanCreatesLogicalPlans),
    ("TransferEngine validates plans", TransferEngineValidatesPlans),
    ("TransferEngine resolves right targets", TransferEngineResolvesRightTargets),
    ("TransferEngine resolves left targets", TransferEngineResolvesLeftTargets),
    ("TransferEngine resolves Any with one target", TransferEngineResolvesAnyWithOneTarget),
    ("TransferEngine enforces required capabilities", TransferEngineEnforcesRequiredCapabilities),
    ("TransferEngine rejects forbidden capabilities", TransferEngineRejectsForbiddenCapabilities),
    ("TransferEngine prepares transfer objects", TransferEnginePreparesTransferObjects),
    ("TransferEngine completes transfer objects", TransferEngineCompletesTransferObjects),
    ("TransferEngine records transfer object history", TransferEngineRecordsTransferObjectHistory),
    ("TransferEngine reports missing source workspaces", TransferEngineReportsMissingSourceWorkspaces),
    ("TransferEngine reports missing targets", TransferEngineReportsMissingTargets),
    ("TransferEngine reports missing transfer objects", TransferEngineReportsMissingTransferObjects),
    ("TransferEngine cancels transfers", TransferEngineCancelsTransfers),
    ("TransferEngine fails transfers", TransferEngineFailsTransfers),
    ("TransferEngine executes logical transfers successfully", TransferEngineExecutesLogicalTransfersSuccessfully),
    ("Transfer engine core assembly has no platform dependencies", TransferEngineCoreAssemblyHasNoPlatformDependencies),
    ("RuntimeState covers the core lifecycle", RuntimeStateCoversCoreLifecycle),
    ("RuntimeConfiguration stores neutral flags", RuntimeConfigurationStoresNeutralFlags),
    ("RuntimeEngine starts core managers", RuntimeEngineStartsCoreManagers),
    ("RuntimeEngine stops core managers", RuntimeEngineStopsCoreManagers),
    ("RuntimeEngine pauses and resumes", RuntimeEnginePausesAndResumes),
    ("RuntimeEngine shuts down", RuntimeEngineShutsDown),
    ("RuntimeEngine reports diagnostics", RuntimeEngineReportsDiagnostics),
    ("RuntimeEngine rejects invalid transitions", RuntimeEngineRejectsInvalidTransitions),
    ("RuntimeEngine records initialization failures", RuntimeEngineRecordsInitializationFailures),
    ("Runtime engine core assembly has no platform dependencies", RuntimeEngineCoreAssemblyHasNoPlatformDependencies),
    ("TransportMessage creates live-ready messages", TransportMessageCreatesLiveReadyMessages),
    ("TransportMessage preserves CorrelationId", TransportMessagePreservesCorrelationId),
    ("TransportEndpoint validates named pipe endpoints", TransportEndpointValidatesNamedPipeEndpoints),
    ("NamedPipeTransport client/server roundtrip", () => NamedPipeTransportClientServerRoundtrip().GetAwaiter().GetResult()),
    ("NamedPipeTransport request response", () => NamedPipeTransportRequestResponse().GetAwaiter().GetResult()),
    ("NamedPipeTransport timeout returns failure", () => NamedPipeTransportTimeoutReturnsFailure().GetAwaiter().GetResult()),
    ("NamedPipeTransport reports wrong target", () => NamedPipeTransportReportsWrongTarget().GetAwaiter().GetResult())
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

static void TransferObjectEnumsCoverObjectModelBaseline()
{
    var expectedTypes = new[]
    {
        ManagedTransferObjectType.Text,
        ManagedTransferObjectType.File,
        ManagedTransferObjectType.Folder,
        ManagedTransferObjectType.PDF,
        ManagedTransferObjectType.Image,
        ManagedTransferObjectType.Clipboard,
        ManagedTransferObjectType.Link,
        ManagedTransferObjectType.Context,
        ManagedTransferObjectType.Binary,
        ManagedTransferObjectType.Unknown
    };
    var expectedStates = new[]
    {
        ManagedTransferObjectState.Created,
        ManagedTransferObjectState.Validated,
        ManagedTransferObjectState.Queued,
        ManagedTransferObjectState.Prepared,
        ManagedTransferObjectState.Locked,
        ManagedTransferObjectState.Completed,
        ManagedTransferObjectState.Cancelled,
        ManagedTransferObjectState.Failed,
        ManagedTransferObjectState.Archived
    };
    var expectedErrors = new[]
    {
        ManagedTransferObjectErrorCode.InvalidId,
        ManagedTransferObjectErrorCode.MissingMetadata,
        ManagedTransferObjectErrorCode.InvalidState,
        ManagedTransferObjectErrorCode.ObjectNotFound,
        ManagedTransferObjectErrorCode.ObjectAlreadyExists,
        ManagedTransferObjectErrorCode.ArchiveFailed,
        ManagedTransferObjectErrorCode.ValidationFailed
    };

    Assert.Equal(expectedTypes.Length, Enum.GetValues<ManagedTransferObjectType>().Length);
    Assert.Equal(expectedStates.Length, Enum.GetValues<ManagedTransferObjectState>().Length);
    Assert.Equal(expectedErrors.Length, Enum.GetValues<ManagedTransferObjectErrorCode>().Length);
    Assert.True(expectedTypes.All(type => Enum.IsDefined(type)));
    Assert.True(expectedStates.All(state => Enum.IsDefined(state)));
    Assert.True(expectedErrors.All(error => Enum.IsDefined(error)));
}

static void TransferObjectIdValidatesAndComparesIds()
{
    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.InvalidId,
        () => ManagedTransferObjectId.Create(" "));

    var id = ManagedTransferObjectId.Create(" Object-A ");
    var same = ManagedTransferObjectId.Create("object-a");
    string value = id;

    Assert.Equal("Object-A", id.ToString());
    Assert.Equal("Object-A", value);
    Assert.True(id.Equals(same));
    Assert.True(id == same);
    Assert.False(id != same);
    Assert.Equal(0, id.CompareTo(same));
    Assert.StartsWith("rkws-obj-", ManagedTransferObjectId.NewId().ToString());
}

static void TransferObjectManagerCreatesObject()
{
    ManagedITransferObjectManager manager = new ManagedTransferObjectManager();
    var metadata = ManagedMetadata("object-a", displayName: "Readme.txt", tags: new[] { "docs", "text" });

    var transferObject = manager.Create(ManagedTransferObjectType.Text, metadata);

    Assert.Equal(metadata.ObjectId, transferObject.Id);
    Assert.Equal(ManagedTransferObjectType.Text, transferObject.ObjectType);
    Assert.Equal(ManagedTransferObjectState.Created, transferObject.State);
    Assert.Equal("Readme.txt", transferObject.Metadata.DisplayName);
    Assert.Equal(2, transferObject.Metadata.Tags.Count);
    Assert.Equal(1, transferObject.History.Count);
    Assert.Same(transferObject, Assert.NotNull(manager.Get(metadata.ObjectId)));
}

static void TransferObjectManagerDeletesObject()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.File, ManagedMetadata("object-a"));

    manager.Delete(transferObject.Id);

    Assert.Null(manager.Get(transferObject.Id));
    Assert.Equal(0, manager.GetAll().Count);
}

static void TransferObjectManagerArchivesObject()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.Image, ManagedMetadata("object-a"));

    var archived = manager.Archive(transferObject.Id);

    Assert.Equal(ManagedTransferObjectState.Archived, archived.State);
    Assert.True(archived.History.Any(entry => entry.Action == "State:Archived"));
    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.ArchiveFailed,
        () => manager.Archive(transferObject.Id));
}

static void TransferObjectManagerClonesObject()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.PDF, ManagedMetadata("object-a", displayName: "Report.pdf"));

    var clone = manager.Clone(transferObject.Id);

    Assert.False(clone.Id == transferObject.Id);
    Assert.Equal(ManagedTransferObjectType.PDF, clone.ObjectType);
    Assert.Equal("Report.pdf Copy", clone.Metadata.DisplayName);
    Assert.Equal(2, manager.GetAll().Count);
    Assert.True(clone.History.Any(entry => entry.Action == "Cloned"));
}

static void TransferObjectManagerUpdatesMetadata()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.Link, ManagedMetadata("object-a"));
    var updated = transferObject.Metadata with
    {
        DisplayName = "Updated link",
        MimeType = "text/uri-list",
        ModifiedAt = new DateTimeOffset(2026, 7, 2, 10, 0, 0, TimeSpan.Zero),
        Priority = 7,
        Tags = new[] { "link", "updated" }
    };

    var result = manager.UpdateMetadata(transferObject.Id, updated);

    Assert.Equal("Updated link", result.Metadata.DisplayName);
    Assert.Equal("text/uri-list", result.Metadata.MimeType);
    Assert.Equal(7, result.Metadata.Priority);
    Assert.Equal(2, result.Metadata.Tags.Count);
    Assert.True(result.History.Any(entry => entry.Action == "MetadataUpdated"));
}

static void TransferObjectManagerUpdatesState()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.Clipboard, ManagedMetadata("object-a"));

    manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Queued);
    manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Prepared);
    var locked = manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Locked);

    Assert.Equal(ManagedTransferObjectState.Locked, locked.State);
    Assert.True(locked.History.Any(entry => entry.Action == "State:Locked"));
}

static void TransferObjectManagerRecordsHistory()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.Binary, ManagedMetadata("object-a"));

    manager.Validate(transferObject.Id);
    manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Queued);
    manager.Archive(transferObject.Id);

    var history = Assert.NotNull(manager.Get(transferObject.Id)).History.ToArray();

    Assert.True(history.Length >= 4);
    Assert.Equal("Created", history[0].Action);
    Assert.True(history.Any(entry => entry.Action == "State:Validated"));
    Assert.True(history.Any(entry => entry.Workspace == "workspace-b"));
}

static void TransferObjectManagerFindsObjects()
{
    var manager = new ManagedTransferObjectManager();
    var text = manager.Create(ManagedTransferObjectType.Text, ManagedMetadata("object-text", priority: 1));
    var image = manager.Create(ManagedTransferObjectType.Image, ManagedMetadata("object-image", priority: 10));

    var highPriority = manager.Find(item => item.Metadata.Priority >= 10).ToArray();
    var images = manager.Find(item => item.ObjectType == ManagedTransferObjectType.Image).ToArray();

    Assert.Equal(1, highPriority.Length);
    Assert.Same(image, highPriority[0]);
    Assert.Equal(1, images.Length);
    Assert.Same(image, images[0]);
    Assert.True(manager.GetAll().Any(item => ReferenceEquals(item, text)));
}

static void TransferObjectManagerSnapshotsObjects()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.Text, ManagedMetadata("object-a", displayName: "Original"));
    var snapshot = manager.Snapshot().ToArray();

    manager.UpdateMetadata(transferObject.Id, transferObject.Metadata with
    {
        DisplayName = "Updated",
        ModifiedAt = new DateTimeOffset(2026, 7, 2, 11, 0, 0, TimeSpan.Zero)
    });

    Assert.Equal(1, snapshot.Length);
    Assert.Equal("Original", snapshot[0].Metadata.DisplayName);
}

static void TransferObjectManagerPreventsDuplicates()
{
    var manager = new ManagedTransferObjectManager();
    var metadata = ManagedMetadata("object-a");

    manager.Create(ManagedTransferObjectType.Text, metadata);

    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.ObjectAlreadyExists,
        () => manager.Create(ManagedTransferObjectType.Text, metadata));
}

static void TransferObjectManagerValidatesObjects()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.File, ManagedMetadata("object-a"));

    manager.Validate(transferObject.Id);

    Assert.Equal(ManagedTransferObjectState.Validated, transferObject.State);
    Assert.True(transferObject.History.Any(entry => entry.Action == "State:Validated"));
}

static void TransferObjectManagerReportsMissingObjects()
{
    var manager = new ManagedTransferObjectManager();

    Assert.Null(manager.Get(ManagedTransferObjectId.Create("missing")));
    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.ObjectNotFound,
        () => manager.Delete(ManagedTransferObjectId.Create("missing")));
    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.ObjectNotFound,
        () => manager.Validate(ManagedTransferObjectId.Create("missing")));
}

static void TransferObjectManagerRejectsInvalidMetadataAndStates()
{
    var manager = new ManagedTransferObjectManager();
    var transferObject = manager.Create(ManagedTransferObjectType.Text, ManagedMetadata("object-a"));

    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.ValidationFailed,
        () => manager.Create(ManagedTransferObjectType.Unknown, ManagedMetadata("unknown")));
    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.MissingMetadata,
        () => manager.Create(ManagedTransferObjectType.Text, transferObject.Metadata with { DisplayName = string.Empty }));
    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.InvalidId,
        () => manager.UpdateMetadata(transferObject.Id, transferObject.Metadata with
        {
            ObjectId = ManagedTransferObjectId.Create("other")
        }));

    manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Queued);
    manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Prepared);
    manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Locked);
    manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Completed);

    Assert.ThrowsWithCode(
        ManagedTransferObjectErrorCode.InvalidState,
        () => manager.UpdateState(transferObject.Id, ManagedTransferObjectState.Queued));
}

static void FakeTransferObjectImplementsTransferObjectContract()
{
    var fake = FakeTransferObject.Create(
        ManagedTransferObjectType.Context,
        ManagedMetadata("fake-object", displayName: "Context"));

    fake.Validate();
    var clone = fake.Clone();
    fake.Archive();

    Assert.Equal(ManagedTransferObjectType.Context, fake.ObjectType);
    Assert.Equal(ManagedTransferObjectState.Archived, fake.State);
    Assert.False(clone.Id == fake.Id);
    Assert.True(fake.History.Any(entry => entry.Action == "Validated"));
    Assert.True(fake.History.Any(entry => entry.Action == "Archived"));
}

static void TransferObjectCoreAssemblyHasNoPlatformDependencies()
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

    var references = typeof(ManagedTransferObjectManager)
        .Assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => forbiddenFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

    Assert.Equal(0, references.Length);
}

static void TransferRequestCreatesAndValidatesRequests()
{
    var request = TransferRequest(
        ManagedTransferObjectId.Create("object-a"),
        required: CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.Clipboard),
        forbidden: CapabilitySet.FromIds(CapabilityId.CloudMode));

    var snapshot = request.Snapshot();
    var requirement = snapshot.ToRequirement();

    Assert.Equal("request-a", snapshot.RequestId);
    Assert.Equal(CoreTransferDirection.Right, snapshot.RequestedDirection);
    Assert.True(requirement.RequiredCapabilities.Contains(CapabilityId.Display));
    Assert.True(requirement.ForbiddenCapabilities.Contains(CapabilityId.CloudMode));
    Assert.ThrowsWithCode(
        CoreTransferFailureReason.InvalidDirection,
        () => (request with { RequestedDirection = CoreTransferDirection.Unknown }).Validate());
}

static void TransferPlanCreatesLogicalPlans()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));

    var plan = fixture.Engine.CreatePlan(TransferRequest(fixture.TransferObject.Id));

    Assert.StartsWith("rkws-plan-", plan.PlanId);
    Assert.Equal(fixture.Source.WorkspaceId, plan.SourceWorkspace.Id);
    Assert.Equal("workspace-b", plan.TargetWorkspace.Id.ToString());
    Assert.Equal(fixture.TransferObject.Id, plan.TransferObject.Id);
    Assert.True(plan.IsValid);
    Assert.Equal(6, plan.Steps.Count);
}

static void TransferEngineValidatesPlans()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));
    var plan = fixture.Engine.CreatePlan(TransferRequest(fixture.TransferObject.Id));

    var validated = fixture.Engine.ValidatePlan(plan);

    Assert.True(validated.IsValid);
    Assert.Equal(0, validated.ValidationMessages.Count);
}

static void TransferEngineResolvesRightTargets()
{
    var fixture = TransferEngineFixture(
        LeftTarget("workspace-left"),
        RightTarget("workspace-right"));

    var plan = fixture.Engine.CreatePlan(TransferRequest(
        fixture.TransferObject.Id,
        direction: CoreTransferDirection.Right));

    Assert.Equal("workspace-right", plan.TargetWorkspace.Id.ToString());
}

static void TransferEngineResolvesLeftTargets()
{
    var fixture = TransferEngineFixture(
        LeftTarget("workspace-left"),
        RightTarget("workspace-right"));

    var plan = fixture.Engine.CreatePlan(TransferRequest(
        fixture.TransferObject.Id,
        direction: CoreTransferDirection.Left));

    Assert.Equal("workspace-left", plan.TargetWorkspace.Id.ToString());
}

static void TransferEngineResolvesAnyWithOneTarget()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));

    var plan = fixture.Engine.CreatePlan(TransferRequest(
        fixture.TransferObject.Id,
        direction: CoreTransferDirection.Any));

    Assert.Equal("workspace-b", plan.TargetWorkspace.Id.ToString());
}

static void TransferEngineEnforcesRequiredCapabilities()
{
    var fixture = TransferEngineFixture(TransferTarget(
        "workspace-b",
        RegistryWorkspacePosition.Right,
        5,
        CapabilityId.Display));

    Assert.ThrowsWithCode(
        CoreTransferFailureReason.RequiredCapabilitiesMissing,
        () => fixture.Engine.CreatePlan(TransferRequest(
            fixture.TransferObject.Id,
            required: CapabilitySet.FromIds(CapabilityId.Display, CapabilityId.Clipboard))));
}

static void TransferEngineRejectsForbiddenCapabilities()
{
    var fixture = TransferEngineFixture(TransferTarget(
        "workspace-cloud",
        RegistryWorkspacePosition.Right,
        100,
        CapabilityId.Display,
        CapabilityId.Clipboard,
        CapabilityId.Encryption,
        CapabilityId.Pairing,
        CapabilityId.CloudMode));

    Assert.ThrowsWithCode(
        CoreTransferFailureReason.ForbiddenCapabilitiesPresent,
        () => fixture.Engine.CreatePlan(TransferRequest(
            fixture.TransferObject.Id,
            forbidden: CapabilitySet.FromIds(CapabilityId.CloudMode))));
}

static void TransferEnginePreparesTransferObjects()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));
    var plan = fixture.Engine.CreatePlan(TransferRequest(fixture.TransferObject.Id));

    var prepared = fixture.Engine.PrepareTransfer(plan);
    var transferObject = Assert.NotNull(fixture.TransferObjectManager.Get(fixture.TransferObject.Id));

    Assert.Equal(ManagedTransferObjectState.Prepared, transferObject.State);
    Assert.Equal("workspace-b", transferObject.Metadata.TargetWorkspace);
    Assert.True(prepared.Steps.Any(step =>
        step.Name == "PrepareTransfer" &&
        step.Status == CoreTransferStepStatus.Completed));
}

static void TransferEngineCompletesTransferObjects()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));
    var plan = fixture.Engine.CreatePlan(TransferRequest(fixture.TransferObject.Id));
    var prepared = fixture.Engine.PrepareTransfer(plan);

    var result = fixture.Engine.CompleteTransfer(prepared);
    var transferObject = Assert.NotNull(fixture.TransferObjectManager.Get(fixture.TransferObject.Id));

    Assert.True(result.IsSuccess);
    Assert.Equal(ManagedTransferObjectState.Completed, result.FinalState);
    Assert.Equal(ManagedTransferObjectState.Completed, transferObject.State);
}

static void TransferEngineRecordsTransferObjectHistory()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));

    var result = fixture.Engine.ExecuteLogicalTransfer(TransferRequest(fixture.TransferObject.Id));
    var transferObject = Assert.NotNull(fixture.TransferObjectManager.Get(fixture.TransferObject.Id));
    var actions = transferObject.History.Select(entry => entry.Action).ToArray();

    Assert.True(result.IsSuccess);
    Assert.True(actions.Contains("Created"));
    Assert.True(actions.Contains("State:Validated"));
    Assert.True(actions.Contains("MetadataUpdated"));
    Assert.True(actions.Contains("State:Prepared"));
    Assert.True(actions.Contains("State:Completed"));
}

static void TransferEngineReportsMissingSourceWorkspaces()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));

    Assert.ThrowsWithCode(
        CoreTransferFailureReason.SourceWorkspaceMissing,
        () => fixture.Engine.CreatePlan(TransferRequest(
            fixture.TransferObject.Id,
            sourceWorkspaceId: RegistryWorkspaceId.Create("missing-source"))));
}

static void TransferEngineReportsMissingTargets()
{
    var fixture = TransferEngineFixture();

    Assert.ThrowsWithCode(
        CoreTransferFailureReason.TargetWorkspaceMissing,
        () => fixture.Engine.CreatePlan(TransferRequest(fixture.TransferObject.Id)));
}

static void TransferEngineReportsMissingTransferObjects()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));

    Assert.ThrowsWithCode(
        CoreTransferFailureReason.TransferObjectMissing,
        () => fixture.Engine.CreatePlan(TransferRequest(ManagedTransferObjectId.Create("missing-object"))));
}

static void TransferEngineCancelsTransfers()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));
    var plan = fixture.Engine.CreatePlan(TransferRequest(fixture.TransferObject.Id));

    var result = fixture.Engine.CancelTransfer(plan);
    var transferObject = Assert.NotNull(fixture.TransferObjectManager.Get(fixture.TransferObject.Id));

    Assert.False(result.IsSuccess);
    Assert.Equal(CoreTransferFailureReason.Cancelled, result.FailureReason);
    Assert.Equal(ManagedTransferObjectState.Cancelled, transferObject.State);
}

static void TransferEngineFailsTransfers()
{
    var fixture = TransferEngineFixture(RightTarget("workspace-b"));
    var plan = fixture.Engine.CreatePlan(TransferRequest(fixture.TransferObject.Id));

    var result = fixture.Engine.FailTransfer(plan, "Unit failure.");
    var transferObject = Assert.NotNull(fixture.TransferObjectManager.Get(fixture.TransferObject.Id));

    Assert.False(result.IsSuccess);
    Assert.Equal(CoreTransferFailureReason.Failed, result.FailureReason);
    Assert.Equal(ManagedTransferObjectState.Failed, transferObject.State);
}

static void TransferEngineExecutesLogicalTransfersSuccessfully()
{
    var fixture = TransferEngineFixture(
        TransferTarget(
            "workspace-low",
            RegistryWorkspacePosition.Right,
            1,
            CapabilityId.Display,
            CapabilityId.Clipboard,
            CapabilityId.Encryption,
            CapabilityId.Pairing),
        TransferTarget(
            "workspace-high",
            RegistryWorkspacePosition.Right,
            20,
            CapabilityId.Display,
            CapabilityId.Clipboard,
            CapabilityId.Encryption,
            CapabilityId.Pairing));

    var result = fixture.Engine.ExecuteLogicalTransfer(TransferRequest(fixture.TransferObject.Id));

    Assert.True(result.IsSuccess);
    Assert.Equal("workspace-high", Assert.NotNull(result.TargetWorkspace).Id.ToString());
    Assert.Equal(ManagedTransferObjectState.Completed, result.FinalState);
}

static void TransferEngineCoreAssemblyHasNoPlatformDependencies()
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

    var references = typeof(CoreTransferEngine)
        .Assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => forbiddenFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

    Assert.Equal(0, references.Length);
}

static void RuntimeStateCoversCoreLifecycle()
{
    var expectedStates = new[]
    {
        CoreRuntimeState.Created,
        CoreRuntimeState.Initializing,
        CoreRuntimeState.Running,
        CoreRuntimeState.Paused,
        CoreRuntimeState.Stopping,
        CoreRuntimeState.Stopped,
        CoreRuntimeState.Failed
    };

    Assert.Equal(expectedStates.Length, Enum.GetValues<CoreRuntimeState>().Length);
    Assert.True(expectedStates.All(state => Enum.IsDefined(state)));
}

static void RuntimeConfigurationStoresNeutralFlags()
{
    var configuration = new CoreRuntimeConfiguration
    {
        LoggingEnabled = false,
        SimulationEnabled = true,
        TestModeEnabled = true,
        DiagnosticsEnabled = true,
        DebugModeEnabled = true
    };

    var snapshot = configuration.Snapshot();

    Assert.False(snapshot.LoggingEnabled);
    Assert.True(snapshot.SimulationEnabled);
    Assert.True(snapshot.TestModeEnabled);
    Assert.True(snapshot.DiagnosticsEnabled);
    Assert.True(snapshot.DebugModeEnabled);
}

static void RuntimeEngineStartsCoreManagers()
{
    var runtime = new CoreRuntimeEngine(new CoreRuntimeConfiguration { TestModeEnabled = true });

    runtime.Start();

    Assert.Equal(CoreRuntimeState.Running, runtime.GetStatus());
    Assert.NotNull(runtime.PluginManager);
    Assert.NotNull(runtime.CapabilityManager);
    Assert.NotNull(runtime.WorkspaceRegistry);
    Assert.NotNull(runtime.TransferObjectManager);
}

static void RuntimeEngineStopsCoreManagers()
{
    var runtime = new CoreRuntimeEngine(new CoreRuntimeConfiguration { TestModeEnabled = true });
    var plugin = FakePlugin.Create("runtime.plugin", PluginType.Testing, "Runtime plugin");

    runtime.Start();
    Assert.Success(runtime.PluginManager.RegisterPlugin(plugin));
    Assert.Success(runtime.PluginManager.ActivatePlugin(plugin.PluginId));

    runtime.Stop();

    Assert.Equal(CoreRuntimeState.Stopped, runtime.GetStatus());
    Assert.Equal(PluginState.Unloaded, plugin.State);
    Assert.Equal(1, plugin.DeactivateCount);
    Assert.Equal(1, plugin.ShutdownCount);
}

static void RuntimeEnginePausesAndResumes()
{
    var runtime = new CoreRuntimeEngine(new CoreRuntimeConfiguration { TestModeEnabled = true });

    runtime.Start();
    runtime.Pause();
    Assert.Equal(CoreRuntimeState.Paused, runtime.GetStatus());

    runtime.Resume();

    Assert.Equal(CoreRuntimeState.Running, runtime.GetStatus());
}

static void RuntimeEngineShutsDown()
{
    var runtime = new CoreRuntimeEngine(new CoreRuntimeConfiguration { TestModeEnabled = true });

    runtime.Start();
    runtime.Shutdown();

    Assert.Equal(CoreRuntimeState.Stopped, runtime.GetStatus());
}

static void RuntimeEngineReportsDiagnostics()
{
    var runtime = new CoreRuntimeEngine(new CoreRuntimeConfiguration
    {
        TestModeEnabled = true,
        DiagnosticsEnabled = true
    });

    runtime.Start();
    Assert.Success(runtime.PluginManager.RegisterPlugin(
        FakePlugin.Create("runtime.diagnostics", PluginType.Testing, "Runtime diagnostics")));
    runtime.WorkspaceRegistry.RegisterWorkspace(RegistryWorkspace.FromDescriptor(RightTarget("workspace-runtime")));
    runtime.TransferObjectManager.Create(
        ManagedTransferObjectType.Text,
        ManagedMetadata("runtime-object", targetWorkspace: string.Empty));

    var diagnostics = runtime.GetDiagnostics();

    Assert.Equal(CoreRuntimeState.Running, diagnostics.RuntimeState);
    Assert.Equal(1, diagnostics.PluginCount);
    Assert.Equal(1, diagnostics.WorkspaceCount);
    Assert.Equal(1, diagnostics.TransferObjectCount);
    Assert.True(diagnostics.StartTime.HasValue);
    Assert.True(diagnostics.Uptime >= TimeSpan.Zero);
    Assert.True(!string.IsNullOrWhiteSpace(diagnostics.RuntimeVersion));
}

static void RuntimeEngineRejectsInvalidTransitions()
{
    var runtime = new CoreRuntimeEngine(new CoreRuntimeConfiguration { TestModeEnabled = true });

    Assert.Throws<CoreRuntimeException>(() => runtime.Pause());

    runtime.Start();

    Assert.Throws<CoreRuntimeException>(() => runtime.Start());
    Assert.Throws<CoreRuntimeException>(() => runtime.Resume());

    runtime.Pause();

    Assert.Throws<CoreRuntimeException>(() => runtime.Start());
    runtime.Resume();
    runtime.Stop();

    Assert.Throws<CoreRuntimeException>(() => runtime.Stop());
    Assert.True(runtime.GetDiagnostics().Errors.Count >= 4);
}

static void RuntimeEngineRecordsInitializationFailures()
{
    var runtime = new CoreRuntimeEngine(
        new CoreRuntimeConfiguration { TestModeEnabled = true },
        pluginManagerFactory: () => throw new InvalidOperationException("factory failed"));

    Assert.Throws<CoreRuntimeException>(() => runtime.Start());

    var diagnostics = runtime.GetDiagnostics();

    Assert.Equal(CoreRuntimeState.Failed, diagnostics.RuntimeState);
    Assert.True(diagnostics.Errors.Any(error => error.Contains("factory failed", StringComparison.Ordinal)));
}

static void RuntimeEngineCoreAssemblyHasNoPlatformDependencies()
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

    var references = typeof(CoreRuntimeEngine)
        .Assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => forbiddenFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

    Assert.Equal(0, references.Length);
}

static void TransportMessageCreatesLiveReadyMessages()
{
    var message = TalTransportMessage.Create(
        TalTransportMessageType.WorkspaceWindowFrame,
        "workspace-a",
        "workspace-b",
        new Dictionary<string, string>
        {
            ["frameId"] = "frame-001"
        },
        headers: new Dictionary<string, string>
        {
            ["contentType"] = "application/rkws-window-frame"
        });

    Assert.StartsWith("rkws-transport-", message.MessageId);
    Assert.Equal(TalTransportMessageType.WorkspaceWindowFrame, message.MessageType);
    Assert.Equal("workspace-a", message.SourceId);
    Assert.Equal("workspace-b", message.TargetId);
    Assert.Equal("frame-001", message.Payload["frameId"]);
    Assert.Equal("application/rkws-window-frame", message.Headers["contentType"]);
    Assert.True(Enum.IsDefined(TalTransportMessageType.LiveSessionEvent));
    Assert.True(Enum.IsDefined(TalTransportMessageType.WorkspaceObjectUpdate));
    Assert.True(Enum.IsDefined(TalTransportMessageType.InputEvent));
}

static void TransportMessagePreservesCorrelationId()
{
    var request = TalTransportMessage.Create(
        TalTransportMessageType.AgentStatusRequest,
        "agent-a",
        "agent-b");
    var response = TalTransportMessage.Create(
        TalTransportMessageType.AgentStatusResponse,
        "agent-b",
        "agent-a",
        correlationId: request.MessageId);

    Assert.Equal(request.MessageId, response.CorrelationId);
}

static void TransportEndpointValidatesNamedPipeEndpoints()
{
    var endpoint = TalTransportEndpoint.NamedPipe("rkws-test-pipe");

    Assert.Equal("named-pipe:rkws-test-pipe", endpoint.EndpointId);
    Assert.Equal("rkws-test-pipe", endpoint.Address);
    Assert.Equal("NamedPipe", endpoint.TransportKind);
    Assert.Throws<TalTransportException>(() => TalTransportEndpoint.NamedPipe("bad/name"));
}

static async Task NamedPipeTransportClientServerRoundtrip()
{
    var transport = TestNamedPipeTransport();
    var endpoint = TalTransportEndpoint.NamedPipe(UniquePipeName());
    var server = transport.CreateServer(endpoint);
    await server.StartAsync();

    var serverTask = Task.Run(async () =>
    {
        var inbound = await server.WaitForMessageAsync();
        Assert.True(inbound.Success);
        var request = Assert.NotNull(inbound.Message);
        Assert.Equal(TalTransportMessageType.AgentHello, request.MessageType);
        var response = TalTransportMessage.Create(
            TalTransportMessageType.AgentStatusResponse,
            "agent-b",
            request.SourceId,
            new Dictionary<string, string>
            {
                ["status"] = "OK"
            },
            correlationId: request.MessageId);
        var sent = await server.SendResponseAsync(response);
        Assert.True(sent.Success);
        await server.StopAsync();
    });

    var client = transport.CreateClient(endpoint);
    var message = TalTransportMessage.Create(
        TalTransportMessageType.AgentHello,
        "agent-a",
        "agent-b");
    var result = await client.RequestAsync(message, TimeSpan.FromSeconds(2));

    Assert.True(result.Success);
    var responseMessage = Assert.NotNull(result.Message);
    Assert.Equal(message.MessageId, responseMessage.CorrelationId);
    Assert.Equal("OK", responseMessage.Payload["status"]);
    await serverTask;
}

static async Task NamedPipeTransportRequestResponse()
{
    var transport = TestNamedPipeTransport();
    var endpoint = TalTransportEndpoint.NamedPipe(UniquePipeName());
    var server = transport.CreateServer(endpoint);
    await server.StartAsync();

    var serverTask = Task.Run(async () =>
    {
        var inbound = await server.WaitForMessageAsync();
        Assert.True(inbound.Success);
        var request = Assert.NotNull(inbound.Message);
        var response = TalTransportMessage.Create(
            TalTransportMessageType.TransferResponse,
            "agent-b",
            request.SourceId,
            new Dictionary<string, string>
            {
                ["success"] = "true"
            },
            correlationId: request.MessageId);
        var sent = await server.SendResponseAsync(response);
        Assert.True(sent.Success);
        await server.StopAsync();
    });

    var client = transport.CreateClient(endpoint);
    var result = await client.RequestAsync(
        TalTransportMessage.Create(
            TalTransportMessageType.TransferRequest,
            "agent-a",
            "agent-b",
            new Dictionary<string, string>
            {
                ["requestId"] = "request-transport"
            }),
        TimeSpan.FromSeconds(2));

    Assert.True(result.Success);
    Assert.Equal("true", Assert.NotNull(result.Message).Payload["success"]);
    await serverTask;
}

static async Task NamedPipeTransportTimeoutReturnsFailure()
{
    var transport = TestNamedPipeTransport();
    var endpoint = TalTransportEndpoint.NamedPipe(UniquePipeName());
    var server = transport.CreateServer(endpoint);
    await server.StartAsync();

    var serverTask = Task.Run(async () =>
    {
        var inbound = await server.WaitForMessageAsync();
        Assert.True(inbound.Success);
        await Task.Delay(500);
        if (inbound.Message is not null)
        {
            try
            {
                await server.SendResponseAsync(TalTransportMessage.Create(
                    TalTransportMessageType.AgentStatusResponse,
                    "agent-b",
                    inbound.Message.SourceId,
                    correlationId: inbound.Message.MessageId));
            }
            catch (IOException)
            {
            }
        }
    });

    var client = transport.CreateClient(endpoint);
    var result = await client.RequestAsync(
        TalTransportMessage.Create(
            TalTransportMessageType.AgentStatusRequest,
            "agent-a",
            "agent-b"),
        TimeSpan.FromMilliseconds(100));

    Assert.False(result.Success);
    Assert.True(result.TimedOut);
    await serverTask;
    await server.StopAsync();
}

static async Task NamedPipeTransportReportsWrongTarget()
{
    var transport = TestNamedPipeTransport();
    var endpoint = TalTransportEndpoint.NamedPipe(UniquePipeName());
    var server = transport.CreateServer(endpoint);
    await server.StartAsync();

    var serverTask = Task.Run(async () =>
    {
        var inbound = await server.WaitForMessageAsync();
        Assert.True(inbound.Success);
        var request = Assert.NotNull(inbound.Message);
        var response = request.TargetId == "agent-b"
            ? TalTransportMessage.Create(
                TalTransportMessageType.AgentStatusResponse,
                "agent-b",
                request.SourceId,
                correlationId: request.MessageId)
            : TalTransportMessage.Create(
                TalTransportMessageType.ErrorResponse,
                "agent-b",
                request.SourceId,
                new Dictionary<string, string>
                {
                    ["error"] = "Wrong target."
                },
                correlationId: request.MessageId);
        var sent = await server.SendResponseAsync(response);
        Assert.NotNull(sent.Message);
        await server.StopAsync();
    });

    var client = transport.CreateClient(endpoint);
    var result = await client.RequestAsync(
        TalTransportMessage.Create(
            TalTransportMessageType.AgentStatusRequest,
            "agent-a",
            "agent-c"),
        TimeSpan.FromSeconds(2));

    Assert.False(result.Success);
    Assert.True(result.Error.Contains("Wrong target.", StringComparison.Ordinal));
    await serverTask;
}

static TalNamedPipeTransport TestNamedPipeTransport()
{
    return new TalNamedPipeTransport(new TalNamedPipeTransportOptions
    {
        DefaultTimeout = TimeSpan.FromSeconds(2)
    });
}

static string UniquePipeName()
{
    return $"rkws-transport-test-{Guid.NewGuid():N}";
}

static (
    RegistryWorkspaceRegistry WorkspaceRegistry,
    CapabilityManager CapabilityManager,
    ManagedTransferObjectManager TransferObjectManager,
    CoreITransferEngine Engine,
    RegistryWorkspaceDescriptor Source,
    ManagedITransferObject TransferObject) TransferEngineFixture(
        params RegistryWorkspaceDescriptor[] targets)
{
    var workspaceRegistry = new RegistryWorkspaceRegistry();
    var capabilityManager = new CapabilityManager();
    var transferObjectManager = new ManagedTransferObjectManager();
    var engine = new CoreTransferEngine(
        workspaceRegistry,
        capabilityManager,
        transferObjectManager);
    var source = RegistryDescriptor(
        "workspace-a",
        position: RegistryWorkspacePosition.Center,
        isTrusted: true,
        priority: 10,
        capabilityIds: new[]
        {
            CapabilityId.Display,
            CapabilityId.Keyboard,
            CapabilityId.Clipboard,
            CapabilityId.Encryption,
            CapabilityId.Pairing
        });

    workspaceRegistry.RegisterWorkspace(RegistryWorkspace.FromDescriptor(source));
    foreach (var target in targets)
    {
        workspaceRegistry.RegisterWorkspace(RegistryWorkspace.FromDescriptor(target));
    }

    var transferObject = transferObjectManager.Create(
        ManagedTransferObjectType.Text,
        ManagedMetadata(
            "object-a",
            displayName: "Transfer text",
            mimeType: "text/plain; charset=utf-8",
            sourceWorkspace: source.WorkspaceId.ToString(),
            targetWorkspace: string.Empty,
            owner: "transfer-engine-test"));

    return (
        workspaceRegistry,
        capabilityManager,
        transferObjectManager,
        engine,
        source,
        transferObject);
}

static CoreTransferRequest TransferRequest(
    ManagedTransferObjectId transferObjectId,
    RegistryWorkspaceId? sourceWorkspaceId = null,
    CoreTransferDirection direction = CoreTransferDirection.Right,
    CapabilitySet? required = null,
    CapabilitySet? optional = null,
    CapabilitySet? forbidden = null,
    string requestId = "request-a")
{
    return new CoreTransferRequest
    {
        RequestId = requestId,
        SourceWorkspaceId = sourceWorkspaceId ?? RegistryWorkspaceId.Create("workspace-a"),
        RequestedDirection = direction,
        TransferObjectId = transferObjectId,
        RequiredCapabilities = required ?? CapabilitySet.FromIds(
            CapabilityId.Display,
            CapabilityId.Clipboard,
            CapabilityId.Encryption,
            CapabilityId.Pairing),
        OptionalCapabilities = optional ?? CapabilitySet.Empty,
        ForbiddenCapabilities = forbidden ?? CapabilitySet.Empty,
        CreatedAt = new DateTimeOffset(2026, 7, 2, 9, 30, 0, TimeSpan.Zero),
        RequestedBy = "unit-test",
        Metadata = new Dictionary<string, string>
        {
            ["test"] = "transfer-engine"
        }
    };
}

static RegistryWorkspaceDescriptor RightTarget(string workspaceId)
{
    return TransferTarget(
        workspaceId,
        RegistryWorkspacePosition.Right,
        5,
        CapabilityId.Display,
        CapabilityId.Clipboard,
        CapabilityId.Encryption,
        CapabilityId.Pairing);
}

static RegistryWorkspaceDescriptor LeftTarget(string workspaceId)
{
    return TransferTarget(
        workspaceId,
        RegistryWorkspacePosition.Left,
        5,
        CapabilityId.Display,
        CapabilityId.Clipboard,
        CapabilityId.Encryption,
        CapabilityId.Pairing);
}

static RegistryWorkspaceDescriptor TransferTarget(
    string workspaceId,
    RegistryWorkspacePosition position,
    int priority,
    params CapabilityId[] capabilityIds)
{
    return RegistryDescriptor(
        workspaceId,
        position: position,
        isTrusted: true,
        priority: priority,
        lastSeen: new DateTimeOffset(2026, 7, 2, 9, 0, 0, TimeSpan.Zero).AddMinutes(priority),
        capabilityIds: capabilityIds);
}

static ManagedTransferMetadata ManagedMetadata(
    string objectId,
    string displayName = "Transfer object",
    string mimeType = "application/octet-stream",
    long size = 42,
    string checksum = "sha256:test",
    string sourceWorkspace = "workspace-a",
    string targetWorkspace = "workspace-b",
    string owner = "rk",
    int priority = 0,
    string[]? tags = null,
    string version = "1.0.0")
{
    return new ManagedTransferMetadata
    {
        ObjectId = ManagedTransferObjectId.Create(objectId),
        DisplayName = displayName,
        MimeType = mimeType,
        Size = size,
        Checksum = checksum,
        CreatedAt = new DateTimeOffset(2026, 7, 2, 9, 0, 0, TimeSpan.Zero),
        ModifiedAt = new DateTimeOffset(2026, 7, 2, 9, 0, 0, TimeSpan.Zero),
        SourceWorkspace = sourceWorkspace,
        TargetWorkspace = targetWorkspace,
        Owner = owner,
        Priority = priority,
        Tags = tags ?? Array.Empty<string>(),
        Version = version
    };
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

    public static void ThrowsWithCode(ManagedTransferObjectErrorCode expectedCode, Action action)
    {
        try
        {
            action();
        }
        catch (ManagedTransferObjectException ex)
        {
            Equal(expectedCode, ex.Code);
            return;
        }

        throw new InvalidOperationException($"Expected transfer object exception {expectedCode}.");
    }

    public static void ThrowsWithCode(CoreTransferFailureReason expectedReason, Action action)
    {
        try
        {
            action();
        }
        catch (CoreTransferEngineException ex)
        {
            Equal(expectedReason, ex.Reason);
            return;
        }

        throw new InvalidOperationException($"Expected transfer engine exception {expectedReason}.");
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
