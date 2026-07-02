using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Provides platform-neutral logical transfer planning and execution.
/// </summary>
public sealed class TransferEngine : ITransferEngine
{
    private readonly IWorkspaceRegistry _workspaceRegistry;
    private readonly ICapabilityManager _capabilityManager;
    private readonly ITransferObjectManager _transferObjectManager;

    /// <summary>
    /// Initializes a transfer engine.
    /// </summary>
    /// <param name="workspaceRegistry">The workspace registry.</param>
    /// <param name="capabilityManager">The capability manager.</param>
    /// <param name="transferObjectManager">The transfer object manager.</param>
    public TransferEngine(
        IWorkspaceRegistry workspaceRegistry,
        ICapabilityManager capabilityManager,
        ITransferObjectManager transferObjectManager)
    {
        _workspaceRegistry = workspaceRegistry ?? throw new ArgumentNullException(nameof(workspaceRegistry));
        _capabilityManager = capabilityManager ?? throw new ArgumentNullException(nameof(capabilityManager));
        _transferObjectManager = transferObjectManager ?? throw new ArgumentNullException(nameof(transferObjectManager));
    }

    /// <inheritdoc />
    public TransferPlan CreatePlan(TransferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Validate();

        var source = _workspaceRegistry.GetWorkspace(request.SourceWorkspaceId);
        if (source is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.SourceWorkspaceMissing,
                $"Source workspace '{request.SourceWorkspaceId}' was not found.",
                request.RequestId);
        }

