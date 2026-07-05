namespace RKWorkspace.Shell;

public enum GlassEdgeState
{
    Hidden,
    Appearing,
    Visible,
    Near,
    Opening,
    Absorbing,
    InBetween,
    Emerging,
    Placed,
    Cancelled
}

public enum GlassEdgeAbsorptionVariant
{
    WholeEdge = 1,
    FocusPoint = 2,
    DirectionalSlot = 3
}

public sealed record GlassEdge(
    string EdgeId,
    AblageIdentity CurrentAblageId,
    AblageIdentity TargetAblageId,
    AblageDirection Direction,
    AblageDirection CounterDirection,
    AblageDistanceKind Distance,
    double Confidence,
    GlassEdgeState State,
    double AppearanceProgress,
    double ActivationProgress,
    double AbsorptionProgress,
    double TargetEmergenceProgress,
    double VisualIntensity,
    bool IsVisible,
    bool IsActive)
{
    public static GlassEdge FromNearest(NearestAblageResult nearest, GlassEdgeState state, double activation, double absorption)
    {
        if (!nearest.HasTarget || nearest.TargetAblageId is null)
        {
            return Hidden(nearest.CurrentAblageId);
        }

        var visual = GlassEdgeVisualProfile.FromDistance(nearest.Distance);
        var appearance = state == GlassEdgeState.Hidden ? 0.0 : Math.Clamp(0.42 + activation * 0.58, 0.0, 1.0);
        return new GlassEdge(
            $"edge-{nearest.CurrentAblageId.Value}-{nearest.TargetAblageId.Value}",
            nearest.CurrentAblageId,
            nearest.TargetAblageId.Value,
            nearest.EdgeHint,
            AblageDirectionMapper.Opposite(nearest.EdgeHint),
            nearest.Distance,
            nearest.Confidence,
            state,
            appearance,
            Math.Clamp(activation, 0.0, 1.0),
            Math.Clamp(absorption, 0.0, 1.0),
            Math.Clamp(absorption * 0.86, 0.0, 1.0),
            visual.Intensity,
            state != GlassEdgeState.Hidden,
            state is GlassEdgeState.Near or GlassEdgeState.Opening or GlassEdgeState.Absorbing or GlassEdgeState.InBetween);
    }

    public static GlassEdge Hidden(AblageIdentity currentAblageId)
    {
        return new GlassEdge(
            "edge-hidden",
            currentAblageId,
            new AblageIdentity(string.Empty),
            AblageDirection.Unknown,
            AblageDirection.Unknown,
            AblageDistanceKind.Unknown,
            0,
            GlassEdgeState.Hidden,
            0,
            0,
            0,
            0,
            0,
            false,
            false);
    }
}

public sealed record GlassEdgeVisualProfile(
    double Opacity,
    double Thickness,
    double Glow,
    double Intensity,
    double NameRevealThreshold,
    double ActivationThreshold)
{
    public static GlassEdgeVisualProfile FromDistance(AblageDistanceKind distance)
    {
        return distance switch
        {
            AblageDistanceKind.VeryNear => new GlassEdgeVisualProfile(0.92, 54, 0.86, 1.00, 0.54, 0.64),
            AblageDistanceKind.Near => new GlassEdgeVisualProfile(0.78, 44, 0.68, 0.84, 0.62, 0.70),
            AblageDistanceKind.Medium => new GlassEdgeVisualProfile(0.56, 32, 0.42, 0.62, 0.72, 0.78),
            AblageDistanceKind.Far => new GlassEdgeVisualProfile(0.36, 24, 0.24, 0.44, 0.82, 0.86),
            AblageDistanceKind.VeryFar => new GlassEdgeVisualProfile(0.24, 18, 0.14, 0.30, 0.90, 0.92),
            _ => new GlassEdgeVisualProfile(0.18, 16, 0.10, 0.22, 0.92, 0.94)
        };
    }
}

public sealed record WorkspaceSurfaceHandoff(
    string HandoffId,
    AblageIdentity SourceAblageId,
    AblageIdentity TargetAblageId,
    AblageDirection SourceEdge,
    AblageDirection TargetCounterEdge,
    string ObjectId,
    double Progress,
    WorkspaceSurfacePlacement Placement);

public sealed record WorkspaceSurfacePlacement(
    AblageIdentity AblageId,
    double X,
    double Y,
    bool IsSimulated);

public sealed record IncomingSurfaceObject(
    string ObjectId,
    AblageIdentity SourceAblageId,
    AblageDirection CounterEdge,
    double EmergenceProgress,
    WorkspaceSurfacePlacement SuggestedPlacement);

public sealed record OutgoingSurfaceObject(
    string ObjectId,
    AblageIdentity TargetAblageId,
    AblageDirection Edge,
    double AbsorptionProgress);
