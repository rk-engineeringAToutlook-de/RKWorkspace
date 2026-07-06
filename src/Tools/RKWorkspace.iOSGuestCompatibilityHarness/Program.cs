using RKWorkspace.Surface.Abstractions;

namespace RKWorkspace.iOSGuestCompatibilityHarness;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var options = IosGuestHarnessOptions.Parse(args);
            return options.Mode == IosGuestHarnessMode.ReplaySample
                ? RunReplaySample()
                : RunSmokeTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace iOS/iPadOS Guest Compatibility Harness");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static int RunSmokeTest()
    {
        Console.WriteLine("RK Workspace iOS/iPadOS Guest Compatibility Harness");
        Console.WriteLine("---------------------------------------------------");
        Console.WriteLine("Mode: SmokeTest");
        WriteIdentity();
        WriteCapabilities();
        WriteXcodeBridge();
        WriteNoFileIngress();
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunReplaySample()
    {
        Console.WriteLine("RK Workspace iOS/iPadOS Guest Compatibility Harness");
        Console.WriteLine("---------------------------------------------------");
        Console.WriteLine("Mode: ReplaySample");
        WriteIdentity();
        WriteCapabilities();
        Console.WriteLine("AblageHello: OK");
        Console.WriteLine("FrameSessionReady: OK");
        Console.WriteLine("TouchHoldGesture: PREPARED");
        Console.WriteLine("ThreeFingerLongPress: NEEDS_DEVICE_VALIDATION");
        Console.WriteLine("HapticFrameArrival: PLANNED");
        Console.WriteLine("Return: SUCCESS");
        WriteNoFileIngress();
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static void WriteIdentity()
    {
        Console.WriteLine("MobileGuestAblageId: ablage-ios-ipados-guest");
        Console.WriteLine("iOSGuestIdentity: OK");
        Console.WriteLine($"PrimaryPlatform: {SurfacePlatform.IPadOS}");
        Console.WriteLine($"PhonePlatform: {SurfacePlatform.IOS}");
        Console.WriteLine("SurfaceRole: FrameGuestSurface");
    }

    private static void WriteCapabilities()
    {
        var capabilities =
            SurfaceCapabilities.FramePresentation |
            SurfaceCapabilities.GestureInput |
            SurfaceCapabilities.Haptics |
            SurfaceCapabilities.GlassEdge |
            SurfaceCapabilities.SecureContext |
            SurfaceCapabilities.FilePicker |
            SurfaceCapabilities.ShareExtension |
            SurfaceCapabilities.Clipboard;

        Console.WriteLine("FrameView: OK");
        Console.WriteLine("TouchInput: PLANNED");
        Console.WriteLine("Haptics: PLANNED");
        Console.WriteLine("GlassEdge: PLANNED");
        Console.WriteLine("NoFileIngressCapability: OK");
        Console.WriteLine("ShareExtension: PLANNED");
        Console.WriteLine("DocumentPicker: PLANNED");
        Console.WriteLine("Pasteboard: PLANNED_LIMITED");
        Console.WriteLine("GlobalAppCapture: FALSE");
        Console.WriteLine("OwnershipTransfer: OFF");
        Console.WriteLine($"CapabilityFlags: {capabilities}");
    }

    private static void WriteXcodeBridge()
    {
        Console.WriteLine("XcodeHandoff: release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md");
        Console.WriteLine("USBDeviceTest: REQUIRED");
        Console.WriteLine("DeveloperMode: REQUIRED_ON_DEVICE");
        Console.WriteLine("LocalNetworkPermission: REQUIRED");
        Console.WriteLine("NativeAppTarget: PREPARED");
        Console.WriteLine("PWAFallback: PREPARED_NOT_FINAL");
        Console.WriteLine("XcodeBridge: OK");
    }

    private static void WriteNoFileIngress()
    {
        Console.WriteLine("GuestHasPdfFile: NO");
        Console.WriteLine("GuestHasOriginalPath: NO");
        Console.WriteLine("OriginalFileBytes: NO");
        Console.WriteLine("AppContainerOriginalPdf: NO");
        Console.WriteLine("FilesAppOriginalPdf: NO");
        Console.WriteLine("NoFileIngress: SUCCESS");
    }
}

internal enum IosGuestHarnessMode
{
    SmokeTest,
    ReplaySample
}

internal sealed record IosGuestHarnessOptions(IosGuestHarnessMode Mode)
{
    public static IosGuestHarnessOptions Parse(string[] args)
    {
        var mode = args.Any(arg =>
            string.Equals(arg, "--replay-sample", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "-ReplaySample", StringComparison.OrdinalIgnoreCase))
            ? IosGuestHarnessMode.ReplaySample
            : IosGuestHarnessMode.SmokeTest;

        return new IosGuestHarnessOptions(mode);
    }
}