        var transferObject = _transferObjectManager.Get(request.TransferObjectId);
        if (transferObject is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.TransferObjectMissing,
                $"Transfer object '{request.TransferObjectId}' was not found.",
                request.RequestId);
        }

        var target = ResolveTarget(request, source);
        var now = DateTimeOffset.UtcNow;

        var steps = new[]
        {
            TransferStep.Pending(1, "ValidateRequest", "Validate transfer request.", now).Complete(now),
            TransferStep.Pending(2, "ResolveSourceWorkspace", "Resolve source workspace.", now).Complete(now),
            TransferStep.Pending(3, "ResolveTransferObject", "Resolve transfer object.", now).Complete(now),
            TransferStep.Pending(4, "ResolveTargetWorkspace", "Resolve target workspace.", now).Complete(now),
            TransferStep.Pending(5, "PrepareTransfer", "Prepare transfer object.", now),
            TransferStep.Pending(6, "CompleteTransfer", "Complete logical transfer.", now)
        };

        return new TransferPlan
        {
            PlanId = $"rkws-plan-{Guid.NewGuid():N}",
            Request = request.Snapshot(),
            SourceWorkspace = source,
            TargetWorkspace = target,
            TransferObject = transferObject,
            Steps = steps,
            CreatedAt = now,
            IsValid = true,
            ValidationMessages = Array.Empty<string>()
        };
    }

    /// <inheritdoc />
    public TransferPlan ValidatePlan(TransferPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        plan.ValidateShape();

        var messages = new List<string>();
        var source = _workspaceRegistry.GetWorkspace(plan.SourceWorkspace.Id);
        var target = _workspaceRegistry.GetWorkspace(plan.TargetWorkspace.Id);
        var transferObject = _transferObjectManager.Get(plan.TransferObject.Id);

        if (source is null)
        {
            messages.Add($"Source workspace '{plan.SourceWorkspace.Id}' is not registered.");
        }

        if (target is null)
        {
            messages.Add($"Target workspace '{plan.TargetWorkspace.Id}' is not registered.");
        }

        if (transferObject is null)
        {
            messages.Add($"Transfer object '{plan.TransferObject.Id}' is not registered.");
        }

        if (source is not null && source.Id != plan.Request.SourceWorkspaceId)
        {
            messages.Add("Plan source workspace does not match request source workspace.");
        }

        if (transferObject is not null && transferObject.Id != plan.Request.TransferObjectId)
        {
            messages.Add("Plan transfer object does not match request transfer object.");
        }

        if (target is not null)
        {
            var capabilityMatch = _capabilityManager.MatchRequirement(
                target.Capabilities,
                plan.Request.ToRequirement());

            if (!capabilityMatch.IsMatch)
            {
                messages.Add(capabilityMatch.Reason);
            }
        }

        if (transferObject is not null && IsTerminal(transferObject.State))
        {
            messages.Add($"Transfer object is already terminal: {transferObject.State}.");
        }

        var validated = plan with
        {
            SourceWorkspace = source ?? plan.SourceWorkspace,
            TargetWorkspace = target ?? plan.TargetWorkspace,
            TransferObject = transferObject ?? plan.TransferObject,
            IsValid = messages.Count == 0,
            ValidationMessages = messages.ToArray()
        };

        if (!validated.IsValid)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidPlan,
                string.Join(" ", validated.ValidationMessages),
                plan.Request.RequestId);
        }

        return validated;
    }

    /// <inheritdoc />
    public TransferPlan PrepareTransfer(TransferPlan plan)
    {
        var validated = ValidatePlan(plan);
        var transferObject = _transferObjectManager.Get(validated.TransferObject.Id);
        if (transferObject is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.TransferObjectMissing,
                $"Transfer object '{validated.TransferObject.Id}' was not found.",
                validated.Request.RequestId);
        }

        if (transferObject.State == TransferObjectState.Created)
        {
            _transferObjectManager.Validate(transferObject.Id);
            transferObject = GetRequiredTransferObject(transferObject.Id, validated.Request.RequestId);
        }

        var targetId = validated.TargetWorkspace.Id.ToString();
        if (!StringComparer.OrdinalIgnoreCase.Equals(transferObject.Metadata.TargetWorkspace, targetId))
        {
            transferObject = _transferObjectManager.UpdateMetadata(
                transferObject.Id,
                transferObject.Metadata with
                {
                    TargetWorkspace = targetId,
                    ModifiedAt = DateTimeOffset.UtcNow
                });
        }

        if (transferObject.State != TransferObjectState.Prepared)
        {
            EnsureStateCanPrepare(transferObject, validated.Request.RequestId);
            transferObject = _transferObjectManager.UpdateState(
                transferObject.Id,
                TransferObjectState.Prepared);
        }

        var now = DateTimeOffset.UtcNow;
        return validated with
        {
            TransferObject = transferObject,
            Steps = CompleteStep(validated.Steps, "PrepareTransfer", now)
        };
    }

    /// <inheritdoc />
    public TransferResult CompleteTransfer(TransferPlan plan)
    {
        var validated = ValidatePlan(plan);
        var transferObject = GetRequiredTransferObject(
            validated.TransferObject.Id,
            validated.Request.RequestId);

        if (transferObject.State != TransferObjectState.Prepared &&
            transferObject.State != TransferObjectState.Locked &&
            transferObject.State != TransferObjectState.Completed)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidObjectState,
                $"Transfer object '{transferObject.Id}' is not prepared.",
                validated.Request.RequestId);
        }

        if (transferObject.State != TransferObjectState.Completed)
        {
            transferObject = _transferObjectManager.UpdateState(
                transferObject.Id,
                TransferObjectState.Completed);
        }

        var now = DateTimeOffset.UtcNow;
        var completedSteps = CompleteStep(
            CompleteStep(validated.Steps, "PrepareTransfer", now),
            "CompleteTransfer",
            now);

        return new TransferResult
        {
            RequestId = validated.Request.RequestId,
            PlanId = validated.PlanId,
            IsSuccess = true,
            SourceWorkspace = validated.SourceWorkspace,
            TargetWorkspace = validated.TargetWorkspace,
            TransferObject = transferObject,
            FinalState = transferObject.State,
            FailureReason = TransferFailureReason.None,
            Messages = completedSteps
                .OrderBy(step => step.StepNumber)
                .Select(step => $"{step.Name}:{step.Status}")
                .ToArray(),
            CompletedAt = now
        };
    }

    /// <inheritdoc />
    public TransferResult CancelTransfer(TransferPlan plan)
    {
        var validated = ValidatePlan(plan);
        var transferObject = GetRequiredTransferObject(
            validated.TransferObject.Id,
            validated.Request.RequestId);

        if (transferObject.State != TransferObjectState.Cancelled)
        {
            transferObject = _transferObjectManager.UpdateState(
                transferObject.Id,
                TransferObjectState.Cancelled);
        }

        return BuildFailureResult(
            validated,
            transferObject,
            TransferFailureReason.Cancelled,
            "Transfer cancelled.");
    }

    /// <inheritdoc />
    public TransferResult FailTransfer(TransferPlan plan, string message)
    {
        var validated = ValidatePlan(plan);
        var transferObject = GetRequiredTransferObject(
            validated.TransferObject.Id,
            validated.Request.RequestId);

        if (transferObject.State != TransferObjectState.Failed)
        {
            transferObject = _transferObjectManager.UpdateState(
                transferObject.Id,
                TransferObjectState.Failed);
        }

        return BuildFailureResult(
            validated,
            transferObject,
            TransferFailureReason.Failed,
            string.IsNullOrWhiteSpace(message) ? "Transfer failed." : message);
    }

    /// <inheritdoc />
    public TransferResult ExecuteLogicalTransfer(TransferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var plan = CreatePlan(request);
            var prepared = PrepareTransfer(plan);
            return CompleteTransfer(prepared);
        }
        catch (TransferEngineException ex)
        {
            return new TransferResult
            {
                RequestId = string.IsNullOrWhiteSpace(request.RequestId)
                    ? string.Empty
                    : request.RequestId,
                IsSuccess = false,
                SourceWorkspace = request.SourceWorkspaceId is null
                    ? null
                    : _workspaceRegistry.GetWorkspace(request.SourceWorkspaceId),
                TransferObject = request.TransferObjectId is null
                    ? null
                    : _transferObjectManager.Get(request.TransferObjectId),
                FinalState = request.TransferObjectId is null
                    ? null
                    : _transferObjectManager.Get(request.TransferObjectId)?.State,
                FailureReason = ex.Reason,
                Messages = new[] { ex.Message },
                CompletedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new TransferResult
            {
                RequestId = string.IsNullOrWhiteSpace(request.RequestId)
                    ? string.Empty
                    : request.RequestId,
                IsSuccess = false,
                SourceWorkspace = request.SourceWorkspaceId is null
                    ? null
                    : _workspaceRegistry.GetWorkspace(request.SourceWorkspaceId),
                TransferObject = request.TransferObjectId is null
                    ? null
                    : _transferObjectManager.Get(request.TransferObjectId),
                FinalState = request.TransferObjectId is null
                    ? null
                    : _transferObjectManager.Get(request.TransferObjectId)?.State,
                FailureReason = TransferFailureReason.Unknown,
                Messages = new[] { ex.Message },
                CompletedAt = DateTimeOffset.UtcNow
            };
        }
    }

    private IWorkspace ResolveTarget(TransferRequest request, IWorkspace source)
    {
        var candidates = GetMatchingCandidates(request, source.Id);

        if (candidates.Length == 0)
        {
            throw new TransferEngineException(
                DetermineTargetFailureReason(request),
                "No matching target workspace was found.",
                request.RequestId);
        }

        if (candidates.Length == 1 || request.RequestedDirection == TransferDirection.Any)
        {
            return GetBestTarget(candidates, request, null);
        }

        var position = MapDirection(request.RequestedDirection);
        var positionedCandidates = candidates
            .Where(workspace => workspace.Descriptor.Position == position)
            .ToArray();

        if (positionedCandidates.Length == 0)
        {
            throw new TransferEngineException(
                TransferFailureReason.TargetWorkspaceMissing,
                $"No matching target workspace was found at {position}.",
                request.RequestId);
        }

        return GetBestTarget(positionedCandidates, request, position);
    }

    private IWorkspace[] GetMatchingCandidates(TransferRequest request, WorkspaceId sourceWorkspaceId)
    {
        var query = new WorkspaceQuery
        {
            TrustedOnly = true,
            AvailableOnly = true,
            RequiredCapabilities = request.RequiredCapabilities,
            OptionalCapabilities = request.OptionalCapabilities
        };

        var requirement = request.ToRequirement();
        return _workspaceRegistry.FindMatching(query)
            .Select(result => result.Workspace)
            .Where(workspace => workspace.Id != sourceWorkspaceId)
            .Where(workspace => _capabilityManager.MatchRequirement(workspace.Capabilities, requirement).IsMatch)
            .ToArray();
    }

    private IWorkspace GetBestTarget(
        IReadOnlyCollection<IWorkspace> candidates,
        TransferRequest request,
        WorkspacePosition? position)
    {
        var registry = new WorkspaceRegistry();
        foreach (var candidate in candidates)
        {
            registry.RegisterWorkspace(candidate);
        }

        var query = new WorkspaceQuery
        {
            Position = position,
            TrustedOnly = true,
            AvailableOnly = true,
            RequiredCapabilities = request.RequiredCapabilities,
            OptionalCapabilities = request.OptionalCapabilities
        };

        return registry.GetBestTarget(query);
    }

    private TransferFailureReason DetermineTargetFailureReason(TransferRequest request)
    {
        var allTargets = _workspaceRegistry.GetAllWorkspaces()
            .Where(workspace => workspace.Id != request.SourceWorkspaceId)
            .ToArray();

        if (allTargets.Length == 0)
        {
            return TransferFailureReason.TargetWorkspaceMissing;
        }

        var requirement = request.ToRequirement();
        var matches = allTargets
            .Select(workspace => _capabilityManager.MatchRequirement(workspace.Capabilities, requirement))
            .ToArray();

        if (matches.Any(match => match.PresentForbiddenCapabilities.Count > 0))
        {
            return TransferFailureReason.ForbiddenCapabilitiesPresent;
        }

        if (matches.Any(match => match.MissingRequiredCapabilities.Count > 0))
        {
            return TransferFailureReason.RequiredCapabilitiesMissing;
        }

        return TransferFailureReason.TargetWorkspaceMissing;
    }

    private ITransferObject GetRequiredTransferObject(TransferObjectId objectId, string requestId)
    {
        return _transferObjectManager.Get(objectId)
            ?? throw new TransferEngineException(
                TransferFailureReason.TransferObjectMissing,
                $"Transfer object '{objectId}' was not found.",
                requestId);
    }

    private static void EnsureStateCanPrepare(ITransferObject transferObject, string requestId)
    {
        if (transferObject.State is
            TransferObjectState.Validated or
            TransferObjectState.Queued or
            TransferObjectState.Prepared)
        {
            return;
        }

        throw new TransferEngineException(
            TransferFailureReason.InvalidObjectState,
            $"Transfer object '{transferObject.Id}' cannot be prepared from {transferObject.State}.",
            requestId);
    }

    private static bool IsTerminal(TransferObjectState state)
    {
        return state is TransferObjectState.Completed
            or TransferObjectState.Cancelled
            or TransferObjectState.Failed
            or TransferObjectState.Archived;
    }

    private static TransferStep[] CompleteStep(
        IEnumerable<TransferStep> steps,
        string name,
        DateTimeOffset completedAt)
    {
        return steps
            .Select(step => string.Equals(step.Name, name, StringComparison.OrdinalIgnoreCase)
                ? step.Complete(completedAt)
                : step)
            .ToArray();
    }

    private static TransferResult BuildFailureResult(
        TransferPlan plan,
        ITransferObject transferObject,
        TransferFailureReason reason,
        string message)
    {
        return new TransferResult
        {
            RequestId = plan.Request.RequestId,
            PlanId = plan.PlanId,
            IsSuccess = false,
            SourceWorkspace = plan.SourceWorkspace,
            TargetWorkspace = plan.TargetWorkspace,
            TransferObject = transferObject,
            FinalState = transferObject.State,
            FailureReason = reason,
            Messages = new[] { message },
            CompletedAt = DateTimeOffset.UtcNow
        };
    }

    private static WorkspacePosition MapDirection(TransferDirection direction)
    {
        return direction switch
        {
            TransferDirection.Left => WorkspacePosition.Left,
            TransferDirection.Right => WorkspacePosition.Right,
            TransferDirection.Above => WorkspacePosition.Above,
            TransferDirection.Below => WorkspacePosition.Below,
            TransferDirection.Front => WorkspacePosition.Front,
            TransferDirection.Back => WorkspacePosition.Back,
            TransferDirection.Any => WorkspacePosition.Unknown,
            _ => throw new TransferEngineException(
                TransferFailureReason.InvalidDirection,
                "Transfer direction must not be Unknown.")
        };
    }
}
