using RKWorkspace.ObjectAdapter.Windows;
using RKWorkspace.Protocol.Ownership;

var root = FindRoot();
var samplePdf = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
var ownerAblageId = "ablage-windows-owner";
var now = DateTimeOffset.UtcNow;

var explorer = new WindowsExplorerSelectionAdapter().CaptureSelectedPath(samplePdf, ownerAblageId, now);
var clipboardText = new WindowsClipboardTextAdapter().CaptureText("RK Workspace Clipboard Text", ownerAblageId, now);
var clipboardImage = new WindowsClipboardImageAdapter().CaptureImageStub(ownerAblageId, now, width: 640, height: 360);
var screenshot = new WindowsScreenshotRegionAdapter().CaptureRegion(
    new WindowsCaptureRegion(10, 20, 320, 180),
    ownerAblageId,
    OwnershipTransferPolicy(RkwpAllowedAction.SnapshotExport),
    now);
var window = new WindowsWindowSnapshotAdapter().PrepareWindow(
    ownerAblageId,
    "RK Workspace Einstellungen",
    new WindowsCaptureRegion(0, 0, 1024, 768),
    isSettingsWindow: true,
    now);
var remote = new WindowsRemoteSessionAdapter().PrepareSession("Remote Lab Session", ownerAblageId, now);

var checks = new Dictionary<string, bool>
{
    ["ExplorerSelection"] = explorer.Succeeded && explorer.ObjectKind == ObjectKind.PdfDocument && explorer.GuestHasNoFileIngress,
    ["ClipboardText"] = clipboardText.Succeeded && clipboardText.ObjectKind == ObjectKind.Text && clipboardText.DefaultMode == OwnershipMode.FrameOnly,
    ["ClipboardImage"] = clipboardImage.Succeeded && clipboardImage.ObjectKind == ObjectKind.Image && !clipboardImage.GuestFileCreated,
    ["ScreenshotRegion"] = screenshot.Succeeded && screenshot.ObjectKind == ObjectKind.ScreenshotRegion && screenshot.DefaultMode == OwnershipMode.SnapshotExport,
    ["WindowSnapshot"] = window.Succeeded && window.ObjectKind == ObjectKind.SettingsWindow && !window.GuestFileCreated,
    ["RemoteSession"] = remote.Succeeded && remote.ObjectKind == ObjectKind.RemoteSession && remote.DefaultMode == OwnershipMode.FrameOnly,
    ["NoFileIngress"] = new[] { explorer, clipboardText, clipboardImage, screenshot, window, remote }.All(result => result.GuestHasNoFileIngress),
    ["NoAutoOwnershipTransfer"] = new[] { explorer, clipboardText, clipboardImage, screenshot, window, remote }.All(result => result.OriginalOwned)
};

Console.WriteLine("RK Workspace Windows Object Adapter Smoke");
Console.WriteLine("-----------------------------------------");
Console.WriteLine($"SamplePdf: {(File.Exists(samplePdf) ? "OK" : "FAILED")}");
Console.WriteLine($"ExplorerSelection: {(checks["ExplorerSelection"] ? "OK" : "FAILED")}");
Console.WriteLine($"ExplorerObjectKind: {explorer.ObjectKind}");
Console.WriteLine($"ClipboardText: {(checks["ClipboardText"] ? "OK" : "FAILED")}");
Console.WriteLine($"ClipboardImage: {(checks["ClipboardImage"] ? "OK" : "FAILED")}");
Console.WriteLine($"ScreenshotRegion: {(checks["ScreenshotRegion"] ? "OK" : "FAILED")}");
Console.WriteLine($"WindowSnapshot: {(checks["WindowSnapshot"] ? "OK" : "FAILED")}");
Console.WriteLine($"RemoteSession: {(checks["RemoteSession"] ? "OK" : "FAILED")}");
Console.WriteLine($"NoFileIngress: {(checks["NoFileIngress"] ? "SUCCESS" : "FAILED")}");
Console.WriteLine($"NoAutoOwnershipTransfer: {(checks["NoAutoOwnershipTransfer"] ? "SUCCESS" : "FAILED")}");
Console.WriteLine($"RESULT: {(checks.Values.All(value => value) ? "SUCCESS" : "FAILED")}");

return checks.Values.All(value => value) ? 0 : 1;

static OwnershipPolicy OwnershipTransferPolicy(RkwpAllowedAction actions)
{
    return OwnershipPolicy.CriticalDefault with
    {
        PolicyId = "policy-object-adapter-smoke",
        AllowedActions = actions,
        OwnershipTransferAllowed = true
    };
}

static string FindRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        throw new InvalidOperationException("Repository root could not be located.");
    }

    return directory.FullName;
}
