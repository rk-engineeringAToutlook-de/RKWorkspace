namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Groups capabilities by their architectural responsibility.
/// </summary>
public enum CapabilityCategory
{
    /// <summary>
    /// Input-related capabilities such as touch, mouse or keyboard.
    /// </summary>
    Input,

    /// <summary>
    /// Display-related capabilities.
    /// </summary>
    Display,

    /// <summary>
    /// Communication-related capabilities.
    /// </summary>
    Communication,

    /// <summary>
    /// Transfer-related capabilities.
    /// </summary>
    Transfer,

    /// <summary>
    /// Security-related capabilities.
    /// </summary>
    Security,

    /// <summary>
    /// Hardware-related capabilities.
    /// </summary>
    Hardware,

    /// <summary>
    /// Firmware-related capabilities.
    /// </summary>
    Firmware,

    /// <summary>
    /// Platform-adapter capabilities.
    /// </summary>
    Platform,

    /// <summary>
    /// User-experience capabilities.
    /// </summary>
    UserExperience,

    /// <summary>
    /// System capabilities such as logging, simulation or testing.
    /// </summary>
    System,

    /// <summary>
    /// Unknown or not yet classified capabilities.
    /// </summary>
    Unknown
}
