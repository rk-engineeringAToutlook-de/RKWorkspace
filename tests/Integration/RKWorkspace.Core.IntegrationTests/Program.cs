using RKWorkspace.Core.TransferObjects;

var runner = new CoreIntegrationScenarioRunner();

var tests = new (string Name, Func<CoreIntegrationScenarioResult> Body, Action<CoreIntegrationScenarioResult> Assert)[]
{
    (
        "CoreIntegrationScenario_TransferText_RightDirection",
        runner.TransferTextRightDirection,
        result =>
        {
            Assert.Success(result);
            Assert.Equal("workspace-b", result.SelectedTarget);
            Assert.Equal(TransferObjectState.Completed, result.FinalState);
            Assert.Contains("State:Prepared", result.HistoryActions);
            Assert.Contains("State:Completed", result.HistoryActions);
            Assert.True(result.Steps.Count >= 10);
        }),
    (
        "CoreIntegrationScenario_NoMatchingTarget_Fails",
        runner.NoMatchingTargetFails,
        result =>
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(string.Empty, result.TransferObjectId);
            Assert.Equal(string.Empty, result.SelectedTarget);
            Assert.Contains("No matching target found.", result.Steps);
            Assert.True(result.ErrorMessage.Contains("No matching target", StringComparison.Ordinal));
        }),
    (
        "CoreIntegrationScenario_OnlyOneTarget_AnyDirection",
        runner.OnlyOneTargetAnyDirection,
        result =>
        {
            Assert.Success(result);
            Assert.Equal("workspace-b", result.SelectedTarget);
            Assert.Equal(TransferObjectState.Completed, result.FinalState);
        }),
    (
        "CoreIntegrationScenario_ForbiddenCapabilityRejected",
        runner.ForbiddenCapabilityRejected,
        result =>
        {
            Assert.Success(result);
            Assert.Equal("workspace-b", result.SelectedTarget);
            Assert.False(result.SelectedTarget == "workspace-cloud");
        }),
    (
        "CoreIntegrationScenario_PriorityBreaksTie",
        runner.PriorityBreaksTie,
        result =>
        {
            Assert.Success(result);
            Assert.Equal("workspace-high", result.SelectedTarget);
        }),
    (
        "CoreIntegrationScenario_RuntimeInitializesAllCoreComponents",
        runner.RuntimeInitializesAllCoreComponents,
        result =>
        {
            Assert.Success(result);
            Assert.Contains("All core managers resolved from Runtime Engine.", result.Steps);
        }),
    (
        "CoreIntegrationScenario_RuntimeStopsCoreComponentsCleanly",
        runner.RuntimeStopsCoreComponentsCleanly,
        result =>
        {
            Assert.Success(result);
            Assert.Contains("Runtime Engine stopped.", result.Steps);
        }),
    (
        "CoreIntegrationScenario_RuntimeDiagnosticsCorrect",
        runner.RuntimeDiagnosticsCorrect,
        result =>
        {
            Assert.Success(result);
            Assert.Contains("Runtime diagnostics fixtures registered.", result.Steps);
        }),
    (
        "Core integration assembly has no platform dependencies",
        PlatformDependencies,
        result => Assert.Success(result))
};

var failed = 0;

foreach (var test in tests)
{
    try
    {
        var result = test.Body();
        test.Assert(result);
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"FAIL {test.Name}");
        Console.WriteLine($"     {ex.GetType().Name}: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine(failed == 0
    ? $"All {tests.Length} integration tests passed."
    : $"{failed} of {tests.Length} integration tests failed.");

return failed == 0 ? 0 : 1;

static CoreIntegrationScenarioResult PlatformDependencies()
{
    var forbiddenFragments = new[]
    {
        "Windows",
        "Presentation",
        "WinForms",
        "Wpf",
        "UIKit",
        "AppKit",
        "Android"
    };

    var references = typeof(CoreIntegrationScenarioRunner)
        .Assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => forbiddenFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

    return new CoreIntegrationScenarioResult
    {
        ScenarioName = "CoreIntegrationScenario_PlatformNeutrality",
        IsSuccess = references.Length == 0,
        Steps = new[] { "Integration test assembly dependencies checked." },
        ErrorMessage = references.Length == 0
            ? string.Empty
            : $"Forbidden references: {string.Join(", ", references)}"
    };
}

internal static class Assert
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
        }
    }

    public static void True(bool value)
    {
        if (!value)
        {
            throw new InvalidOperationException("Expected true, got false.");
        }
    }

    public static void False(bool value)
    {
        if (value)
        {
            throw new InvalidOperationException("Expected false, got true.");
        }
    }

    public static void Contains<T>(T expected, IEnumerable<T> values)
    {
        if (!values.Contains(expected))
        {
            throw new InvalidOperationException($"Expected collection to contain {expected}.");
        }
    }

    public static void Success(CoreIntegrationScenarioResult result)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Expected success for {result.ScenarioName}, got error: {result.ErrorMessage}");
        }
    }
}
