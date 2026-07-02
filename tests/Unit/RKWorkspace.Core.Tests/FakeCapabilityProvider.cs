using RKWorkspace.Core.Capabilities;

internal sealed class FakeCapabilityProvider : ICapabilityProvider
{
    private CapabilitySet _capabilities;

    private FakeCapabilityProvider(string providerId, string displayName, CapabilitySet capabilities)
    {
        ProviderId = providerId;
        DisplayName = displayName;
        _capabilities = capabilities;
    }

    public string ProviderId { get; }

    public string DisplayName { get; }

    public CapabilitySet GetCapabilities()
    {
        return _capabilities.Snapshot();
    }

    public void SetCapabilities(CapabilitySet capabilities)
    {
        _capabilities = capabilities;
    }

    public static FakeCapabilityProvider Create(
        string providerId,
        string displayName,
        params CapabilityId[] capabilityIds)
    {
        return new FakeCapabilityProvider(
            providerId,
            displayName,
            CapabilitySet.FromIds(capabilityIds));
    }
}
