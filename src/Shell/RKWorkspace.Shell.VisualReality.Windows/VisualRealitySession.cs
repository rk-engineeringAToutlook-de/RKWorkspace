namespace RKWorkspace.Shell.VisualReality.Windows;

public sealed class VisualRealitySession
{
    private readonly List<string> _events = new();

    public VisualRealitySession()
    {
        LensVariants =
        [
            new VisualRealityLensHypothesis
            {
                Kind = VisualRealityLensKind.ReferenceLens,
                Name = "Linse 1 - Glasbrunnen-Portal",
                Intention = "Ruhige Glaslinse, die sich bei Naehe zu einem tiefen, kontrollierten Portal vertieft.",
                UsesTransparency = true,
                UsesLightRefraction = true,
                UsesDepth = true,
                UsesLivingMotion = true,
                UsesGlassMaterial = true,
                UsesGravityWell = true,
                UsesCalmPortal = true,
                AvoidsGreenPointUi = true,
                AvoidsButtonShape = true
            },
            new VisualRealityLensHypothesis
            {
                Kind = VisualRealityLensKind.Glass,
                Name = "Linse 2 - Glasmaterial",
                Intention = "Echte optische Materialitaet, Brechung und ruhige physische Praesenz.",
                UsesTransparency = true,
                UsesLightRefraction = true,
                UsesDepth = true,
                UsesLivingMotion = true,
                UsesGlassMaterial = true,
                AvoidsGreenPointUi = true,
                AvoidsButtonShape = true
            },
            new VisualRealityLensHypothesis
            {
                Kind = VisualRealityLensKind.GravityWell,
                Name = "Linse 3 - Raumbrunnen",
                Intention = "Die Ablage wird weich, tiefer und zieht das Ding kontrolliert hinein.",
                UsesTransparency = true,
                UsesLightRefraction = true,
                UsesDepth = true,
                UsesLivingMotion = true,
                UsesGlassMaterial = true,
                UsesGravityWell = true,
                AvoidsGreenPointUi = true,
                AvoidsButtonShape = true
            },
            new VisualRealityLensHypothesis
            {
                Kind = VisualRealityLensKind.QuietPortal,
                Name = "Linse 4 - Ruhiges Portal",
                Intention = "Eine stille Oeffnung mit Tiefe, Randlicht und Mini-Ablage ohne Effektlaerm.",
                UsesTransparency = true,
                UsesLightRefraction = true,
                UsesDepth = true,
                UsesLivingMotion = true,
                UsesGlassMaterial = true,
                UsesGravityWell = true,
                UsesCalmPortal = true,
                AvoidsGreenPointUi = true,
                AvoidsButtonShape = true
            },
            new VisualRealityLensHypothesis
            {
                Kind = VisualRealityLensKind.MinimalRift,
                Name = "Linse 5 - Minimaler Raumriss",
                Intention = "Reduzierte Gegenprobe: fast unsichtbare Oeffnung nur ueber Lichtkante und Tiefe.",
                UsesTransparency = true,
                UsesLightRefraction = false,
                UsesDepth = true,
                UsesLivingMotion = true,
                UsesCalmPortal = true,
                AvoidsGreenPointUi = true,
                AvoidsButtonShape = true
            }
        ];

        Lenses =
        [
            new VisualRealityLens { LensId = "monitor", Label = "Ablage Monitor", Edge = VisualRealityLensEdge.Right, X = 0.93f, Y = 0.50f },
            new VisualRealityLens { LensId = "tablet", Label = "Ablage Tablet", Edge = VisualRealityLensEdge.UpperRight, X = 0.82f, Y = 0.17f },
            new VisualRealityLens { LensId = "handy", Label = "Ablage Handy", Edge = VisualRealityLensEdge.Bottom, X = 0.50f, Y = 0.91f },
            new VisualRealityLens { LensId = "beamer", Label = "Ablage Wand", Edge = VisualRealityLensEdge.Top, X = 0.55f, Y = 0.08f }
        ];
    }

    public VisualRealityState State { get; private set; } = VisualRealityState.Listening;

    public IReadOnlyList<VisualRealityLensHypothesis> LensVariants { get; }

    public IReadOnlyList<VisualRealityLens> Lenses { get; }

    public IReadOnlyList<string> Events => _events;

    public VisualRealityLensHypothesis ActiveVariant => LensVariants[ActiveVariantIndex];

    public int ActiveVariantIndex { get; private set; }

    public string ThingContent { get; } = "Rechnung.pdf";

    public string ActiveLensId { get; private set; } = string.Empty;

    public int LensEmergenceDurationMs { get; } = 1400;

    public float LensEmergenceProgress { get; private set; }

    public bool DesktopRemainsVisible { get; } = true;

    public bool BrowserSurfaceRejected { get; } = true;

    public bool GreenPointStyleRejected => LensVariants.All(variant => variant.AvoidsGreenPointUi);

    public bool ButtonTargetShapeRejected => LensVariants.All(variant => variant.AvoidsButtonShape);

