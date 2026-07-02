namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record FirstContactSnapshot
{
    public required DateTime StartedAtUtc { get; init; }

    public required DateTime ObservedAtUtc { get; init; }

    public DateTime? FirstGripAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public required bool IsGrabbed { get; init; }

    public required bool IsOverTarget { get; init; }

    public required bool IsCompleted { get; init; }

    public required string ObjectLocation { get; init; }

    public required int FailedAttempts { get; init; }

    public required int Cancellations { get; init; }

    public required int UnnecessaryClicks { get; init; }

    public long ElapsedMs => MillisecondsBetween(StartedAtUtc, CompletedAtUtc ?? ObservedAtUtc);

    public long? TimeToGripMs => FirstGripAtUtc is null
        ? null
        : MillisecondsBetween(StartedAtUtc, FirstGripAtUtc.Value);

    public long? TimeToPlaceMs => CompletedAtUtc is null
        ? null
        : MillisecondsBetween(StartedAtUtc, CompletedAtUtc.Value);

    public bool IsSuccessWithinThirtySeconds => TimeToPlaceMs is <= 30_000;

    public string Hint
    {
        get
        {
            if (IsCompleted)
            {
                return "Es liegt jetzt dort.";
            }

            if (IsGrabbed && IsOverTarget)
            {
                return "Lege es hier ab.";
            }

            if (IsGrabbed)
            {
                return "Trage es nach rechts.";
            }

            return "Nimm dieses Objekt.";
        }
    }

    public string ResultSummary
    {
        get
        {
            var grip = TimeToGripMs is null ? "-" : $"{TimeToGripMs.Value} ms";
            var place = TimeToPlaceMs is null ? "-" : $"{TimeToPlaceMs.Value} ms";
            return $"Greifen: {grip} | Ablegen: {place} | Fehlversuche: {FailedAttempts} | Abbrueche: {Cancellations} | Unnoetige Klicks: {UnnecessaryClicks}";
        }
    }

    private static long MillisecondsBetween(DateTime startUtc, DateTime endUtc)
    {
        return Math.Max(0, (long)(endUtc - startUtc).TotalMilliseconds);
    }
}
