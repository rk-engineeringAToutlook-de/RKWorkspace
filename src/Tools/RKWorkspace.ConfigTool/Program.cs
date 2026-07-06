using System.Text.Json;
using RKWorkspace.Configuration;

namespace RKWorkspace.ConfigTool;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var options = ConfigToolOptions.Parse(args);
            return options.Mode switch
            {
                ConfigToolMode.Show => Show(options),
                ConfigToolMode.CreateSample => CreateSample(options),
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

        Console.WriteLine("ConfigToolSmoke: SUCCESS");
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
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
    Show,
    CreateSample,
    SmokeTest
}

internal sealed record ConfigToolOptions(ConfigToolMode Mode, string ConfigPath)
{
    public static ConfigToolOptions Parse(IReadOnlyList<string> args)
    {
        var mode = ConfigToolMode.Validate;
        var configPath = Path.Combine(FindRepoRootFromCurrent(), "config", "samples", "rkworkspace.sample.json");

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
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

        return new ConfigToolOptions(mode, configPath);
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