    public bool SpaceBackdropRejected => LensVariants.All(variant => variant.AvoidsSpaceBackdrop);

    public bool ReferenceBoardDirectionPrepared => LensVariants[0].Kind == VisualRealityLensKind.ReferenceLens &&
        LensVariants[0].UsesGlassMaterial &&
        LensVariants[0].UsesGravityWell &&
        LensVariants[0].UsesCalmPortal &&
        LensVariants[0].PreservesRealDesktop &&
        LensVariants[0].AvoidsSpaceBackdrop;

    public bool LensesAtRealAblageEdges { get; } = true;

    public bool LensesVisible => LensEmergenceProgress > 0 || State is VisualRealityState.Picked or VisualRealityState.Carrying or VisualRealityState.LensEmerging or VisualRealityState.NearLens or VisualRealityState.LensOpen or VisualRealityState.Gliding;

    public bool LensLivingMotion => ActiveVariant.UsesLivingMotion;

    public bool LensDepthVisible { get; private set; }

    public bool LensOpen { get; private set; }

    public bool MiniAblageVisible { get; private set; }

    public bool TargetGhostVisible { get; private set; }

    public bool ThingVisible { get; private set; } = true;

    public bool ThingCompact { get; private set; }

    public bool ThingPartiallyOccluded { get; private set; }

    public bool GripShadowVisible { get; private set; }

    public bool OpticalHapticsPrepared { get; } = true;

    public bool FuturePhysicalHapticsMarked { get; } = true;

    public bool IsGlidingIntoLens { get; private set; }

    public float TiltX { get; private set; }

    public float TiltY { get; private set; }

    public float ShadowX { get; private set; }

    public float ShadowY { get; private set; } = 18;

    public float SourceVisualProgress { get; private set; }

    public float TargetVisualProgress { get; private set; }

    public float TargetPositionX { get; private set; } = 0.5f;

    public float TargetPositionY { get; private set; } = 0.5f;

    public VisualRealityLensNameVisibility NameVisibility { get; private set; } = VisualRealityLensNameVisibility.Hidden;

    public void Start()
    {
        State = VisualRealityState.Listening;
        ActiveLensId = string.Empty;
        LensEmergenceProgress = 0;
        ResetCarryVisuals();
        _events.Add("Visual reality lab listening");
    }

    public void SwitchVariant(int index)
    {
        if (index < 0 || index >= LensVariants.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Lens variant index is outside the prepared hypotheses.");
        }

        ActiveVariantIndex = index;
        _events.Add($"Lens variant switched: {ActiveVariant.Name}");
    }

    public void Pick()
    {
        State = VisualRealityState.LensEmerging;
        ThingVisible = true;
        ThingCompact = true;
        ThingPartiallyOccluded = true;
        GripShadowVisible = true;
        LensEmergenceProgress = 0.01f;
        _events.Add("Thing picked with visual haptics");
    }

    public void AdvanceLensEmergence(int milliseconds)
    {
        if (State == VisualRealityState.Listening)
        {
            return;
        }

        LensEmergenceProgress = Math.Clamp(
            LensEmergenceProgress + ((float)milliseconds / LensEmergenceDurationMs),
            0.0f,
            1.0f);
        if (LensEmergenceProgress >= 1.0f && State == VisualRealityState.LensEmerging)
        {
            State = VisualRealityState.Carrying;
        }
    }

    public void Carry(float movementX, float movementY)
    {
        if (State is not VisualRealityState.NearLens and not VisualRealityState.LensOpen and not VisualRealityState.Gliding)
        {
            State = LensEmergenceProgress < 1 ? VisualRealityState.LensEmerging : VisualRealityState.Carrying;
        }

        ApplyVectorResponse(movementX, movementY);
        _events.Add("Thing answers to movement vector");
    }

    public void ApproachLens(string lensId, float normalizedDistance)
    {
        State = VisualRealityState.NearLens;
        ActiveLensId = lensId;
        LensDepthVisible = true;
        NameVisibility = normalizedDistance switch
        {
            >= 0.92f => VisualRealityLensNameVisibility.ActionReadable,
            >= 0.76f => VisualRealityLensNameVisibility.NameReadable,
            >= 0.52f => VisualRealityLensNameVisibility.MicroText,
            _ => VisualRealityLensNameVisibility.Hidden
        };
        _events.Add($"Lens approached: {lensId}");
    }

    public void OpenLens()
    {
        State = VisualRealityState.LensOpen;
        LensOpen = true;
        LensDepthVisible = true;
        MiniAblageVisible = true;
        NameVisibility = VisualRealityLensNameVisibility.ActionReadable;
        _events.Add("Lens opened to mini ablage");
    }

    public void GlideIntoLens()
    {
        State = VisualRealityState.Gliding;
        IsGlidingIntoLens = true;
        TargetGhostVisible = true;
        SourceVisualProgress = 0.58f;
        TargetVisualProgress = 0.72f;
        _events.Add("Thing glides into living lens");
    }

