namespace RKWorkspace.Protocol.Ownership;

public sealed record PolicyBindingValidation(
    bool IsValid,
    string PolicyId,
    int PolicyVersion,
    string Message)
{
    public static PolicyBindingValidation Valid(string policyId, int policyVersion)
    {
        return new PolicyBindingValidation(true, policyId, policyVersion, "Policy binding is valid.");
    }

    public static PolicyBindingValidation Denied(string policyId, int policyVersion, string message)
    {
        return new PolicyBindingValidation(false, policyId, policyVersion, message);
    }
}
