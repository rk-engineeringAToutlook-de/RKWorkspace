namespace RKWorkspace.Protocol.Security;

public sealed record RkwpSecurityGateDecision(
    bool Allowed,
    bool SecureSessionRequired,
    bool AuditRequired,
    bool ReplayProtectionRequired,
    bool PolicyBindingRequired,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public static RkwpSecurityGateDecision Allow(
        bool secureSessionRequired,
        bool auditRequired,
        bool replayProtectionRequired,
        bool policyBindingRequired,
        IReadOnlyList<string> warnings)
    {
        return new RkwpSecurityGateDecision(
            true,
            secureSessionRequired,
            auditRequired,
            replayProtectionRequired,
            policyBindingRequired,
            Array.Empty<string>(),
            warnings);
    }

    public static RkwpSecurityGateDecision Deny(
        bool secureSessionRequired,
        bool auditRequired,
        bool replayProtectionRequired,
        bool policyBindingRequired,
        IReadOnlyList<string> errors,
        IReadOnlyList<string> warnings)
    {
        return new RkwpSecurityGateDecision(
            false,
            secureSessionRequired,
            auditRequired,
            replayProtectionRequired,
            policyBindingRequired,
            errors,
            warnings);
    }
}

public static class RkwpSecurityGate
{
    public static RkwpSecurityGateDecision Evaluate(
        RkwpSecurityConfiguration configuration,
        RkwpSecurityMode requestedSessionSecurityMode)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var secureSessionRequired = configuration.RequireMutualAuthentication || configuration.RequireEncryption;
        var auditRequired = configuration.RequireAudit;
        var replayProtectionRequired = configuration.RequireReplayProtection;
        var policyBindingRequired = configuration.RequirePolicyBinding;

        if (requestedSessionSecurityMode == RkwpSecurityMode.DevelopmentInsecure)
        {
            if (!configuration.AllowDevelopmentInsecure)
            {
                errors.Add($"{configuration.SecurityMode} rejects DevelopmentInsecure sessions.");
            }
            else
            {
                warnings.Add("DevelopmentInsecure is allowed only for non-production development and smoke tests.");
            }
        }

        switch (configuration.SecurityMode)
        {
            case RkwpSecurityEnvironmentMode.Production:
                secureSessionRequired = true;
                auditRequired = true;
                replayProtectionRequired = true;
                policyBindingRequired = true;
                if (configuration.AllowDevelopmentInsecure)
                {
                    errors.Add("Production must not allow DevelopmentInsecure.");
                }

                if (!configuration.RequireMutualAuthentication)
                {
                    errors.Add("Production requires mutual authentication.");
                }

                if (!configuration.RequireEncryption)
                {
                    errors.Add("Production requires encryption.");
                }

                if (!configuration.RequireAudit)
                {
                    errors.Add("Production requires audit.");
                }

                if (!configuration.RequireReplayProtection)
                {
                    errors.Add("Production requires replay protection.");
                }

                if (!configuration.RequirePolicyBinding)
                {
                    errors.Add("Production requires policy binding.");
                }

                break;
            case RkwpSecurityEnvironmentMode.Staging:
                warnings.Add("Staging should mirror production; any relaxed setting must be explicit and temporary.");
                break;
            case RkwpSecurityEnvironmentMode.Test:
                warnings.Add("Test may allow DevelopmentInsecure only for automated non-production checks.");
                break;
            case RkwpSecurityEnvironmentMode.Development:
                warnings.Add("Development mode must log visible warnings when DevelopmentInsecure is used.");
                break;
            default:
                errors.Add("Unknown RKWP security environment.");
                break;
        }

        return errors.Count == 0
            ? RkwpSecurityGateDecision.Allow(
                secureSessionRequired,
                auditRequired,
                replayProtectionRequired,
                policyBindingRequired,
                warnings)
            : RkwpSecurityGateDecision.Deny(
                secureSessionRequired,
                auditRequired,
                replayProtectionRequired,
                policyBindingRequired,
                errors,
                warnings);
    }
}
