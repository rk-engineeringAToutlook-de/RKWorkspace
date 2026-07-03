using RKWorkspace.Shell;

namespace RKWorkspace.Shell.NativeOverlay.Windows;

public sealed class NativeSpatialOverlaySession
{
    private readonly List<string> _events = new();

    public NativeSpatialOverlaySession()
    {
        Bubbles =
        [
            new NativeSpatialBubble { BubbleId = "monitor", Label = "Ablage Monitor", Edge = NativeSpatialBubbleEdge.Right, X = 0.94f, Y = 0.50f },
            new NativeSpatialBubble { BubbleId = "tablet", Label = "Ablage Tablet", Edge = NativeSpatialBubbleEdge.UpperRight, X = 0.84f, Y = 0.14f },
            new NativeSpatialBubble { BubbleId = "handy", Label = "Ablage Handy", Edge = NativeSpatialBubbleEdge.Bottom, X = 0.50f, Y = 0.92f },
            new NativeSpatialBubble { BubbleId = "beamer", Label = "Ablage Wand", Edge = NativeSpatialBubbleEdge.Top, X = 0.52f, Y = 0.08f }
        ];
    }

    public WorkspaceOverlayState State { get; private set; } = WorkspaceOverlayState.Inactive;

    public WorkspaceCarryState CarryState => WorkspaceOverlayStateMapper.ToCarryState(State);

    public string ActivationGesture { get; } = "Ctrl+Alt+Space";

    public string ThingContent { get; } = "Rechnung.pdf";

