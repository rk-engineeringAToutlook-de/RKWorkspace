namespace RKWorkspace.DualAgentHarness;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var result = new DualAgentSimulationHarness().Run();
            PrintResult(result);
            return result.TransferSuccess ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace Dual Agent Harness");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static void PrintResult(DualAgentSimulationResult result)
    {
        Console.WriteLine("RK Workspace Dual Agent Harness");
        Console.WriteLine("--------------------------------");
        Console.WriteLine($"Agent A: {result.AgentARunning.AgentId} Runtime={result.AgentARunning.RuntimeState} Workspace={result.AgentARunning.WorkspaceName} WorkspaceId={result.AgentARunning.WorkspaceId}");
        Console.WriteLine($"Agent B: {result.AgentBRunning.AgentId} Runtime={result.AgentBRunning.RuntimeState} Workspace={result.AgentBRunning.WorkspaceName} WorkspaceId={result.AgentBRunning.WorkspaceId}");
        Console.WriteLine("Parallel Start: OK");
        Console.WriteLine("Distinct AgentIds: OK");
        Console.WriteLine("Distinct WorkspaceIds: OK");
        Console.WriteLine($"Transfer Request: {result.RequestId}");
        Console.WriteLine($"Transfer Object: {result.TransferObjectId}");
        Console.WriteLine($"Transfer Result: {(result.TransferSuccess ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"Source: {result.SourceWorkspaceId}");
        Console.WriteLine($"Target: {result.TargetWorkspaceId}");
        Console.WriteLine($"FinalState: {result.FinalState}");
        Console.WriteLine($"TransferObjectCount: AgentA={result.AgentATransferObjectCount} AgentB={result.AgentBTransferObjectCount} Total={result.AgentATransferObjectCount + result.AgentBTransferObjectCount}");
        Console.WriteLine($"Runtime Stability: AgentA={result.AgentARuntimeAfterTransfer} AgentB={result.AgentBRuntimeAfterTransfer}");
        Console.WriteLine($"History: {string.Join(" -> ", result.History)}");
        Console.WriteLine($"Parallel Stop: AgentA={result.AgentAStopped.State} AgentB={result.AgentBStopped.State}");
        Console.WriteLine("Checks:");
        foreach (var check in result.Checks)
        {
            Console.WriteLine($"[OK] {check}");
        }

        Console.WriteLine(result.TransferSuccess ? "RESULT: SUCCESS" : "RESULT: FAILED");
    }
}
