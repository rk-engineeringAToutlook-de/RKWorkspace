namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Describes a single platform-neutral capability.
/// </summary>
public sealed record Capability
{
    /// <summary>
    /// Gets the capability id.
    /// </summary>
    public required CapabilityId CapabilityId { get; init; }

    /// <summary>
    /// Gets the user-visible display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the capability category.
    /// </summary>
    public required CapabilityCategory Category { get; init; }

    /// <summary>
    /// Gets the capability description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this capability is required by its provider.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets whether this capability is experimental.
    /// </summary>
    public bool IsExperimental { get; init; }

    /// <summary>
    /// Gets the capability version.
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Creates a simple capability with a default category inferred from the id.
    /// </summary>
    /// <param name="capabilityId">The capability id.</param>
    /// <returns>A capability descriptor.</returns>
    public static Capability Create(CapabilityId capabilityId)
    {
        return new Capability
        {
            CapabilityId = capabilityId,
            DisplayName = capabilityId.ToString(),
            Category = GetDefaultCategory(capabilityId)
        };
    }

    /// <summary>
    /// Gets the default category for a capability id.
    /// </summary>
    /// <param name="capabilityId">The capability id.</param>
    /// <returns>The default category.</returns>
    public static CapabilityCategory GetDefaultCategory(CapabilityId capabilityId)
    {
        return capabilityId switch
        {
            CapabilityId.Touch
                or CapabilityId.Touchpad
                or CapabilityId.Mouse
                or CapabilityId.Keyboard => CapabilityCategory.Input,
            CapabilityId.Display
                or CapabilityId.MultipleDisplays => CapabilityCategory.Display,
            CapabilityId.BLE
                or CapabilityId.WiFi
                or CapabilityId.LAN => CapabilityCategory.Communication,
            CapabilityId.Clipboard
                or CapabilityId.DragDrop
                or CapabilityId.ContextTransfer => CapabilityCategory.Transfer,
            CapabilityId.Encryption
                or CapabilityId.Pairing => CapabilityCategory.Security,
            CapabilityId.USB
                or CapabilityId.USBC
                or CapabilityId.UWB
                or CapabilityId.HardwareNode
                or CapabilityId.DisplayNode => CapabilityCategory.Hardware,
            CapabilityId.FirmwareUpdate => CapabilityCategory.Firmware,
            CapabilityId.HapticFeedback
                or CapabilityId.Animation
                or CapabilityId.Overlay
                or CapabilityId.Notification => CapabilityCategory.UserExperience,
            CapabilityId.Logging
                or CapabilityId.OfflineMode
                or CapabilityId.CloudMode
                or CapabilityId.Simulation
                or CapabilityId.Testing => CapabilityCategory.System,
            _ => CapabilityCategory.Unknown
        };
    }
}
