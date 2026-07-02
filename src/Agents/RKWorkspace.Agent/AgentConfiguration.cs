using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Agent;

internal enum AgentRunMode
{
    LocalOnly
}

internal sealed record AgentConfiguration
{
    public string AgentId { get; init; } = "rkws-agent-local";

    public string DisplayName { get; init; } = "RKWS Local Agent";

    public string WorkspaceName { get; init; } = "RKWS Local Workspace";

    public WorkspaceType WorkspaceType { get; init; } = WorkspaceType.SmartDevice;

    public WorkspacePosition WorkspacePosition { get; init; } = WorkspacePosition.Center;

    public AgentRunMode RunMode { get; init; } = AgentRunMode.LocalOnly;

    public bool EnableDemoWorkspace { get; init; } = true;

    public bool EnableConsoleStatus { get; init; } = true;

    public int HeartbeatIntervalSeconds { get; init; } = 5;

    public AgentConfiguration Validate()
    {
        if (string.IsNullOrWhiteSpace(AgentId))
        {
            throw new AgentException("AgentId is required.");
        }

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            throw new AgentException("DisplayName is required.");
        }

        if (string.IsNullOrWhiteSpace(WorkspaceName))
        {
            throw new AgentException("WorkspaceName is required.");
        }

        if (WorkspaceType == WorkspaceType.Unknown)
        {
            throw new AgentException("WorkspaceType must not be Unknown.");
        }

        if (WorkspacePosition == WorkspacePosition.Unknown)
        {
            throw new AgentException("WorkspacePosition must not be Unknown.");
        }

        if (HeartbeatIntervalSeconds <= 0)
        {
            throw new AgentException("HeartbeatIntervalSeconds must be positive.");
        }

        return this;
    }
}