    public void SetTargetPosition(float x, float y)
    {
        TargetPositionX = Math.Clamp(x, 0.08f, 0.92f);
        TargetPositionY = Math.Clamp(y, 0.08f, 0.92f);
        _events.Add($"Target position set: {TargetPositionX:0.00}/{TargetPositionY:0.00}");
    }

    public void Place()
    {
        State = VisualRealityState.Placed;
        ThingCompact = false;
        ThingPartiallyOccluded = false;
        GripShadowVisible = false;
        LensOpen = false;
        IsGlidingIntoLens = false;
        _events.Add("Thing emerges and is placed");
    }

    public void Cancel()
    {
        State = VisualRealityState.Cancelled;
        ActiveLensId = string.Empty;
        ResetCarryVisuals();
        _events.Add("Visual reality lab cancelled");
    }

    public void ApplyVectorResponse(float movementX, float movementY)
    {
        TiltX = Math.Clamp(movementX * 0.046f, -4.4f, 4.4f);
        TiltY = Math.Clamp(-movementY * 0.04f, -4.4f, 4.4f);
        ShadowX = Math.Clamp(-TiltX * 1.4f, -8.5f, 8.5f);
        ShadowY = Math.Clamp(18f + (MathF.Abs(TiltY) * 1.45f), 16f, 27f);
    }

    public bool CheckVariantSwitching()
    {
        for (var index = 0; index < LensVariants.Count; index++)
        {
            SwitchVariant(index);
            if (ActiveVariantIndex != index)
            {
                return false;
            }
        }

        return true;
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
        }

        return true;
    }

    public bool SmokeCheck()
    {
        Start();
        var hiddenBeforePick = !LensesVisible && ThingVisible;
        var variantsOk = LensVariants.Count >= 5 &&
            LensVariants.All(variant => variant.UsesTransparency && variant.UsesDepth && variant.UsesLivingMotion) &&
            GreenPointStyleRejected &&
            ButtonTargetShapeRejected &&
            SpaceBackdropRejected &&
            ReferenceBoardDirectionPrepared;
        var switchOk = CheckVariantSwitching();
        Pick();
        var digitalHandOk = ThingCompact && ThingPartiallyOccluded && GripShadowVisible && OpticalHapticsPrepared;
        var emergenceBeginsSoftly = LensEmergenceProgress > 0 && LensEmergenceProgress < 1 &&
            LensEmergenceDurationMs is >= 1000 and <= 2000;
        AdvanceLensEmergence(LensEmergenceDurationMs);
        var emergenceOk = emergenceBeginsSoftly && Math.Abs(LensEmergenceProgress - 1.0f) < 0.001f && LensesVisible;
        Carry(22, -18);
        var vectorOk = Math.Abs(TiltX) > 0.01f && Math.Abs(TiltY) > 0.01f;
        var diagonalOk = CheckDiagonalVectorResponses();
        ApproachLens("monitor", 0.58f);
        var microTextOk = NameVisibility == VisualRealityLensNameVisibility.MicroText;
        ApproachLens("monitor", 0.80f);
        var readableOk = NameVisibility == VisualRealityLensNameVisibility.NameReadable;
        ApproachLens("monitor", 0.96f);
        var actionOk = NameVisibility == VisualRealityLensNameVisibility.ActionReadable;
        OpenLens();
        var lensOpenOk = State == VisualRealityState.LensOpen && LensOpen && MiniAblageVisible && LensDepthVisible;
        GlideIntoLens();
        var glideOk = IsGlidingIntoLens &&
            TargetGhostVisible &&
            SourceVisualProgress > 0 &&
            TargetVisualProgress > 0;
        SetTargetPosition(0.62f, 0.44f);
        Place();
        var placeOk = State == VisualRealityState.Placed &&
            Math.Abs(TargetPositionX - 0.62f) < 0.001f &&
            Math.Abs(TargetPositionY - 0.44f) < 0.001f;

        return DesktopRemainsVisible &&
            BrowserSurfaceRejected &&
            hiddenBeforePick &&
            variantsOk &&
            switchOk &&
            digitalHandOk &&
            emergenceOk &&
            LensLivingMotion &&
            vectorOk &&
            diagonalOk &&
            microTextOk &&
            readableOk &&
            actionOk &&
            lensOpenOk &&
            glideOk &&
            placeOk &&
            FuturePhysicalHapticsMarked;
    }

    private void ResetCarryVisuals()
    {
        LensDepthVisible = false;
        LensOpen = false;
        MiniAblageVisible = false;
        TargetGhostVisible = false;
        ThingCompact = false;
        ThingPartiallyOccluded = false;
        GripShadowVisible = false;
        IsGlidingIntoLens = false;
        SourceVisualProgress = 0;
        TargetVisualProgress = 0;
        TiltX = 0;
        TiltY = 0;
        ShadowX = 0;
        ShadowY = 18;
        NameVisibility = VisualRealityLensNameVisibility.Hidden;
    }
}
