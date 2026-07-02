using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class WorkspaceCapabilityProvider : ICapabilityProvider
{
    private readonly WorkspaceDescriptor _descriptor;

    public WorkspaceCapabilityProvider(WorkspaceDescriptor descriptor)
    {
        _descriptor = descriptor.Snapshot();
    }

    public string ProviderId => $"workspace:{_descriptor.WorkspaceId}";

    public string DisplayName => _descriptor.DisplayName;

    public CapabilitySet GetCapabilities()
    {
        return _descriptor.Capabilities.Snapshot();
    }
}
