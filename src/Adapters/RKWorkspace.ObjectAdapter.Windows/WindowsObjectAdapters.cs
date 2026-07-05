using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.ObjectAdapter.Windows;

public enum ObjectCaptureMode
{
    FileReference,
    ClipboardText,
    ClipboardImage,
    ScreenshotRegion,
    WindowSnapshot,
    AppSpecific,
    Unknown
}

public enum ObjectCaptureStatus
{
    Captured,
    Prepared,
    Unsupported,
    Failed
}

public interface IObjectSourceAdapter
{
    ObjectCaptureMode CaptureMode { get; }
}

public interface IObjectPreviewProvider
{
    string PreviewProviderRef { get; }
}

public interface IObjectFrameProvider
{
    string FrameProviderRef { get; }
}

public interface IObjectPolicyClassifier
{
    ObjectKindRule Classify(ObjectKind objectKind);
}

public sealed record ObjectCaptureResult(
    ObjectCaptureStatus Status,
    string ThingId,
    string OwnerAblageId,
    ObjectKind ObjectKind,
    ObjectCaptureMode CaptureMode,
    OriginReference OriginReference,
    IReadOnlyDictionary<string, string> Metadata,
    OwnershipMode DefaultMode,
    string PreviewProviderRef,
    string FrameProviderRef,
    bool OriginalOwned,
    bool GuestFileCreated,
    string Message)
{
    public bool Succeeded => Status is ObjectCaptureStatus.Captured or ObjectCaptureStatus.Prepared;

    public bool GuestHasNoFileIngress => !GuestFileCreated;
}

public sealed class WindowsObjectPolicyClassifier : IObjectPolicyClassifier
{
    public ObjectKindRule Classify(ObjectKind objectKind)
    {
        return ObjectKindRules.For(objectKind);
    }
}

public sealed class WindowsFileReferenceAdapter : IObjectSourceAdapter, IObjectPreviewProvider, IObjectFrameProvider
{
    private readonly IObjectPolicyClassifier classifier;

    public WindowsFileReferenceAdapter()
        : this(new WindowsObjectPolicyClassifier())
    {
    }

    public WindowsFileReferenceAdapter(IObjectPolicyClassifier classifier)
    {
        this.classifier = classifier;
    }

    public ObjectCaptureMode CaptureMode => ObjectCaptureMode.FileReference;

    public string PreviewProviderRef => "windows-file-preview";

    public string FrameProviderRef => "windows-file-frame-provider";

    public ObjectCaptureResult CaptureFileReference(string path, string ownerAblageId, DateTimeOffset capturedAt)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Failed(ObjectKind.Unknown, ownerAblageId, "File path is required.");
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            return Failed(ObjectKind.Unknown, ownerAblageId, "File does not exist.");
        }

        var extension = Path.GetExtension(fullPath);
        var objectKind = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase)
            ? ObjectKind.PdfDocument
            : ObjectKind.ExplorerFile;
        var rule = classifier.Classify(objectKind);
        var info = new FileInfo(fullPath);
        var metadata = new Dictionary<string, string>
        {
            ["file-name"] = info.Name,
            ["extension"] = extension,
            ["length"] = info.Length.ToString(),
            ["captured-at"] = capturedAt.ToString("O")
        };

        return new ObjectCaptureResult(
            ObjectCaptureStatus.Captured,
            CreateThingId(objectKind, fullPath),
            ownerAblageId,
            objectKind,
            CaptureMode,
            new OriginReference(CreateThingId(objectKind, fullPath), ownerAblageId, "WindowsFileReference", fullPath, null),
            metadata,
            rule.DefaultMode,
            PreviewProviderRef,
            FrameProviderRef,
            OriginalOwned: true,
            GuestFileCreated: false,
            objectKind == ObjectKind.PdfDocument
                ? "PDF file reference captured as original-owned frame source."
                : "File reference captured as original-owned object source.");
    }

    private ObjectCaptureResult Failed(ObjectKind objectKind, string ownerAblageId, string message)
    {
        return new ObjectCaptureResult(
            ObjectCaptureStatus.Failed,
            "thing-invalid",
            ownerAblageId,
            objectKind,
            CaptureMode,
            new OriginReference("thing-invalid", ownerAblageId, "WindowsFileReference", null, null),
            new Dictionary<string, string>(),
            OwnershipMode.NotTransferable,
            PreviewProviderRef,
            FrameProviderRef,
            OriginalOwned: true,
            GuestFileCreated: false,
            message);
    }

    private static string CreateThingId(ObjectKind objectKind, string source)
    {
        return $"{objectKind.ToString().ToLowerInvariant()}-{Math.Abs(source.GetHashCode()):x}";
    }
}

