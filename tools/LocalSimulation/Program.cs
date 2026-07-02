using RKWorkspace.Core.Simulation;

var simulation = new LocalTransferSimulation();
var result = simulation.RunTextTransferFromAToB();
var transfer = result.TransferObject;

Console.WriteLine("RK Workspace local simulation");
Console.WriteLine("-----------------------------");
Console.WriteLine($"Source workspace : {result.SourceWorkspace.DisplayName} ({result.SourceWorkspace.WorkspaceId})");
Console.WriteLine($"Target direction : {result.Direction}");
Console.WriteLine($"Resolved target  : {result.TargetWorkspace.DisplayName} ({result.TargetWorkspace.WorkspaceId})");
Console.WriteLine($"Object type      : {transfer.ObjectType}");
Console.WriteLine($"Object id        : {transfer.ObjectId}");
Console.WriteLine($"MIME type        : {transfer.MimeType}");
Console.WriteLine($"Size             : {transfer.Size} bytes");
Console.WriteLine($"Checksum         : {transfer.Checksum}");
Console.WriteLine($"Result           : {(result.Success ? "OK" : "FAILED")} - text transfer planned from A to B");
Console.WriteLine();
Console.WriteLine("Simulation log");
Console.WriteLine("--------------");

foreach (var entry in result.Log)
{
    Console.WriteLine($"{entry.Sequence:00} {entry.Stage,-28} {entry.Message}");
}
