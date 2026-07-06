namespace RKWorkspace.Surface.Abstractions;

public enum HapticPattern
{
    Pick,
    EdgeNear,
    EdgeEnter,
    FrameArrived,
    Return,
    Denied,
    ConnectionLost
}

public enum HapticIntensity
{
    None,
    Soft,
    Medium,
    Strong
}

public sealed record HapticCapability(
    bool IsAvailable,
    bool SupportsIntensity,
    bool SupportsCustomDuration,
    HapticIntensity MaximumIntensity)
{
    public static HapticCapability None { get; } = new(false, false, false, HapticIntensity.None);

    public static HapticCapability Basic { get; } = new(true, false, false, HapticIntensity.Medium);

    public static HapticCapability Rich { get; } = new(true, true, true, HapticIntensity.Strong);
}

public sealed record HapticHint(
    HapticPattern Pattern,
    HapticIntensity Intensity,
    TimeSpan Duration,
    string Meaning)
{
    public static HapticHint Pick { get; } = new(
        HapticPattern.Pick,
        HapticIntensity.Soft,
        TimeSpan.FromMilliseconds(28),
        "Das Ding wurde genommen.");

    public static HapticHint EdgeNear { get; } = new(
        HapticPattern.EdgeNear,
        HapticIntensity.Soft,
        TimeSpan.FromMilliseconds(18),
        "Eine passende Ablage ist nahe.");

    public static HapticHint EdgeEnter { get; } = new(
        HapticPattern.EdgeEnter,
        HapticIntensity.Medium,
        TimeSpan.FromMilliseconds(34),
        "Das Ding tritt in die Kante ein.");

    public static HapticHint FrameArrived { get; } = new(
        HapticPattern.FrameArrived,
        HapticIntensity.Medium,
        TimeSpan.FromMilliseconds(40),
        "Der Frame liegt auf der anderen Ablage.");

    public static HapticHint Return { get; } = new(
        HapticPattern.Return,
        HapticIntensity.Soft,
        TimeSpan.FromMilliseconds(32),
        "Das Ding ist zurueckgegeben.");

    public static HapticHint Denied { get; } = new(
        HapticPattern.Denied,
        HapticIntensity.Strong,
        TimeSpan.FromMilliseconds(46),
        "Die Handlung ist nicht erlaubt.");

    public static HapticHint ConnectionLost { get; } = new(
        HapticPattern.ConnectionLost,
        HapticIntensity.Strong,
        TimeSpan.FromMilliseconds(54),
        "Die Verbindung ist verloren.");

    public static IReadOnlyList<HapticHint> Defaults { get; } =
    [
        Pick,
        EdgeNear,
        EdgeEnter,
        FrameArrived,
        Return,
        Denied,
        ConnectionLost
    ];
}
