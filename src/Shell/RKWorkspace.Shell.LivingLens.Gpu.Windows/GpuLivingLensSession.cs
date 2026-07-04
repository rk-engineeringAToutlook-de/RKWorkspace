namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public sealed class GpuLivingLensSession
{
    private float _openTarget;

    public float LensX { get; } = 0.995f;

    public float LensY { get; } = 0.50f;

    public float LensVisibleRatio { get; } = 0.87f;

    public bool GpuCompositionPrepared { get; } = true;

    public bool DesktopRefractionPrepared { get; } = true;

    public bool RefractionMapPrepared { get; } = true;

    public bool TunnelDepthPrepared { get; } = true;

    public bool HlslShaderContractPrepared { get; } = true;

    public bool RectangularThingPrepared { get; } = true;

    public bool RectangularShadowPrepared { get; } = true;

    public bool GentleCarryTiltPrepared { get; private set; }

    public bool SoftShadowPrepared { get; } = true;

    public bool PortalEdgePullPrepared { get; private set; }

    public bool NoWhiteBlock { get; } = true;

    public bool DropRequiresRelease { get; private set; } = true;

    public bool PullOutPrepared { get; private set; }

    public bool VectorTiltPrepared { get; private set; }

    public bool ShadowPrepared { get; private set; }

    public bool ShadowSuctionPrepared { get; private set; }

    public bool PerspectiveTrapezoidPrepared { get; private set; }

    public bool LensAppearsOnPickPrepared { get; private set; }

    public bool CarryShadowOnlyPrepared { get; private set; } = true;

    public bool PrimaryLensHugsScreenEdge => LensX >= 0.99f && LensVisibleRatio is >= 0.85f and <= 0.90f;

    public bool EdgeContinuationPrepared { get; } = true;

    public bool IsHoldingThing { get; private set; }

    public float LensEmergence { get; private set; }

    public float LensOpen { get; private set; }

    public float Absorption { get; private set; }

    public float PullOutRecovery { get; private set; } = 1f;

    public float TiltX { get; private set; }

    public float TiltY { get; private set; }

    public float ShadowX { get; private set; }

    public float ShadowY { get; private set; } = 22f;

    public void Pick()
    {
        IsHoldingThing = true;
        LensEmergence = Math.Max(LensEmergence, 0.24f);
        Absorption = 0f;
        PullOutRecovery = 1f;
        LensAppearsOnPickPrepared = LensEmergence >= 0.20f;
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

        _openTarget = nearness >= 0.68f
            ? Math.Clamp((nearness - 0.68f) / 0.32f, 0f, 1f)
            : 0f;
    }

    public void LeaveLens()
    {
        _openTarget = 0f;
    }

    public void PlaceIntoLens()
    {
        IsHoldingThing = false;
        DropRequiresRelease = true;
        LensEmergence = 1f;
        LensOpen = 1f;
        Absorption = 0.01f;
        ShadowSuctionPrepared = true;
    }

    public void PlaceOnSurface()
    {
        IsHoldingThing = false;
        Absorption = 0f;
        PullOutRecovery = 1f;
        TiltX = 0f;
        TiltY = 0f;
        ShadowX = 0f;
        ShadowY = 0f;
        _openTarget = 0f;
        CarryShadowOnlyPrepared = true;
    }

    public void PullOutFromLens()
    {
        IsHoldingThing = true;
        PullOutPrepared = true;
        PullOutRecovery = 0f;
        Absorption = 0f;
        _openTarget = 0f;
    }

    public void Advance(int milliseconds)
    {
        if (IsHoldingThing)
        {
            LensEmergence = Math.Clamp(LensEmergence + ((float)milliseconds / 540f), 0f, 1f);
        }
        else if (Absorption <= 0f)
        {
            LensEmergence = Math.Clamp(LensEmergence - ((float)milliseconds / 520f), 0f, 1f);
            TiltX *= 0.86f;
            TiltY *= 0.86f;
            ShadowX *= 0.82f;
            ShadowY *= 0.82f;
        }

        if (_openTarget > LensOpen)
        {
            LensOpen = Math.Clamp(LensOpen + ((float)milliseconds / 240f), 0f, _openTarget);
        }
        else if (_openTarget < LensOpen && Absorption <= 0f)
        {
            LensOpen = Math.Clamp(LensOpen - ((float)milliseconds / 380f), _openTarget, 1f);
        }

        if (Absorption > 0f && Absorption < 1f)
        {
            Absorption = Math.Clamp(Absorption + ((float)milliseconds / 980f), 0f, 1f);
        }

        if (PullOutRecovery < 1f)
        {
            PullOutRecovery = Math.Clamp(PullOutRecovery + ((float)milliseconds / 620f), 0f, 1f);
        }
    }

    public void RunSmokeScenario()
    {
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
        Advance(720);
        PullOutFromLens();
        Advance(660);
        DropRequiresRelease = DropRequiresRelease && noAutoAbsorption;
    }
}
