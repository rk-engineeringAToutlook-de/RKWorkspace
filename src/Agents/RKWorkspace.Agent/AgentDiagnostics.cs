using RKWorkspace.Core.Runtime;

namespace RKWorkspace.Agent;

internal sealed record AgentDiagnostics
{
    public required string AgentId { get; init; }

    public required string DisplayName { get; init; }

    public required AgentState State { get; init; }

    public required string WorkspaceId { get; init; }

    public required string WorkspaceName { get; init; }

    public required string Mode { get; init; }

    public required RuntimeState RuntimeState { get; init; }

    public required int WorkspaceCount { get; init; }

    public required int PluginCount { get; init; }

    public required int TransferObjectCount { get; init; }

    public required string Capabilities { get; init; }

    public required IReadOnlyCollection<string> Errors { get; init; }
}
