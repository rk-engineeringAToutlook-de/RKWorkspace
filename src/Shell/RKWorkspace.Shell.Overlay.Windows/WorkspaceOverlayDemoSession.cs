using RKWorkspace.Shell;

namespace RKWorkspace.Shell.Overlay.Windows;

public sealed class WorkspaceOverlayDemoSession
{
    private readonly List<string> _events = new();

    public WorkspaceOverlayState State { get; private set; } = WorkspaceOverlayState.Inactive;

    public WorkspaceCarryState CarryState => WorkspaceOverlayStateMapper.ToCarryState(State);

    public string ActiveAblage { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string ThingLabel { get; } = "Digitales Ding";

    public string ThingContent { get; } = "Rechnung.pdf";

    public IReadOnlyList<string> Events => _events;

    public void Start()
    {
        State = WorkspaceOverlayState.Listening;
        Message = "Ding nehmen";
        ActiveAblage = string.Empty;
        _events.Add("Overlay gestartet");
    }

    public void MarkCandidate()
    {
        State = WorkspaceOverlayState.CarryCandidate;
        Message = "Das gehoert zu meiner Arbeit";
        _events.Add("Arbeitsobjekt erkannt");
    }

    public void Pick()
    {
        State = WorkspaceOverlayState.Picked;
        Message = "Gegriffen";
        ActiveAblage = string.Empty;
        _events.Add("Ding gegriffen");
    }

    public void Carry()
    {
        State = WorkspaceOverlayState.Carried;
        Message = "Weitertragen";
        ActiveAblage = string.Empty;
        _events.Add("Ding wird getragen");
    }

    public void NearAblage(string ablage)
    {
        State = WorkspaceOverlayState.NearAblage;
        ActiveAblage = ablage;
        Message = "Hier ablegen";
        _events.Add($"Ablage {ablage} reagiert");
    }

    public void Place()
    {
        State = WorkspaceOverlayState.Placed;
        Message = "Abgelegt";
        _events.Add("Ding abgelegt");
    }

    public void Cancel()
    {
        State = WorkspaceOverlayState.Cancelled;
        Message = "Zurueck";
        ActiveAblage = string.Empty;
        _events.Add("Ding zurueckgelegt");
    }

    public void Fail(string reason)
    {
        State = WorkspaceOverlayState.Failed;
        Message = reason;
        ActiveAblage = string.Empty;
        _events.Add($"Overlay Fehler: {reason}");
    }

    public bool SmokeCheck()
    {
        Start();
        var startSuccess = State == WorkspaceOverlayState.Listening &&
            CarryState == WorkspaceCarryState.Empty;
        MarkCandidate();
        var candidateSuccess = State == WorkspaceOverlayState.CarryCandidate &&
            CarryState == WorkspaceCarryState.Candidate;
        Pick();
        var pickSuccess = State == WorkspaceOverlayState.Picked &&
            CarryState == WorkspaceCarryState.Picked;
        Carry();
        var carrySuccess = State == WorkspaceOverlayState.Carried &&
            CarryState == WorkspaceCarryState.Carried;
        NearAblage("rechts");
        var nearSuccess = State == WorkspaceOverlayState.NearAblage &&
            CarryState == WorkspaceCarryState.NearSurface &&
            string.Equals(ActiveAblage, "rechts", StringComparison.Ordinal);
        Place();
        var placedSuccess = State == WorkspaceOverlayState.Placed &&
            CarryState == WorkspaceCarryState.Placed &&
            string.Equals(Message, "Abgelegt", StringComparison.Ordinal);

        return startSuccess &&
            candidateSuccess &&
            pickSuccess &&
            carrySuccess &&
            nearSuccess &&
            placedSuccess;
    }
}
