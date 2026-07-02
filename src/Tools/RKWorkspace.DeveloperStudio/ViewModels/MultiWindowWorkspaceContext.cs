using System.Diagnostics;
using RKWorkspace.DeveloperStudio;
using RKWorkspace.Core.Capabilities;
using RKWorkspace.Core.Runtime;
using RKWorkspace.Core.TransferObjects;
using RKWorkspace.Core.Transfers;
using RKWorkspace.Core.Workspaces;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class MultiWindowWorkspaceContext
{
    public static readonly WorkspaceId WorkspaceAId = WorkspaceId.Create("RKWS-MultiWindow-Workspace-A");
    public static readonly WorkspaceId WorkspaceBId = WorkspaceId.Create("RKWS-MultiWindow-Workspace-B");
    private readonly List<MultiWindowLogRow> _log = new();
    private RuntimeEngine _runtime = CreateRuntime();
    private TransferEngine _transferEngine;
    private string _lastResult = "Not run";
    private string _lastError = string.Empty;
    private string _highlightWorkspaceId = string.Empty;
    private string _activeDragObjectId = string.Empty;
    private string _suggestedWorkspaceId = string.Empty;
    private string _lastStatusHint = "Bereit.";
    private string _successHint = string.Empty;
    private string _lastTransferDisplayName = string.Empty;
    private string _lastTargetWorkspaceId = string.Empty;
    private DateTimeOffset? _grabbedAt;
    private DateTimeOffset? _lastGrabbedAt;
    private DateTimeOffset? _dragStartedAt;
    private DateTimeOffset? _edgeLockedAt;
    private DateTimeOffset _successPulseUntil = DateTimeOffset.MinValue;
    private int _dragStartCount;
    private int _targetDetectedCount;
    private int _dropCount;
    private int _successfulTransfers;
    private int _failedTransfers;
    private int _returnTransferCount;
    private double _lastDragDurationMs;
    private double _lastTransferDurationMs;
    private double _lastTransitionDurationMs;
    private WorkspaceSessionCandidateStatus _candidateStatus = WorkspaceSessionCandidateStatus.None;
    private WorkspaceSessionCandidate _sessionCandidate = WorkspaceSessionCandidate.Empty;

    public MultiWindowWorkspaceContext()
    {
        _runtime.Start();
        _transferEngine = CreateTransferEngine(_runtime);
        InitializeWorkspaces();
        InitializeObjects();
        AddLog("global", "Multi Window Created", "Ready", string.Empty);
    }

    public event EventHandler? Changed;

    public WorkspaceId SourceWorkspaceId => WorkspaceAId;

    public WorkspaceId TargetWorkspaceId => WorkspaceBId;

    public bool RunFullDemo()
    {
        Reset();
        var firstObject = _runtime.TransferObjectManager.GetAll()
            .OrderBy(item => item.Metadata.DisplayName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (firstObject is null)
        {
            return false;
        }

        BeginDrag(firstObject.Id.ToString(), WorkspaceAId.ToString());
        SetTargetHighlighted(WorkspaceBId.ToString(), highlighted: true);
        var success = CompleteDrop(firstObject.Id.ToString(), WorkspaceBId.ToString());
        SetTargetHighlighted(WorkspaceBId.ToString(), highlighted: false);
        return success &&
            string.Equals(_lastResult, "SUCCESS", StringComparison.Ordinal) &&
            GetObjectLocation(firstObject.Id) == WorkspaceBId.ToString();
    }

    public bool RunRoundTripDemo()
    {
        Reset();
        var firstObject = _runtime.TransferObjectManager.GetAll()
            .OrderBy(item => item.Metadata.DisplayName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (firstObject is null)
        {
            return false;
        }

        var objectId = firstObject.Id.ToString();
        BeginDrag(objectId, WorkspaceAId.ToString());
        UpdateEdgeSuggestion(WorkspaceAId.ToString(), MultiWindowEdge.Right);
        SetTargetHighlighted(WorkspaceBId.ToString(), highlighted: true);
        var forward = CompleteDrop(objectId, WorkspaceBId.ToString());
        SetTargetHighlighted(WorkspaceBId.ToString(), highlighted: false);

        BeginDrag(objectId, WorkspaceBId.ToString());
        UpdateEdgeSuggestion(WorkspaceBId.ToString(), MultiWindowEdge.Left);
        SetTargetHighlighted(WorkspaceAId.ToString(), highlighted: true);
        var back = CompleteDrop(objectId, WorkspaceAId.ToString());
        SetTargetHighlighted(WorkspaceAId.ToString(), highlighted: false);

        return forward &&
            back &&
            string.Equals(_lastResult, "SUCCESS", StringComparison.Ordinal) &&
            GetObjectLocation(firstObject.Id) == WorkspaceAId.ToString();
    }

    public WorkspaceIllusionSmokeResult RunWorkspaceIllusionDemo()
    {
        Reset();
        var firstObject = _runtime.TransferObjectManager.GetAll()
            .OrderBy(item => item.Metadata.DisplayName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (firstObject is null)
        {
            var emptyDiagnostics = GetSnapshot(WorkspaceAId.ToString()).UxDiagnostics;
            return new WorkspaceIllusionSmokeResult
            {
                GripStateSet = false,
                EdgeCandidateCreated = false,
                EdgeLockedReached = false,
                ForwardTransferSuccess = false,
                ReturnTransferSuccess = false,
                CandidateCompleted = false,
                Diagnostics = emptyDiagnostics
            };
        }

        var objectId = firstObject.Id.ToString();
        BeginDrag(objectId, WorkspaceAId.ToString());
        var gripSnapshot = GetSnapshot(WorkspaceAId.ToString());
        var gripStateSet = gripSnapshot.IsObjectGrabbed &&
            gripSnapshot.StatusHint.Contains("Objekt gefasst", StringComparison.OrdinalIgnoreCase);

        UpdateEdgeSuggestion(WorkspaceAId.ToString(), MultiWindowEdge.Right);
        var candidateSnapshot = GetSnapshot(WorkspaceAId.ToString());
        var edgeCandidateCreated =
            candidateSnapshot.UxDiagnostics.CandidateStatus == WorkspaceSessionCandidateStatus.Candidate &&
            string.Equals(
                candidateSnapshot.UxDiagnostics.SessionCandidate.TargetWorkspaceId,
                WorkspaceBId.ToString(),
                StringComparison.Ordinal);

        SetTargetHighlighted(WorkspaceBId.ToString(), highlighted: true);
        var lockedSnapshot = GetSnapshot(WorkspaceBId.ToString());
        var edgeLockedReached = lockedSnapshot.IsEdgeLocked &&
            lockedSnapshot.UxDiagnostics.CandidateStatus == WorkspaceSessionCandidateStatus.EdgeLocked;
        var forward = CompleteDrop(objectId, WorkspaceBId.ToString());
        var forwardInTarget = forward && HasTransferredObjectInTarget();
        SetTargetHighlighted(WorkspaceBId.ToString(), highlighted: false);

        BeginDrag(objectId, WorkspaceBId.ToString());
        UpdateEdgeSuggestion(WorkspaceBId.ToString(), MultiWindowEdge.Left);
        SetTargetHighlighted(WorkspaceAId.ToString(), highlighted: true);
        var back = CompleteDrop(objectId, WorkspaceAId.ToString());
        SetTargetHighlighted(WorkspaceAId.ToString(), highlighted: false);

        var finalSnapshot = GetSnapshot(WorkspaceAId.ToString());
        var diagnostics = finalSnapshot.UxDiagnostics;
        var candidateCompleted =
            diagnostics.CandidateStatus == WorkspaceSessionCandidateStatus.Completed &&
            diagnostics.ReturnTransferCount >= 1 &&
            string.Equals(
                diagnostics.SessionCandidate.TargetWorkspaceId,
                WorkspaceAId.ToString(),
                StringComparison.Ordinal);

        return new WorkspaceIllusionSmokeResult
        {
            GripStateSet = gripStateSet,
            EdgeCandidateCreated = edgeCandidateCreated,
            EdgeLockedReached = edgeLockedReached,
            ForwardTransferSuccess = forwardInTarget,
            ReturnTransferSuccess = back && GetObjectLocation(firstObject.Id) == WorkspaceAId.ToString(),
            CandidateCompleted = candidateCompleted,
            Diagnostics = diagnostics
        };
    }

    public void Reset()
    {
        if (_runtime.GetStatus() is RuntimeState.Running or RuntimeState.Paused)
        {
            _runtime.Shutdown();
        }

        _runtime = CreateRuntime();
        _runtime.Start();
        _transferEngine = CreateTransferEngine(_runtime);
        _log.Clear();
        _lastResult = "Not run";
        _lastError = string.Empty;
        _highlightWorkspaceId = string.Empty;
        _activeDragObjectId = string.Empty;
        _suggestedWorkspaceId = string.Empty;
        _lastStatusHint = "Bereit.";
        _successHint = string.Empty;
        _lastTransferDisplayName = string.Empty;
        _lastTargetWorkspaceId = string.Empty;
        _grabbedAt = null;
        _lastGrabbedAt = null;
        _dragStartedAt = null;
        _edgeLockedAt = null;
        _successPulseUntil = DateTimeOffset.MinValue;
        _dragStartCount = 0;
        _targetDetectedCount = 0;
        _dropCount = 0;
        _successfulTransfers = 0;
        _failedTransfers = 0;
        _returnTransferCount = 0;
        _lastDragDurationMs = 0;
        _lastTransferDurationMs = 0;
        _lastTransitionDurationMs = 0;
        _candidateStatus = WorkspaceSessionCandidateStatus.None;
        _sessionCandidate = WorkspaceSessionCandidate.Empty;
        InitializeWorkspaces();
        InitializeObjects();
        AddLog("global", "Multi Window Reset", "Ready", string.Empty);
        NotifyChanged();
    }

    public MultiWindowWorkspaceSnapshot GetSnapshot(string workspaceId)
    {
        var workspace = _runtime.WorkspaceRegistry.GetWorkspace(WorkspaceId.Create(workspaceId))
            ?? throw new InvalidOperationException($"Workspace '{workspaceId}' is missing.");
        var descriptor = workspace.Descriptor;
        var allObjects = _runtime.TransferObjectManager.GetAll()
            .Select(ToObjectRow)
            .ToArray();
        var localObjects = allObjects
            .Where(item => string.Equals(item.Location, workspaceId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var localHistory = _runtime.TransferObjectManager.GetAll()
            .Where(item => string.Equals(GetObjectLocation(item.Id), workspaceId, StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => item.History)
            .OrderBy(entry => entry.Timestamp)
            .Select(entry => new StudioTransferHistoryRow
            {
                Time = entry.Timestamp.ToLocalTime().ToString("HH:mm:ss"),
                Action = entry.Action,
                Workspace = entry.Workspace,
                Description = entry.Description
            })
            .ToArray();

        var activeDragSourceWorkspaceId = GetCurrentDragSourceWorkspaceId();
        return new MultiWindowWorkspaceSnapshot
        {
            WorkspaceId = workspace.Id.ToString(),
            WorkspaceName = descriptor.DisplayName,
            WorkspaceType = descriptor.WorkspaceType.ToString(),
            WorkspacePosition = descriptor.Position.ToString(),
            WorkspaceStatus = descriptor.WorkspaceState.ToString(),
            IsDropTargetHighlighted = string.Equals(
                _highlightWorkspaceId,
                workspaceId,
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    _suggestedWorkspaceId,
                    workspaceId,
                    StringComparison.OrdinalIgnoreCase),
            IsObjectGrabbed = !string.IsNullOrWhiteSpace(_activeDragObjectId),
            IsEdgeCandidateActive = !string.IsNullOrWhiteSpace(_suggestedWorkspaceId),
            IsEdgeLocked = _candidateStatus == WorkspaceSessionCandidateStatus.EdgeLocked &&
                string.Equals(_suggestedWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase),
            ActiveDragObjectId = _activeDragObjectId,
            ActiveDragSourceWorkspaceId = activeDragSourceWorkspaceId,
            SuggestedWorkspaceId = _suggestedWorkspaceId,
            SuggestedWorkspaceName = string.IsNullOrWhiteSpace(_suggestedWorkspaceId)
                ? string.Empty
                : GetWorkspaceName(_suggestedWorkspaceId),
            SuggestedWorkspacePreview = BuildWorkspacePreview(_suggestedWorkspaceId),
            ActiveEdge = ResolveActiveEdgeForWorkspace(workspaceId, activeDragSourceWorkspaceId),
            EdgeHotZoneHint = BuildEdgeHotZoneHint(workspaceId, activeDragSourceWorkspaceId),
            EdgeTransitionHint = BuildEdgeTransitionHint(workspaceId, activeDragSourceWorkspaceId),
            EdgeGhostObjectName = BuildEdgeGhostObjectName(workspaceId, activeDragSourceWorkspaceId),
            StatusHint = GetStatusHintForWorkspace(workspaceId),
            SuccessHint = _successHint,
            IsSuccessPulseActive = string.Equals(_lastTargetWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase) &&
                _successPulseUntil > DateTimeOffset.UtcNow,
            TransferObjects = localObjects,
            History = localHistory,
            Log = _log
                .Where(item =>
                    string.Equals(item.Workspace, "global", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.Workspace, workspaceId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Time, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Diagnostics = BuildDiagnostics(),
            LastResult = _lastResult,
            LastError = _lastError,
            UxDiagnostics = BuildUxDiagnostics()
        };
    }

    public bool CanDrag(string objectId, string workspaceId)
    {
        var transferObject = GetTransferObject(objectId);
        return transferObject is not null &&
            IsKnownWorkspace(workspaceId) &&
            string.Equals(GetObjectLocation(transferObject.Id), workspaceId, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanDrop(string objectId, string workspaceId)
    {
        var transferObject = GetTransferObject(objectId);
        return transferObject is not null &&
            IsKnownWorkspace(workspaceId) &&
            !string.Equals(GetObjectLocation(transferObject.Id), workspaceId, StringComparison.OrdinalIgnoreCase);
    }

    public void BeginDrag(string objectId, string workspaceId)
    {
        if (!CanDrag(objectId, workspaceId))
        {
            return;
        }

        _activeDragObjectId = objectId;
        _suggestedWorkspaceId = string.Empty;
        _successHint = string.Empty;
        _lastTargetWorkspaceId = string.Empty;
        _grabbedAt = DateTimeOffset.UtcNow;
        _lastGrabbedAt = _grabbedAt;
        _dragStartedAt = _grabbedAt;
        _edgeLockedAt = null;
        _candidateStatus = WorkspaceSessionCandidateStatus.None;
        _dragStartCount++;
        _sessionCandidate = WorkspaceSessionCandidate.Empty;
        _lastStatusHint = "Objekt gefasst. Zum Rand bewegen.";
        var displayName = GetTransferObject(objectId)?.Metadata.DisplayName ?? objectId;
        AddLog(workspaceId, "Objekt gefasst", displayName, "Greifzustand aktiv.");
        NotifyChanged();
    }

    public void SetTargetHighlighted(string workspaceId, bool highlighted)
    {
        var shouldHighlight = highlighted && IsKnownWorkspace(workspaceId);
        var nextValue = shouldHighlight ? workspaceId : string.Empty;
        if (string.Equals(_highlightWorkspaceId, nextValue, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var wasHighlighted = string.Equals(_highlightWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase);
        _highlightWorkspaceId = nextValue;
        if (shouldHighlight)
        {
            var now = DateTimeOffset.UtcNow;
            _suggestedWorkspaceId = workspaceId;
            _targetDetectedCount++;
            _candidateStatus = WorkspaceSessionCandidateStatus.EdgeLocked;
            _edgeLockedAt ??= now;
            _sessionCandidate = BuildSessionCandidate(
                _activeDragObjectId,
                GetCurrentDragSourceWorkspaceId(),
                workspaceId,
                WorkspaceSessionCandidateStatus.EdgeLocked,
                "Zielarbeitsflaeche uebernimmt",
                _edgeLockedAt,
                null);
            _lastStatusHint = $"{GetWorkspaceShortName(workspaceId)} uebernimmt. Loslassen zum Ablegen.";
            AddLog(workspaceId, "Edge Locked", GetWorkspaceShortName(workspaceId), "Workspace uebernimmt.");
        }
        else if (wasHighlighted && string.Equals(_suggestedWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            _suggestedWorkspaceId = string.Empty;
            _lastStatusHint = string.IsNullOrWhiteSpace(_activeDragObjectId)
                ? "Bereit."
                : "Objekt gefasst. Weiter zum Rand bewegen.";
        }
        else if (string.IsNullOrWhiteSpace(nextValue) && string.IsNullOrWhiteSpace(_suggestedWorkspaceId))
        {
            _lastStatusHint = string.IsNullOrWhiteSpace(_activeDragObjectId)
                ? "Bereit."
                : "Objekt gefasst. Kein Ziel unter dem Cursor.";
        }

        NotifyChanged();
    }

    public void UpdateEdgeSuggestion(string workspaceId, MultiWindowEdge edge)
    {
        if (string.IsNullOrWhiteSpace(_activeDragObjectId) ||
            !IsKnownWorkspace(workspaceId))
        {
            return;
        }

        var suggestion = SuggestTargetForEdge(edge);
        if (string.Equals(_suggestedWorkspaceId, suggestion.WorkspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _suggestedWorkspaceId = suggestion.WorkspaceId;
        _lastStatusHint = suggestion.Hint;
        if (!string.IsNullOrWhiteSpace(suggestion.WorkspaceId))
        {
            _targetDetectedCount++;
            _candidateStatus = WorkspaceSessionCandidateStatus.Candidate;
            _edgeLockedAt = null;
            _sessionCandidate = BuildSessionCandidate(
                _activeDragObjectId,
                workspaceId,
                suggestion.WorkspaceId,
                WorkspaceSessionCandidateStatus.Candidate,
                suggestion.Hint,
                null,
                null);
            AddLog(workspaceId, "Edge Candidate", suggestion.WorkspaceName, suggestion.Hint);
        }
        else
        {
            _candidateStatus = WorkspaceSessionCandidateStatus.None;
            _sessionCandidate = WorkspaceSessionCandidate.Empty;
        }

        NotifyChanged();
    }

    public MultiWindowEdge DetectWindowEdge(
        int cursorScreenX,
        int windowLeft,
        int windowRight,
        int thresholdPixels)
    {
        if (thresholdPixels <= 0 || windowRight <= windowLeft)
        {
            return MultiWindowEdge.None;
        }

        if (cursorScreenX >= windowRight - thresholdPixels)
        {
            return MultiWindowEdge.Right;
        }

        if (cursorScreenX <= windowLeft + thresholdPixels)
        {
            return MultiWindowEdge.Left;
        }

        return MultiWindowEdge.None;
    }

    public MultiWindowEdgeTargetSuggestion SuggestTargetForEdge(MultiWindowEdge edge)
    {
        return edge switch
        {
            MultiWindowEdge.Right => new MultiWindowEdgeTargetSuggestion
            {
                Edge = edge,
                WorkspaceId = WorkspaceBId.ToString(),
                WorkspaceName = GetWorkspaceName(WorkspaceBId.ToString()),
                Hint = "Rechter Rand erkennt: Arbeitsflaeche rechts. Weiter nach rechts ziehen."
            },
            MultiWindowEdge.Left => new MultiWindowEdgeTargetSuggestion
            {
                Edge = edge,
                WorkspaceId = WorkspaceAId.ToString(),
                WorkspaceName = GetWorkspaceName(WorkspaceAId.ToString()),
                Hint = "Linker Rand erkennt: Arbeitsflaeche links. Weiter nach links ziehen."
            },
            _ => new MultiWindowEdgeTargetSuggestion
            {
                Edge = MultiWindowEdge.None,
                WorkspaceId = string.Empty,
                WorkspaceName = string.Empty,
                Hint = string.IsNullOrWhiteSpace(_activeDragObjectId)
                    ? "Bereit."
                    : "Objekt gefasst. Kein Randziel erkannt."
            }
        };
    }

    public void CancelDrag(string objectId, string workspaceId)
    {
        if (!string.Equals(_activeDragObjectId, objectId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var displayName = GetObjectDisplayName(objectId);
        _activeDragObjectId = string.Empty;
        _highlightWorkspaceId = string.Empty;
        _suggestedWorkspaceId = string.Empty;
        _successHint = string.Empty;
        _candidateStatus = WorkspaceSessionCandidateStatus.Cancelled;
        _sessionCandidate = BuildSessionCandidate(
            objectId,
            workspaceId,
            string.Empty,
            WorkspaceSessionCandidateStatus.Cancelled,
            "Kein gueltiges Ziel",
            _edgeLockedAt,
            DateTimeOffset.UtcNow);
        _lastResult = "FAILED";
        _lastError = "Kein gueltiges Ziel.";
        _failedTransfers++;
        _lastDragDurationMs = CalculateDragDurationMs(DateTimeOffset.UtcNow);
        _lastTransitionDurationMs = CalculateTransitionDurationMs(DateTimeOffset.UtcNow);
        _grabbedAt = null;
        _edgeLockedAt = null;
        _lastStatusHint = "Kein gueltiges Ziel. Das Objekt bleibt in der Ausgangsarbeitsflaeche.";
        AddLog(workspaceId, "Kein gueltiges Ziel", displayName, "Objekt springt zurueck.");
        NotifyChanged();
    }

    public bool CompleteDrop(string objectId, string workspaceId)
    {
        try
        {
            if (!CanDrop(objectId, workspaceId))
            {
                _dropCount++;
                _activeDragObjectId = string.Empty;
                _highlightWorkspaceId = string.Empty;
                _suggestedWorkspaceId = string.Empty;
                _successHint = string.Empty;
                _candidateStatus = WorkspaceSessionCandidateStatus.Cancelled;
                _sessionCandidate = BuildSessionCandidate(
                    objectId,
                    GetCurrentDragSourceWorkspaceId(),
                    workspaceId,
                    WorkspaceSessionCandidateStatus.Cancelled,
                    "Kein gueltiges Ziel",
                    _edgeLockedAt,
                    DateTimeOffset.UtcNow);
                _failedTransfers++;
                _lastDragDurationMs = CalculateDragDurationMs(DateTimeOffset.UtcNow);
                _lastTransitionDurationMs = CalculateTransitionDurationMs(DateTimeOffset.UtcNow);
                _grabbedAt = null;
                _edgeLockedAt = null;
                _lastStatusHint = "Kein gueltiges Ziel. Das Objekt bleibt in der Ausgangsarbeitsflaeche.";
                AddLog(workspaceId, "Kein gueltiges Ziel", GetObjectDisplayName(objectId), "Objekt springt zurueck.");
                _lastResult = "FAILED";
                _lastError = "Kein gueltiges Ziel.";
                NotifyChanged();
                return false;
            }

            var transferObject = GetRequiredTransferObject(objectId);
            var sourceWorkspaceId = WorkspaceId.Create(GetObjectLocation(transferObject.Id));
            var targetWorkspaceId = WorkspaceId.Create(workspaceId);
            _dropCount++;
            var completedAt = DateTimeOffset.UtcNow;
            _lastDragDurationMs = CalculateDragDurationMs(completedAt);
            _lastTransitionDurationMs = CalculateTransitionDurationMs(completedAt);
            _sessionCandidate = BuildSessionCandidate(
                objectId,
                sourceWorkspaceId.ToString(),
                targetWorkspaceId.ToString(),
                WorkspaceSessionCandidateStatus.EdgeLocked,
                "Objekt laeuft durch den Rand",
                _edgeLockedAt,
                null);
            AddLog(workspaceId, "Objekt tritt ein", transferObject.Metadata.DisplayName, "Workspace uebernimmt.");
            var stopwatch = Stopwatch.StartNew();
            var result = ExecuteWorkspaceExperienceTransfer(
                transferObject,
                sourceWorkspaceId,
                targetWorkspaceId);
            stopwatch.Stop();
            _lastTransferDurationMs = stopwatch.Elapsed.TotalMilliseconds;

            _lastResult = result.IsSuccess ? "SUCCESS" : $"FAILED: {result.FailureReason}";
            _lastError = result.IsSuccess ? string.Empty : string.Join(" ", result.Messages);
            _activeDragObjectId = string.Empty;
            _highlightWorkspaceId = string.Empty;
            _suggestedWorkspaceId = string.Empty;

            if (result.IsSuccess)
            {
                _successfulTransfers++;
                if (ResolveDirection(sourceWorkspaceId.ToString(), targetWorkspaceId.ToString()) == TransferDirection.Left)
                {
                    _returnTransferCount++;
                }

                _candidateStatus = WorkspaceSessionCandidateStatus.Completed;
                _sessionCandidate = BuildSessionCandidate(
                    objectId,
                    sourceWorkspaceId.ToString(),
                    targetWorkspaceId.ToString(),
                    WorkspaceSessionCandidateStatus.Completed,
                    "Transfer abgeschlossen",
                    _edgeLockedAt,
                    DateTimeOffset.UtcNow);
                _lastTransferDisplayName = transferObject.Metadata.DisplayName;
                _lastTargetWorkspaceId = workspaceId;
                _successPulseUntil = DateTimeOffset.UtcNow.AddSeconds(2);
                _grabbedAt = null;
                _edgeLockedAt = null;
                _successHint = "Transfer abgeschlossen. Objekt liegt in der Zielarbeitsflaeche.";
                _lastStatusHint = _successHint;
                AddLog(sourceWorkspaceId.ToString(), "Transfer erfolgreich abgeschlossen", transferObject.Metadata.DisplayName, string.Empty);
                AddLog(workspaceId, "Transfer erfolgreich abgeschlossen", transferObject.Metadata.DisplayName, string.Empty);
            }
            else
            {
                _failedTransfers++;
                _candidateStatus = WorkspaceSessionCandidateStatus.Cancelled;
                _sessionCandidate = BuildSessionCandidate(
                    objectId,
                    sourceWorkspaceId.ToString(),
                    targetWorkspaceId.ToString(),
                    WorkspaceSessionCandidateStatus.Cancelled,
                    "Transfer fehlgeschlagen",
                    _edgeLockedAt,
                    DateTimeOffset.UtcNow);
                _successHint = string.Empty;
                _grabbedAt = null;
                _edgeLockedAt = null;
                _lastStatusHint = "Transfer fehlgeschlagen.";
                AddLog(workspaceId, "Transfer failed", _lastResult, _lastError);
            }

            NotifyChanged();
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _lastResult = "FAILED";
            _lastError = ex.Message;
            _failedTransfers++;
            _activeDragObjectId = string.Empty;
            _highlightWorkspaceId = string.Empty;
            _suggestedWorkspaceId = string.Empty;
            _successHint = string.Empty;
            _candidateStatus = WorkspaceSessionCandidateStatus.Cancelled;
            _sessionCandidate = BuildSessionCandidate(
                objectId,
                GetCurrentDragSourceWorkspaceId(),
                workspaceId,
                WorkspaceSessionCandidateStatus.Cancelled,
                "Transfer fehlgeschlagen",
                _edgeLockedAt,
                DateTimeOffset.UtcNow);
            _grabbedAt = null;
            _edgeLockedAt = null;
            _lastStatusHint = "Transfer fehlgeschlagen.";
            AddLog(workspaceId, "Transfer failed", objectId, ex.Message);
            NotifyChanged();
            return false;
        }
    }

    public string GetObjectDisplayName(string objectId)
    {
        return GetTransferObject(objectId)?.Metadata.DisplayName ?? objectId;
    }

    public bool HasTransferredObjectInTarget()
    {
        return _runtime.TransferObjectManager.GetAll()
            .Any(item => string.Equals(
                GetObjectLocation(item.Id),
                WorkspaceBId.ToString(),
                StringComparison.OrdinalIgnoreCase));
    }

    private void InitializeWorkspaces()
    {
        RegisterWorkspace(WorkspaceAId, "Arbeitsflaeche links / Laptop", WorkspacePosition.Center, priority: 10);
        RegisterWorkspace(WorkspaceBId, "Arbeitsflaeche rechts / Display rechts", WorkspacePosition.Right, priority: 5);
    }

    private void RegisterWorkspace(
        WorkspaceId workspaceId,
        string displayName,
        WorkspacePosition position,
        int priority)
    {
        var descriptor = new WorkspaceDescriptor
        {
            WorkspaceId = workspaceId,
            DisplayName = displayName,
            WorkspaceType = workspaceId == WorkspaceAId ? WorkspaceType.SmartDevice : WorkspaceType.DisplayNode,
            WorkspaceState = WorkspaceState.Available,
            Position = position,
            Capabilities = CapabilitySet.FromIds(
                CapabilityId.Display,
                CapabilityId.Keyboard,
                CapabilityId.Clipboard,
                CapabilityId.Encryption,
                CapabilityId.Pairing),
            IsTrusted = true,
            Priority = priority,
            LastSeen = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["tool"] = "developer-studio",
                ["prototype"] = "multi-window"
            }
        };

        if (!_runtime.WorkspaceRegistry.ContainsWorkspace(workspaceId))
        {
            _runtime.WorkspaceRegistry.RegisterWorkspace(Workspace.FromDescriptor(descriptor));
        }

        var provider = new WorkspaceCapabilityProvider(descriptor);
        if (_runtime.CapabilityManager.GetProvider(provider.ProviderId) is null)
        {
            _runtime.CapabilityManager.RegisterProvider(provider);
        }
    }

    private void InitializeObjects()
    {
        CreateObject(TransferObjectType.Text, "Textobjekt 1", "text/plain; charset=utf-8", 18, "text-one");
        CreateObject(TransferObjectType.Text, "Textobjekt 2", "text/plain; charset=utf-8", 22, "text-two");
        CreateObject(TransferObjectType.PDF, "PDF-Kurzinfo", "application/pdf", 4096, "pdf");
        CreateObject(TransferObjectType.Image, "Workspace-Skizze", "image/png", 8192, "image");
        CreateObject(TransferObjectType.Link, "Projekt-Link", "text/uri-list", 128, "link");
    }

    private void CreateObject(
        TransferObjectType objectType,
        string displayName,
        string mimeType,
        long size,
        string checksumSuffix)
    {
        _runtime.TransferObjectManager.Create(
            objectType,
            new TransferMetadata
            {
                ObjectId = TransferObjectId.NewId(),
                DisplayName = displayName,
                MimeType = mimeType,
                Size = size,
                Checksum = $"sha256:multi-window-{checksumSuffix}",
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow,
                SourceWorkspace = WorkspaceAId.ToString(),
                TargetWorkspace = string.Empty,
                Owner = "developer-studio",
                Priority = 1,
                Tags = new[] { "developer-studio", "multi-window", objectType.ToString() },
                Version = "1.0.0"
            });
    }

    private MultiWindowTransferObjectRow ToObjectRow(ITransferObject transferObject)
    {
        return new MultiWindowTransferObjectRow
        {
            ObjectId = transferObject.Id.ToString(),
            ObjectType = transferObject.ObjectType.ToString(),
            Symbol = GetSymbol(transferObject.ObjectType),
            DisplayName = transferObject.Metadata.DisplayName,
            Preview = GetPreview(transferObject),
            State = transferObject.State.ToString(),
            Source = transferObject.Metadata.SourceWorkspace,
            Target = transferObject.Metadata.TargetWorkspace,
            Location = GetObjectLocation(transferObject.Id),
            MimeType = transferObject.Metadata.MimeType,
            IsBeingDragged = string.Equals(
                _activeDragObjectId,
                transferObject.Id.ToString(),
                StringComparison.OrdinalIgnoreCase)
        };
    }

    private string GetObjectLocation(TransferObjectId objectId)
    {
        var transferObject = _runtime.TransferObjectManager.Get(objectId);
        if (transferObject is null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(transferObject.Metadata.TargetWorkspace)
            ? transferObject.Metadata.SourceWorkspace
            : transferObject.Metadata.TargetWorkspace;
    }

    private ITransferObject? GetTransferObject(string objectId)
    {
        return string.IsNullOrWhiteSpace(objectId)
            ? null
            : _runtime.TransferObjectManager.Get(TransferObjectId.Create(objectId));
    }

    private ITransferObject GetRequiredTransferObject(string objectId)
    {
        return GetTransferObject(objectId)
            ?? throw new InvalidOperationException($"Transfer object '{objectId}' was not found.");
    }

    private string BuildDiagnostics()
    {
        var diagnostics = _runtime.GetDiagnostics();
        var transferText = string.IsNullOrWhiteSpace(_lastTransferDisplayName)
            ? "keiner"
            : _lastTransferDisplayName;
        return $"Runtime={StudioUiText.Display(diagnostics.RuntimeState.ToString())}; Arbeitsflaechen={diagnostics.WorkspaceCount}; Objekte={diagnostics.TransferObjectCount}; Ergebnis={StudioUiText.Display(_lastResult)}; Letzter Transfer={transferText}; Candidate={_candidateStatus}";
    }

    private MultiWindowUxDiagnosticsSnapshot BuildUxDiagnostics()
    {
        var successRate = _dropCount == 0
            ? 0
            : Math.Round(_successfulTransfers * 100.0 / _dropCount, 1);
        return new MultiWindowUxDiagnosticsSnapshot
        {
            DragStartCount = _dragStartCount,
            TargetDetectedCount = _targetDetectedCount,
            DropCount = _dropCount,
            SuccessfulTransfers = _successfulTransfers,
            FailedTransfers = _failedTransfers,
            ReturnTransferCount = _returnTransferCount,
            FailedAttempts = _failedTransfers,
            LastDragDurationMs = Math.Round(_lastDragDurationMs, 1),
            LastTransferDurationMs = Math.Round(_lastTransferDurationMs, 1),
            LastTransitionDurationMs = Math.Round(_lastTransitionDurationMs, 1),
            SuccessRatePercent = successRate,
            LastGrabbedAt = GetLastGrabbedAt(),
            LastEdgeLockedAt = _edgeLockedAt ?? _sessionCandidate.EdgeLockedAt,
            ActiveDirection = _sessionCandidate.Direction.ToString(),
            CandidateStatus = _candidateStatus,
            SessionCandidate = _sessionCandidate
        };
    }

    private TransferResult ExecuteWorkspaceExperienceTransfer(
        ITransferObject transferObject,
        WorkspaceId sourceWorkspaceId,
        WorkspaceId targetWorkspaceId)
    {
        if (transferObject.State == TransferObjectState.Completed)
        {
            return MoveCompletedTransferObject(transferObject, sourceWorkspaceId, targetWorkspaceId);
        }

        return _transferEngine.ExecuteLogicalTransfer(new TransferRequest
        {
            RequestId = $"multi-window-request-{Guid.NewGuid():N}",
            SourceWorkspaceId = sourceWorkspaceId,
            RequestedDirection = TransferDirection.Any,
            TransferObjectId = transferObject.Id,
            RequiredCapabilities = CapabilitySet.FromIds(
                CapabilityId.Display,
                CapabilityId.Clipboard,
                CapabilityId.Encryption,
                CapabilityId.Pairing),
            OptionalCapabilities = CapabilitySet.Empty,
            ForbiddenCapabilities = CapabilitySet.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
            RequestedBy = "workspace-illusion-sprint",
            Metadata = new Dictionary<string, string>
            {
                ["tool"] = "developer-studio",
                ["interaction"] = "workspace-illusion-edge-transition",
                ["direction"] = ResolveDirection(sourceWorkspaceId.ToString(), targetWorkspaceId.ToString()).ToString()
            }
        });
    }

    private TransferResult MoveCompletedTransferObject(
        ITransferObject transferObject,
        WorkspaceId sourceWorkspaceId,
        WorkspaceId targetWorkspaceId)
    {
        var now = DateTimeOffset.UtcNow;
        var updated = _runtime.TransferObjectManager.UpdateMetadata(
            transferObject.Id,
            transferObject.Metadata with
            {
                TargetWorkspace = targetWorkspaceId.ToString(),
                ModifiedAt = now
            });
        return new TransferResult
        {
            RequestId = $"workspace-experience-move-{Guid.NewGuid():N}",
            IsSuccess = true,
            SourceWorkspace = _runtime.WorkspaceRegistry.GetWorkspace(sourceWorkspaceId),
            TargetWorkspace = _runtime.WorkspaceRegistry.GetWorkspace(targetWorkspaceId),
            TransferObject = updated,
            FinalState = updated.State,
            FailureReason = TransferFailureReason.None,
            Messages = new[] { "WorkspaceExperienceMove:Completed" },
            CompletedAt = now
        };
    }

    private WorkspaceSessionCandidate BuildSessionCandidate(
        string objectId,
        string sourceWorkspaceId,
        string targetWorkspaceId,
        WorkspaceSessionCandidateStatus status,
        string reason,
        DateTimeOffset? edgeLockedAt,
        DateTimeOffset? completedAt)
    {
        return new WorkspaceSessionCandidate
        {
            CandidateId = $"candidate-{Guid.NewGuid():N}",
            TransferObjectId = objectId,
            SourceWorkspaceId = sourceWorkspaceId,
            TargetWorkspaceId = targetWorkspaceId,
            Direction = ResolveDirection(sourceWorkspaceId, targetWorkspaceId),
            Status = status,
            Reason = reason,
            CreatedAt = DateTimeOffset.UtcNow,
            EdgeLockedAt = edgeLockedAt,
            CompletedAt = completedAt,
            IsLiveWorkspacePrepared = false
        };
    }

    private string GetCurrentDragSourceWorkspaceId()
    {
        var transferObject = GetTransferObject(_activeDragObjectId);
        return transferObject is null
            ? string.Empty
            : GetObjectLocation(transferObject.Id);
    }

    private double CalculateDragDurationMs(DateTimeOffset completedAt)
    {
        if (_dragStartedAt is null)
        {
            return 0;
        }

        var duration = (completedAt - _dragStartedAt.Value).TotalMilliseconds;
        _dragStartedAt = null;
        return Math.Max(0, duration);
    }

    private double CalculateTransitionDurationMs(DateTimeOffset completedAt)
    {
        if (_edgeLockedAt is null)
        {
            return 0;
        }

        return Math.Max(1, (completedAt - _edgeLockedAt.Value).TotalMilliseconds);
    }

    private DateTimeOffset? GetLastGrabbedAt()
    {
        return _grabbedAt ?? _lastGrabbedAt;
    }

    private MultiWindowEdge ResolveActiveEdgeForWorkspace(
        string workspaceId,
        string activeDragSourceWorkspaceId)
    {
        if (string.IsNullOrWhiteSpace(_activeDragObjectId))
        {
            return MultiWindowEdge.None;
        }

        if (string.Equals(workspaceId, activeDragSourceWorkspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(workspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase)
                ? MultiWindowEdge.Right
                : MultiWindowEdge.Left;
        }

        if (string.Equals(workspaceId, _suggestedWorkspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(workspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase)
                ? MultiWindowEdge.Left
                : MultiWindowEdge.Right;
        }

        return MultiWindowEdge.None;
    }

    private string BuildEdgeHotZoneHint(
        string workspaceId,
        string activeDragSourceWorkspaceId)
    {
        var edge = ResolveActiveEdgeForWorkspace(workspaceId, activeDragSourceWorkspaceId);
        if (edge == MultiWindowEdge.None)
        {
            return string.Empty;
        }

        if (string.Equals(workspaceId, activeDragSourceWorkspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return edge == MultiWindowEdge.Right
                ? "Nach rechts schieben"
                : "Nach links schieben";
        }

        return $"{GetWorkspaceShortName(workspaceId)} uebernimmt";
    }

    private string BuildEdgeTransitionHint(
        string workspaceId,
        string activeDragSourceWorkspaceId)
    {
        if (string.IsNullOrWhiteSpace(_activeDragObjectId))
        {
            return string.Empty;
        }

        if (string.Equals(workspaceId, activeDragSourceWorkspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(workspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase)
                ? "Das Objekt gleitet aus der rechten Kante."
                : "Das Objekt gleitet aus der linken Kante.";
        }

        if (string.Equals(workspaceId, _suggestedWorkspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(workspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase)
                ? "Eintritt links: Objekt kommt von der linken Arbeitsflaeche."
                : "Eintritt rechts: Objekt kommt von der rechten Arbeitsflaeche.";
        }

        return string.Empty;
    }

    private string BuildEdgeGhostObjectName(
        string workspaceId,
        string activeDragSourceWorkspaceId)
    {
        if (string.IsNullOrWhiteSpace(_activeDragObjectId))
        {
            return string.Empty;
        }

        if (string.Equals(workspaceId, activeDragSourceWorkspaceId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workspaceId, _suggestedWorkspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return GetObjectDisplayName(_activeDragObjectId);
        }

        return string.Empty;
    }

    private string GetStatusHintForWorkspace(string workspaceId)
    {
        if (string.Equals(_highlightWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return $"{GetWorkspaceShortName(workspaceId)} uebernimmt. Loslassen zum Ablegen.";
        }

        if (string.Equals(_suggestedWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return workspaceId == WorkspaceBId.ToString()
                ? "Eintritt links aktiv. Arbeitsflaeche rechts erkennt das Objekt."
                : "Eintritt rechts aktiv. Arbeitsflaeche links erkennt das Objekt.";
        }

        return _lastStatusHint;
    }

    private string GetWorkspaceName(string workspaceId)
    {
        return _runtime.WorkspaceRegistry.GetWorkspace(WorkspaceId.Create(workspaceId))
            ?.Descriptor.DisplayName ?? workspaceId;
    }

    private static string GetWorkspaceShortName(string workspaceId)
    {
        if (string.Equals(workspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return "Arbeitsflaeche links";
        }

        if (string.Equals(workspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return "Arbeitsflaeche rechts";
        }

        return "Arbeitsflaeche";
    }

    private string BuildWorkspacePreview(string workspaceId)
    {
        if (string.IsNullOrWhiteSpace(workspaceId) || !IsKnownWorkspace(workspaceId))
        {
            return string.Empty;
        }

        var workspace = _runtime.WorkspaceRegistry.GetWorkspace(WorkspaceId.Create(workspaceId));
        if (workspace is null)
        {
            return string.Empty;
        }

        var objectCount = _runtime.TransferObjectManager.GetAll()
            .Count(item => string.Equals(
                GetObjectLocation(item.Id),
                workspaceId,
                StringComparison.OrdinalIgnoreCase));
        return $"{workspace.Descriptor.DisplayName} | {objectCount} Objekte | {StudioUiText.Display(workspace.Descriptor.WorkspaceState.ToString())}";
    }

    private static bool IsKnownWorkspace(string workspaceId)
    {
        return string.Equals(workspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static TransferDirection ResolveDirection(string sourceWorkspaceId, string targetWorkspaceId)
    {
        if (string.Equals(sourceWorkspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(targetWorkspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return TransferDirection.Right;
        }

        if (string.Equals(sourceWorkspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(targetWorkspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return TransferDirection.Left;
        }

        return string.IsNullOrWhiteSpace(targetWorkspaceId)
            ? TransferDirection.Unknown
            : TransferDirection.Any;
    }

    private static string GetSymbol(TransferObjectType objectType)
    {
        return objectType switch
        {
            TransferObjectType.Text => "TXT",
            TransferObjectType.PDF => "PDF",
            TransferObjectType.Image => "IMG",
            TransferObjectType.Link => "URL",
            _ => "OBJ"
        };
    }

    private static string GetPreview(ITransferObject transferObject)
    {
        return transferObject.ObjectType switch
        {
            TransferObjectType.Text => "Kurznotiz fuer die Arbeitsflaeche",
            TransferObjectType.PDF => "PDF-Dokument mit Demo-Inhalt",
            TransferObjectType.Image => "Bildvorschau fuer Workspace-Skizze",
            TransferObjectType.Link => "https://rk.workspace/demo",
            _ => transferObject.Metadata.MimeType
        };
    }

    private void AddLog(string workspaceId, string action, string result, string error)
    {
        _log.Add(new MultiWindowLogRow
        {
            Time = DateTimeOffset.Now.ToString("HH:mm:ss"),
            Workspace = workspaceId,
            Action = action,
            Result = result,
            Error = error
        });
    }

    private void NotifyChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static RuntimeEngine CreateRuntime()
    {
        return new RuntimeEngine(new RuntimeConfiguration
        {
            LoggingEnabled = true,
            SimulationEnabled = true,
            TestModeEnabled = true,
            DiagnosticsEnabled = true,
            DebugModeEnabled = false
        });
    }

    private static TransferEngine CreateTransferEngine(RuntimeEngine runtime)
    {
        return new TransferEngine(
            runtime.WorkspaceRegistry,
            runtime.CapabilityManager,
            runtime.TransferObjectManager);
    }
}
