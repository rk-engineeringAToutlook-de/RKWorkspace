namespace RKWorkspace.Configuration;

public static class RKWorkspaceConfigurationValidator
{
    private static readonly HashSet<string> SecurityModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "DevelopmentInsecure",
        "Authenticated",
        "Encrypted",
        "EncryptedAndAuthenticated",
        "ProductionRequired"
    };

    private static readonly HashSet<string> PolicyProfiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "CriticalInfrastructure",
        "OfficeDefault",
        "DevelopmentLab",
        "PresentationOnly",
        "TrustedPersonalDevices"
    };

    private static readonly HashSet<string> FrameCachePolicies = new(StringComparer.OrdinalIgnoreCase)
    {
        "MemoryOnly",
        "TemporaryEncrypted",
        "Disabled",
        "DevInspectable"
    };

    public static ConfigurationValidationResult Validate(RKWorkspaceConfiguration configuration)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        ValidateAblage(configuration.Ablage, errors);
        ValidateRkwp(configuration.Rkwp, errors);
        ValidateSecurity(configuration.Security, errors, warnings);
        ValidatePolicy(configuration.Policy, errors);
        ValidateProximity(configuration.Proximity, errors);
        ValidateFrame(configuration.Frame, errors);
        ValidateSurface(configuration.Surface, errors);
        ValidateGesture(configuration.Gesture, errors);

        if (configuration.Policy.PolicyProfile.Equals("CriticalInfrastructure", StringComparison.OrdinalIgnoreCase))
        {
            if (!configuration.Security.SecureSessionRequired)
            {
                errors.Add("CriticalInfrastructure requires SecureSessionRequired.");
            }

            if (configuration.Policy.OwnershipTransferAllowed)
            {
                errors.Add("CriticalInfrastructure blocks OwnershipTransferAllowed.");
            }

            if (configuration.Frame.FrameCachePolicy.Equals("DevInspectable", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("CriticalInfrastructure blocks DevInspectable frame cache.");
            }
        }

        return errors.Count == 0
            ? ConfigurationValidationResult.Success(warnings)
            : ConfigurationValidationResult.Failed(errors, warnings);
    }

    public static ConfigurationValidationResult ValidatePolicy(PolicyConfiguration policy)
    {
        var errors = new List<string>();
        ValidatePolicy(policy, errors);
        return errors.Count == 0
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failed(errors);
    }

    public static ConfigurationValidationResult ValidateRkwp(RkwpConfiguration rkwp)
    {
        var errors = new List<string>();
        ValidateRkwp(rkwp, errors);
        return errors.Count == 0
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failed(errors);
    }

    private static void ValidateAblage(AblageConfiguration ablage, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(ablage.AblageId))
        {
            errors.Add("AblageId is required.");
        }

        if (string.IsNullOrWhiteSpace(ablage.DisplayName))
        {
            errors.Add("Ablage DisplayName is required.");
        }
    }

    private static void ValidateRkwp(RkwpConfiguration rkwp, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(rkwp.TransportProfile))
        {
            errors.Add("Rkwp TransportProfile is required.");
        }

        if (rkwp.Port is <= 0 or > 65535)
        {
            errors.Add("Rkwp port must be between 1 and 65535.");
        }

        if (rkwp.HeartbeatIntervalSeconds <= 0)
        {
            errors.Add("HeartbeatIntervalSeconds must be greater than zero.");
        }
    }

    private static void ValidateSecurity(SecurityConfiguration security, List<string> errors, List<string> warnings)
    {
        if (!SecurityModes.Contains(security.SecurityMode))
        {
            errors.Add($"Unsupported SecurityMode: {security.SecurityMode}");
        }

        if (security.SecurityMode.Equals("DevelopmentInsecure", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("DevelopmentInsecure is lab-only.");
        }
    }

    private static void ValidatePolicy(PolicyConfiguration policy, List<string> errors)
    {
        if (!PolicyProfiles.Contains(policy.PolicyProfile))
        {
            errors.Add($"Unsupported PolicyProfile: {policy.PolicyProfile}");
        }

        if (!policy.NoFileIngress)
        {
            errors.Add("NoFileIngress must remain true.");
        }
    }

    private static void ValidateProximity(ProximityConfiguration proximity, List<string> errors)
    {
        if (proximity.MinimumConfidence is < 0 or > 1)
        {
            errors.Add("MinimumConfidence must be between 0 and 1.");
        }

        if (proximity.EdgeSwitchDelayMs < 0)
        {
            errors.Add("EdgeSwitchDelayMs must not be negative.");
        }
    }

    private static void ValidateFrame(FrameConfiguration frame, List<string> errors)
    {
        if (!FrameCachePolicies.Contains(frame.FrameCachePolicy))
        {
            errors.Add($"Unsupported FrameCachePolicy: {frame.FrameCachePolicy}");
        }

        if (frame.AllowGuestFileIngress)
        {
            errors.Add("AllowGuestFileIngress must remain false.");
        }
    }

    private static void ValidateSurface(SurfaceConfiguration surface, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(surface.Platform))
        {
            errors.Add("Surface Platform is required.");
        }
    }

    private static void ValidateGesture(GestureConfiguration gesture, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(gesture.PickGesture))
        {
            errors.Add("PickGesture is required.");
        }

        if (gesture.LongPressMs <= 0)
        {
            errors.Add("LongPressMs must be greater than zero.");
        }
    }
}
