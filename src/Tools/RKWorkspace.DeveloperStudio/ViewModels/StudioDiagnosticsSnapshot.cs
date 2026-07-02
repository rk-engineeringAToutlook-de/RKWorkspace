namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record StudioDiagnosticsSnapshot
{
    public required string RuntimeVersion { get; init; }

    public required string RuntimeState { get; init; }

    public required int PluginCount { get; init; }

    public required int WorkspaceCount { get; init; }

    public required int TransferObjectCount { get; init; }

    public required string Capabilities { get; init; }

    public required string LastResult { get; init; }

    public required string LastError { get; init; }
}
