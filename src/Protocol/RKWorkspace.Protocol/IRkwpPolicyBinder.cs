using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Protocol;

public interface IRkwpPolicyBinder
{
    PolicyBindingValidation Validate(CarryLease lease, FrameSession frameSession);
}
