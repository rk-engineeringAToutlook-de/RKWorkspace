using RKWorkspace.Core.Models;

namespace RKWorkspace.Core.Services;

public sealed class TransferPlanner
{
    public TransferObject PlanTextTransfer(
        Workspace source,
        WorkspaceMap workspaceMap,
        WorkspacePosition direction,
        string text,
        string displayName = "Text")
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(workspaceMap);

        var target = workspaceMap.ResolveDirectionalTarget(direction);
        EnsureTransferAllowed(source, target, TransferObjectType.Text);

        return TransferObject.CreateText(
            source.WorkspaceId,
            target.WorkspaceId,
            text,
            displayName);
    }

    public TransferObject PlanFileTransfer(
        Workspace source,
        WorkspaceMap workspaceMap,
        WorkspacePosition direction,
        string localPath,
        long size,
        string checksum,
        string mimeType = "application/octet-stream",
        TransferObjectType objectType = TransferObjectType.File)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(workspaceMap);

        var target = workspaceMap.ResolveDirectionalTarget(direction);
        EnsureTransferAllowed(source, target, objectType);

        return TransferObject.CreateFile(
            source.WorkspaceId,
            target.WorkspaceId,
            localPath,
            size,
            checksum,
            mimeType: mimeType,
            objectType: objectType);
    }

    private static void EnsureTransferAllowed(Workspace source, Workspace target, TransferObjectType objectType)
    {
        if (!source.CanParticipateInTransfers())
        {
            throw new InvalidOperationException($"Source workspace {source.WorkspaceId} is not paired or trusted.");
        }

        if (!target.CanParticipateInTransfers())
        {
            throw new InvalidOperationException($"Target workspace {target.WorkspaceId} is not paired or trusted.");
        }

        if (!source.Supports(objectType))
        {
            throw new InvalidOperationException($"Source workspace {source.WorkspaceId} does not support {objectType} transfers.");
        }

        if (!target.Supports(objectType))
        {
            throw new InvalidOperationException($"Target workspace {target.WorkspaceId} does not support {objectType} transfers.");
        }
    }
}
