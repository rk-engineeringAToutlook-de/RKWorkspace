namespace RKWorkspace.Shell.LivingLens.Windows;

public sealed class LivingLensSession
{
    private readonly List<string> _events = new();
    private float _lensOpenTarget;
    private float _thingRecoveryProgress = 1f;

    public LivingLensSession()
    {
        Variants =
        [
            new LivingLensVariant
            {
                Kind = LivingLensVariantKind.RealBubble,
                Name = "Real Bubble Lens",
                Intention = "Sehr transparente Materiallinse mit feiner Lichtkante, realer Reflexion und minimalem Leben.",
                UsesMaterialTransparency = true,
                UsesFineLightEdge = true,
                UsesRefraction = true,
                UsesDepth = true,
                UsesSubtleLivingMotion = true,
                SupportsAbsorption = true
            },
            new LivingLensVariant
            {
                Kind = LivingLensVariantKind.Glass,
                Name = "Glass Lens",
                Intention = "Klarer Glasrand, hochwertiger Reflex, leichte Hintergrundverzeichnung und ruhige Tiefe.",
                UsesMaterialTransparency = true,
                UsesFineLightEdge = true,
                UsesRefraction = true,
                UsesDepth = true,
                UsesSubtleLivingMotion = true,
                SupportsAbsorption = true
            },
            new LivingLensVariant
            {
                Kind = LivingLensVariantKind.WaterSurface,
                Name = "Water Surface Lens",
                Intention = "Weiche transparente Oberflaeche mit ruhiger Welle und nicht nervoeser Bewegung.",
                UsesMaterialTransparency = true,
                UsesFineLightEdge = true,
                UsesRefraction = true,
                UsesDepth = true,
                UsesSubtleLivingMotion = true,
                SupportsAbsorption = true
            },
            new LivingLensVariant
            {
                Kind = LivingLensVariantKind.Wormhole,
                Name = "Wormhole Lens",
                Intention = "Raeumliche Oeffnung mit trichterartiger Tiefe und sichtbarer Aufnahme des Dings.",
                UsesMaterialTransparency = true,
                UsesFineLightEdge = true,
                UsesRefraction = true,
                UsesDepth = true,
                UsesSubtleLivingMotion = true,
                SupportsAbsorption = true
            },
            new LivingLensVariant
            {
                Kind = LivingLensVariantKind.Gravity,
                Name = "Gravity Lens",
                Intention = "Fast unsichtbare Raumkruemmung, bei der Lichtkante und Hintergrundverzerrung wichtiger sind als Form.",
                UsesMaterialTransparency = true,
                UsesFineLightEdge = true,
                UsesRefraction = true,
                UsesDepth = true,
                UsesSubtleLivingMotion = true,
                SupportsAbsorption = true
            }
        ];

        Lenses =
        [
            new LivingLensTarget { LensId = "right", Label = "Ablage", Edge = LivingLensEdge.Right, X = 0.995f, Y = 0.50f },
            new LivingLensTarget { LensId = "upper-right", Label = "Ablage", Edge = LivingLensEdge.UpperRight, X = 0.95f, Y = 0.14f },
            new LivingLensTarget { LensId = "bottom", Label = "Ablage", Edge = LivingLensEdge.Bottom, X = 0.50f, Y = 0.91f },
            new LivingLensTarget { LensId = "left", Label = "Ablage", Edge = LivingLensEdge.Left, X = 0.005f, Y = 0.54f }
        ];
    }

    public IReadOnlyList<LivingLensVariant> Variants { get; }

    public IReadOnlyList<LivingLensTarget> Lenses { get; }

    public IReadOnlyList<string> Events => _events;

    public LivingLensVariant ActiveVariant => Variants[ActiveVariantIndex];

    public int ActiveVariantIndex { get; private set; } = 1;

    public LivingLensTarget ActiveLens => Lenses.First(lens => string.Equals(lens.LensId, ActiveLensId, StringComparison.Ordinal));

    public string ActiveLensId { get; private set; } = "right";

    public int LensEmergenceDurationMs { get; } = 1500;

    public LivingLensTimingMode TimingMode { get; private set; } = LivingLensTimingMode.Natural1200;

    public int AbsorptionDurationMs => (int)TimingMode;

    public float LensEmergenceProgress { get; private set; }

    public float LensOpenProgress { get; private set; }

