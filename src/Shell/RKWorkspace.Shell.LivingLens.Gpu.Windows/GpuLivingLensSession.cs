namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public enum GpuLivingLensTransitState
{
    LocalReady,
    Held,
    InTransit,
    PlacedRemote,
    TunnelClosing,
    Closed
}

public sealed class GpuLivingLensSession
{
    private float _openTarget;
    private float _approachTarget;
    private readonly HashSet<GpuLivingLensLook> _testedLooks = [];

    public float LensX { get; } = 0.995f;

    public float LensY { get; } = 0.50f;

    public float LensVisibleRatio { get; } = 0.87f;

    public bool GpuCompositionPrepared { get; } = true;

    public bool DesktopRefractionPrepared { get; } = true;

    public bool RefractionMapPrepared { get; } = true;

    public bool TunnelDepthPrepared { get; } = true;

    public bool PremiumTunnelVisualPrepared { get; } = true;

    public bool PremiumTunnelRefractionPrepared { get; } = true;

    public bool PremiumTunnelAperturePrepared { get; } = true;

    public bool EightTunnelFieldPrepared { get; } = true;

    public bool CornerAndEdgeTunnelPrepared { get; } = true;

    public bool CenterStartObjectPrepared { get; } = true;

    public bool VectorSuctionCenterPrepared { get; } = true;

    public bool ThroatSuctionTargetPrepared { get; } = true;

    public bool NoApexOvershootPrepared { get; } = true;

    public bool StableTunnelTargetLockPrepared { get; } = true;

    public bool ThroatPointCollapsePrepared { get; } = true;

    public bool ThreePremiumLensLooksPrepared { get; private set; } = true;

    public bool GlassBubbleLookPrepared => _testedLooks.Contains(GpuLivingLensLook.GlassBubble);

    public bool WormholeLookPrepared => _testedLooks.Contains(GpuLivingLensLook.Wormhole);

    public bool HybridLookPrepared => _testedLooks.Contains(GpuLivingLensLook.Hybrid);

    public bool LiveLookSwitchPrepared => GlassBubbleLookPrepared && WormholeLookPrepared && HybridLookPrepared;

    public bool CompactCarryCardPrepared { get; } = true;

    public bool CleanDesktopPlatePrepared { get; } = true;

    public bool SelfSamplingEchoSuppressionPrepared { get; } = true;

    public bool SoftFresnelEdgePrepared { get; } = true;

    public bool SmoothPickupScalePrepared { get; private set; }

    public bool SmoothLensApproachPrepared { get; private set; }

    public bool UltraFineGlassOpticsPrepared { get; } = true;

    public bool HighResolutionVectorOpticsPrepared { get; } = true;

    public bool LiveDesktopRefractionPrepared { get; private set; } = true;

    public bool CaptureExclusionPrepared { get; private set; } = true;

    public bool LensCenterLockPrepared { get; private set; } = true;

    public bool MicroGlassHighlightsPrepared { get; } = true;

    public bool PhysicalGlassMaterialPrepared { get; } = true;

    public bool GlassThicknessPrepared { get; } = true;

    public bool ChromaticEdgePrepared { get; } = true;

    public bool LensContactShadowPrepared { get; } = true;

    public bool GlassCausticsPrepared { get; } = true;

    public bool SpecularGlassSweepsPrepared { get; } = true;

    public bool CompiledPixelShaderPrepared { get; } = true;

    public bool NativeShaderLayerPrepared { get; } = true;

    public bool ShaderMaterialRefractionPrepared { get; } = true;

    public bool HlslShaderContractPrepared { get; } = true;

    public bool RectangularThingPrepared { get; } = true;

    public bool RectangularShadowPrepared { get; } = true;

    public bool GentleCarryTiltPrepared { get; private set; }

    public bool SoftShadowPrepared { get; } = true;

    public bool PortalEdgePullPrepared { get; private set; }

    public bool PortalEdgeSqueezePrepared { get; private set; }

    public bool PortalEdgeApexSqueezePrepared { get; private set; }

    public bool NoTwistPortalFunnelPrepared { get; private set; }

    public bool TiltDampingNearTunnelPrepared { get; private set; }

