using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Describes a platform-neutral logical transfer request.
/// </summary>
public sealed record TransferRequest
{
    /// <summary>
    /// Gets the request id.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// Gets the source workspace id.
    /// </summary>
    public required WorkspaceId SourceWorkspaceId { get; init; }

    /// <summary>
    /// Gets the requested logical direction.
    /// </summary>
    public required TransferDirection RequestedDirection { get; init; }

    /// <summary>
    /// Gets the transfer object id.
    /// </summary>
    public required TransferObjectId TransferObjectId { get; init; }

    /// <summary>
    /// Gets target capabilities that must be present.
    /// </summary>
    public CapabilitySet RequiredCapabilities { get; init; } = CapabilitySet.Empty;

    /// <summary>
    /// Gets target capabilities used for ranking.
    /// </summary>
    public CapabilitySet OptionalCapabilities { get; init; } = CapabilitySet.Empty;

    /// <summary>
    /// Gets target capabilities that must not be present.
    /// </summary>
    public CapabilitySet ForbiddenCapabilities { get; init; } = CapabilitySet.Empty;

    /// <summary>
    /// Gets the request creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the neutral requester value.
    /// </summary>
    public string RequestedBy { get; init; } = string.Empty;

    /// <summary>
    /// Gets platform-neutral request metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Creates an immutable request snapshot.
    /// </summary>
    /// <returns>The request snapshot.</returns>
    public TransferRequest Snapshot()
    {
        Validate();

        return this with
        {
            RequiredCapabilities = RequiredCapabilities.Snapshot(),
            OptionalCapabilities = OptionalCapabilities.Snapshot(),
            ForbiddenCapabilities = ForbiddenCapabilities.Snapshot(),
            Metadata = Metadata.ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// Validates the request.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RequestId))
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidRequest,
                "Transfer request id is required.");
        }

        if (SourceWorkspaceId is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidRequest,
                "Source workspace id is required.",
                RequestId);
        }

        if (RequestedDirection == TransferDirection.Unknown)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidDirection,
                "Transfer direction must not be Unknown.",
                RequestId);
        }

        if (TransferObjectId is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidRequest,
                "Transfer object id is required.",
                RequestId);
        }

        if (RequiredCapabilities is null ||
            OptionalCapabilities is null ||
            ForbiddenCapabilities is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidRequest,
                "Transfer request capability sets are required.",
                RequestId);
        }

        if (CreatedAt == default)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidRequest,
                "Transfer request creation timestamp is required.",
                RequestId);
        }

        if (Metadata is null)
        {
            throw new TransferEngineException(
                TransferFailureReason.InvalidRequest,
                "Transfer request metadata is required.",
                RequestId);
        }

        ToRequirement().Validate();
    }

    /// <summary>
    /// Converts request capabilities to a capability requirement.
    /// </summary>
    /// <returns>The capability requirement.</returns>
    public CapabilityRequirement ToRequirement()
    {
        return new CapabilityRequirement
        {
            RequiredCapabilities = RequiredCapabilities.Capabilities
                .Select(capability => capability.CapabilityId)
                .ToArray(),
            OptionalCapabilities = OptionalCapabilities.Capabilities
                .Select(capability => capability.CapabilityId)
                .ToArray(),
            ForbiddenCapabilities = ForbiddenCapabilities.Capabilities
                .Select(capability => capability.CapabilityId)
                .ToArray()
        };
    }
}