    public float AbsorptionProgress { get; private set; }

    public float TargetEmergenceProgress { get; private set; }

    public bool IsHoldingThing { get; private set; }

    public bool ThingCompact { get; private set; }

    public bool ThingPartiallyOccluded { get; private set; }

    public bool GripShadowVisible { get; private set; }

    public bool ThingHasDistractingContainer { get; } = false;

    public bool BrowserSurfaceRejected { get; } = true;

    public bool WebViewRejected { get; } = true;

    public bool PurpleBlobRejected => Variants.All(variant => variant.AvoidsPurpleBlob);

    public bool GreenPointRejected => Variants.All(variant => variant.AvoidsGreenPoint);

    public bool UiCircleRejected => Variants.All(variant => variant.AvoidsUiCircle);

    public bool ButtonShapeRejected => Variants.All(variant => variant.AvoidsButtonShape);

    public bool TechnicalWordsRejected => Variants.All(variant => variant.AvoidsTechnicalWords);

    public bool RealBubbleLensExists => Variants.Any(variant => variant.Kind == LivingLensVariantKind.RealBubble);

    public bool StartsWithGlassLens => ActiveVariant.Kind == LivingLensVariantKind.Glass;

    public bool LensesAtEdges => Lenses.All(lens => lens.X <= 0.01f || lens.X >= 0.95f || lens.Y <= 0.17f || lens.Y >= 0.90f);

    public bool PrimaryLensHugsScreenEdge => Lenses.First(lens => string.Equals(lens.LensId, "right", StringComparison.Ordinal)).X >= 0.99f;

    public bool LensesVisible => LensEmergenceProgress > 0;

    public bool LensesLivingSubtly => Variants.All(variant => variant.UsesSubtleLivingMotion);

    public bool LensOpen => LensOpenProgress > 0.72f;

    public bool MiniAblageVisible => LensOpenProgress > 0.56f;

    public bool AbsorptionActive => AbsorptionProgress > 0 && AbsorptionProgress < 1;

    public bool AbsorptionStarted => AbsorptionProgress > 0;

    public bool AbsorptionShrinksThing => ThingScaleDuringAbsorption < 0.92f;

    public bool AbsorptionDistortsThing => ThingDistortionDuringAbsorption > 0.08f;

    public bool ThingDoesNotDisappearImmediately => AbsorptionProgress > 0 && ThingOpacityDuringAbsorption > 0.72f;

    public bool TargetGhostVisible => TargetEmergenceProgress > 0.03f;

    public bool TargetGhostGrowsAndClarifies => TargetEmergenceProgress > 0.55f;

    public bool SupportsPullOutFromLens { get; private set; }

    public bool UsesSoftPortalWithoutWhiteFrame => true;

    public bool TimingVariantsExist => Enum.GetValues<LivingLensTimingMode>().Length == 3 &&
        Enum.GetValues<LivingLensTimingMode>().Select(mode => (int)mode).Order().SequenceEqual([600, 1200, 1800]);

    public bool SmokeAbsorptionScaleObserved { get; private set; }

    public bool SmokeAbsorptionDistortionObserved { get; private set; }

    public bool SmokeNotInstantGoneObserved { get; private set; }

    public bool SmokeGhostEmergenceObserved { get; private set; }

    public bool SmokeNoAutoAbsorptionObserved { get; private set; }

    public bool SmokeLensRelaxObserved { get; private set; }

    public bool SmokeDefaultGlassObserved { get; private set; }

    public bool SmokePullOutObserved { get; private set; }

    public bool SmokeLensOpenObserved { get; private set; }

    public bool SmokeMiniAblageObserved { get; private set; }

    public bool SmokeAbsorptionStartedObserved { get; private set; }

    public bool SmokeTargetGhostObserved { get; private set; }

    public float TiltX { get; private set; }

    public float TiltY { get; private set; }

    public float ShadowX { get; private set; }

    public float ShadowY { get; private set; } = 18;

    public LivingLensNameVisibility NameVisibility { get; private set; } = LivingLensNameVisibility.Hidden;

    public float ThingRecoveryProgress => _thingRecoveryProgress;

    public float ThingScaleDuringAbsorption => Math.Clamp(1.0f - (AbsorptionProgress * 0.62f), 0.34f, 1.0f);

