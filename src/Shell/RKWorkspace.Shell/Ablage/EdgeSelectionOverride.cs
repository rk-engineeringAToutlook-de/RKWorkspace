namespace RKWorkspace.Shell;

public enum EdgeOverrideKind
{
    None,
    PreferredAblage,
    TemporaryOverride
}

public sealed record EdgeOverride(
    EdgeOverrideKind Kind,
    AblageIdentity? TargetAblageId,
    DateTimeOffset CreatedAt,
    TimeSpan? Duration,
    string Reason)
{
    public static EdgeOverride None { get; } = new(
        EdgeOverrideKind.None,
        null,
        DateTimeOffset.MinValue,
        null,
        "Automatic nearest ablage selection.");

    public static EdgeOverride PreferredAblage(AblageIdentity targetAblageId, DateTimeOffset now, string reason)
    {
        return new EdgeOverride(EdgeOverrideKind.PreferredAblage, targetAblageId, now, null, reason);
    }

    public static EdgeOverride Temporary(AblageIdentity targetAblageId, DateTimeOffset now, TimeSpan duration, string reason)
    {
        return new EdgeOverride(EdgeOverrideKind.TemporaryOverride, targetAblageId, now, duration, reason);
    }

    public bool IsActive(DateTimeOffset now)
    {
        return Kind switch
        {
            EdgeOverrideKind.None => false,
            EdgeOverrideKind.PreferredAblage => TargetAblageId is not null,
            EdgeOverrideKind.TemporaryOverride => TargetAblageId is not null &&
                                                  Duration is not null &&
                                                  now < CreatedAt + Duration,
            _ => false
        };
    }
}

public sealed record EdgeSelectionDecision(
    NearestAblageResult Automatic,
    NearestAblageResult Selected,
    EdgeOverride AppliedOverride,
    bool OverrideApplied)
{
    public bool IsAutomatic => !OverrideApplied;
}

public static class EdgeSelectionOverrideResolver
{
    public static EdgeSelectionDecision Resolve(
        NearestAblageResult automatic,
        AblageProximitySnapshot snapshot,
        EdgeOverride edgeOverride,
        DateTimeOffset now)
    {
        if (!edgeOverride.IsActive(now) || edgeOverride.TargetAblageId is null)
        {
            return new EdgeSelectionDecision(automatic, automatic, EdgeOverride.None, OverrideApplied: false);
        }

        var target = snapshot.AvailableTargets()
            .FirstOrDefault(surface => surface.Id == edgeOverride.TargetAblageId);
        if (target is null)
        {
            return new EdgeSelectionDecision(automatic, automatic, EdgeOverride.None, OverrideApplied: false);
        }

        var selected = new NearestAblageResult(
            snapshot.CurrentAblageId,
            target.Id,
            target.DisplayName,
            target.Platform,
            target.Pose.Direction,
            AblageDirectionMapper.ToPrimaryEdge(target.Pose),
            target.Distance.Kind,
            target.Distance.DistanceMeters,
            target.Distance.Confidence,
            target.Distance.Source,
            IsStable: true,
            HasTarget: true,
            $"Owner override selected {edgeOverride.Kind}.")
        {
            SelectedAt = now,
            StableSince = now
        };

        return new EdgeSelectionDecision(automatic, selected, edgeOverride, OverrideApplied: true);
    }
}