public sealed class WindowsClipboardTextAdapter : IObjectSourceAdapter, IObjectPreviewProvider, IObjectFrameProvider
{
    public ObjectCaptureMode CaptureMode => ObjectCaptureMode.ClipboardText;

    public string PreviewProviderRef => "windows-clipboard-text-preview";

    public string FrameProviderRef => "windows-clipboard-text-frame-provider";

    public ObjectCaptureResult CaptureText(string text, string ownerAblageId, DateTimeOffset capturedAt)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ObjectCaptureResult(
                ObjectCaptureStatus.Prepared,
                "clipboard-text-empty",
                ownerAblageId,
                ObjectKind.Text,
                CaptureMode,
                new OriginReference("clipboard-text-empty", ownerAblageId, "WindowsClipboardText", "Clipboard", null),
                new Dictionary<string, string> { ["stub"] = "clipboard-access-not-required-in-test" },
                OwnershipMode.FrameOnly,
                PreviewProviderRef,
                FrameProviderRef,
                OriginalOwned: true,
                GuestFileCreated: false,
                "Clipboard text adapter prepared; no OS clipboard read required in tests.");
        }

        var thingId = $"clipboard-text-{Math.Abs(text.GetHashCode()):x}";
        return new ObjectCaptureResult(
            ObjectCaptureStatus.Captured,
            thingId,
            ownerAblageId,
            ObjectKind.Text,
            CaptureMode,
            new OriginReference(thingId, ownerAblageId, "WindowsClipboardText", "Clipboard", null),
            new Dictionary<string, string>
            {
                ["text-length"] = text.Length.ToString(),
                ["captured-at"] = capturedAt.ToString("O")
            },
            OwnershipMode.FrameOnly,
            PreviewProviderRef,
            FrameProviderRef,
            OriginalOwned: true,
            GuestFileCreated: false,
            "Clipboard text captured as original-owned text frame source.");
    }
}

public sealed class WindowsScreenshotRegionAdapter : IObjectSourceAdapter
{
    public ObjectCaptureMode CaptureMode => ObjectCaptureMode.ScreenshotRegion;

    public ObjectCaptureResult Prepare(string ownerAblageId, DateTimeOffset capturedAt)
    {
        return Prepared(ObjectKind.ScreenshotRegion, ownerAblageId, "WindowsScreenshotRegion", capturedAt, "Screenshot region adapter prepared; real capture is not implemented in this slice.");
    }

    private static ObjectCaptureResult Prepared(ObjectKind objectKind, string ownerAblageId, string sourceKind, DateTimeOffset capturedAt, string message)
    {
        var thingId = $"{sourceKind.ToLowerInvariant()}-prepared";
        return new ObjectCaptureResult(
            ObjectCaptureStatus.Prepared,
            thingId,
            ownerAblageId,
            objectKind,
            ObjectCaptureMode.ScreenshotRegion,
            new OriginReference(thingId, ownerAblageId, sourceKind, null, null),
            new Dictionary<string, string> { ["captured-at"] = capturedAt.ToString("O"), ["stub"] = "true" },
            ObjectKindRules.For(objectKind).DefaultMode,
            "windows-screenshot-preview",
            "windows-screenshot-frame-provider",
            OriginalOwned: true,
            GuestFileCreated: false,
            message);
    }
}

public sealed class WindowsWindowSnapshotAdapter : IObjectSourceAdapter
{
    public ObjectCaptureMode CaptureMode => ObjectCaptureMode.WindowSnapshot;

    public ObjectCaptureResult Prepare(string ownerAblageId, DateTimeOffset capturedAt)
    {
        var thingId = "window-snapshot-prepared";
        return new ObjectCaptureResult(
            ObjectCaptureStatus.Prepared,
            thingId,
            ownerAblageId,
            ObjectKind.SettingsWindow,
            CaptureMode,
            new OriginReference(thingId, ownerAblageId, "WindowsWindowSnapshot", null, null),
            new Dictionary<string, string> { ["captured-at"] = capturedAt.ToString("O"), ["stub"] = "true" },
            OwnershipMode.InteractiveFrame,
            "windows-window-snapshot-preview",
            "windows-window-snapshot-frame-provider",
            OriginalOwned: true,
            GuestFileCreated: false,
            "Window snapshot adapter prepared; SettingsWindow remains not transferable.");
    }
}