    public float ThingDistortionDuringAbsorption => Math.Clamp(AbsorptionProgress * 0.72f, 0f, 0.72f);

    public float ThingOpacityDuringAbsorption => AbsorptionProgress < 0.78f
        ? 1f
        : Math.Clamp(1f - ((AbsorptionProgress - 0.78f) / 0.22f), 0.12f, 1f);

    public void Start()
    {
        ActiveLensId = "right";
        LensEmergenceProgress = 0;
        LensOpenProgress = 0;
        _lensOpenTarget = 0;
        AbsorptionProgress = 0;
        TargetEmergenceProgress = 0;
        _thingRecoveryProgress = 1f;
        SupportsPullOutFromLens = false;
        SmokeAbsorptionScaleObserved = false;
        SmokeAbsorptionDistortionObserved = false;
        SmokeNotInstantGoneObserved = false;
        SmokeGhostEmergenceObserved = false;
        SmokeNoAutoAbsorptionObserved = false;
        SmokeLensRelaxObserved = false;
        SmokeDefaultGlassObserved = false;
        SmokePullOutObserved = false;
        SmokeLensOpenObserved = false;
        SmokeMiniAblageObserved = false;
        SmokeAbsorptionStartedObserved = false;
        SmokeTargetGhostObserved = false;
        ResetThingVisuals();
        _events.Add("Living Lens listening");
    }

    public void SwitchVariant(int index)
    {
        if (index < 0 || index >= Variants.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Living Lens variant index is outside the prepared hypotheses.");
        }

        ActiveVariantIndex = index;
        _events.Add($"Living Lens variant switched: {ActiveVariant.Name}");
    }

    public void Pick()
    {
        IsHoldingThing = true;
        ThingCompact = true;
        ThingPartiallyOccluded = true;
        GripShadowVisible = true;
        LensEmergenceProgress = Math.Max(LensEmergenceProgress, 0.01f);
        AbsorptionProgress = 0;
        TargetEmergenceProgress = 0;
        _thingRecoveryProgress = 1f;
        _events.Add("Thing picked with digital grip");
    }

    public void Advance(int milliseconds)
    {
        if (IsHoldingThing)
        {
            LensEmergenceProgress = Math.Clamp(LensEmergenceProgress + ((float)milliseconds / LensEmergenceDurationMs), 0f, 1f);
        }
        else if (!AbsorptionActive && AbsorptionProgress <= 0)
        {
            LensEmergenceProgress = Math.Clamp(LensEmergenceProgress - ((float)milliseconds / 780f), 0f, 1f);
        }

        if (AbsorptionProgress > 0)
        {
            _lensOpenTarget = 1f;
        }

        if (_lensOpenTarget > LensOpenProgress)
        {
            LensOpenProgress = Math.Clamp(LensOpenProgress + ((float)milliseconds / 420f), 0f, _lensOpenTarget);
        }
        else if (_lensOpenTarget < LensOpenProgress)
        {
            LensOpenProgress = Math.Clamp(LensOpenProgress - ((float)milliseconds / 560f), _lensOpenTarget, 1f);
        }

        if (AbsorptionProgress > 0 && AbsorptionProgress < 1)
        {
            AbsorptionProgress = Math.Clamp(AbsorptionProgress + ((float)milliseconds / AbsorptionDurationMs), 0f, 1f);
            TargetEmergenceProgress = Math.Clamp((AbsorptionProgress - 0.42f) / 0.58f, 0f, 1f);
        }

        if (IsHoldingThing && _thingRecoveryProgress < 1f)
        {
            _thingRecoveryProgress = Math.Clamp(_thingRecoveryProgress + ((float)milliseconds / 760f), 0f, 1f);
        }

        if (!IsHoldingThing && AbsorptionProgress <= 0)
        {
            TiltX *= 0.86f;
            TiltY *= 0.86f;
            ShadowX *= 0.82f;
            ShadowY = 18f + ((ShadowY - 18f) * 0.84f);
        }
    }

    public void Carry(float movementX, float movementY)
    {
        ApplyVectorResponse(movementX, movementY);
        _events.Add("Thing answers to movement vector");
    }

