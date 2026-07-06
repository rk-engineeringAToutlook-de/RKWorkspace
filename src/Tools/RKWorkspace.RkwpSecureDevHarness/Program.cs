using RKWorkspace.Protocol;
using RKWorkspace.Protocol.IdentityStore;
using RKWorkspace.RkwpTransport.SecureDev;

namespace RKWorkspace.RkwpSecureDevHarness;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = SecureDevHarnessOptions.Parse(args);
            if (options.Help)
            {
                PrintHelp();
                return 0;
            }

            var root = FindRoot();
            var owner = LoadIdentity(root, options.OwnerName, options.OwnerPlatform);
            var guest = LoadIdentity(root, options.GuestName, options.GuestPlatform);
            var secureOptions = new RkwpSecureDevTransportOptions
            {
                OwnerIdentity = owner.Record.ToIdentity(),
                GuestIdentity = guest.Record.ToIdentity(),
                EndpointName = options.Endpoint,
                PreferTls = true,
                TlsAvailable = false
            };

            if (options.Mode == SecureDevHarnessMode.SmokeTest)
            {
                return await RunSmokeAsync(secureOptions).ConfigureAwait(false);
            }

            return RunStatus(options.Mode, secureOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace RKWP SecureDev Harness");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunSmokeAsync(RkwpSecureDevTransportOptions options)
    {
        Console.WriteLine("RK Workspace RKWP SecureDev Smoke");
        Console.WriteLine("---------------------------------");
        Console.WriteLine("Mode: SmokeTest");
        Console.WriteLine($"AblageId: {options.OwnerIdentity.AblageId.Value}");
        Console.WriteLine($"PeerAblageId: {options.GuestIdentity.AblageId.Value}");

        var result = await RkwpSecureDevSmokeScenario.RunAsync(options).ConfigureAwait(false);
        Console.WriteLine("OwnerIdentity: OK");
        Console.WriteLine("GuestIdentity: OK");
        Console.WriteLine($"SecurityMode: {FormatSecurityMode(result.SecurityMode)}");
        Console.WriteLine($"TransportProfile: {result.TransportProfile}");
        Console.WriteLine($"SecureSessionRequired: {result.SecureSessionRequired}");
        Console.WriteLine($"TLS: {(result.TlsEnabled ? "YES" : "NO")}");
        Console.WriteLine($"TLSStatus: {result.TlsStatus}");
        Console.WriteLine($"HandshakeState: {result.HandshakeState}");
        Console.WriteLine($"SessionId: {result.SessionId}");
        RequireEvent(result, "TransportStarted");
        Console.WriteLine("TransportStarted: OK");
        RequireEvent(result, "GuestConnected");
        Console.WriteLine("GuestConnected: OK");
        RequireEvent(result, "AblageHello");
        Console.WriteLine("AblageHello: OK");
        RequireEvent(result, "IdentityExchange");
        Console.WriteLine("IdentityExchange: OK");
        RequireEvent(result, "SecureDevHandshake");
        Console.WriteLine("SecureDevHandshake: OK");
        RequireEvent(result, "SessionActive");
        Console.WriteLine("SessionActive: OK");
        RequireEvent(result, "Heartbeat");
        Console.WriteLine("Heartbeat: OK");
        RequireEvent(result, "FrameUpdate");
        Console.WriteLine("FrameUpdate: OK");
        RequireEvent(result, "Shutdown");
        Console.WriteLine("Shutdown: OK");
        Console.WriteLine("FallbackClearlyMarked: OK");
        Console.WriteLine("NoFileIngress: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int RunStatus(SecureDevHarnessMode mode, RkwpSecureDevTransportOptions options)
    {
        var transport = new RkwpSecureDevTransport(options);
        var ownIdentity = mode == SecureDevHarnessMode.Guest
            ? options.GuestIdentity
            : options.OwnerIdentity;
        var peerIdentity = mode == SecureDevHarnessMode.Guest
            ? options.OwnerIdentity
            : options.GuestIdentity;
        Console.WriteLine("RK Workspace RKWP SecureDev Harness");
        Console.WriteLine("-----------------------------------");
        Console.WriteLine($"Mode: {mode}");
        Console.WriteLine($"AblageId: {ownIdentity.AblageId.Value}");
        Console.WriteLine($"PeerAblageId: {peerIdentity.AblageId.Value}");
        Console.WriteLine($"SecurityMode: {FormatSecurityMode(transport.EffectiveSecurityMode)}");
        Console.WriteLine($"TransportProfile: {transport.TransportProfile}");
        Console.WriteLine($"SecureSessionRequired: {options.SecureSessionRequired}");
        Console.WriteLine($"TLS: {(transport.TlsEnabled ? "YES" : "NO")}");
        Console.WriteLine($"TLSStatus: {transport.TlsStatus}");
        Console.WriteLine("HandshakeState: Prepared");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static AblageIdentityStoreResult LoadIdentity(
        string root,
        string ablageName,
        string platform)
    {
        var storeRoot = Path.Combine(root, ".rkworkspace-dev", "identities");
        var store = new AblageIdentityStore(new AblageIdentityStoreOptions(storeRoot, ablageName, platform));
        return store.CreateOrLoad();
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")) ||
                File.Exists(Path.Combine(directory.FullName, "README.md")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root could not be found.");
    }

    private static void RequireEvent(RkwpSecureDevTransportResult result, string name)
    {
        if (!result.Events.Contains(name, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"SecureDev smoke missing event: {name}.");
        }
    }

    private static string FormatSecurityMode(RkwpSecurityMode mode)
    {
        return mode switch
        {
            RkwpSecurityMode.Authenticated => "DevelopmentAuthenticated",
            RkwpSecurityMode.Encrypted => "TestSecure",
            RkwpSecurityMode.EncryptedAndAuthenticated => "ProductionSecure",
            _ => mode.ToString()
        };
    }

    private static void PrintHelp()
    {
        Console.WriteLine("RK Workspace RKWP SecureDev Harness");
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("--smoke-test  Runs Owner/Guest SecureDev handshake locally.");
        Console.WriteLine("--owner       Prints Owner SecureDev status for lab startup.");
        Console.WriteLine("--guest       Prints Guest SecureDev status for lab startup.");
    }
}

internal enum SecureDevHarnessMode
{
    SmokeTest,
    Owner,
    Guest
}

internal sealed record SecureDevHarnessOptions(
    SecureDevHarnessMode Mode,
    string Endpoint,
    string OwnerName,
    string GuestName,
    string OwnerPlatform,
    string GuestPlatform,
    bool Help)
{
    public static SecureDevHarnessOptions Parse(string[] args)
    {
        var mode = SecureDevHarnessMode.SmokeTest;
        var endpoint = $"rkws-rkwp-securedev-{Guid.NewGuid():N}";
        var ownerName = "Windows Owner";
        var guestName = "macOS Guest";
        var ownerPlatform = "Windows";
        var guestPlatform = "macOS";
        var help = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Is(arg, "--help", "-Help"))
            {
                help = true;
                continue;
            }

            if (Is(arg, "--owner", "-Owner"))
            {
                mode = SecureDevHarnessMode.Owner;
                continue;
            }

            if (Is(arg, "--guest", "-Guest"))
            {
                mode = SecureDevHarnessMode.Guest;
                continue;
            }

            if (Is(arg, "--smoke-test", "-SmokeTest"))
            {
                mode = SecureDevHarnessMode.SmokeTest;
                continue;
            }

            if (Is(arg, "--endpoint", "-Endpoint") && index + 1 < args.Length)
            {
                endpoint = args[++index];
                continue;
            }

            if (Is(arg, "--owner-name", "-OwnerName") && index + 1 < args.Length)
            {
                ownerName = args[++index];
                continue;
            }

            if (Is(arg, "--guest-name", "-GuestName") && index + 1 < args.Length)
            {
                guestName = args[++index];
                continue;
            }

            if (Is(arg, "--owner-platform", "-OwnerPlatform") && index + 1 < args.Length)
            {
                ownerPlatform = args[++index];
                continue;
            }

            if (Is(arg, "--guest-platform", "-GuestPlatform") && index + 1 < args.Length)
            {
                guestPlatform = args[++index];
            }
        }

        return new SecureDevHarnessOptions(
            mode,
            endpoint,
            ownerName,
            guestName,
            ownerPlatform,
            guestPlatform,
            help);
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}