    public string ActiveBubbleId { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public IReadOnlyList<NativeSpatialBubble> Bubbles { get; }

    public IReadOnlyList<string> Events => _events;

    public bool DemoThingCreated { get; private set; }

    public bool BubblesVisible => State is WorkspaceOverlayState.Picked or WorkspaceOverlayState.Carried or WorkspaceOverlayState.NearAblage;

    public bool ThingCompact { get; private set; }

    public bool ThingPartiallyOccluded { get; private set; }

    public bool GripShadowVisible { get; private set; }

    public bool OpticalHapticsPrepared { get; private set; } = true;

    public bool PortalOpen { get; private set; }

    public bool MiniAblageVisible { get; private set; }

    public bool IsGlidingIntoPortal { get; private set; }

    public bool TargetGhostVisible { get; private set; }

    public bool BubbleLensStyle { get; private set; } = true;

    public bool GreenPointStyleRejected { get; private set; } = true;

    public float TiltX { get; private set; }

    public float TiltY { get; private set; }

    public float ShadowX { get; private set; }

    public float ShadowY { get; private set; } = 18;

    public float SourceVisualProgress { get; private set; }

    public float TargetVisualProgress { get; private set; }

    public float TargetPositionX { get; private set; } = 0.5f;

    public float TargetPositionY { get; private set; } = 0.5f;

    public void Start()
    {
        State = WorkspaceOverlayState.Listening;
        Message = string.Empty;
        ActiveBubbleId = string.Empty;
        DemoThingCreated = false;
        ResetCarryVisuals();
        _events.Add("Native overlay listening");
    }

    public void ActivatePick()
    {
        State = WorkspaceOverlayState.Picked;
        DemoThingCreated = true;
        ThingCompact = true;
        ThingPartiallyOccluded = true;
        GripShadowVisible = true;
        Message = string.Empty;
        ActiveBubbleId = string.Empty;
        _events.Add("Demo thing picked");
    }

    public void Carry(float movementX, float movementY)
    {
        if (State != WorkspaceOverlayState.NearAblage)
        {
            State = WorkspaceOverlayState.Carried;
        }

        ApplyVectorResponse(movementX, movementY);
        _events.Add("Thing carried by movement vector");
    }

    public void NearPortal(string bubbleId)
    {
        State = WorkspaceOverlayState.NearAblage;
        ActiveBubbleId = bubbleId;
        PortalOpen = true;
        MiniAblageVisible = true;
        Message = "Hier ablegen";
        _events.Add($"Portal opened: {bubbleId}");
    }

    public void GlideIntoPortal()
    {
        IsGlidingIntoPortal = true;
        TargetGhostVisible = true;
        SourceVisualProgress = 0.62f;
        TargetVisualProgress = 0.78f;
        _events.Add("Thing glides into portal");
    }

    public void SetTargetPosition(float x, float y)
    {
        TargetPositionX = Math.Clamp(x, 0.08f, 0.92f);
        TargetPositionY = Math.Clamp(y, 0.08f, 0.92f);
        _events.Add($"Target position set: {TargetPositionX:0.00}/{TargetPositionY:0.00}");
    }

    public void Place()
    {
        State = WorkspaceOverlayState.Placed;
        ThingCompact = false;
        ThingPartiallyOccluded = false;
        GripShadowVisible = false;
        PortalOpen = false;
        Message = "Liegt hier";
        _events.Add("Thing placed");
    }

    public void Cancel()
    {
        State = WorkspaceOverlayState.Cancelled;
        ActiveBubbleId = string.Empty;
        Message = string.Empty;
        ResetCarryVisuals();
        _events.Add("Native overlay cancelled");
    }

    public void ApplyVectorResponse(float movementX, float movementY)
    {
        TiltX = Math.Clamp(movementX * 0.045f, -4.2f, 4.2f);
        TiltY = Math.Clamp(-movementY * 0.038f, -4.2f, 4.2f);
        ShadowX = Math.Clamp(-TiltX * 1.35f, -8f, 8f);
        ShadowY = Math.Clamp(18f + (MathF.Abs(TiltY) * 1.4f), 16f, 26f);
    }

    public bool SmokeCheck()
    {
        Start();
        var emptyHasNoBubbles = !BubblesVisible && State == WorkspaceOverlayState.Listening;
        ActivatePick();
        var pickOk = CarryState == WorkspaceCarryState.Picked &&
            DemoThingCreated &&
            ThingCompact &&
            ThingPartiallyOccluded &&
            GripShadowVisible &&
            BubblesVisible;
        var diagonalOk = CheckDiagonalVectorResponses();
        var bubbleStyleOk = BubbleLensStyle && GreenPointStyleRejected && Bubbles.Count >= 4;
        NearPortal("monitor");
        var portalOk = State == WorkspaceOverlayState.NearAblage &&
            CarryState == WorkspaceCarryState.NearSurface &&
            PortalOpen &&
            MiniAblageVisible &&
            string.Equals(ActiveBubbleId, "monitor", StringComparison.Ordinal);
        GlideIntoPortal();
        var glideOk = IsGlidingIntoPortal &&
            TargetGhostVisible &&
            SourceVisualProgress > 0 &&
            TargetVisualProgress > 0;
        SetTargetPosition(0.64f, 0.42f);
        Place();
        var placeOk = State == WorkspaceOverlayState.Placed &&
            CarryState == WorkspaceCarryState.Placed &&
            Math.Abs(TargetPositionX - 0.64f) < 0.001f &&
            Math.Abs(TargetPositionY - 0.42f) < 0.001f;

        return emptyHasNoBubbles &&
            pickOk &&
            diagonalOk &&
            bubbleStyleOk &&
            portalOk &&
            glideOk &&
            placeOk;
    }

    public bool CheckDiagonalVectorResponses()
    {
        var vectors = new (float X, float Y)[]
        {
            (20, 0),
            (-20, 0),
            (0, 20),
            (0, -20),
            (20, -20),
            (20, 20),
            (-20, -20),
            (-20, 20)
        };

        foreach (var vector in vectors)
        {
            ApplyVectorResponse(vector.X, vector.Y);
            var horizontalExpected = Math.Abs(vector.X) > 0;
            var verticalExpected = Math.Abs(vector.Y) > 0;
            if (horizontalExpected && Math.Abs(TiltX) <= 0.01f)
            {
                return false;
            }

            if (verticalExpected && Math.Abs(TiltY) <= 0.01f)
            {
                return false;
            }

            if (horizontalExpected && verticalExpected && (Math.Abs(TiltX) <= 0.01f || Math.Abs(TiltY) <= 0.01f))
            {
                return false;
            }
        }

        return true;
    }

    private void ResetCarryVisuals()
    {
        ThingCompact = false;
        ThingPartiallyOccluded = false;
        GripShadowVisible = false;
        PortalOpen = false;
        MiniAblageVisible = false;
        IsGlidingIntoPortal = false;
        TargetGhostVisible = false;
        SourceVisualProgress = 0;
        TargetVisualProgress = 0;
        TiltX = 0;
        TiltY = 0;
        ShadowX = 0;
        ShadowY = 18;
    }
}