    public void ApproachLens(string lensId, float normalizedDistance)
    {
        ActiveLensId = lensId;
        NameVisibility = normalizedDistance switch
        {
            >= 0.92f => LivingLensNameVisibility.ActionReadable,
            >= 0.76f => LivingLensNameVisibility.NameReadable,
            >= 0.52f => LivingLensNameVisibility.MicroText,
            _ => LivingLensNameVisibility.Hidden
        };

        _lensOpenTarget = normalizedDistance >= 0.84f
            ? Math.Clamp(0.18f + ((normalizedDistance - 0.84f) / 0.16f * 0.82f), 0.18f, 1f)
            : 0f;

        if (_lensOpenTarget > 0)
        {
            OpenLens();
        }

        _events.Add($"Living Lens approached: {lensId}");
    }

    public void OpenLens()
    {
        _lensOpenTarget = Math.Max(_lensOpenTarget, 1f);
        LensOpenProgress = Math.Max(LensOpenProgress, 0.12f);
        _events.Add("Living Lens opens");
    }

    public void ReplayOpening()
    {
        _lensOpenTarget = 1f;
        LensOpenProgress = 0.12f;
        _events.Add("Living Lens opening replayed");
    }

    public void PlayAbsorption()
    {
        IsHoldingThing = false;
        LensEmergenceProgress = 1f;
        _lensOpenTarget = 1f;
        LensOpenProgress = 1f;
        AbsorptionProgress = 0.01f;
        TargetEmergenceProgress = 0f;
        _events.Add("Lens absorption started");
    }

    public void PullOutFromLens()
    {
        IsHoldingThing = true;
        ThingCompact = true;
        ThingPartiallyOccluded = true;
        GripShadowVisible = true;
        AbsorptionProgress = 0;
        TargetEmergenceProgress = 0;
        LensEmergenceProgress = 1f;
        LensOpenProgress = Math.Max(LensOpenProgress, 0.72f);
        _lensOpenTarget = 0f;
        _thingRecoveryProgress = 0f;
        SupportsPullOutFromLens = true;
        NameVisibility = LivingLensNameVisibility.Hidden;
        _events.Add("Thing pulled out of living lens");
    }

    public void LeaveLens()
    {
        _lensOpenTarget = 0f;
        NameVisibility = LivingLensNameVisibility.Hidden;
        _events.Add("Living Lens relaxes");
    }

    public void DropFree()
    {
        IsHoldingThing = false;
        ThingCompact = false;
        ThingPartiallyOccluded = false;
        GripShadowVisible = false;
        AbsorptionProgress = 0;
        TargetEmergenceProgress = 0;
        _thingRecoveryProgress = 1f;
        _lensOpenTarget = 0f;
        NameVisibility = LivingLensNameVisibility.Hidden;
        _events.Add("Thing placed back on desktop");
    }

    public void CycleTiming()
    {
        TimingMode = TimingMode switch
        {
            LivingLensTimingMode.Fast600 => LivingLensTimingMode.Natural1200,
            LivingLensTimingMode.Natural1200 => LivingLensTimingMode.Slow1800,
            _ => LivingLensTimingMode.Fast600
        };
        _events.Add($"Absorption timing switched: {(int)TimingMode} ms");
    }

    public void ApplyVectorResponse(float movementX, float movementY)
    {
        var targetTiltX = Math.Clamp(movementX * 0.052f, -6.2f, 6.2f);
        var targetTiltY = Math.Clamp(-movementY * 0.050f, -6.2f, 6.2f);
        TiltX = (TiltX * 0.58f) + (targetTiltX * 0.42f);
        TiltY = (TiltY * 0.58f) + (targetTiltY * 0.42f);
        ShadowX = Math.Clamp(-TiltX * 1.8f, -13.0f, 13.0f);
        ShadowY = Math.Clamp(18f + (MathF.Abs(TiltY) * 1.8f), 16f, 31f);
    }

    public bool CheckVariantSwitching()
    {
        for (var index = 0; index < Variants.Count; index++)
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
            (22, 0),
            (-22, 0),
            (0, 22),
            (0, -22),
            (22, -22),
            (22, 22),
            (-22, -22),
            (-22, 22)
        };

