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
                StringComparison.OrdinalIgnoreCase),
            ActiveDragObjectId = _activeDragObjectId,
            SuggestedWorkspaceId = _suggestedWorkspaceId,
            StatusHint = GetStatusHintForWorkspace(workspaceId),
            SuccessHint = _successHint,
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
            LastError = _lastError
        };
    }

    public bool CanDrag(string objectId, string workspaceId)
    {
        var transferObject = GetTransferObject(objectId);
        return transferObject is not null &&
            string.Equals(GetObjectLocation(transferObject.Id), workspaceId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(workspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(workspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public bool CanDrop(string objectId, string workspaceId)
    {
        var transferObject = GetTransferObject(objectId);
        return transferObject is not null &&
            string.Equals(workspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(GetObjectLocation(transferObject.Id), WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase);
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
        _lastStatusHint = "Objekt wird gezogen. Am Fensterrand wird eine Zielarbeitsflaeche vorgeschlagen.";
        AddLog(workspaceId, "Objekt wird gezogen", GetTransferObject(objectId)?.Metadata.DisplayName ?? objectId, string.Empty);
        NotifyChanged();
    }

    public void SetTargetHighlighted(string workspaceId, bool highlighted)
    {
        var shouldHighlight = highlighted &&
            string.Equals(workspaceId, WorkspaceBId.ToString(), StringComparison.OrdinalIgnoreCase);
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
            !string.Equals(workspaceId, WorkspaceAId.ToString(), StringComparison.OrdinalIgnoreCase))
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
                Hint = "Rechter Fensterrand: Arbeitsflaeche B wird vorgeschlagen."
            },
            MultiWindowEdge.Left => new MultiWindowEdgeTargetSuggestion
            {
                Edge = edge,
                WorkspaceId = WorkspaceAId.ToString(),
                WorkspaceName = GetWorkspaceName(WorkspaceAId.ToString()),
                Hint = "Linker Fensterrand: Arbeitsflaeche A wird vorgeschlagen."
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
                _activeDragObjectId = string.Empty;
                _highlightWorkspaceId = string.Empty;
                _suggestedWorkspaceId = string.Empty;
                _successHint = string.Empty;
                _lastStatusHint = "Kein gueltiges Ziel. Das Objekt bleibt in der Ausgangsarbeitsflaeche.";
                AddLog(workspaceId, "Kein gueltiges Ziel", GetObjectDisplayName(objectId), "Objekt springt zurueck.");
                _lastResult = "FAILED";
                _lastError = "Kein gueltiges Ziel.";
                NotifyChanged();
                return false;
            }

            var transferObject = GetRequiredTransferObject(objectId);
            AddLog(workspaceId, "Transfer received", transferObject.Metadata.DisplayName, string.Empty);
            var result = _transferEngine.ExecuteLogicalTransfer(new TransferRequest
            {
                RequestId = $"multi-window-request-{Guid.NewGuid():N}",
                SourceWorkspaceId = WorkspaceAId,
                RequestedDirection = TransferDirection.Right,
                TransferObjectId = transferObject.Id,
                RequiredCapabilities = CapabilitySet.FromIds(
                    CapabilityId.Display,
                    CapabilityId.Clipboard,
                    CapabilityId.Encryption,
                    CapabilityId.Pairing),
                OptionalCapabilities = CapabilitySet.Empty,
                ForbiddenCapabilities = CapabilitySet.Empty,
                CreatedAt = DateTimeOffset.UtcNow,
                RequestedBy = "multi-window-workspace-prototype",
                Metadata = new Dictionary<string, string>
                {
                    ["tool"] = "developer-studio",
                    ["interaction"] = "multi-window-drop"
                }
            });

            _lastResult = result.IsSuccess ? "SUCCESS" : $"FAILED: {result.FailureReason}";
            _lastError = result.IsSuccess ? string.Empty : string.Join(" ", result.Messages);
            _activeDragObjectId = string.Empty;
            _highlightWorkspaceId = string.Empty;
            _suggestedWorkspaceId = string.Empty;

            if (result.IsSuccess)
            {
                _lastTransferDisplayName = transferObject.Metadata.DisplayName;
                _successHint = "SUCCESS: Transfer erfolgreich abgeschlossen.";
                _lastStatusHint = _successHint;
                AddLog(WorkspaceAId.ToString(), "Transfer erfolgreich abgeschlossen", transferObject.Metadata.DisplayName, string.Empty);
                AddLog(workspaceId, "Transfer erfolgreich abgeschlossen", transferObject.Metadata.DisplayName, string.Empty);
            }
            else
            {
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
            DisplayName = transferObject.Metadata.DisplayName,
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

    private string GetStatusHintForWorkspace(string workspaceId)
    {
        if (string.Equals(_highlightWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return "Hier ablegen";
        }

        if (string.Equals(_suggestedWorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
        {
            return workspaceId == WorkspaceBId.ToString()
                ? "Rechter Rand: Arbeitsflaeche B vorgeschlagen."
                : "Linker Rand: Arbeitsflaeche A vorgeschlagen.";
        }

        return _lastStatusHint;
    }

    private string GetWorkspaceName(string workspaceId)
    {
        return _runtime.WorkspaceRegistry.GetWorkspace(WorkspaceId.Create(workspaceId))
            ?.Descriptor.DisplayName ?? workspaceId;
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
