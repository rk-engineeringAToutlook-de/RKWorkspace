namespace RKWorkspace.Protocol.Identity;

public sealed class DevAblagePairingService
{
    public AblagePairingRequest RequestPairing(
        AblageIdentity requestingAblage,
        AblageIdentity targetAblage,
        string purpose = "Development pairing")
    {
        return new AblagePairingRequest(
            $"pairing-dev-{Guid.NewGuid():N}",
            requestingAblage.AblageId,
            targetAblage.AblageId,
            DateTimeOffset.UtcNow,
            AblagePairingState.PairingPending,
            purpose);
    }

    public AblagePairingDecision Decide(
        AblagePairingRequest request,
        bool approved,
        string reason = "")
    {
        return new AblagePairingDecision(
            request.PairingRequestId,
            request.RequestingAblageId,
            request.TargetAblageId,
            approved,
            approved ? AblageTrustLevel.DevTrusted : AblageTrustLevel.Untrusted,
            DateTimeOffset.UtcNow,
            string.IsNullOrWhiteSpace(reason)
                ? approved ? "Development pairing approved." : "Development pairing denied."
                : reason);
    }

    public AblageIdentity ApplyDecision(
        AblageIdentity identity,
        AblagePairingDecision decision)
    {
        if (identity.AblageId != decision.RequestingAblageId &&
            identity.AblageId != decision.TargetAblageId)
        {
            throw new AblageTrustException("Pairing decision does not belong to this ablage identity.");
        }

        return decision.Approved
            ? identity with
            {
                TrustLevel = decision.GrantedTrustLevel,
                PairingState = AblagePairingState.Paired,
                LastSeen = decision.DecidedAt
            }
            : identity with
            {
                TrustLevel = AblageTrustLevel.Untrusted,
                PairingState = AblagePairingState.Denied,
                LastSeen = decision.DecidedAt
            };
    }
}
