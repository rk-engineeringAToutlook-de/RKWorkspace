using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        if (string.Equals(
            Environment.GetEnvironmentVariable("RKWS_STUDIO_SMOKE_TEST"),
            "1",
            StringComparison.Ordinal))
        {
            return RunSmokeTest();
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
        return 0;
    }

    private static int RunSmokeTest()
    {
        var viewModel = new StudioViewModel();
        var success = viewModel.RunFullInteractiveDemo();
        var diagnostics = viewModel.Diagnostics;
        var interactive = viewModel.InteractiveWorkspace;
        var history = viewModel.TransferHistory.ToArray();
        var interactiveSuccess = success &&
            string.Equals(diagnostics.LastResult, "SUCCESS", StringComparison.Ordinal) &&
            string.Equals(interactive.ObjectLocation, "Workspace B", StringComparison.Ordinal) &&
            history.Any(entry => string.Equals(entry.Action, "State:Completed", StringComparison.Ordinal));

        Console.WriteLine("RK Workspace Developer Studio Smoke Test");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"RuntimeState: {diagnostics.RuntimeState}");
        Console.WriteLine($"PluginCount: {diagnostics.PluginCount}");
        Console.WriteLine($"WorkspaceCount: {diagnostics.WorkspaceCount}");
        Console.WriteLine($"TransferObjectCount: {diagnostics.TransferObjectCount}");
        Console.WriteLine($"AgentCount: {viewModel.Agents.Count}");
        Console.WriteLine($"InteractiveObjectLocation: {interactive.ObjectLocation}");
        Console.WriteLine($"InteractiveObjectState: {interactive.ObjectState}");
        Console.WriteLine($"InteractiveHistory: {string.Join(" -> ", history.Select(entry => entry.Action))}");
        foreach (var agent in viewModel.Agents)
        {
            Console.WriteLine($"Agent: {agent.AgentId} Runtime={agent.Runtime} Workspace={agent.Workspace} Status={agent.Status}");
        }

        Console.WriteLine($"LastResult: {diagnostics.LastResult}");
        Console.WriteLine($"LastError: {diagnostics.LastError}");
        Console.WriteLine(interactiveSuccess ? "InteractiveDemo: SUCCESS" : "InteractiveDemo: FAILED");
        Console.WriteLine(interactiveSuccess ? "RESULT: SUCCESS" : "RESULT: FAILED");

        viewModel.StopDualAgents();

        return interactiveSuccess ? 0 : 1;
    }
}
