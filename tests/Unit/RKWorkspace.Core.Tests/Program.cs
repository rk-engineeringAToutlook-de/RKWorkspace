using RKWorkspace.Core.Models;
using RKWorkspace.Core.Services;
using RKWorkspace.Core.Simulation;

var tests = new (string Name, Action Body)[]
{
    ("Workspace capability checks match transfer object types", WorkspaceCapabilitiesMatchObjectTypes),
    ("WorkspaceMap resolves the right-side target", WorkspaceMapResolvesRightTarget),
    ("TransferPlanner creates a text transfer from A to B", TransferPlannerCreatesTextTransfer),
    ("TransferPlanner rejects untrusted targets", TransferPlannerRejectsUntrustedTarget),
    ("DeviceIdentity creates required identity fields", DeviceIdentityCreatesRequiredFields),
    ("Local simulation logs a complete text transfer from A to B", LocalSimulationLogsCompleteTextTransfer)
};

var failed = 0;

foreach (var test in tests)
{
    try
    {
        test.Body();
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
    ? $"All {tests.Length} tests passed."
    : $"{failed} of {tests.Length} tests failed.");

return failed == 0 ? 0 : 1;

static void WorkspaceCapabilitiesMatchObjectTypes()
{
    var workspace = TestWorkspace(
        "workspace-a",
        WorkspacePosition.Center,
        WorkspaceCapability.TextTransfer | WorkspaceCapability.PdfTransfer);

    Assert.True(workspace.Supports(TransferObjectType.Text));
    Assert.True(workspace.Supports(TransferObjectType.Pdf));
    Assert.False(workspace.Supports(TransferObjectType.Image));
}

static void WorkspaceMapResolvesRightTarget()
{
    var source = TestWorkspace("workspace-a", WorkspacePosition.Center);
    var target = TestWorkspace("workspace-b", WorkspacePosition.Right);

    var map = new WorkspaceMap(new[] { source, target });

    Assert.Equal(target.WorkspaceId, map.ResolveDirectionalTarget(WorkspacePosition.Right).WorkspaceId);
    Assert.Throws<ArgumentException>(() => map.ResolveDirectionalTarget(WorkspacePosition.Center));
}

static void TransferPlannerCreatesTextTransfer()
{
    var source = TestWorkspace("workspace-a", WorkspacePosition.Center);
    var target = TestWorkspace("workspace-b", WorkspacePosition.Right);
    var planner = new TransferPlanner();

    var transfer = planner.PlanTextTransfer(
        source,
        new WorkspaceMap(new[] { source, target }),
        WorkspacePosition.Right,
        "Hallo RKWS",
        "Greeting.txt");

    Assert.Equal(TransferObjectType.Text, transfer.ObjectType);
    Assert.Equal(source.WorkspaceId, transfer.SourceWorkspaceId);
    Assert.Equal(target.WorkspaceId, transfer.TargetWorkspaceId);
    Assert.Equal("Greeting.txt", transfer.DisplayName);
    Assert.Equal("text/plain; charset=utf-8", transfer.MimeType);
    Assert.Equal(TransferStatus.TransferPending, transfer.TransferStatus);
    Assert.Equal(10, transfer.Size);
    Assert.Equal(64, transfer.Checksum.Length);
    Assert.Equal(PayloadReferenceKind.InlineText, transfer.PayloadReference.Kind);
}

static void TransferPlannerRejectsUntrustedTarget()
{
    var source = TestWorkspace("workspace-a", WorkspacePosition.Center);
    var target = TestWorkspace("workspace-b", WorkspacePosition.Right) with
    {
        TrustState = TrustState.Untrusted
    };

    var planner = new TransferPlanner();

    Assert.Throws<InvalidOperationException>(() => planner.PlanTextTransfer(
        source,
        new WorkspaceMap(new[] { source, target }),
        WorkspacePosition.Right,
        "No transfer"));
}

static void DeviceIdentityCreatesRequiredFields()
{
    var identity = DeviceIdentity.Create(
        "RK Windows Laptop",
        WorkspacePlatform.Windows,
        "sha256:test");

    Assert.StartsWith("rkws-dev-", identity.DeviceId);
    Assert.Equal("RK Windows Laptop", identity.DisplayName);
    Assert.Equal(WorkspacePlatform.Windows, identity.Platform);
    Assert.Equal("sha256:test", identity.PublicKeyFingerprint);
    Assert.Equal(6, identity.PairingId.Length);
    Assert.True(identity.CreatedAt <= identity.LastSeen);
}

static void LocalSimulationLogsCompleteTextTransfer()
{
    var simulation = new LocalTransferSimulation();

    var result = simulation.RunTextTransferFromAToB();

    Assert.True(result.Success);
    Assert.Equal("workspace-a", result.SourceWorkspace.WorkspaceId);
    Assert.Equal("workspace-b", result.TargetWorkspace.WorkspaceId);
    Assert.Equal("workspace-b", result.TransferObject.TargetWorkspaceId);
    Assert.Equal(TransferObjectType.Text, result.TransferObject.ObjectType);
    Assert.Equal(TransferStatus.TransferPending, result.TransferObject.TransferStatus);
    Assert.True(result.Log.Count >= 10);
    Assert.ContainsStage("SIMULATION_START", result.Log);
    Assert.ContainsStage("TARGET_RESOLVED", result.Log);
    Assert.ContainsStage("TRANSFER_OBJECT_CREATED", result.Log);
    Assert.ContainsStage("SIMULATION_COMPLETE", result.Log);

    for (var i = 0; i < result.Log.Count; i++)
    {
        Assert.Equal(i + 1, result.Log[i].Sequence);
    }
}

static Workspace TestWorkspace(
    string workspaceId,
    WorkspacePosition position,
    WorkspaceCapability capabilities =
        WorkspaceCapability.TextTransfer
        | WorkspaceCapability.FileTransfer
        | WorkspaceCapability.PdfTransfer
        | WorkspaceCapability.DirectionalTransfer
        | WorkspaceCapability.Pairing
        | WorkspaceCapability.Discovery)
{
    return Workspace.Create(
        workspaceId,
        workspaceId,
        $"device-{workspaceId}",
        WorkspacePlatform.Windows,
        capabilities,
        position,
        TrustState.Trusted);
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

    public static void StartsWith(string expectedPrefix, string actual)
    {
        if (!actual.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected prefix {expectedPrefix}, got {actual}.");
        }
    }

    public static void Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Expected exception {typeof(TException).Name}.");
    }

    public static void ContainsStage(string stage, IEnumerable<SimulationLogEntry> log)
    {
        if (!log.Any(entry => entry.Stage == stage))
        {
            throw new InvalidOperationException($"Expected log stage {stage}.");
        }
    }
}
