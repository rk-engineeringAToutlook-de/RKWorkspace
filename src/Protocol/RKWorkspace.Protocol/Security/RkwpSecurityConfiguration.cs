namespace RKWorkspace.Protocol.Security;

public enum RkwpSecurityEnvironmentMode
{
    Development,
    Test,
    Staging,
    Production
}

public sealed record RkwpSecurityConfiguration(
    RkwpSecurityEnvironmentMode SecurityMode,
    bool AllowDevelopmentInsecure,
    bool RequireMutualAuthentication,
    bool RequireEncryption,
    bool RequireReplayProtection,
    bool RequireAudit,
    bool RequirePolicyBinding,
    string EnvironmentName)
{
    public static RkwpSecurityConfiguration Development(string environmentName = "Development")
    {
        return new RkwpSecurityConfiguration(
            RkwpSecurityEnvironmentMode.Development,
            AllowDevelopmentInsecure: true,
            RequireMutualAuthentication: false,
            RequireEncryption: false,
            RequireReplayProtection: true,
            RequireAudit: true,
            RequirePolicyBinding: true,
            environmentName);
    }

    public static RkwpSecurityConfiguration Test(string environmentName = "Test")
    {
        return new RkwpSecurityConfiguration(
            RkwpSecurityEnvironmentMode.Test,
            AllowDevelopmentInsecure: true,
            RequireMutualAuthentication: false,
            RequireEncryption: false,
            RequireReplayProtection: true,
            RequireAudit: true,
            RequirePolicyBinding: true,
            environmentName);
    }

    public static RkwpSecurityConfiguration Staging(string environmentName = "Staging")
    {
        return new RkwpSecurityConfiguration(
            RkwpSecurityEnvironmentMode.Staging,
            AllowDevelopmentInsecure: false,
            RequireMutualAuthentication: true,
            RequireEncryption: true,
            RequireReplayProtection: true,
            RequireAudit: true,
            RequirePolicyBinding: true,
            environmentName);
    }

    public static RkwpSecurityConfiguration Production(string environmentName = "Production")
    {
        return new RkwpSecurityConfiguration(
            RkwpSecurityEnvironmentMode.Production,
            AllowDevelopmentInsecure: false,
            RequireMutualAuthentication: true,
            RequireEncryption: true,
            RequireReplayProtection: true,
            RequireAudit: true,
            RequirePolicyBinding: true,
            environmentName);
    }
}
