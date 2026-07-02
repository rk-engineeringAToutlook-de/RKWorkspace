namespace RKWorkspace.Core.Models;

public sealed record Workspace
{
    public required string WorkspaceId { get; init; }

    public required string DisplayName { get; init; }

    public required string DeviceId { get; init; }

    public required WorkspacePlatform Platform { get; init; }

    public required WorkspaceCapability Capabilities { get; init; }

    public required WorkspacePosition Position { get; init; }

    public required TrustState TrustState { get; init; }

    public required DateTimeOffset LastSeen { get; init; }

    public bool Supports(TransferObjectType objectType)
    {
        return objectType switch
        {
            TransferObjectType.Text => Capabilities.HasFlag(WorkspaceCapability.TextTransfer),
            TransferObjectType.File => Capabilities.HasFlag(WorkspaceCapability.FileTransfer),
            TransferObjectType.Folder => Capabilities.HasFlag(WorkspaceCapability.FolderTransfer),
            TransferObjectType.Pdf => Capabilities.HasFlag(WorkspaceCapability.PdfTransfer),
            TransferObjectType.Image => Capabilities.HasFlag(WorkspaceCapability.ImageTransfer),
            TransferObjectType.Link => Capabilities.HasFlag(WorkspaceCapability.LinkTransfer),
            TransferObjectType.Clipboard => Capabilities.HasFlag(WorkspaceCapability.ClipboardRead)
                || Capabilities.HasFlag(WorkspaceCapability.ClipboardWrite),
            TransferObjectType.Context => Capabilities.HasFlag(WorkspaceCapability.ContextTransfer),
            TransferObjectType.Application => Capabilities.HasFlag(WorkspaceCapability.ApplicationTransfer),
            _ => false
        };
    }

    public bool CanParticipateInTransfers()
    {
        return TrustState is TrustState.Paired or TrustState.Trusted;
    }

    public static Workspace Create(
        string workspaceId,
        string displayName,
        string deviceId,
        WorkspacePlatform platform,
        WorkspaceCapability capabilities,
        WorkspacePosition position,
        TrustState trustState = TrustState.Unknown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        return new Workspace
        {
            WorkspaceId = workspaceId,
            DisplayName = displayName,
            DeviceId = deviceId,
            Platform = platform,
            Capabilities = capabilities,
            Position = position,
            TrustState = trustState,
            LastSeen = DateTimeOffset.UtcNow
        };
    }
}
