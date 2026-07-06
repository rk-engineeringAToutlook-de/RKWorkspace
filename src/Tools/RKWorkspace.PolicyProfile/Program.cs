using RKWorkspace.Protocol.Ownership;

var options = PolicyProfileToolOptions.Parse(args);

try
{
    if (options.ShowHelp)
    {
        PrintHelp();
        return 0;
    }

    if (options.SmokeTest)
    {
        return RunSmokeTest();
    }

    if (options.Validate)
    {
        var validation = RkwpPolicyProfileValidator.ValidateAll(RkwpPolicyProfileStore.All);
        PrintHeader("Validate");
        PrintValidation(validation);
        Console.WriteLine($"RESULT: {(validation.IsValid ? "SUCCESS" : "FAILED")}");
        return validation.IsValid ? 0 : 1;
    }

    if (!string.IsNullOrWhiteSpace(options.Show))
    {
        if (!RkwpPolicyProfileStore.TryGet(options.Show, out var profile))
        {
            throw new ArgumentException($"Unknown policy profile: {options.Show}");
        }

        PrintHeader("Show");
        PrintProfile(profile);
        Console.WriteLine("RESULT: SUCCESS");
        return 0;
    }

    PrintHeader("List");
    foreach (var profile in RkwpPolicyProfileStore.All)
    {
        Console.WriteLine($"{profile.Name}: {profile.DisplayName} ({profile.ProfileId})");
    }

    Console.WriteLine("RESULT: SUCCESS");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine("RK Workspace Policy Profiles");
    Console.WriteLine("----------------------------");
    Console.WriteLine($"RESULT: FAILED - {ex.Message}");
    return 1;
}

