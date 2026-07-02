using RKWorkspace.Core.Models;

namespace RKWorkspace.Core.Services;

public sealed class WorkspaceMap
{
    private readonly IReadOnlyList<Workspace> _workspaces;

    public WorkspaceMap(IEnumerable<Workspace> workspaces)
    {
        ArgumentNullException.ThrowIfNull(workspaces);

        var list = workspaces.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("A workspace map needs at least one workspace.", nameof(workspaces));
        }

        var distinctIds = list
            .Select(workspace => workspace.WorkspaceId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        if (distinctIds != list.Count)
        {
            throw new ArgumentException("Workspace IDs must be unique.", nameof(workspaces));
        }

        _workspaces = list;
    }

    public IReadOnlyList<Workspace> Workspaces => _workspaces;

    public Workspace? Center => _workspaces.FirstOrDefault(workspace => workspace.Position == WorkspacePosition.Center);

    public Workspace? FindByPosition(WorkspacePosition position)
    {
        return _workspaces.FirstOrDefault(workspace => workspace.Position == position);
    }

    public Workspace ResolveDirectionalTarget(WorkspacePosition direction)
    {
        if (direction is WorkspacePosition.Center or WorkspacePosition.Unknown)
        {
            throw new ArgumentException("A transfer direction must be Left, Right, Above or Below.", nameof(direction));
        }

        return FindByPosition(direction)
            ?? throw new InvalidOperationException($"No workspace is configured at position {direction}.");
    }
}
