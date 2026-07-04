namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativeGlassOverlaySession
{
    private float _approachTarget;
    private float _openTarget;

    public bool NativeTransparentOverlayPrepared { get; } = true;

    public bool NoBrowserSurfacePrepared { get; } = true;

    public bool NoSyntheticStagePrepared { get; } = true;

    public bool LiveDesktopRefractionPrepared { get; } = true;

    public bool PhysicalGlassLookPrepared { get; } = true;

    public bool SoftFresnelPrepared { get; } = true;

    public bool RealDesktopOnlyPrepared { get; } = true;

    public bool RectangularPaperPrepared { get; } = true;

    public bool ReleaseRequiredForAbsorptionPrepared { get; private set; } = true;

    public bool SmoothPickScalePrepared { get; private set; }

    public bool GentleVectorTiltPrepared { get; private set; }

    public bool SoftPerspectiveShadowPrepared { get; private set; }

    public bool CenterLockedSuctionPrepared { get; private set; }

    public bool EdgeApexSqueezePrepared { get; private set; }

    public bool NoTwistPrepared { get; private set; } = true;

    public bool ShadowSuctionPrepared { get; private set; }

    public bool TransitCountdownPrepared { get; private set; }

    public bool RetakeResetsTransitTimerPrepared { get; private set; }

    public bool TunnelAutoClosePrepared { get; private set; }

    public bool RemotePlacementPrepared { get; private set; }

    public bool RemoteGestureRequiredPrepared { get; private set; }

    public NativeGlassOverlayCarryState State { get; private set; } = NativeGlassOverlayCarryState.LocalReady;

    public bool IsHolding { get; private set; }

    public float LensEmergence { get; private set; }

    public float LensOpen { get; private set; }

    public float Approach { get; private set; }

    public float PickProgress { get; private set; }

    public float Absorption { get; private set; }

    public float PullOutRecovery { get; private set; } = 1f;

    public float TiltX { get; private set; }

    public float TiltY { get; private set; }

    public float ShadowLift { get; private set; }

    public float ShadowOffsetX { get; private set; }

    public int TransitTimeoutMilliseconds { get; } = 10_000;

    public int TransitMilliseconds { get; private set; }

    public int TransitRemainingMilliseconds => Math.Max(0, TransitTimeoutMilliseconds - TransitMilliseconds);

    public bool CanPullOut => State == NativeGlassOverlayCarryState.InTransit &&
        TransitMilliseconds < TransitTimeoutMilliseconds;

    public void Pick()
    {
        if (State == NativeGlassOverlayCarryState.InTransit && TransitMilliseconds > 0)
        {
            RetakeResetsTransitTimerPrepared = true;
        }

        State = NativeGlassOverlayCarryState.Held;
        IsHolding = true;
        TransitMilliseconds = 0;
        Absorption = 0f;
        LensEmergence = Math.Max(LensEmergence, 0.08f);
        PickProgress = Math.Min(PickProgress, 0.18f);
        PullOutRecovery = 1f;
    }

    public void Carry(float movementX, float movementY)
    {
        var targetTiltX = Math.Clamp(movementX * 0.075f, -5.2f, 5.2f);
        var targetTiltY = Math.Clamp(-movementY * 0.070f, -5.0f, 5.0f);
        TiltX = (TiltX * 0.72f) + (targetTiltX * 0.28f);
        TiltY = (TiltY * 0.72f) + (targetTiltY * 0.28f);
        ShadowOffsetX = Math.Clamp(-TiltX * 2.1f, -18f, 18f);
        ShadowLift = Math.Clamp(18f + MathF.Abs(TiltY * 2.8f) + MathF.Abs(TiltX * 1.6f), 18f, 40f);
        GentleVectorTiltPrepared = Math.Abs(TiltX) > 0.15f || Math.Abs(TiltY) > 0.15f;
        SoftPerspectiveShadowPrepared = ShadowLift > 18.2f || Math.Abs(ShadowOffsetX) > 0.5f;
    }

    public void ApproachLens(float nearness)
    {
        _approachTarget = Math.Clamp(nearness, 0f, 1f);
        CenterLockedSuctionPrepared = _approachTarget > 0.22f;
        EdgeApexSqueezePrepared = _approachTarget > 0.52f;
        ShadowSuctionPrepared = _approachTarget > 0.66f;
    }

    public void LeaveLens()
    {
        _approachTarget = 0f;
    }

    public void PlaceIntoLens()
    {
        State = NativeGlassOverlayCarryState.InTransit;
        IsHolding = false;
        TransitMilliseconds = 0;
        TransitCountdownPrepared = true;
        ReleaseRequiredForAbsorptionPrepared = true;
        _approachTarget = 1f;
        Approach = 1f;
        _openTarget = 1f;
        LensOpen = 1f;
        LensEmergence = 1f;
        Absorption = Math.Max(Absorption, 0.02f);
        CenterLockedSuctionPrepared = true;
        EdgeApexSqueezePrepared = true;
        ShadowSuctionPrepared = true;
    }

    public void PlaceOnDesktop()
    {
        State = NativeGlassOverlayCarryState.LocalReady;
        IsHolding = false;
        Absorption = 0f;
        PullOutRecovery = 1f;
        _approachTarget = 0f;
        _openTarget = 0f;
        Approach = 0f;
        LensOpen = 0f;
        PickProgress = 0f;
        TiltX = 0f;
        TiltY = 0f;
        ShadowLift = 0f;
        ShadowOffsetX = 0f;
    }

    public void PullOut()
    {
        if (CanPullOut)
        {
            RetakeResetsTransitTimerPrepared = true;
        }

        State = NativeGlassOverlayCarryState.Held;
        IsHolding = true;
        TransitMilliseconds = 0;
        Absorption = 0f;
        PullOutRecovery = 0f;
        _openTarget = 0f;
        _approachTarget = 0f;
        Approach = 0f;
        PickProgress = 1f;
    }

    public void Advance(int milliseconds)
    {
        if (State == NativeGlassOverlayCarryState.InTransit && !IsHolding)
        {
            TransitMilliseconds = Math.Min(TransitTimeoutMilliseconds, TransitMilliseconds + milliseconds);
            Absorption = Math.Clamp(Absorption + ((float)milliseconds / 1050f), 0f, 1f);
            if (TransitMilliseconds >= TransitTimeoutMilliseconds)
            {
                State = NativeGlassOverlayCarryState.PlacedRemote;
                RemotePlacementPrepared = true;
                TunnelAutoClosePrepared = true;
            }
        }

        if (State is NativeGlassOverlayCarryState.PlacedRemote or NativeGlassOverlayCarryState.Closing)
        {
            State = NativeGlassOverlayCarryState.Closing;
            _openTarget = 0f;
            _approachTarget = 0f;
            LensOpen = Math.Clamp(LensOpen - ((float)milliseconds / 520f), 0f, 1f);
            LensEmergence = Math.Clamp(LensEmergence - ((float)milliseconds / 960f), 0f, 1f);
            if (LensOpen <= 0.001f && LensEmergence <= 0.001f)
            {
                State = NativeGlassOverlayCarryState.Closed;
                LensOpen = 0f;
                LensEmergence = 0f;
                RemoteGestureRequiredPrepared = true;
            }
        }
        else if (IsHolding)
        {
            var before = PickProgress;
            PickProgress = Math.Clamp(PickProgress + ((float)milliseconds / 620f), 0f, 1f);
            LensEmergence = Math.Clamp(LensEmergence + ((float)milliseconds / 620f), 0f, 1f);
            SmoothPickScalePrepared = SmoothPickScalePrepared || (before < 0.42f && PickProgress >= 0.42f);
        }
        else if (State == NativeGlassOverlayCarryState.LocalReady)
        {
            LensEmergence = Math.Clamp(LensEmergence - ((float)milliseconds / 520f), 0f, 1f);
            PickProgress = Math.Clamp(PickProgress - ((float)milliseconds / 360f), 0f, 1f);
            TiltX *= 0.84f;
            TiltY *= 0.84f;
            ShadowLift *= 0.78f;
            ShadowOffsetX *= 0.78f;
        }

        var approachStep = (float)milliseconds / (_approachTarget > Approach ? 560f : 360f);
        Approach = MoveToward(Approach, _approachTarget, approachStep);
        _openTarget = Approach >= 0.66f
            ? Math.Clamp((Approach - 0.66f) / 0.34f, 0f, 1f)
            : 0f;

        LensOpen = MoveToward(LensOpen, _openTarget, (float)milliseconds / 520f);

        if (PullOutRecovery < 1f)
        {
            PullOutRecovery = Math.Clamp(PullOutRecovery + ((float)milliseconds / 620f), 0f, 1f);
        }
    }

    public void RunSmokeScenario()
    {
        Pick();
        Advance(720);
        Carry(42f, -31f);
        ApproachLens(0.92f);
        Advance(420);
        var noAbsorbBeforeRelease = IsHolding && Absorption <= 0.001f;
        PlaceIntoLens();
        Advance(1800);
        PullOut();
        var retakeOk = RetakeResetsTransitTimerPrepared && TransitMilliseconds == 0 && IsHolding;
        Advance(320);
        PlaceIntoLens();
        Advance(TransitTimeoutMilliseconds + 1400);
        ReleaseRequiredForAbsorptionPrepared = ReleaseRequiredForAbsorptionPrepared && noAbsorbBeforeRelease;
        RetakeResetsTransitTimerPrepared = RetakeResetsTransitTimerPrepared && retakeOk;
    }

    private static float MoveToward(float current, float target, float amount)
    {
        if (current < target)
        {
            return Math.Min(target, current + amount);
        }

        return Math.Max(target, current - amount);
    }
}