static int RunSmokeTest()
{
    var critical = RkwpPolicyProfileStore.Get(RkwpPolicyProfileName.CriticalInfrastructure);
    var office = RkwpPolicyProfileStore.Get(RkwpPolicyProfileName.OfficeDefault);
    var development = RkwpPolicyProfileStore.Get(RkwpPolicyProfileName.DevelopmentLab);
    var presentation = RkwpPolicyProfileStore.Get(RkwpPolicyProfileName.PresentationOnly);
    var validation = RkwpPolicyProfileValidator.ValidateAll(RkwpPolicyProfileStore.All);

    var criticalBlocksOwnership = OwnershipTransferService.Decide(
        ObjectKind.PdfDocument,
        Request("critical", OwnershipMode.MoveOwnership, critical.Ownership),
        critical.Ownership).Decision == OwnershipTransferDecisionKind.Denied;
    var criticalSecure = critical.SecureSessionRequired && critical.EvaluateSecurityGate().SecureSessionRequired;
    var presentationBlocksInput = !presentation.Frame.InputAllowed && !presentation.Frame.AllowPointer && !presentation.Frame.AllowKeyboard;
    var developmentAllowsDev = development.DevelopmentModeAllowed &&
        development.EvaluateSecurityGate().Allowed &&
        development.EvaluateSecurityGate().Warnings.Count > 0;
    var officeCopyOutNeedsConfirmation = OwnershipTransferService.Decide(
        ObjectKind.PdfDocument,
        Request("office-copyout", OwnershipMode.CopyOut, office.Ownership),
        office.Ownership).Decision == OwnershipTransferDecisionKind.RequiresUserConfirmation;

    var success = validation.IsValid &&
        criticalBlocksOwnership &&
        criticalSecure &&
        presentationBlocksInput &&
        developmentAllowsDev &&
        officeCopyOutNeedsConfirmation;

    PrintHeader("SmokeTest");
    Console.WriteLine($"Profiles: {RkwpPolicyProfileStore.All.Count}");
    Console.WriteLine($"CriticalInfrastructureBlocksOwnershipTransfer: {(criticalBlocksOwnership ? "OK" : "FAILED")}");
    Console.WriteLine($"CriticalInfrastructureRequiresSecureSession: {(criticalSecure ? "OK" : "FAILED")}");
    Console.WriteLine($"PresentationOnlyBlocksInput: {(presentationBlocksInput ? "OK" : "FAILED")}");
    Console.WriteLine($"DevelopmentLabAllowsDevMode: {(developmentAllowsDev ? "OK" : "FAILED")}");
    Console.WriteLine($"OfficeDefaultCopyOutRequiresConfirmation: {(officeCopyOutNeedsConfirmation ? "OK" : "FAILED")}");
    Console.WriteLine($"PolicyProfileValidate: {(validation.IsValid ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"RESULT: {(success ? "SUCCESS" : "FAILED")}");
    return success ? 0 : 1;
}

static OwnershipTransferRequest Request(string id, OwnershipMode mode, OwnershipPolicy policy)
{
    return new OwnershipTransferRequest(
        $"request-{id}",
        "thing-policy-smoke",
        "ablage-owner",
        "ablage-guest",
        mode,
        DateTimeOffset.UtcNow,
        PolicyId: policy.PolicyId);
}

static void PrintHelp()
{
    Console.WriteLine("RK Workspace Policy Profiles");
    Console.WriteLine("----------------------------");
    Console.WriteLine("Usage:");
    Console.WriteLine("  run-policy-profile.ps1 -List");
    Console.WriteLine("  run-policy-profile.ps1 -Show CriticalInfrastructure");
    Console.WriteLine("  run-policy-profile.ps1 -Validate");
    Console.WriteLine("  run-policy-profile.ps1 -SmokeTest");
}

static void PrintHeader(string mode)
{
    Console.WriteLine("RK Workspace Policy Profiles");
    Console.WriteLine("----------------------------");
    Console.WriteLine($"Mode: {mode}");
}

static void PrintProfile(RkwpPolicyProfile profile)
{
    Console.WriteLine($"Name: {profile.Name}");
    Console.WriteLine($"ProfileId: {profile.ProfileId}");
    Console.WriteLine($"DisplayName: {profile.DisplayName}");
    Console.WriteLine($"Description: {profile.Description}");
    Console.WriteLine($"DefaultMode: {profile.Ownership.DefaultMode}");
    Console.WriteLine($"NoFileIngress: {profile.NoFileIngress}");
    Console.WriteLine($"OwnershipTransferAllowed: {profile.OwnershipTransferAllowed}");
    Console.WriteLine($"InputAllowed: {profile.InputAllowed}");
    Console.WriteLine($"SecureSessionRequired: {profile.SecureSessionRequired}");
    Console.WriteLine($"AuditRequired: {profile.AuditRequired}");
    Console.WriteLine($"DevelopmentModeAllowed: {profile.DevelopmentModeAllowed}");
    Console.WriteLine($"SimulatedProximityAllowed: {profile.SimulatedProximityAllowed}");
    Console.WriteLine($"LeaseTimeoutSeconds: {profile.LeaseTimeout.TotalSeconds:0}");
}

static void PrintValidation(RkwpPolicyProfileValidationResult validation)
{
    Console.WriteLine($"PolicyProfileValidate: {(validation.IsValid ? "SUCCESS" : "FAILED")}");
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}

public sealed record PolicyProfileToolOptions(
    bool List,
    string? Show,
    bool Validate,
    bool SmokeTest,
    bool ShowHelp)
{
    public static PolicyProfileToolOptions Parse(string[] args)
    {
        var list = false;
        string? show = null;
        var validate = false;
        var smokeTest = false;
        var showHelp = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (Matches(arg, "--list", "-List"))
            {
                list = true;
                continue;
            }

            if (Matches(arg, "--show", "-Show") && index + 1 < args.Length)
            {
                show = args[++index];
                continue;
            }

            if (Matches(arg, "--validate", "-Validate"))
            {
                validate = true;
                continue;
            }

            if (Matches(arg, "--smoke-test", "-SmokeTest"))
            {
                smokeTest = true;
                continue;
            }

            if (Matches(arg, "--help", "-Help", "-?"))
            {
                showHelp = true;
            }
        }

        if (!list && show is null && !validate && !smokeTest && !showHelp)
        {
            list = true;
        }

        return new PolicyProfileToolOptions(list, show, validate, smokeTest, showHelp);
    }

    private static bool Matches(string value, params string[] candidates)
    {
        return candidates.Any(candidate => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));
    }
}
