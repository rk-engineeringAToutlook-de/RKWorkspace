using RKWorkspace.Protocol.Security;

namespace RKWorkspace.Protocol.Ownership;

public enum RkwpPolicyProfileName
{
    CriticalInfrastructure,
    OfficeDefault,
    DevelopmentLab,
    PresentationOnly,
    TrustedPersonalDevices
}

public sealed record RkwpPolicyProfile(
    RkwpPolicyProfileName Name,
    string ProfileId,
    string DisplayName,
    string Description,
    OwnershipPolicy Ownership,
    FramePolicy Frame,
    ExtractionPolicy Extraction,
    OwnershipTransferPolicy OwnershipTransfer,
    RkwpSecurityConfiguration Security,
    RkwpSecurityMode MinimumSessionSecurityMode,
    bool SecureSessionRequired,
    bool DevelopmentModeAllowed,
    bool SimulatedProximityAllowed,
    bool HapticsAllowed,
    TimeSpan LeaseTimeout)
{
    public bool FrameOnlyDefault => Ownership.DefaultMode == OwnershipMode.FrameOnly;

    public bool NoFileIngress => Ownership.NoFileIngress && !Extraction.FileIngressAllowed;

    public bool OwnershipTransferAllowed => Ownership.OwnershipTransferAllowed;

    public bool AuditRequired => Ownership.AuditRequired || Security.RequireAudit;

    public bool InputAllowed => Frame.InputAllowed;

    public RkwpSecurityGateDecision EvaluateSecurityGate()
    {
        return RkwpSecurityGate.Evaluate(Security, MinimumSessionSecurityMode);
    }
}

public static class RkwpPolicyProfileStore
{
    public static IReadOnlyList<RkwpPolicyProfile> All { get; } =
    [
        CreateCriticalInfrastructure(),
        CreateOfficeDefault(),
        CreateDevelopmentLab(),
        CreatePresentationOnly(),
        CreateTrustedPersonalDevices()
    ];

    public static RkwpPolicyProfile Get(RkwpPolicyProfileName name)
    {
        return All.Single(profile => profile.Name == name);
    }

    public static bool TryGet(string name, out RkwpPolicyProfile profile)
    {
        if (Enum.TryParse<RkwpPolicyProfileName>(name, ignoreCase: true, out var parsed))
        {
            profile = Get(parsed);
            return true;
        }

        profile = All.FirstOrDefault(candidate =>
            string.Equals(candidate.ProfileId, name, StringComparison.OrdinalIgnoreCase))!;
        return profile is not null;
    }

    private static RkwpPolicyProfile CreateCriticalInfrastructure()
    {
        var ownership = new OwnershipPolicy(
            "policy-profile-critical-infrastructure",
            OwnershipMode.FrameOnly,
            RkwpAllowedAction.View | RkwpAllowedAction.Return | RkwpAllowedAction.Revoke,
            NoFileIngress: true,
            OwnershipTransferAllowed: false,
            Revocable: true,
            AuditRequired: true,
            EncryptedRequired: true);
        return new RkwpPolicyProfile(
            RkwpPolicyProfileName.CriticalInfrastructure,
            "critical-infrastructure",
            "Critical Infrastructure",
            "FrameOnly, NoFileIngress, no ownership transfer, audit and secure session required.",
            ownership,
            FramePolicy.CriticalViewOnly,
            new ExtractionPolicy("policy-profile-critical-extract", 1, TextAllowed: false, ImageAllowed: false, FileIngressAllowed: false),
            new OwnershipTransferPolicy("policy-profile-critical-transfer", 1, CopyOutAllowed: false, ForkVersionAllowed: false, MoveOwnershipAllowed: false, RequiresUserConfirmation: true),
            RkwpSecurityConfiguration.Production("CriticalInfrastructure"),
            RkwpSecurityMode.ProductionRequired,
            SecureSessionRequired: true,
            DevelopmentModeAllowed: false,
            SimulatedProximityAllowed: false,
            HapticsAllowed: false,
            LeaseTimeout: TimeSpan.FromMinutes(2));
    }

