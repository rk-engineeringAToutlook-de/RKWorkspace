namespace RKWorkspace.Protocol.Ownership;

public enum ObjectKind
{
    PdfDocument,
    Image,
    Text,
    EmailDraft,
    EmailMessage,
    SettingsWindow,
    RemoteSession,
    BrowserTab,
    ExplorerFile,
    ScreenshotRegion,
    Unknown
}

public sealed record ObjectKindRule(
    ObjectKind ObjectKind,
    OwnershipMode DefaultMode,
    IReadOnlySet<OwnershipMode> OptionalModes,
    bool GuestReceivesOriginalFile,
    bool OwnershipTransferSupported,
    string Rule)
{
    public bool AllowsOptionalMode(OwnershipMode mode) => OptionalModes.Contains(mode);
}

public static class ObjectKindRules
{
    public static ObjectKindRule For(ObjectKind kind)
    {
        return kind switch
        {
            ObjectKind.PdfDocument => new ObjectKindRule(kind, OwnershipMode.FrameOnly, Optional(OwnershipMode.InteractiveFrame, OwnershipMode.CopyOut, OwnershipMode.ForkVersion, OwnershipMode.MoveOwnership), false, true, "PDF remains owned by original ablage; guest sees rendered frames."),
            ObjectKind.EmailDraft => new ObjectKindRule(kind, OwnershipMode.InteractiveFrame, Optional(OwnershipMode.CopyOut, OwnershipMode.SnapshotExport, OwnershipMode.SessionHandoff), false, true, "Mail ownership remains with mail ablage unless adapter and policy allow export."),
            ObjectKind.EmailMessage => new ObjectKindRule(kind, OwnershipMode.InteractiveFrame, Optional(OwnershipMode.CopyOut, OwnershipMode.SnapshotExport), false, true, "Mail message is presented as frame by default."),
            ObjectKind.SettingsWindow => new ObjectKindRule(kind, OwnershipMode.InteractiveFrame, Optional(OwnershipMode.SnapshotExport), false, false, "Settings windows are not transferable."),
            ObjectKind.RemoteSession => new ObjectKindRule(kind, OwnershipMode.FrameOnly, Optional(OwnershipMode.SessionHandoff), false, true, "Remote sessions stay framed unless session handoff is explicitly supported."),
            ObjectKind.ScreenshotRegion => new ObjectKindRule(kind, OwnershipMode.SnapshotExport, Optional(OwnershipMode.FrameOnly, OwnershipMode.CopyOut), false, true, "Screenshot regions can be framed or snapshotted by policy."),
            ObjectKind.Text => new ObjectKindRule(kind, OwnershipMode.FrameOnly, Optional(OwnershipMode.ExtractOnly, OwnershipMode.CopyOut), false, true, "Text is framed or extracted only when policy permits."),
            _ => new ObjectKindRule(kind, OwnershipMode.FrameOnly, Optional(OwnershipMode.NotTransferable), false, false, "Unknown objects default to frame-only.")
        };
    }

    private static IReadOnlySet<OwnershipMode> Optional(params OwnershipMode[] modes)
    {
        return new HashSet<OwnershipMode>(modes);
    }
}
