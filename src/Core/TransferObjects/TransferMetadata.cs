namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Describes platform-neutral transfer object metadata.
/// </summary>
public sealed record TransferMetadata
{
    /// <summary>
    /// Gets the transfer object id.
    /// </summary>
    public required TransferObjectId ObjectId { get; init; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the MIME type.
    /// </summary>
    public required string MimeType { get; init; }

    /// <summary>
    /// Gets the object size in bytes.
    /// </summary>
    public required long Size { get; init; }

    /// <summary>
    /// Gets the checksum.
    /// </summary>
    public required string Checksum { get; init; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the last modification timestamp.
    /// </summary>
    public required DateTimeOffset ModifiedAt { get; init; }

    /// <summary>
    /// Gets the source workspace id value.
    /// </summary>
    public string SourceWorkspace { get; init; } = string.Empty;

    /// <summary>
    /// Gets the target workspace id value.
    /// </summary>
    public string TargetWorkspace { get; init; } = string.Empty;

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>
    /// Gets the transfer priority. Higher values win.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Gets object tags.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the metadata version.
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Creates a metadata snapshot.
    /// </summary>
    /// <returns>A metadata snapshot.</returns>
    public TransferMetadata Snapshot()
    {
        Validate(this);

        return this with
        {
            Tags = Tags.ToArray()
        };
    }

    /// <summary>
    /// Validates metadata.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    public static void Validate(TransferMetadata metadata)
    {
        if (metadata is null)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer metadata is required.");
        }

        if (metadata.ObjectId is null)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.InvalidId,
                "Transfer object id is required.");
        }

        if (string.IsNullOrWhiteSpace(metadata.DisplayName))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer object display name is required.",
                metadata.ObjectId.ToString());
        }

        if (string.IsNullOrWhiteSpace(metadata.MimeType))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer object MIME type is required.",
                metadata.ObjectId.ToString());
        }

        if (metadata.Size < 0)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer object size must not be negative.",
                metadata.ObjectId.ToString());
        }

        if (string.IsNullOrWhiteSpace(metadata.Checksum))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer object checksum is required.",
                metadata.ObjectId.ToString());
        }

        if (metadata.CreatedAt == default || metadata.ModifiedAt == default)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer object timestamps are required.",
                metadata.ObjectId.ToString());
        }

        if (metadata.Tags is null)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer object tags are required.",
                metadata.ObjectId.ToString());
        }

        if (string.IsNullOrWhiteSpace(metadata.Version))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.MissingMetadata,
                "Transfer object metadata version is required.",
                metadata.ObjectId.ToString());
        }
    }
}
