using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public enum OwnerFrameUxState
{
    OriginalOwned,
    LeasedToGuest,
    LockedOnOwner,
    Returned,
    RecoveredByOwner
}

public enum GuestFrameUxState
{
    FrameOpening,
    FrameReady,
    FrameActive,
    Returning,
    Revoked,
    Expired
}

public sealed record VisibleFrameState(string Scope, string State, string Text);

public sealed record VisibleFrameStateLanguageResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static VisibleFrameStateLanguageResult Success { get; } = new(true, []);
}

public static class OwnerGuestFrameStateUx
{
    private static readonly string[] ForbiddenVisibleTerms =
    [
        "uebertragen",
        "übertragen",
        "empfangen",
        "heruntergeladen",
        "gesendet",
        "download",
        "upload",
        "sync",
        "server",
        "client",
        "endpoint",
        "device",
        "geraet",
        "gerät",
        "agent",
        "workspace",
        "ipc",
        "request",
        "response"
    ];

    public static string GetOwnerText(OwnerFrameUxState state)
    {
        return state switch
        {
            OwnerFrameUxState.OriginalOwned => "verfuegbar",
            OwnerFrameUxState.LeasedToGuest => "ausgeliehen",
            OwnerFrameUxState.LockedOnOwner => "wartet auf Rueckgabe",
            OwnerFrameUxState.Returned => "wieder verfuegbar",
            OwnerFrameUxState.RecoveredByOwner => "wiederhergestellt",
            _ => "nicht verfuegbar"
        };
    }

    public static string GetGuestText(GuestFrameUxState state)
    {
        return state switch
        {
            GuestFrameUxState.FrameOpening => "liegt gleich hier",
            GuestFrameUxState.FrameReady => "liegt hier im Frame",
            GuestFrameUxState.FrameActive => "liegt hier im Frame",
            GuestFrameUxState.Returning => "zurueckgeben",
            GuestFrameUxState.Revoked => "nicht verfuegbar",
            GuestFrameUxState.Expired => "Verbindung verloren",
            _ => "nicht verfuegbar"
        };
    }

    public static OwnerFrameUxState GetOwnerState(ThingOwnership ownership, CarryLease lease)
    {
        if (lease.State == CarryLeaseState.Returned || ownership.State == OwnershipState.ReturnedToOwner)
        {
            return OwnerFrameUxState.Returned;
        }

        if (lease.State == CarryLeaseState.RecoveredByOwner || ownership.State == OwnershipState.RecoveredByOwner)
        {
            return OwnerFrameUxState.RecoveredByOwner;
        }

        if (ownership.State == OwnershipState.LockedOnOwner)
        {
            return OwnerFrameUxState.LockedOnOwner;
        }

        if (lease.State == CarryLeaseState.Active)
        {
            return OwnerFrameUxState.LeasedToGuest;
        }

        return OwnerFrameUxState.OriginalOwned;
    }

    public static GuestFrameUxState GetGuestState(FrameSession frameSession)
    {
        return frameSession.State switch
        {
            FrameSessionState.Opening => GuestFrameUxState.FrameOpening,
            FrameSessionState.Ready => GuestFrameUxState.FrameReady,
            FrameSessionState.Active => GuestFrameUxState.FrameActive,
            FrameSessionState.Returning or FrameSessionState.Closed => GuestFrameUxState.Returning,
            FrameSessionState.Revoked => GuestFrameUxState.Revoked,
            FrameSessionState.Expired => GuestFrameUxState.Expired,
            _ => GuestFrameUxState.Expired
        };
    }

    public static IReadOnlyList<VisibleFrameState> CreateTimeline(
        ThingOwnership activeOwnership,
        CarryLease activeLease,
        FrameSession activeFrame,
        CarryLease returnedLease,
        CarryLeaseRecovery recovery)
    {
        return
        [
            new VisibleFrameState("Owner", OwnerFrameUxState.OriginalOwned.ToString(), GetOwnerText(OwnerFrameUxState.OriginalOwned)),
            new VisibleFrameState("Owner", OwnerFrameUxState.LeasedToGuest.ToString(), GetOwnerText(OwnerFrameUxState.LeasedToGuest)),
            new VisibleFrameState("Owner", GetOwnerState(activeOwnership, activeLease).ToString(), GetOwnerText(GetOwnerState(activeOwnership, activeLease))),
            new VisibleFrameState("Owner", GetOwnerState(activeOwnership.ReturnToOwner(), returnedLease).ToString(), GetOwnerText(GetOwnerState(activeOwnership.ReturnToOwner(), returnedLease))),
            new VisibleFrameState("Owner", recovery.FinalState.ToString(), GetOwnerText(OwnerFrameUxState.RecoveredByOwner)),
            new VisibleFrameState("Guest", GuestFrameUxState.FrameOpening.ToString(), GetGuestText(GuestFrameUxState.FrameOpening)),
            new VisibleFrameState("Guest", GuestFrameUxState.FrameReady.ToString(), GetGuestText(GuestFrameUxState.FrameReady)),
            new VisibleFrameState("Guest", GetGuestState(activeFrame).ToString(), GetGuestText(GetGuestState(activeFrame))),
            new VisibleFrameState("Guest", GuestFrameUxState.Returning.ToString(), GetGuestText(GuestFrameUxState.Returning)),
            new VisibleFrameState("Guest", GuestFrameUxState.Revoked.ToString(), GetGuestText(GuestFrameUxState.Revoked)),
            new VisibleFrameState("Guest", GuestFrameUxState.Expired.ToString(), GetGuestText(GuestFrameUxState.Expired))
        ];
    }

    public static VisibleFrameStateLanguageResult ValidateVisibleText(IEnumerable<string> visibleTexts)
    {
        var errors = new List<string>();

        foreach (var text in visibleTexts)
        {
            foreach (var term in ForbiddenVisibleTerms)
            {
                if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Visible text contains forbidden term '{term}': {text}");
                }
            }
        }

        return errors.Count == 0
            ? VisibleFrameStateLanguageResult.Success
            : new VisibleFrameStateLanguageResult(false, errors);
    }
}
