using System.Security.Cryptography;
using System.Text;

namespace RKWorkspace.Core.Models;

public sealed record TransferObject
{
    public required string ObjectId { get; init; }

    public required TransferObjectType ObjectType { get; init; }

    public required string SourceWorkspaceId { get; init; }

    public required string TargetWorkspaceId { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required string DisplayName { get; init; }

    public required string MimeType { get; init; }

    public required long Size { get; init; }

    public required PayloadReference PayloadReference { get; init; }

    public required string Checksum { get; init; }

    public required EncryptionInfo EncryptionInfo { get; init; }

    public required TransferStatus TransferStatus { get; init; }

    public static TransferObject CreateText(
        string sourceWorkspaceId,
        string targetWorkspaceId,
        string text,
        string displayName = "Text")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceWorkspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetWorkspaceId);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var bytes = Encoding.UTF8.GetBytes(text);

        return new TransferObject
        {
            ObjectId = NewObjectId(),
            ObjectType = TransferObjectType.Text,
            SourceWorkspaceId = sourceWorkspaceId,
            TargetWorkspaceId = targetWorkspaceId,
            CreatedAt = DateTimeOffset.UtcNow,
            DisplayName = displayName,
            MimeType = "text/plain; charset=utf-8",
            Size = bytes.LongLength,
            PayloadReference = PayloadReference.InlineText(text),
            Checksum = Sha256Hex(bytes),
            EncryptionInfo = EncryptionInfo.NotEncrypted,
            TransferStatus = TransferStatus.TransferPending
        };
    }

    public static TransferObject CreateFile(
        string sourceWorkspaceId,
        string targetWorkspaceId,
        string localPath,
        long size,
        string checksum,
        string? displayName = null,
        string mimeType = "application/octet-stream",
        TransferObjectType objectType = TransferObjectType.File)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceWorkspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetWorkspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(localPath);
        ArgumentOutOfRangeException.ThrowIfNegative(size);
        ArgumentException.ThrowIfNullOrWhiteSpace(checksum);

        return new TransferObject
        {
            ObjectId = NewObjectId(),
            ObjectType = objectType,
            SourceWorkspaceId = sourceWorkspaceId,
            TargetWorkspaceId = targetWorkspaceId,
            CreatedAt = DateTimeOffset.UtcNow,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Path.GetFileName(localPath) : displayName,
            MimeType = string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType,
            Size = size,
            PayloadReference = PayloadReference.LocalPath(localPath),
            Checksum = checksum,
            EncryptionInfo = EncryptionInfo.NotEncrypted,
            TransferStatus = TransferStatus.TransferPending
        };
    }

    private static string NewObjectId()
    {
        return $"rkws-obj-{Guid.NewGuid():N}";
    }

    private static string Sha256Hex(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
