using RKWorkspace.Core.Capabilities;

namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Provides platform-neutral workspace registration, search and target selection.
/// </summary>
public sealed class WorkspaceRegistry : IWorkspaceRegistry
{
    private readonly Dictionary<WorkspaceId, IWorkspace> _workspaces = new();

    /// <inheritdoc />
    public void RegisterWorkspace(IWorkspace workspace)
    {
        if (workspace is null)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidDescriptor,
                "Workspace is required.");
        }

        var descriptor = workspace.Descriptor;
        WorkspaceDescriptor.Validate(descriptor);

        if (_workspaces.ContainsKey(workspace.Id))
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.WorkspaceAlreadyRegistered,
                $"Workspace '{workspace.Id}' is already registered.",
                workspace.Id.ToString());
        }

        _workspaces.Add(workspace.Id, workspace);
    }

    /// <inheritdoc />
    public void UnregisterWorkspace(WorkspaceId workspaceId)
    {
        EnsureWorkspaceId(workspaceId);

        if (!_workspaces.Remove(workspaceId))
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.WorkspaceNotRegistered,
                $"Workspace '{workspaceId}' is not registered.",
                workspaceId.ToString());
        }
    }

    /// <inheritdoc />
    public void UpdateWorkspace(WorkspaceDescriptor descriptor)
    {
        WorkspaceDescriptor.Validate(descriptor);

        if (!_workspaces.TryGetValue(descriptor.WorkspaceId, out var workspace))
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.WorkspaceNotRegistered,
                $"Workspace '{descriptor.WorkspaceId}' is not registered.",
                descriptor.WorkspaceId.ToString());
        }

        workspace.UpdateDescriptor(descriptor.Snapshot());
    }

    /// <inheritdoc />
    public IWorkspace? GetWorkspace(WorkspaceId workspaceId)
    {
        if (workspaceId is null)
        {
            return null;
        }

        return _workspaces.TryGetValue(workspaceId, out var workspace)
            ? workspace
            : null;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IWorkspace> GetAllWorkspaces()
    {
        return OrderWorkspaces(_workspaces.Values).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IWorkspace> FindByPosition(WorkspacePosition position)
    {
        return OrderWorkspaces(_workspaces.Values
            .Where(workspace => workspace.Descriptor.Position == position))
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IWorkspace> FindByCapability(CapabilityId capabilityId)
    {
        if (capabilityId == CapabilityId.Unknown)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidQuery,
                "Capability id must not be Unknown.");
        }

        return OrderWorkspaces(_workspaces.Values
            .Where(workspace => workspace.Capabilities.Contains(capabilityId)))
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<WorkspaceMatchResult> FindMatching(WorkspaceQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        query.Validate();

        return _workspaces.Values
            .Select(workspace => MatchWorkspace(workspace, query))
            .Where(result => result.IsMatch)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Workspace.Id.ToString(), StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <inheritdoc />
    public IWorkspace GetBestTarget(WorkspaceQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        query.Validate();

        var matches = _workspaces.Values
            .Select(workspace => MatchWorkspace(workspace, query))
            .Where(result => result.IsMatch)
            .ToArray();

        if (matches.Length == 0)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.TargetNotFound,
                "No matching workspace target was found.");
        }

        if (matches.Length == 1)
        {
            return matches[0].Workspace;
        }

        return matches
            .OrderByDescending(result => PositionScore(result.Workspace.Descriptor, query))
            .ThenByDescending(result => CapabilityScore(result.Workspace.Descriptor, query))
            .ThenByDescending(result => result.Workspace.Descriptor.IsTrusted)
            .ThenByDescending(result => result.Workspace.Descriptor.Priority)
            .ThenByDescending(result => result.Workspace.Descriptor.LastSeen)
            .ThenBy(result => result.Workspace.Id.ToString(), StringComparer.OrdinalIgnoreCase)
            .First()
            .Workspace;
    }

    /// <inheritdoc />
    public bool ContainsWorkspace(WorkspaceId workspaceId)
    {
        return workspaceId is not null && _workspaces.ContainsKey(workspaceId);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<WorkspaceDescriptor> CreateSnapshot()
    {
        return OrderWorkspaces(_workspaces.Values)
            .Select(workspace => workspace.Descriptor.Snapshot())
            .ToArray();
    }

    private static WorkspaceMatchResult MatchWorkspace(IWorkspace workspace, WorkspaceQuery query)
    {
        var descriptor = workspace.Descriptor;
        var reasons = new List<string>();
        var missingCapabilities = query.RequiredCapabilities.Capabilities
            .Where(capability => !descriptor.Capabilities.Contains(capability.CapabilityId))
            .Select(capability => capability.CapabilityId)
            .Distinct()
            .ToArray();

        var isMatch = true;

        if (query.WorkspaceType.HasValue && descriptor.WorkspaceType != query.WorkspaceType.Value)
        {
            isMatch = false;
            reasons.Add($"Workspace type {descriptor.WorkspaceType} does not match {query.WorkspaceType.Value}.");
        }

        if (query.WorkspaceState.HasValue && descriptor.WorkspaceState != query.WorkspaceState.Value)
        {
            isMatch = false;
            reasons.Add($"Workspace state {descriptor.WorkspaceState} does not match {query.WorkspaceState.Value}.");
        }

        if (query.Position.HasValue && descriptor.Position != query.Position.Value)
        {
            isMatch = false;
            reasons.Add($"Workspace position {descriptor.Position} does not match {query.Position.Value}.");
        }

        if (query.TrustedOnly && !descriptor.IsTrusted)
        {
            isMatch = false;
            reasons.Add("Workspace is not trusted.");
        }

        if (query.AvailableOnly && !IsAvailableState(descriptor.WorkspaceState))
        {
            isMatch = false;
            reasons.Add("Workspace is not available.");
        }

        if (query.MinimumPriority.HasValue && descriptor.Priority < query.MinimumPriority.Value)
        {
            isMatch = false;
            reasons.Add($"Workspace priority {descriptor.Priority} is below {query.MinimumPriority.Value}.");
        }

        if (missingCapabilities.Length > 0)
        {
            isMatch = false;
            reasons.Add($"Missing capabilities: {string.Join(", ", missingCapabilities)}.");
        }

        if (isMatch)
        {
            reasons.Add("Workspace matched.");
        }

        return new WorkspaceMatchResult
        {
            Workspace = workspace,
            IsMatch = isMatch,
            Score = isMatch ? Score(descriptor, query) : 0,
            Reasons = reasons,
            MissingCapabilities = missingCapabilities
        };
    }

    private static int Score(WorkspaceDescriptor descriptor, WorkspaceQuery query)
    {
        return PositionScore(descriptor, query)
            + CapabilityScore(descriptor, query)
            + (descriptor.IsTrusted ? 100 : 0)
            + descriptor.Priority;
    }

    private static int PositionScore(WorkspaceDescriptor descriptor, WorkspaceQuery query)
    {
        return query.Position.HasValue && descriptor.Position == query.Position.Value
            ? 10_000
            : 0;
    }

    private static int CapabilityScore(WorkspaceDescriptor descriptor, WorkspaceQuery query)
    {
        var requiredScore = query.RequiredCapabilities.Capabilities
            .Count(capability => descriptor.Capabilities.Contains(capability.CapabilityId)) * 100;
        var optionalScore = query.OptionalCapabilities.Capabilities
            .Count(capability => descriptor.Capabilities.Contains(capability.CapabilityId)) * 10;

        return requiredScore + optionalScore;
    }

    private static bool IsAvailableState(WorkspaceState state)
    {
        return state is WorkspaceState.Available or WorkspaceState.Trusted;
    }

    private static IEnumerable<IWorkspace> OrderWorkspaces(IEnumerable<IWorkspace> workspaces)
    {
        return workspaces.OrderBy(workspace => workspace.Id.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    private static void EnsureWorkspaceId(WorkspaceId workspaceId)
    {
        if (workspaceId is null)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.MissingWorkspaceId,
                "Workspace id is required.");
        }
    }
}