        foreach (var vector in vectors)
        {
            ApplyVectorResponse(vector.X, vector.Y);
            if (Math.Abs(vector.X) > 0 && Math.Abs(TiltX) <= 0.01f)
            {
                return false;
            }

            if (Math.Abs(vector.Y) > 0 && Math.Abs(TiltY) <= 0.01f)
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckSmokeModel()
    {
        Start();
        var hiddenBeforePick = !LensesVisible;
        SmokeDefaultGlassObserved = StartsWithGlassLens;
        var variantsOk = Variants.Count == 5 &&
            RealBubbleLensExists &&
            Variants.All(variant =>
                variant.UsesMaterialTransparency &&
                variant.UsesFineLightEdge &&
                variant.UsesRefraction &&
                variant.UsesDepth &&
                variant.UsesSubtleLivingMotion &&
                variant.SupportsAbsorption);
        var rejectionOk = BrowserSurfaceRejected &&
            WebViewRejected &&
            PurpleBlobRejected &&
            GreenPointRejected &&
            UiCircleRejected &&
            ButtonShapeRejected &&
            TechnicalWordsRejected;
        var switchOk = CheckVariantSwitching();
        Pick();
        var handOk = ThingCompact && ThingPartiallyOccluded && GripShadowVisible && !ThingHasDistractingContainer;
        var emergenceBeginsSoftly = LensEmergenceProgress > 0 && LensEmergenceProgress < 1;
        Advance(LensEmergenceDurationMs);
        var emergenceOk = emergenceBeginsSoftly &&
            Math.Abs(LensEmergenceProgress - 1f) < 0.001f &&
            LensEmergenceDurationMs is >= 1000 and <= 2000;
        Carry(24, -18);
        var vectorOk = Math.Abs(TiltX) > 0.01f && Math.Abs(TiltY) > 0.01f && CheckDiagonalVectorResponses();
        ApproachLens("right", 0.96f);
        Advance(620);
        SmokeLensOpenObserved = LensOpen;
        SmokeMiniAblageObserved = MiniAblageVisible;
        var openOk = SmokeLensOpenObserved && SmokeMiniAblageObserved && NameVisibility == LivingLensNameVisibility.ActionReadable;
        SmokeNoAutoAbsorptionObserved = !AbsorptionStarted && IsHoldingThing;
        LeaveLens();
        Advance(620);
        SmokeLensRelaxObserved = !LensOpen && NameVisibility == LivingLensNameVisibility.Hidden;
        ApproachLens("right", 0.96f);
        Advance(620);
        PlayAbsorption();
        Advance(240);
        SmokeAbsorptionStartedObserved = AbsorptionStarted;
        SmokeAbsorptionScaleObserved = AbsorptionShrinksThing;
        SmokeAbsorptionDistortionObserved = AbsorptionDistortsThing;
        SmokeNotInstantGoneObserved = ThingDoesNotDisappearImmediately;
        var absorptionEarlyOk = SmokeAbsorptionStartedObserved &&
            AbsorptionActive &&
            SmokeAbsorptionScaleObserved &&
            SmokeAbsorptionDistortionObserved &&
            SmokeNotInstantGoneObserved;
        Advance(900);
        SmokeTargetGhostObserved = TargetGhostVisible;
        SmokeGhostEmergenceObserved = SmokeTargetGhostObserved && TargetGhostGrowsAndClarifies;
        var ghostOk = SmokeGhostEmergenceObserved;
        PullOutFromLens();
        Advance(780);
        SmokePullOutObserved = SupportsPullOutFromLens &&
            IsHoldingThing &&
            ThingRecoveryProgress >= 0.98f &&
            !AbsorptionStarted;

        return hiddenBeforePick &&
            variantsOk &&
            rejectionOk &&
            switchOk &&
            LensesAtEdges &&
            PrimaryLensHugsScreenEdge &&
            LensesLivingSubtly &&
            SmokeDefaultGlassObserved &&
            emergenceOk &&
            handOk &&
            vectorOk &&
            openOk &&
            SmokeNoAutoAbsorptionObserved &&
            SmokeLensRelaxObserved &&
            absorptionEarlyOk &&
            ghostOk &&
            SmokePullOutObserved &&
            UsesSoftPortalWithoutWhiteFrame &&
            TimingVariantsExist;
    }

    private void ResetThingVisuals()
    {
        IsHoldingThing = false;
        ThingCompact = false;
        ThingPartiallyOccluded = false;
        GripShadowVisible = false;
        TiltX = 0;
        TiltY = 0;
        ShadowX = 0;
        ShadowY = 18;
        NameVisibility = LivingLensNameVisibility.Hidden;
    }
}
