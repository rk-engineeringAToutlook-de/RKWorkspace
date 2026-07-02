using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Plugins;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Transfers;
using RKWorkspace.Core.Workspaces;

internal sealed class CoreIntegrationScenarioRunner
{
    public CoreIntegrationScenarioResult TransferTextRightDirection()
    {
        return RunTransferScenario(new ScenarioOptions
        {
            Name = "CoreIntegrationScenario_TransferText_RightDirection",
            Direction = TransferDirection.Right,
            TargetWorkspaces =
            {
                Workspace(
                    "workspace-b",
                    WorkspacePosition.Right,
                    priority: 5,
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing)
            }
        });
    }

    public CoreIntegrationScenarioResult NoMatchingTargetFails()
    {
        return RunTransferScenario(new ScenarioOptions
        {
            Name = "CoreIntegrationScenario_NoMatchingTarget_Fails",
            Direction = TransferDirection.Right,
            ExpectSuccess = false,
            TargetWorkspaces =
            {
                Workspace(
                    "workspace-b",
                    WorkspacePosition.Right,
                    priority: 5,
                    CapabilityId.Display,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing)
            }
        });
    }

    public CoreIntegrationScenarioResult OnlyOneTargetAnyDirection()
    {
        return RunTransferScenario(new ScenarioOptions
        {
            Name = "CoreIntegrationScenario_OnlyOneTarget_AnyDirection",
            Direction = TransferDirection.Left,
            TargetWorkspaces =
            {
                Workspace(
                    "workspace-b",
                    WorkspacePosition.Right,
                    priority: 5,
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing)
            }
        });
    }

    public CoreIntegrationScenarioResult ForbiddenCapabilityRejected()
    {
        return RunTransferScenario(new ScenarioOptions
        {
            Name = "CoreIntegrationScenario_ForbiddenCapabilityRejected",
            Direction = TransferDirection.Right,
            ForbiddenCapabilities = CapabilitySet.FromIds(CapabilityId.CloudMode),
            TargetWorkspaces =
            {
                Workspace(
                    "workspace-cloud",
                    WorkspacePosition.Right,
                    priority: 100,
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing,
                    CapabilityId.CloudMode),
                Workspace(
                    "workspace-b",
                    WorkspacePosition.Right,
                    priority: 5,
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing)
            }
        });
    }

    public CoreIntegrationScenarioResult PriorityBreaksTie()
    {
        return RunTransferScenario(new ScenarioOptions
        {
            Name = "CoreIntegrationScenario_PriorityBreaksTie",
            Direction = TransferDirection.Right,
            TargetWorkspaces =
            {
                Workspace(
                    "workspace-low",
                    WorkspacePosition.Right,
                    priority: 1,
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing),
                Workspace(
                    "workspace-high",
                    WorkspacePosition.Right,
                    priority: 20,
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing)
            }
        });
    }

