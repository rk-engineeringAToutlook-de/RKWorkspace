namespace RKWorkspace.Protocol.Ownership;

public enum CarryLeaseState
{
    Requested,
    Granted,
    Active,
    HeartbeatMissing,
    GracePeriod,
    ConnectionLost,
    Returning,
    Returned,
    Revoked,
    Expired,
    RecoveredByOwner,
    Failed
}

public sealed class CarryLeaseException : Exception
{
    public CarryLeaseException(string message)
        : base(message)
    {
    }
}

public sealed record CarryLeasePolicy(
    string PolicyId,
    int PolicyVersion,
    OwnershipMode Mode,
    RkwpAllowedAction AllowedActions,
    TimeSpan Duration,
    TimeSpan HeartbeatInterval,
    TimeSpan GracePeriod,
    bool Revocable,
    bool AuditRequired,
    string? PolicyHash = null)
{
    public static CarryLeasePolicy FrameOnlyDefault { get; } = new(
        "policy-lease-frameonly",
        1,
        OwnershipMode.FrameOnly,
        RkwpAllowedAction.View | RkwpAllowedAction.Scroll | RkwpAllowedAction.Zoom | RkwpAllowedAction.Return | RkwpAllowedAction.Revoke,
        TimeSpan.FromMinutes(15),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        Revocable: true,
        AuditRequired: true);
}

public sealed record CarryLeaseRecovery(
    bool OwnerRecoveredThing,
    bool GuestFrameInvalidated,
    bool AuditEntryRequired,
    CarryLeaseRecoveryReason Reason,
    CarryLeaseState FinalState);

public enum CarryLeaseRecoveryReason
{
    LeaseExpired,
    RecoveredByOwner,
    Revoked,
    ConnectionLost,
    Returned,
    Unknown
}

public sealed record CarryLease
{
    public required string LeaseId { get; init; }

    public required string SessionId { get; init; }

    public required string ThingId { get; init; }

    public required string OwnerAblageId { get; init; }

    public required string GuestAblageId { get; init; }

    public required OwnershipMode Mode { get; init; }

    public required RkwpAllowedAction AllowedActions { get; init; }

    public required CarryLeaseState State { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required TimeSpan HeartbeatInterval { get; init; }

    public required DateTimeOffset LastHeartbeat { get; init; }

    public required TimeSpan GracePeriod { get; init; }

    public required bool Revocable { get; init; }

    public required bool AuditRequired { get; init; }

    public required string PolicyId { get; init; }

    public required int PolicyVersion { get; init; }

    public string? PolicyHash { get; init; }

    public static CarryLease Grant(string thingId, string ownerAblageId, string guestAblageId, CarryLeasePolicy policy, DateTimeOffset now, string? sessionId = null)
    {
        return new CarryLease
        {
            LeaseId = $"lease-{Guid.NewGuid():N}",
            SessionId = sessionId ?? $"rkwp-session-lease-{Guid.NewGuid():N}",
            ThingId = thingId,
            OwnerAblageId = ownerAblageId,
            GuestAblageId = guestAblageId,
            Mode = policy.Mode,
            AllowedActions = policy.AllowedActions,
            State = CarryLeaseState.Active,
            StartedAt = now,
            ExpiresAt = now.Add(policy.Duration),
            HeartbeatInterval = policy.HeartbeatInterval,
            LastHeartbeat = now,
            GracePeriod = policy.GracePeriod,
            Revocable = policy.Revocable,
            AuditRequired = policy.AuditRequired,
            PolicyId = policy.PolicyId,
            PolicyVersion = policy.PolicyVersion,
            PolicyHash = policy.PolicyHash
        };
    }

    public CarryLease Heartbeat(DateTimeOffset now)
    {
        if (State is CarryLeaseState.Revoked or CarryLeaseState.Expired or CarryLeaseState.Returned or CarryLeaseState.RecoveredByOwner)
        {
            throw new CarryLeaseException("Cannot heartbeat a closed carry lease.");
        }

        return this with { LastHeartbeat = now, State = CarryLeaseState.Active };
    }

    public CarryLease Return(DateTimeOffset now)
    {
        return this with { State = CarryLeaseState.Returned, LastHeartbeat = now };
    }

    public CarryLease Revoke(DateTimeOffset now)
    {
        if (!Revocable)
        {
            throw new CarryLeaseException("Carry lease is not revocable.");
        }

        return this with { State = CarryLeaseState.Revoked, LastHeartbeat = now };
    }

    public CarryLease MarkConnectionLost(DateTimeOffset now)
    {
        if (State is CarryLeaseState.Returned or CarryLeaseState.Revoked or CarryLeaseState.Expired or CarryLeaseState.RecoveredByOwner)
        {
            return this;
        }

        return this with { State = CarryLeaseState.ConnectionLost, LastHeartbeat = now };
    }

    public CarryLease Advance(DateTimeOffset now)
    {
        if (State is CarryLeaseState.Returned or CarryLeaseState.Revoked or CarryLeaseState.Expired or CarryLeaseState.RecoveredByOwner)
        {
            return this;
        }

        if (now >= ExpiresAt)
        {
            return this with { State = CarryLeaseState.Expired };
        }

        var missingSince = LastHeartbeat.Add(HeartbeatInterval + HeartbeatInterval);
        if (now < missingSince)
        {
            return this;
        }

        var graceEnds = missingSince.Add(GracePeriod);
        if (now >= graceEnds)
        {
            return this with { State = CarryLeaseState.RecoveredByOwner };
        }

        return State == CarryLeaseState.Active
            ? this with { State = CarryLeaseState.HeartbeatMissing }
            : this with { State = CarryLeaseState.GracePeriod };
    }

    public CarryLeaseRecovery Recover()
    {
        var reason = State switch
        {
            CarryLeaseState.Expired => CarryLeaseRecoveryReason.LeaseExpired,
            CarryLeaseState.Revoked => CarryLeaseRecoveryReason.Revoked,
            CarryLeaseState.ConnectionLost => CarryLeaseRecoveryReason.ConnectionLost,
            CarryLeaseState.Returned => CarryLeaseRecoveryReason.Returned,
            CarryLeaseState.RecoveredByOwner => CarryLeaseRecoveryReason.RecoveredByOwner,
            CarryLeaseState.HeartbeatMissing or CarryLeaseState.GracePeriod => CarryLeaseRecoveryReason.RecoveredByOwner,
            _ => CarryLeaseRecoveryReason.Unknown
        };

        var final = reason switch
        {
            CarryLeaseRecoveryReason.LeaseExpired => CarryLeaseState.Expired,
            CarryLeaseRecoveryReason.Revoked => CarryLeaseState.Revoked,
            CarryLeaseRecoveryReason.Returned => CarryLeaseState.Returned,
            _ => CarryLeaseState.RecoveredByOwner
        };

        return new CarryLeaseRecovery(
            OwnerRecoveredThing: true,
            GuestFrameInvalidated: true,
            AuditEntryRequired: AuditRequired,
            reason,
            final);
    }
}