    private static RkwpPolicyProfile CreateOfficeDefault()
    {
        var ownership = new OwnershipPolicy(
            "policy-profile-office-default",
            OwnershipMode.FrameOnly,
            RkwpAllowedAction.View | RkwpAllowedAction.Scroll | RkwpAllowedAction.Zoom | RkwpAllowedAction.CopyOut | RkwpAllowedAction.Return | RkwpAllowedAction.Revoke,
            NoFileIngress: true,
            OwnershipTransferAllowed: true,
            Revocable: true,
            AuditRequired: false,
            EncryptedRequired: true,
            RequiresUserConfirmation: true);
        return new RkwpPolicyProfile(
            RkwpPolicyProfileName.OfficeDefault,
            "office-default",
            "Office Default",
            "FrameOnly by default, optional text extraction and CopyOut only with confirmation.",
            ownership,
            FramePolicy.InteractiveView,
            new ExtractionPolicy("policy-profile-office-extract", 1, TextAllowed: true, ImageAllowed: false, FileIngressAllowed: false),
            new OwnershipTransferPolicy("policy-profile-office-transfer", 1, CopyOutAllowed: true, ForkVersionAllowed: false, MoveOwnershipAllowed: false, RequiresUserConfirmation: true),
            RkwpSecurityConfiguration.Staging("OfficeDefault"),
            RkwpSecurityMode.EncryptedAndAuthenticated,
            SecureSessionRequired: true,
            DevelopmentModeAllowed: false,
            SimulatedProximityAllowed: false,
            HapticsAllowed: true,
            LeaseTimeout: TimeSpan.FromMinutes(15));
    }

    private static RkwpPolicyProfile CreateDevelopmentLab()
    {
        var ownership = new OwnershipPolicy(
            "policy-profile-development-lab",
            OwnershipMode.FrameOnly,
            RkwpAllowedAction.View | RkwpAllowedAction.Scroll | RkwpAllowedAction.Zoom | RkwpAllowedAction.Input | RkwpAllowedAction.Annotate | RkwpAllowedAction.Return | RkwpAllowedAction.Revoke,
            NoFileIngress: true,
            OwnershipTransferAllowed: false,
            Revocable: true,
            AuditRequired: true,
            EncryptedRequired: false);
        return new RkwpPolicyProfile(
            RkwpPolicyProfileName.DevelopmentLab,
            "development-lab",
            "Development Lab",
            "Development mode and simulated proximity are allowed with visible warnings; NoFileIngress still remains testable.",
            ownership,
            FramePolicy.Annotate,
            new ExtractionPolicy("policy-profile-dev-extract", 1, TextAllowed: true, ImageAllowed: true, FileIngressAllowed: false),
            new OwnershipTransferPolicy("policy-profile-dev-transfer", 1, CopyOutAllowed: false, ForkVersionAllowed: false, MoveOwnershipAllowed: false, RequiresUserConfirmation: true),
            RkwpSecurityConfiguration.Development("DevelopmentLab"),
            RkwpSecurityMode.DevelopmentInsecure,
            SecureSessionRequired: false,
            DevelopmentModeAllowed: true,
            SimulatedProximityAllowed: true,
            HapticsAllowed: true,
            LeaseTimeout: TimeSpan.FromMinutes(30));
    }

    private static RkwpPolicyProfile CreatePresentationOnly()
    {
        var ownership = new OwnershipPolicy(
            "policy-profile-presentation-only",
            OwnershipMode.FrameOnly,
            RkwpAllowedAction.View | RkwpAllowedAction.Return,
            NoFileIngress: true,
            OwnershipTransferAllowed: false,
            Revocable: true,
            AuditRequired: true,
            EncryptedRequired: true);
        return new RkwpPolicyProfile(
            RkwpPolicyProfileName.PresentationOnly,
            "presentation-only",
            "Presentation Only",
            "ViewOnly, no input, no extract, no ownership transfer.",
            ownership,
            FramePolicy.CriticalViewOnly,
            new ExtractionPolicy("policy-profile-presentation-extract", 1, TextAllowed: false, ImageAllowed: false, FileIngressAllowed: false),
            new OwnershipTransferPolicy("policy-profile-presentation-transfer", 1, CopyOutAllowed: false, ForkVersionAllowed: false, MoveOwnershipAllowed: false, RequiresUserConfirmation: true),
            RkwpSecurityConfiguration.Staging("PresentationOnly"),
            RkwpSecurityMode.EncryptedAndAuthenticated,
            SecureSessionRequired: true,
            DevelopmentModeAllowed: false,
            SimulatedProximityAllowed: false,
            HapticsAllowed: false,
            LeaseTimeout: TimeSpan.FromMinutes(8));
    }

