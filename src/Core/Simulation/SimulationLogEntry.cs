namespace RKWorkspace.Core.Simulation;

public sealed record SimulationLogEntry(
    int Sequence,
    DateTimeOffset Timestamp,
    string Stage,
    string Message);