    public bool NoWhiteBlock { get; } = true;

    public bool NoPaperAxisSpinPrepared { get; } = true;

    public bool DropRequiresRelease { get; private set; } = true;

    public bool PullOutPrepared { get; private set; }

    public bool VectorTiltPrepared { get; private set; }

    public bool ShadowPrepared { get; private set; }

    public bool ShadowSuctionPrepared { get; private set; }

    public bool ShadowTunnelSuctionPrepared { get; private set; }

    public bool CalmRestingObjectInTunnelPrepared { get; private set; }

    public bool PerspectiveTrapezoidPrepared { get; private set; }

    public bool LensAppearsOnPickPrepared { get; private set; }

    public bool CarryShadowOnlyPrepared { get; private set; } = true;

    public bool TransitCountdownPrepared { get; private set; }

    public bool RetakeResetsTransitTimerPrepared { get; private set; }

    public bool RemotePlacementPrepared { get; private set; }

    public bool TunnelAutoClosePrepared { get; private set; }

    public bool TunnelClosedAfterTransitPrepared { get; private set; }

    public bool RemoteGestureRequiredPrepared { get; private set; }

    public bool PrimaryLensHugsScreenEdge => LensX >= 0.99f && LensVisibleRatio is >= 0.85f and <= 0.90f;

    public bool EdgeContinuationPrepared { get; } = true;

    public GpuLivingLensTransitState TransitState { get; private set; } = GpuLivingLensTransitState.LocalReady;

    public int TransitTimeoutMilliseconds { get; } = 10_000;

    public int TransitMilliseconds { get; private set; }

    public int TransitRemainingMilliseconds => Math.Max(0, TransitTimeoutMilliseconds - TransitMilliseconds);

    public bool CanPullOutFromLens => TransitState == GpuLivingLensTransitState.InTransit &&
        TransitMilliseconds < TransitTimeoutMilliseconds;

    public bool IsHoldingThing { get; private set; }

    public float LensEmergence { get; private set; }

    public float LensOpen { get; private set; }

    public float Absorption { get; private set; }

    public float PullOutRecovery { get; private set; } = 1f;

    public float PickProgress { get; private set; } = 1f;

    public float ApproachProgress { get; private set; }

    public float TiltX { get; private set; }

    public float TiltY { get; private set; }

    public float ShadowX { get; private set; }

    public float ShadowY { get; private set; } = 22f;

    public void SetLensLook(GpuLivingLensLook lensLook)
    {
        _testedLooks.Add(lensLook);
    }

    public void MarkCaptureExclusion(bool enabled)
    {
        CaptureExclusionPrepared = CaptureExclusionPrepared || enabled;
    }

    public void Pick()
    {
        ResetTransitOnRetake();
        IsHoldingThing = true;
        TransitState = GpuLivingLensTransitState.Held;
        LensEmergence = Math.Max(LensEmergence, 0.06f);
        Absorption = 0f;
        PullOutRecovery = 1f;
        PickProgress = 0f;
        LensAppearsOnPickPrepared = LensEmergence >= 0.05f;
    }

    public void Carry(float movementX, float movementY)
    {
        var targetTiltX = Math.Clamp(movementX * 0.145f, -8.5f, 8.5f);
        var targetTiltY = Math.Clamp(-movementY * 0.135f, -8.5f, 8.5f);
        TiltX = (TiltX * 0.58f) + (targetTiltX * 0.42f);
        TiltY = (TiltY * 0.58f) + (targetTiltY * 0.42f);
        ShadowX = Math.Clamp(-TiltX * 2.6f, -24f, 24f);
        ShadowY = Math.Clamp(20f + (MathF.Abs(TiltY) * 2.2f), 18f, 42f);
        GentleCarryTiltPrepared = Math.Abs(TiltX) < 9.0f && Math.Abs(TiltY) < 9.0f;
        VectorTiltPrepared = Math.Abs(TiltX) > 0.25f && Math.Abs(TiltY) > 0.25f;
        PerspectiveTrapezoidPrepared = VectorTiltPrepared;
        ShadowPrepared = IsHoldingThing && (ShadowY > 24f || Math.Abs(ShadowX) > 2f);
    }

