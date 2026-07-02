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

        AddLog(workspaceId, "Transfer started", GetTransferObject(objectId)?.Metadata.DisplayName ?? objectId, string.Empty);
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

        _highlightWorkspaceId = nextValue;
        if (shouldHighlight)
        {
            AddLog(workspaceId, "Target highlighted", "Ready to receive", string.Empty);
        }

        NotifyChanged();
    }

    public bool CompleteDrop(string objectId, string workspaceId)
    {
        try
        {
            if (!CanDrop(objectId, workspaceId))
            {
                SetTargetHighlighted(workspaceId, highlighted: false);
                AddLog(workspaceId, "Drop rejected", objectId, "Object cannot be dropped here.");
                _lastResult = "FAILED";
                _lastError = "Object cannot be dropped here.";
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
            _highlightWorkspaceId = string.Empty;

            if (result.IsSuccess)
            {
                AddLog(WorkspaceAId.ToString(), "Transfer completed", transferObject.Metadata.DisplayName, string.Empty);
                AddLog(workspaceId, "Transfer completed", transferObject.Metadata.DisplayName, string.Empty);
            }
            else
            {
                AddLog(workspaceId, "Transfer failed", _lastResult, _lastError);
            }

            NotifyChanged();
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _lastResult = "FAILED";
            _lastError = ex.Message;
            _highlightWorkspaceId = string.Empty;
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
            MimeType = transferObject.Metadata.MimeType
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
        return $"Runtime={StudioUiText.Display(diagnostics.RuntimeState.ToString())}; Arbeitsflaechen={diagnostics.WorkspaceCount}; Objekte={diagnostics.TransferObjectCount}; Ergebnis={StudioUiText.Display(_lastResult)}";
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
