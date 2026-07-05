namespace RKWorkspace.Shell.SpatialTray;

using RKWorkspace.Shell;

public static class MobileSpatialSurfaceModel
{
    public const double MobileLensNameRevealThreshold = 0.62;
    public const double MobileLensActivationThreshold = 0.78;

    public static object ActivateGesture()
    {
        return new
        {
            mobileSpatialMode = "Active",
            gesture = "long-touch",
            lensesVisible = true,
            forbiddenWordsVisible = false,
            haptics = new
            {
                requested = true,
                mode = "navigator.vibrate-or-optical",
                skippedSafely = true,
                opticalFallback = true
            },
            parameters = new
            {
                mobileLensScaleByDistance = "VeryFar:0.42,Far:0.52,Medium:0.72,Near:1.05,VeryNear:1.28",
                mobileLensOpacityByDistance = "VeryFar:0.18,Far:0.26,Medium:0.46,Near:0.78,VeryNear:1.00",
                mobileLensNameRevealThreshold = MobileLensNameRevealThreshold,
                mobileLensActivationThreshold = MobileLensActivationThreshold
            },
            lenses = new[]
            {
                Lens("monitor", "Ablage", "VeryNear", 0.92, 0.48, 1.28, 1.00, true, true, "PortalOpen"),
                Lens("desktop", "Ablage", "Near", 0.50, 0.91, 1.05, 0.78, true, false, "Near"),
                Lens("tablet", "Ablage", "Medium", 0.13, 0.16, 0.72, 0.46, false, false, "Visible"),
                Lens("iphone", "Ablage", "Near", 0.16, 0.62, 0.98, 0.72, true, false, "Visible"),
                Lens("beamer", "Ablage", "Far", 0.84, 0.14, 0.52, 0.26, false, false, "Distant")
            }
        };
    }

    public static object ActivateGlassEdgeGesture()
    {
        var current = SimulatedAblageProximityProvider.WindowsAblageId;
        var selector = new NearestAblageSelector();
        var nearest = selector.Select(new SimulatedAblageProximityProvider().GetSnapshot(current));
        var edge = GlassEdge.FromNearest(nearest, GlassEdgeState.Near, 0.72, 0.0);
        var profile = GlassEdgeVisualProfile.FromDistance(nearest.Distance);

        return new
        {
            mobileGlassEdgeMode = "Active",
            gesture = "long-touch",
            glassEdgeVisible = edge.IsVisible,
            visibleGlassEdges = edge.IsVisible ? 1 : 0,
            forbiddenWordsVisible = false,
            nearestAblage = new
            {
                ablageId = nearest.TargetAblageId?.Value,
                displayName = nearest.TargetDisplayName,
                platform = nearest.Platform.ToString(),
                direction = nearest.EdgeHint.ToString(),
                distance = nearest.Distance.ToString(),
                confidence = nearest.Confidence,
                source = nearest.Source.ToString()
            },
            glassEdge = new
            {
                direction = edge.Direction.ToString(),
                counterDirection = edge.CounterDirection.ToString(),
                state = edge.State.ToString(),
                edge.AppearanceProgress,
                edge.ActivationProgress,
                edge.AbsorptionProgress,
                edge.TargetEmergenceProgress,
                profile.Opacity,
                profile.Thickness,
                profile.Glow,
                profile.NameRevealThreshold,
                profile.ActivationThreshold,
                absorptionVariants = new[] { "WholeEdge", "FocusPoint", "DirectionalSlot" },
                textWhenNear = "Hier ablegen"
            },
            incomingGhost = new
            {
                visible = true,
                counterEdge = edge.CounterDirection.ToString(),
                emergenceProgress = 0.44,
                placement = new { x = 0.62, y = 0.42, simulated = true }
            },
            haptics = new
            {
                requested = true,
                mode = "navigator.vibrate-or-optical",
                skippedSafely = true,
                opticalFallback = true,
                moments = Enum.GetNames<SurfaceHapticMoment>()
            }
        };
    }

    private static object Lens(
        string ablageId,
        string displayName,
        string distance,
        double x,
        double y,
        double scale,
        double opacity,
        bool nameVisible,
        bool opens,
        string state)
    {
        return new
        {
            ablageId,
            displayName,
            shortName = displayName,
            distance,
            x,
            y,
            scale,
            opacity,
            nameVisible,
            nameReadable = nameVisible,
            opens,
            isPortal = opens,
            state,
            openingText = opens ? "oeffnet sich" : string.Empty,
            actionText = opens ? "Hier ablegen" : string.Empty
        };
    }
}
