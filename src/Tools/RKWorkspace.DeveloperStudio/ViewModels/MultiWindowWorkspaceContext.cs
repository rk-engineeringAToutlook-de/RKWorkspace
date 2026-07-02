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
    private DateTimeOffset? _dragStartedAt;
    private DateTimeOffset _successPulseUntil = DateTimeOffset.MinValue;
    private int _dragStartCount;
    private int _targetDetectedCount;
    private int _dropCount;
    private int _successfulTransfers;
    private int _failedTransfers;
    private double _lastDragDurationMs;
    private double _lastTransferDurationMs;
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
        _dragStartedAt = null;
        _successPulseUntil = DateTimeOffset.MinValue;
        _dragStartCount = 0;
        _targetDetectedCount = 0;
        _dropCount = 0;
        _successfulTransfers = 0;
        _failedTransfers = 0;
        _lastDragDurationMs = 0;
        _lastTransferDurationMs = 0;
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
            ActiveDragObjectId = _activeDragObjectId,
            SuggestedWorkspaceId = _suggestedWorkspaceId,
            SuggestedWorkspaceName = string.IsNullOrWhiteSpace(_suggestedWorkspaceId)
                ? string.Empty
                : GetWorkspaceName(_suggestedWorkspaceId),
            SuggestedWorkspacePreview = BuildWorkspacePreview(_suggestedWorkspaceId),
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
        _dragStartedAt = DateTimeOffset.UtcNow;
        _dragStartCount++;
        _sessionCandidate = BuildSessionCandidate(
            objectId,
            workspaceId,
            string.Empty,
            "Drag gestartet");
        _lastStatusHint = "Objekt wird gezogen. Am Fensterrand wird eine Zielarbeitsflaeche vorgeschlagen.";
        AddLog(workspaceId, "Objekt wird gezogen", GetTransferObject(objectId)?.Metadata.DisplayName ?? objectId, string.Empty);
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
            _suggestedWorkspaceId = workspaceId;
            _targetDetectedCount++;
            _sessionCandidate = BuildSessionCandidate(
                _activeDragObjectId,
                GetCurrentDragSourceWorkspaceId(),
                workspaceId,
                "Drop-Ziel unter Cursor erkannt");
            _lastStatusHint = "Hier ablegen";
            AddLog(workspaceId, "Zielarbeitsflaeche erkannt", "Hier ablegen", string.Empty);
        }
        else if (wasHighlighted && string.Equals(_suggestedWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            _suggestedWorkspaceId = string.Empty;
            _lastStatusHint = string.IsNullOrWhiteSpace(_activeDragObjectId)
                ? "Bereit."
                : "Objekt wird gezogen. Kein Ziel unter dem Cursor.";
        }
        else if (string.IsNullOrWhiteSpace(nextValue) && string.IsNullOrWhiteSpace(_suggestedWorkspaceId))
        {
            _lastStatusHint = string.IsNullOrWhiteSpace(_activeDragObjectId)
                ? "Bereit."
                : "Objekt wird gezogen. Kein Ziel unter dem Cursor.";
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
            _sessionCandidate = BuildSessionCandidate(
                _activeDragObjectId,
                workspaceId,
                suggestion.WorkspaceId,
                suggestion.Hint);
            AddLog(workspaceId, "Randziel vorgeschlagen", suggestion.WorkspaceName, suggestion.Hint);
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
                Hint = "-> Workspace B erkannt. Loslassen zum Uebertragen."
            },
            MultiWindowEdge.Left => new MultiWindowEdgeTargetSuggestion
            {
                Edge = edge,
                WorkspaceId = WorkspaceAId.ToString(),
                WorkspaceName = GetWorkspaceName(WorkspaceAId.ToString()),
                Hint = "<- Workspace A erkannt. Loslassen zum Uebertragen."
            },
            _ => new MultiWindowEdgeTargetSuggestion
            {
                Edge = MultiWindowEdge.None,
                WorkspaceId = string.Empty,
                WorkspaceName = string.Empty,
                Hint = string.IsNullOrWhiteSpace(_activeDragObjectId)
                    ? "Bereit."
                    : "Objekt wird gezogen. Kein Randziel erkannt."
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
        _lastResult = "FAILED";
        _lastError = "Kein gueltiges Ziel.";
        _failedTransfers++;
        _lastDragDurationMs = CalculateDragDurationMs(DateTimeOffset.UtcNow);
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
                _failedTransfers++;
                _lastDragDurationMs = CalculateDragDurationMs(DateTimeOffset.UtcNow);
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
            _lastDragDurationMs = CalculateDragDurationMs(DateTimeOffset.UtcNow);
            _sessionCandidate = BuildSessionCandidate(
                objectId,
                sourceWorkspaceId.ToString(),
                targetWorkspaceId.ToString(),
                "Drop ausgefuehrt");
            AddLog(workspaceId, "Transfer received", transferObject.Metadata.DisplayName, string.Empty);
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
                _lastTransferDisplayName = transferObject.Metadata.DisplayName;
                _lastTargetWorkspaceId = workspaceId;
                _successPulseUntil = DateTimeOffset.UtcNow.AddSeconds(2);
                _successHint = "SUCCESS: Transfer erfolgreich abgeschlossen.";
                _lastStatusHint = _successHint;
                AddLog(sourceWorkspaceId.ToString(), "Transfer erfolgreich abgeschlossen", transferObject.Metadata.DisplayName, string.Empty);
                AddLog(workspaceId, "Transfer erfolgreich abgeschlossen", transferObject.Metadata.DisplayName, string.Empty);
            }
            else
            {
                _failedTransfers++;
                _successHint = string.Empty;
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
        RegisterWorkspace(WorkspaceAId, "Arbeitsflaeche A / Laptop", WorkspacePosition.Center, priority: 10);
        RegisterWorkspace(WorkspaceBId, "Arbeitsflaeche B / Anzeige rechts", WorkspacePosition.Right, priority: 5);
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
        return $"Runtime={StudioUiText.Display(diagnostics.RuntimeState.ToString())}; Arbeitsflaechen={diagnostics.WorkspaceCount}; Objekte={diagnostics.TransferObjectCount}; Ergebnis={StudioUiText.Display(_lastResult)}; Letzter Transfer={transferText}";
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
            LastDragDurationMs = Math.Round(_lastDragDurationMs, 1),
            LastTransferDurationMs = Math.Round(_lastTransferDurationMs, 1),
            SuccessRatePercent = successRate,
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
            RequestedBy = "workspace-experience-sprint",
            Metadata = new Dictionary<string, string>
            {
                ["tool"] = "developer-studio",
                ["interaction"] = "workspace-experience-drop",
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
        string reason)
    {
        return new WorkspaceSessionCandidate
        {
            CandidateId = $"candidate-{Guid.NewGuid():N}",
            TransferObjectId = objectId,
            SourceWorkspaceId = sourceWorkspaceId,
            TargetWorkspaceId = targetWorkspaceId,
            Direction = ResolveDirection(sourceWorkspaceId, targetWorkspaceId),
            Reason = reason,
            CreatedAt = DateTimeOffset.UtcNow,
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

    private string GetStatusHintForWorkspace(string workspaceId)
    {
        if (string.Equals(_highlightWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return "Hier ablegen";
        }

        if (string.Equals(_suggestedWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return workspaceId == WorkspaceBId.ToString()
                ? "-> Workspace B erkannt. Loslassen zum Uebertragen."
                : "<- Workspace A erkannt. Loslassen zum Uebertragen.";
        }

        return _lastStatusHint;
    }

    private string GetWorkspaceName(string workspaceId)
    {
        return _runtime.WorkspaceRegistry.GetWorkspace(WorkspaceId.Create(workspaceId))
            ?.Descriptor.DisplayName ?? workspaceId;
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
