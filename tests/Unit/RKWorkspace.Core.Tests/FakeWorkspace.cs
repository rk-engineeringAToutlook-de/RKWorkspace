using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Workspaces;

internal sealed class FakeWorkspace : IWorkspace
{
    private WorkspaceDescriptor _descriptor;

    private FakeWorkspace(WorkspaceDescriptor descriptor)
    {
        _descriptor = descriptor.Snapshot();
    }

    public WorkspaceId Id => _descriptor.WorkspaceId;

    public WorkspaceDescriptor Descriptor => _descriptor.Snapshot();

    public CapabilitySet Capabilities => _descriptor.Capabilities.Snapshot();

    public WorkspaceState State => _descriptor.WorkspaceState;

    public int UpdateCount { get; private set; }

    public void UpdateDescriptor(WorkspaceDescriptor descriptor)
    {
        WorkspaceDescriptor.Validate(descriptor);
        if (descriptor.WorkspaceId != Id)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidOperation,
                "Fake workspace id cannot change.",
                Id.ToString());
        }

        _descriptor = descriptor.Snapshot();
        UpdateCount++;
    }

    public static FakeWorkspace Create(
        string workspaceId,
        WorkspaceType type = WorkspaceType.SmartDevice,
        WorkspaceState state = WorkspaceState.Available,
        WorkspacePosition position = WorkspacePosition.Unknown,
        bool isTrusted = true,
        int priority = 0,
        DateTimeOffset? lastSeen = null,
        string? displayName = null,
        params CapabilityId[] capabilityIds)
    {
        return new FakeWorkspace(new WorkspaceDescriptor
        {
            WorkspaceId = WorkspaceId.Create(workspaceId),
            DisplayName = displayName ?? workspaceId,
            WorkspaceType = type,
            WorkspaceState = state,
            Position = position,
            Capabilities = CapabilitySet.FromIds(capabilityIds),
            IsTrusted = isTrusted,
            Priority = priority,
            LastSeen = lastSeen ?? DateTimeOffset.UnixEpoch,
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "test"
            }
        });
    }
}