    private static RkwpPolicyProfile CreateTrustedPersonalDevices()
    {
        var ownership = new OwnershipPolicy(
            "policy-profile-trusted-personal-devices",
            OwnershipMode.FrameOnly,
            RkwpAllowedAction.View | RkwpAllowedAction.Scroll | RkwpAllowedAction.Zoom | RkwpAllowedAction.Input | RkwpAllowedAction.CopyOut | RkwpAllowedAction.Return | RkwpAllowedAction.Revoke,
            NoFileIngress: true,
            OwnershipTransferAllowed: true,
            Revocable: true,
            AuditRequired: false,
            EncryptedRequired: true,
            RequiresUserConfirmation: true);
        return new RkwpPolicyProfile(
            RkwpPolicyProfileName.TrustedPersonalDevices,
            "trusted-personal-devices",
            "Trusted Personal Devices",
            "Interactive frames and haptics are allowed; CopyOut remains confirmation-bound.",
            ownership,
            FramePolicy.InteractiveView,
            new ExtractionPolicy("policy-profile-personal-extract", 1, TextAllowed: true, ImageAllowed: true, FileIngressAllowed: false),
            new OwnershipTransferPolicy("policy-profile-personal-transfer", 1, CopyOutAllowed: true, ForkVersionAllowed: false, MoveOwnershipAllowed: false, RequiresUserConfirmation: true),
            RkwpSecurityConfiguration.Staging("TrustedPersonalDevices"),
            RkwpSecurityMode.EncryptedAndAuthenticated,
            SecureSessionRequired: true,
            DevelopmentModeAllowed: false,
            SimulatedProximityAllowed: false,
            HapticsAllowed: true,
            LeaseTimeout: TimeSpan.FromMinutes(20));
    }
}

public sealed record RkwpPolicyProfileValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static RkwpPolicyProfileValidationResult Success { get; } = new(true, []);
}

public static class RkwpPolicyProfileValidator
{
    public static RkwpPolicyProfileValidationResult ValidateAll(IEnumerable<RkwpPolicyProfile> profiles)
    {
        var errors = new List<string>();
        var names = new HashSet<RkwpPolicyProfileName>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var profile in profiles)
        {
            errors.AddRange(Validate(profile).Errors);
            if (!names.Add(profile.Name))
            {
                errors.Add($"Duplicate profile name {profile.Name}.");
            }

            if (!ids.Add(profile.ProfileId))
            {
                errors.Add($"Duplicate profile id {profile.ProfileId}.");
            }
        }

        return errors.Count == 0
            ? RkwpPolicyProfileValidationResult.Success
            : new RkwpPolicyProfileValidationResult(false, errors);
    }

    public static RkwpPolicyProfileValidationResult Validate(RkwpPolicyProfile profile)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.ProfileId))
        {
            errors.Add($"{profile.Name}: ProfileId is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            errors.Add($"{profile.Name}: DisplayName is required.");
        }

        if (!profile.NoFileIngress)
        {
            errors.Add($"{profile.Name}: NoFileIngress must remain true.");
        }

        if (profile.LeaseTimeout <= TimeSpan.Zero)
        {
            errors.Add($"{profile.Name}: LeaseTimeout must be positive.");
        }

        if (profile.Name == RkwpPolicyProfileName.CriticalInfrastructure)
        {
            if (!profile.FrameOnlyDefault ||
                profile.OwnershipTransferAllowed ||
                !profile.AuditRequired ||
                !profile.SecureSessionRequired ||
                profile.DevelopmentModeAllowed)
            {
                errors.Add("CriticalInfrastructure profile violates required restrictions.");
            }
        }

        if (profile.Name == RkwpPolicyProfileName.PresentationOnly &&
            profile.InputAllowed)
        {
            errors.Add("PresentationOnly must not allow input.");
        }

        if (profile.Name == RkwpPolicyProfileName.DevelopmentLab &&
            (!profile.DevelopmentModeAllowed || !profile.SimulatedProximityAllowed))
        {
            errors.Add("DevelopmentLab must allow development mode and simulated proximity.");
        }

        return errors.Count == 0
            ? RkwpPolicyProfileValidationResult.Success
            : new RkwpPolicyProfileValidationResult(false, errors);
    }
}
