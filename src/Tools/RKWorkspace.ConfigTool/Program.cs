using System.Text.Json;
using RKWorkspace.Configuration;

namespace RKWorkspace.ConfigTool;

internal static class Program
{
    private static readonly string[] ProfileNames =
    [
        "CriticalInfrastructure",
        "OfficeDefault",
        "DevelopmentLab",
        "PresentationOnly",
        "TrustedPersonalDevices"
    ];

    private static int Main(string[] args)
    {
        try
        {
            var options = ConfigToolOptions.Parse(args);
            return options.Mode switch
            {
                ConfigToolMode.List => List(options),
                ConfigToolMode.Show => Show(options),
                ConfigToolMode.CreateSample => CreateSample(options),
                ConfigToolMode.CreateLocal => CreateLocal(options),
                ConfigToolMode.UseProfile => UseProfile(options),
                ConfigToolMode.Redact => Redact(options),
                ConfigToolMode.SmokeTest => SmokeTest(options),
                _ => Validate(options)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace Config Tool");
            Console.WriteLine("------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static int Validate(ConfigToolOptions options)
    {
        var configuration = RKWorkspaceConfiguration.Load(options.ConfigPath);
        var result = RKWorkspaceConfigurationValidator.Validate(configuration);
        PrintValidation(result);
        Console.WriteLine(result.IsValid ? "ConfigurationValidate: SUCCESS" : "ConfigurationValidate: FAILED");
        Console.WriteLine(result.IsValid ? "RESULT: SUCCESS" : "RESULT: FAILED");
        return result.IsValid ? 0 : 1;
    }

    private static int Show(ConfigToolOptions options)
    {
        var configuration = RKWorkspaceConfiguration.Load(options.ConfigPath);
        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        Console.WriteLine(configuration.ToJson());
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int List(ConfigToolOptions options)
    {
        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        Console.WriteLine("Mode: List");
        Console.WriteLine("Profiles:");
        foreach (var profile in ProfileNames)
        {
            Console.WriteLine($"- {profile}");
        }

        Console.WriteLine("Samples:");
        var samplesRoot = Path.Combine(FindRepoRoot(), "config", "samples");
        foreach (var sample in Directory.EnumerateFiles(samplesRoot, "*.json").OrderBy(Path.GetFileName))
        {
            Console.WriteLine($"- {Path.GetFileName(sample)}");
        }

        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int CreateSample(ConfigToolOptions options)
    {
        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        Console.WriteLine("CreateSampleMode: PRINT_ONLY");
        Console.WriteLine(new RKWorkspaceConfiguration().ToJson());
        Console.WriteLine($"SamplePath: {options.ConfigPath}");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int CreateLocal(ConfigToolOptions options)
    {
        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        Console.WriteLine("Mode: CreateLocal");
        var fullPath = Path.GetFullPath(options.ConfigPath);
        if (File.Exists(fullPath))
        {
            var existing = RKWorkspaceConfiguration.Load(fullPath);
            var existingResult = RKWorkspaceConfigurationValidator.Validate(existing);
            PrintValidation(existingResult);
            Console.WriteLine("LocalConfigExists: OK");
            Console.WriteLine(existingResult.IsValid ? "RESULT: SUCCESS" : "RESULT: FAILED");
            return existingResult.IsValid ? 0 : 1;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");
        File.WriteAllText(fullPath, new RKWorkspaceConfiguration().ToJson());
        Console.WriteLine($"LocalConfigCreated: {fullPath}");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int UseProfile(ConfigToolOptions options)
    {
        var configuration = File.Exists(options.ConfigPath)
            ? RKWorkspaceConfiguration.Load(options.ConfigPath)
            : new RKWorkspaceConfiguration();
        var updated = ApplyProfile(configuration, options.Profile ?? "DevelopmentLab");
        var validation = RKWorkspaceConfigurationValidator.Validate(updated);

        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        Console.WriteLine("Mode: UseProfile");
        Console.WriteLine($"Profile: {updated.Policy.PolicyProfile}");
        PrintValidation(validation);
        if (!validation.IsValid)
        {
            Console.WriteLine("RESULT: FAILED");
            return 1;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(options.ConfigPath)) ?? ".");
        File.WriteAllText(options.ConfigPath, updated.ToJson());
        Console.WriteLine($"ConfigUpdated: {Path.GetFullPath(options.ConfigPath)}");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int Redact(ConfigToolOptions options)
    {
        var configuration = RKWorkspaceConfiguration.Load(options.ConfigPath);
        var redacted = new
        {
            ablage = new
            {
                ablageId = RedactValue(configuration.Ablage.AblageId),
                configuration.Ablage.DisplayName,
                configuration.Ablage.Platform
            },
            rkwp = new
            {
                configuration.Rkwp.TransportProfile,
                host = RedactValue(configuration.Rkwp.Host),
                configuration.Rkwp.Port,
                configuration.Rkwp.HeartbeatIntervalSeconds
            },
            security = new
            {
                configuration.Security.SecurityMode,
                configuration.Security.SecureSessionRequired,
                configuration.Security.AuditRequired
            },
            policy = configuration.Policy,
            proximity = configuration.Proximity,
            frame = configuration.Frame,
            surface = configuration.Surface,
            gesture = configuration.Gesture
        };

        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        Console.WriteLine("Mode: Redact");
        Console.WriteLine(JsonSerializer.Serialize(redacted, Json.Options));
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static int SmokeTest(ConfigToolOptions options)
    {
        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        Console.WriteLine("Mode: SmokeTest");

        var configuration = RKWorkspaceConfiguration.Load(options.ConfigPath);
        var result = RKWorkspaceConfigurationValidator.Validate(configuration);
        Ensure(result.IsValid, "Sample config did not validate.");
        Console.WriteLine("SampleConfigLoaded: OK");
        Console.WriteLine("DevelopmentConfig: OK");
        Console.WriteLine("LocalSecretsNotRequired: OK");

        var invalid = configuration with { Ablage = configuration.Ablage with { AblageId = string.Empty } };
        var invalidResult = RKWorkspaceConfigurationValidator.Validate(invalid);
        Ensure(!invalidResult.IsValid, "Invalid config was not rejected.");
        Console.WriteLine("InvalidConfigRejected: OK");

        var samplesRoot = Path.Combine(FindRepoRoot(), "config", "samples");
        var criticalPolicy = LoadJson<PolicyConfiguration>(Path.Combine(samplesRoot, "policy-critical.sample.json"));
        var criticalResult = RKWorkspaceConfigurationValidator.ValidatePolicy(criticalPolicy);
        Ensure(criticalResult.IsValid, "Critical policy sample did not validate.");
        Ensure(!criticalPolicy.OwnershipTransferAllowed, "Critical policy must block ownership transfer.");
        Console.WriteLine("CriticalPolicy: OK");

        var rkwp = LoadJson<RkwpConfiguration>(Path.Combine(samplesRoot, "rkwp-dev.sample.json"));
        var rkwpResult = RKWorkspaceConfigurationValidator.ValidateRkwp(rkwp);
        Ensure(rkwpResult.IsValid, "RKWP dev sample did not validate.");
        Console.WriteLine("RkwpDevConfig: OK");

        var manualMapPath = Path.Combine(samplesRoot, "manual-ablage-map.sample.json");
        Ensure(File.Exists(manualMapPath), "Manual map sample is missing.");
        var manualMapJson = File.ReadAllText(manualMapPath);
        Ensure(manualMapJson.Contains("\"entries\"", StringComparison.OrdinalIgnoreCase), "Manual map sample has no entries.");
        Console.WriteLine("ManualMapConfig: OK");

        var listResult = List(options);
        Ensure(listResult == 0, "List mode failed.");
        Console.WriteLine("ListMode: OK");

        var tempRoot = Path.Combine(Path.GetTempPath(), $"RKWorkspace_ConfigTool_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        try
        {
            var localConfig = Path.Combine(tempRoot, "rkworkspace.local.json");
            var createLocalResult = CreateLocal(options with { Mode = ConfigToolMode.CreateLocal, ConfigPath = localConfig });
            Ensure(createLocalResult == 0 && File.Exists(localConfig), "CreateLocal mode failed.");
            Console.WriteLine("CreateLocalMode: OK");

            var useProfileResult = UseProfile(options with
            {
                Mode = ConfigToolMode.UseProfile,
                ConfigPath = localConfig,
                Profile = "CriticalInfrastructure"
            });
            Ensure(useProfileResult == 0, "UseProfile mode failed.");
            var profiled = RKWorkspaceConfiguration.Load(localConfig);
            Ensure(profiled.Policy.PolicyProfile == "CriticalInfrastructure", "UseProfile did not set CriticalInfrastructure.");
            Console.WriteLine("UseProfileMode: OK");

            var redactResult = Redact(options with { Mode = ConfigToolMode.Redact, ConfigPath = localConfig });
            Ensure(redactResult == 0, "Redact mode failed.");
            Console.WriteLine("RedactMode: OK");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }

        Console.WriteLine("ConfigToolSmoke: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    private static RKWorkspaceConfiguration ApplyProfile(RKWorkspaceConfiguration configuration, string profile)
    {
        if (!ProfileNames.Contains(profile))
        {
            throw new ConfigurationException($"Unsupported profile: {profile}");
        }

        return profile.Equals("CriticalInfrastructure", StringComparison.OrdinalIgnoreCase)
            ? configuration with
            {
                Security = configuration.Security with
                {
                    SecurityMode = "ProductionRequired",
                    SecureSessionRequired = true,
                    AuditRequired = true
                },
                Policy = configuration.Policy with
                {
                    PolicyProfile = "CriticalInfrastructure",
                    NoFileIngress = true,
                    OwnershipTransferAllowed = false
                },
                Frame = configuration.Frame with
                {
                    FrameCachePolicy = "MemoryOnly",
                    AllowGuestFileIngress = false
                },
                Rkwp = configuration.Rkwp with { DevPairingAllowed = false }
            }
            : configuration with
            {
                Policy = configuration.Policy with
                {
                    PolicyProfile = profile,
                    NoFileIngress = true,
                    OwnershipTransferAllowed = !profile.Equals("PresentationOnly", StringComparison.OrdinalIgnoreCase)
                }
            };
    }

    private static string RedactValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= 4 ? "***" : $"{value[..2]}***{value[^2..]}";
    }

    private static T LoadJson<T>(string path)
    {
        var value = JsonSerializer.Deserialize<T>(File.ReadAllText(path), Json.Options);
        return value ?? throw new ConfigurationException($"Sample config was empty: {path}");
    }

    private static void PrintValidation(ConfigurationValidationResult result)
    {
        Console.WriteLine("RK Workspace Config Tool");
        Console.WriteLine("------------------------");
        foreach (var warning in result.Warnings)
        {
            Console.WriteLine($"Warning: {warning}");
        }

        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Error: {error}");
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new ConfigurationException("Repository root could not be located.");
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new ConfigurationException(message);
        }
    }
}

internal enum ConfigToolMode
{
    Validate,
    List,
    Show,
    CreateSample,
    CreateLocal,
    UseProfile,
    Redact,
    SmokeTest
}

internal sealed record ConfigToolOptions(ConfigToolMode Mode, string ConfigPath, string? Profile = null)
{
    public static ConfigToolOptions Parse(IReadOnlyList<string> args)
    {
        var mode = ConfigToolMode.Validate;
        var root = FindRepoRootFromCurrent();
        var configPath = Path.Combine(root, "config", "samples", "rkworkspace.sample.json");
        string? profile = null;

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            if (Is(arg, "--list", "-List"))
            {
                mode = ConfigToolMode.List;
                continue;
            }

            if (Is(arg, "--validate", "-Validate"))
            {
                mode = ConfigToolMode.Validate;
                continue;
            }

            if (Is(arg, "--show", "-Show"))
            {
                mode = ConfigToolMode.Show;
                continue;
            }

            if (Is(arg, "--create-sample", "-CreateSample"))
            {
                mode = ConfigToolMode.CreateSample;
                continue;
            }

            if (Is(arg, "--create-local", "-CreateLocal"))
            {
                mode = ConfigToolMode.CreateLocal;
                if (configPath.EndsWith(Path.Combine("config", "samples", "rkworkspace.sample.json"), StringComparison.OrdinalIgnoreCase))
                {
                    configPath = Path.Combine(root, "config", "rkworkspace.local.json");
                }

                continue;
            }

            if (Is(arg, "--use-profile", "-UseProfile") && index + 1 < args.Count)
            {
                mode = ConfigToolMode.UseProfile;
                profile = args[++index];
                if (configPath.EndsWith(Path.Combine("config", "samples", "rkworkspace.sample.json"), StringComparison.OrdinalIgnoreCase))
                {
                    configPath = Path.Combine(root, "config", "rkworkspace.local.json");
                }

                continue;
            }

            if (Is(arg, "--redact", "-Redact"))
            {
                mode = ConfigToolMode.Redact;
                continue;
            }

            if (Is(arg, "--smoke-test", "-SmokeTest"))
            {
                mode = ConfigToolMode.SmokeTest;
                continue;
            }

            if (Is(arg, "--config", "-Config") && index + 1 < args.Count)
            {
                configPath = args[++index];
            }
        }

        return new ConfigToolOptions(mode, configPath, profile);
    }

    private static string FindRepoRootFromCurrent()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? Environment.CurrentDirectory;
    }

    private static bool Is(string value, params string[] names) =>
        names.Any(name => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
}
