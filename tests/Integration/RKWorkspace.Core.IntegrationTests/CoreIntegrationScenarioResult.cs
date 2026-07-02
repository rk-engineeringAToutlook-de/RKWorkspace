using RKWorkspace.Core.TransferObjects;

internal sealed record CoreIntegrationScenarioResult
{
    public required string ScenarioName { get; init; }

    public required bool IsSuccess { get; init; }

    public IReadOnlyCollection<string> Steps { get; init; } = Array.Empty<string>();

    public string SelectedTarget { get; init; } = string.Empty;

    public string TransferObjectId { get; init; } = string.Empty;

    public TransferObjectState? FinalState { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public IReadOnlyCollection<string> HistoryActions { get; init; } = Array.Empty<string>();
}