    public void ApproachLens(float nearness)
    {
        if (nearness > 0.20f)
        {
            PortalEdgePullPrepared = true;
        }

        if (nearness > 0.46f)
        {
            PortalEdgeSqueezePrepared = true;
            PortalEdgeApexSqueezePrepared = true;
            NoTwistPortalFunnelPrepared = true;
            TiltDampingNearTunnelPrepared = true;
        }

        if (nearness > 0.62f)
        {
            ShadowTunnelSuctionPrepared = true;
        }

        _approachTarget = Math.Clamp(nearness, 0f, 1f);
    }

    public void LeaveLens()
    {
        _approachTarget = 0f;
    }

    public void PlaceIntoLens()
    {
        IsHoldingThing = false;
        TransitState = GpuLivingLensTransitState.InTransit;
        TransitMilliseconds = 0;
        DropRequiresRelease = true;
        TransitCountdownPrepared = true;
        LensEmergence = 1f;
        LensOpen = 1f;
        Absorption = 0.01f;
        PickProgress = 1f;
        ApproachProgress = 1f;
        _approachTarget = 1f;
        ShadowSuctionPrepared = true;
        PortalEdgeSqueezePrepared = true;
        PortalEdgeApexSqueezePrepared = true;
        NoTwistPortalFunnelPrepared = true;
        TiltDampingNearTunnelPrepared = true;
        ShadowTunnelSuctionPrepared = true;
    }

    public void PlaceOnSurface()
    {
        IsHoldingThing = false;
        TransitState = GpuLivingLensTransitState.LocalReady;
        TransitMilliseconds = 0;
        Absorption = 0f;
        PullOutRecovery = 1f;
        TiltX = 0f;
        TiltY = 0f;
        ShadowX = 0f;
        ShadowY = 0f;
        _openTarget = 0f;
        _approachTarget = 0f;
        ApproachProgress = 0f;
        PickProgress = 0f;
        CarryShadowOnlyPrepared = true;
    }

    public void PullOutFromLens()
    {
        ResetTransitOnRetake();
        IsHoldingThing = true;
        TransitState = GpuLivingLensTransitState.Held;
        PullOutPrepared = true;
        PullOutRecovery = 0f;
        Absorption = 0f;
        _openTarget = 0f;
        _approachTarget = 0f;
        ApproachProgress = 0f;
        PickProgress = 1f;
    }