    private static CoreIntegrationScenarioResult RunTransferScenario(ScenarioOptions options)
    {
        var steps = new List<string>();

        try
        {
            var pluginManager = new PluginManager();
            var capabilityManager = new CapabilityManager();
            var workspaceRegistry = new WorkspaceRegistry();
            var transferObjectManager = new TransferObjectManager();
            var transferEngine = new TransferEngine(
                workspaceRegistry,
                capabilityManager,
                transferObjectManager);
            steps.Add("Core managers created.");
            steps.Add("Transfer Engine created.");

            var plugin = new IntegrationPlugin(
                "integration.transfer",
                PluginType.Testing,
                "Core integration transfer scenario",
                new[]
                {
                    CapabilityId.Display.ToString(),
                    CapabilityId.Clipboard.ToString(),
                    CapabilityId.Encryption.ToString(),
                    CapabilityId.Pairing.ToString()
                });
            var pluginRegistration = pluginManager.RegisterPlugin(plugin);
            if (!pluginRegistration.Success)
            {
                return Failure(options.Name, steps, $"Plugin registration failed: {pluginRegistration.Error?.Code}");
            }

            var pluginActivation = pluginManager.ActivatePlugin(plugin.PluginId);
            if (!pluginActivation.Success)
            {
                return Failure(options.Name, steps, $"Plugin activation failed: {pluginActivation.Error?.Code}");
            }

            steps.Add("Plugin Manager registered and activated integration plugin.");

            var source = Workspace(
                "workspace-a",
                WorkspacePosition.Center,
                priority: 10,
                CapabilityId.Display,
                CapabilityId.Keyboard,
                CapabilityId.Clipboard,
                CapabilityId.Encryption,
                CapabilityId.Pairing);

            RegisterWorkspace(workspaceRegistry, capabilityManager, source);
            steps.Add("Workspace A registered.");

            foreach (var targetDescriptor in options.TargetWorkspaces)
            {
                RegisterWorkspace(workspaceRegistry, capabilityManager, targetDescriptor);
                steps.Add($"{targetDescriptor.WorkspaceId} registered.");
            }

            var sourceRequirement = new CapabilityRequirement
            {
                RequiredCapabilities = new[]
                {
                    CapabilityId.Display,
                    CapabilityId.Keyboard,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing
                }
            };
            var sourceMatch = capabilityManager.MatchRequirement(source.Capabilities, sourceRequirement);
            if (!sourceMatch.IsMatch)
            {
                return Failure(options.Name, steps, "Workspace A does not meet source capability requirements.");
            }

            steps.Add("Workspace A capabilities checked.");

            var transferObject = transferObjectManager.Create(
                TransferObjectType.Text,
                Metadata(source.WorkspaceId.ToString()));
            steps.Add("Transfer object created.");

            var request = new TransferRequest
            {
                RequestId = $"request-{options.Name}",
                SourceWorkspaceId = source.WorkspaceId,
                RequestedDirection = options.Direction,
                TransferObjectId = transferObject.Id,
                RequiredCapabilities = options.RequiredCapabilities,
                OptionalCapabilities = options.OptionalCapabilities,
                ForbiddenCapabilities = options.ForbiddenCapabilities,
                CreatedAt = new DateTimeOffset(2026, 7, 2, 12, 0, 40, TimeSpan.Zero),
                RequestedBy = "integration",
                Metadata = new Dictionary<string, string>
                {
                    ["scenario"] = options.Name
                }
            };
            steps.Add("Transfer request created.");

            var result = transferEngine.ExecuteLogicalTransfer(request);
            steps.Add("Transfer Engine executed logical transfer.");

            var finalObject = transferObjectManager.Get(transferObject.Id);
            if (finalObject is null)
            {
                return Failure(options.Name, steps, "Transfer object disappeared.");
            }

            var historyActions = finalObject.History
                .Select(entry => entry.Action)
                .ToArray();

            if (!options.ExpectSuccess)
            {
                return new CoreIntegrationScenarioResult
                {
                    ScenarioName = options.Name,
                    IsSuccess = !result.IsSuccess,
                    Steps = steps.Append("No matching target found.").ToArray(),
                    SelectedTarget = result.TargetWorkspace?.Descriptor.WorkspaceId.ToString() ?? string.Empty,
                    TransferObjectId = string.Empty,
                    FinalState = finalObject.State,
                    HistoryActions = historyActions,
                    ErrorMessage = result.Messages.FirstOrDefault() ?? string.Empty
                };
            }

            if (result.TargetWorkspace is not null)
            {
                steps.Add($"Target selected: {result.TargetWorkspace.Descriptor.WorkspaceId}.");
            }

            steps.Add("Transfer completed.");

            var hasCoreHistory = new[]
            {
                "Created",
                "State:Validated",
                "MetadataUpdated",
                "State:Prepared",
                "State:Completed"
            }.All(historyActions.Contains);

            return new CoreIntegrationScenarioResult
            {
                ScenarioName = options.Name,
                IsSuccess = result.IsSuccess &&
                    finalObject.State == TransferObjectState.Completed &&
                    hasCoreHistory,
                Steps = steps,
                SelectedTarget = result.TargetWorkspace?.Descriptor.WorkspaceId.ToString() ?? string.Empty,
                TransferObjectId = finalObject.Id.ToString(),
                FinalState = finalObject.State,
                HistoryActions = historyActions,
                ErrorMessage = hasCoreHistory ? string.Empty : "Transfer object history is incomplete."
            };
        }
        catch (Exception ex)
        {
            return Failure(options.Name, steps, ex.Message);
        }
    }

    private static void RegisterWorkspace(
        WorkspaceRegistry workspaceRegistry,
        CapabilityManager capabilityManager,
        WorkspaceDescriptor descriptor)
    {
        var workspace = RKWorkspace.Core.Workspaces.Workspace.FromDescriptor(descriptor);
        workspaceRegistry.RegisterWorkspace(workspace);
        capabilityManager.RegisterProvider(new WorkspaceCapabilityProvider(descriptor));
    }

    private static WorkspaceDescriptor Workspace(
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
            LastSeen = new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero),
            Metadata = new Dictionary<string, string>
            {
                ["scenario"] = "core-integration"
            }
        };
    }

    private static TransferMetadata Metadata(string sourceWorkspace)
    {
        return new TransferMetadata
        {
            ObjectId = TransferObjectId.NewId(),
            DisplayName = "Integration text",
            MimeType = "text/plain; charset=utf-8",
            Size = 24,
            Checksum = "sha256:integration-text",
            CreatedAt = new DateTimeOffset(2026, 7, 2, 12, 0, 30, TimeSpan.Zero),
            ModifiedAt = new DateTimeOffset(2026, 7, 2, 12, 0, 30, TimeSpan.Zero),
            SourceWorkspace = sourceWorkspace,
            TargetWorkspace = string.Empty,
            Owner = "integration",
            Priority = 1,
            Tags = new[] { "integration", "text" },
            Version = "1.0.0"
        };
    }

    private static CoreIntegrationScenarioResult Failure(
        string scenarioName,
        IReadOnlyCollection<string> steps,
        string errorMessage)
    {
        return new CoreIntegrationScenarioResult
        {
            ScenarioName = scenarioName,
            IsSuccess = false,
            Steps = steps,
            ErrorMessage = errorMessage
        };
    }

    private sealed class ScenarioOptions
    {
        public required string Name { get; init; }

        public TransferDirection Direction { get; init; } = TransferDirection.Right;

        public CapabilitySet RequiredCapabilities { get; init; } =
            CapabilitySet.FromIds(
                CapabilityId.Display,
                CapabilityId.Clipboard,
                CapabilityId.Encryption,
                CapabilityId.Pairing);

        public CapabilitySet OptionalCapabilities { get; init; } = CapabilitySet.Empty;

        public CapabilitySet ForbiddenCapabilities { get; init; } = CapabilitySet.Empty;

        public bool ExpectSuccess { get; init; } = true;

        public List<WorkspaceDescriptor> TargetWorkspaces { get; } = new();
    }

    private sealed class WorkspaceCapabilityProvider : ICapabilityProvider
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

    private sealed class IntegrationPlugin : IPlugin
    {
        private readonly string[] _capabilities;

        public IntegrationPlugin(
            string pluginId,
            PluginType type,
            string name,
            string[] capabilities)
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
}
