namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Identifies a platform-neutral RK Workspace capability.
/// </summary>
public enum CapabilityId
{
    /// <summary>
    /// Direct touch input.
    /// </summary>
    Touch,

    /// <summary>
    /// Touchpad input.
    /// </summary>
    Touchpad,

    /// <summary>
    /// Mouse or pointer input.
    /// </summary>
    Mouse,

    /// <summary>
    /// Keyboard input.
    /// </summary>
    Keyboard,

    /// <summary>
    /// Clipboard access.
    /// </summary>
    Clipboard,

    /// <summary>
    /// Drag and drop support.
    /// </summary>
    DragDrop,

    /// <summary>
    /// Bluetooth Low Energy discovery capability.
    /// </summary>
    BLE,

    /// <summary>
    /// Wi-Fi capability.
    /// </summary>
    WiFi,

    /// <summary>
    /// Local area network capability.
    /// </summary>
    LAN,

    /// <summary>
    /// USB capability.
    /// </summary>
    USB,

    /// <summary>
    /// USB-C capability.
    /// </summary>
    USBC,

    /// <summary>
    /// Ultra-wideband positioning capability.
    /// </summary>
    UWB,

    /// <summary>
    /// Display capability.
    /// </summary>
    Display,

    /// <summary>
    /// Multiple display capability.
    /// </summary>
    MultipleDisplays,

    /// <summary>
    /// Haptic feedback capability.
    /// </summary>
    HapticFeedback,

    /// <summary>
    /// Animation capability.
    /// </summary>
    Animation,

    /// <summary>
    /// Overlay capability.
    /// </summary>
    Overlay,

    /// <summary>
    /// Notification capability.
    /// </summary>
    Notification,

    /// <summary>
    /// Logging capability.
    /// </summary>
    Logging,

    /// <summary>
    /// Encryption capability.
    /// </summary>
    Encryption,

    /// <summary>
    /// Pairing capability.
    /// </summary>
    Pairing,

    /// <summary>
    /// Offline operation capability.
    /// </summary>
    OfflineMode,

    /// <summary>
    /// Cloud operation capability.
    /// </summary>
    CloudMode,

    /// <summary>
    /// Firmware update capability.
    /// </summary>
    FirmwareUpdate,

    /// <summary>
    /// Hardware node capability.
    /// </summary>
    HardwareNode,

    /// <summary>
    /// Display node capability.
    /// </summary>
    DisplayNode,

    /// <summary>
    /// Context transfer capability.
    /// </summary>
    ContextTransfer,

    /// <summary>
    /// Simulation capability.
    /// </summary>
    Simulation,

    /// <summary>
    /// Testing capability.
    /// </summary>
    Testing,

    /// <summary>
    /// Unknown capability.
    /// </summary>
    Unknown
}
