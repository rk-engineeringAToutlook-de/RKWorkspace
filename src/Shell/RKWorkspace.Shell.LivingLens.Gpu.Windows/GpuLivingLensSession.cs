namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public sealed class GpuLivingLensSession
{
    private float _openTarget;

    public float LensX { get; } = 0.995f;

    public float LensY { get; } = 0.50f;

    public bool GpuCompositionPrepared { get; } = true;

    public bool DesktopRefractionPrepared { get; } = true;

    public bool RefractionMapPrepared { get; } = true;

    public bool NoWhiteBlock { get; } = true;

    public bool DropRequiresRelease { get; private set; } = true;

    public bool PullOutPrepared { get; private set; }

    public bool VectorTiltPrepared { get; private set; }

    public bool ShadowPrepared { get; private set; }

    public bool PrimaryLensHugsScreenEdge => LensX >= 0.99f;

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
        LensEmergence = Math.Max(LensEmergence, 0.01f);
        Absorption = 0f;
        PullOutRecovery = 1f;
    }

    public void Carry(float movementX, float movementY)
    {
        var targetTiltX = Math.Clamp(movementX * 0.070f, -8.5f, 8.5f);
        var targetTiltY = Math.Clamp(-movementY * 0.066f, -8.5f, 8.5f);
        TiltX = (TiltX * 0.50f) + (targetTiltX * 0.50f);
        TiltY = (TiltY * 0.50f) + (targetTiltY * 0.50f);
        ShadowX = Math.Clamp(-TiltX * 2.2f, -18f, 18f);
        ShadowY = Math.Clamp(21f + (MathF.Abs(TiltY) * 2.0f), 18f, 38f);
        VectorTiltPrepared = Math.Abs(TiltX) > 0.2f && Math.Abs(TiltY) > 0.2f;
        ShadowPrepared = ShadowY > 22f || Math.Abs(ShadowX) > 1f;
    }

    public void ApproachLens(float nearness)
    {
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
            LensEmergence = Math.Clamp(LensEmergence + ((float)milliseconds / 1000f), 0f, 1f);
        }
        else if (Absorption <= 0f)
        {
            LensEmergence = Math.Clamp(LensEmergence - ((float)milliseconds / 520f), 0f, 1f);
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
        PlaceIntoLens();
        Advance(720);
        PullOutFromLens();
        Advance(660);
        DropRequiresRelease = DropRequiresRelease && noAutoAbsorption;
    }
}