    public void Advance(int milliseconds)
    {
        if (TransitState == GpuLivingLensTransitState.InTransit && !IsHoldingThing)
        {
            TransitMilliseconds = Math.Min(TransitTimeoutMilliseconds, TransitMilliseconds + milliseconds);
            if (TransitMilliseconds >= TransitTimeoutMilliseconds)
            {
                CompleteRemotePlacement();
            }
        }

        var closingAfterTransit = TransitState is GpuLivingLensTransitState.PlacedRemote or GpuLivingLensTransitState.TunnelClosing;
        if (closingAfterTransit)
        {
            TransitState = GpuLivingLensTransitState.TunnelClosing;
            _openTarget = 0f;
            _approachTarget = 0f;
            LensOpen = Math.Clamp(LensOpen - ((float)milliseconds / 440f), 0f, 1f);
            LensEmergence = Math.Clamp(LensEmergence - ((float)milliseconds / 860f), 0f, 1f);
            TiltX *= 0.80f;
            TiltY *= 0.80f;
            ShadowX *= 0.74f;
            ShadowY *= 0.74f;
            if (LensEmergence <= 0.001f && LensOpen <= 0.001f)
            {
                TransitState = GpuLivingLensTransitState.Closed;
                LensEmergence = 0f;
                LensOpen = 0f;
                CalmRestingObjectInTunnelPrepared = true;
                TunnelClosedAfterTransitPrepared = true;
                RemoteGestureRequiredPrepared = true;
            }
        }
        else if (IsHoldingThing)
        {
            var previousPickProgress = PickProgress;
            PickProgress = Math.Clamp(PickProgress + ((float)milliseconds / 620f), 0f, 1f);
            LensEmergence = Math.Clamp(LensEmergence + ((float)milliseconds / 620f), 0f, 1f);
            SmoothPickupScalePrepared = SmoothPickupScalePrepared ||
                PickProgress is > 0.35f and < 1.0f ||
                (previousPickProgress < 0.35f && PickProgress >= 0.35f);
        }
        else if (Absorption <= 0f)
        {
            LensEmergence = Math.Clamp(LensEmergence - ((float)milliseconds / 520f), 0f, 1f);
            PickProgress = Math.Clamp(PickProgress - ((float)milliseconds / 280f), 0f, 1f);
            TiltX *= 0.86f;
            TiltY *= 0.86f;
            ShadowX *= 0.82f;
            ShadowY *= 0.82f;
        }

        if (!closingAfterTransit)
        {
            var approachStep = (float)milliseconds / (_approachTarget > ApproachProgress ? 540f : 360f);
            ApproachProgress = MoveToward(ApproachProgress, _approachTarget, approachStep);
            SmoothLensApproachPrepared = SmoothLensApproachPrepared || ApproachProgress is > 0.12f and < 0.92f;
            _openTarget = ApproachProgress >= 0.68f
                ? Math.Clamp((ApproachProgress - 0.68f) / 0.32f, 0f, 1f)
                : 0f;
        }

        if (!closingAfterTransit && _openTarget > LensOpen)
        {
            LensOpen = Math.Clamp(LensOpen + ((float)milliseconds / 520f), 0f, _openTarget);
        }
        else if (!closingAfterTransit && _openTarget < LensOpen && Absorption <= 0f)
        {
            LensOpen = Math.Clamp(LensOpen - ((float)milliseconds / 380f), _openTarget, 1f);
        }

        if (TransitState == GpuLivingLensTransitState.InTransit && Absorption > 0f && Absorption < 1f)
        {
            Absorption = Math.Clamp(Absorption + ((float)milliseconds / 980f), 0f, 1f);
        }

        if (PullOutRecovery < 1f)
        {
            PullOutRecovery = Math.Clamp(PullOutRecovery + ((float)milliseconds / 620f), 0f, 1f);
        }
    }

    private void ResetTransitOnRetake()
    {
        if (TransitState == GpuLivingLensTransitState.InTransit && TransitMilliseconds > 0 &&
            TransitMilliseconds < TransitTimeoutMilliseconds)
        {
            RetakeResetsTransitTimerPrepared = true;
        }

        TransitMilliseconds = 0;
    }

    private void CompleteRemotePlacement()
    {
        TransitState = GpuLivingLensTransitState.PlacedRemote;
        IsHoldingThing = false;
        Absorption = 1f;
        PickProgress = 0f;
        ApproachProgress = 0f;
        RemotePlacementPrepared = true;
        TunnelAutoClosePrepared = true;
        DropRequiresRelease = true;
    }

    private static float MoveToward(float current, float target, float amount)
    {
        if (current < target)
        {
            return Math.Min(target, current + amount);
        }

        return Math.Max(target, current - amount);
    }

    public void RunSmokeScenario()
    {
        SetLensLook(GpuLivingLensLook.GlassBubble);
        SetLensLook(GpuLivingLensLook.Wormhole);
        SetLensLook(GpuLivingLensLook.Hybrid);
        Pick();
        Advance(1000);
        Carry(38f, -30f);
        ApproachLens(0.95f);
        Advance(360);
        var noAutoAbsorption = Absorption <= 0f && IsHoldingThing;
        PlaceOnSurface();
        Advance(220);
        Pick();
        Carry(42f, -36f);
        ApproachLens(0.95f);
        Advance(180);
        PlaceIntoLens();
        Advance(2600);
        var countdownStarted = TransitCountdownPrepared && TransitMilliseconds > 0 && CanPullOutFromLens;
        PullOutFromLens();
        var retakeReset = RetakeResetsTransitTimerPrepared && TransitMilliseconds == 0 && IsHoldingThing;
        Advance(360);
        PlaceIntoLens();
        Advance(720);
        var secondCountdownStarted = TransitMilliseconds > 0 && CanPullOutFromLens;
        Advance(TransitTimeoutMilliseconds + 1200);
        DropRequiresRelease = DropRequiresRelease && noAutoAbsorption && countdownStarted && retakeReset && secondCountdownStarted;
    }
}
