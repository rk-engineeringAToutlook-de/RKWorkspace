# Workspace Agent Runtime

Dokument-ID: RKWS-DEV-WORKSPACE-AGENT-RUNTIME
Version: 1.3.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Die Workspace Agent Runtime ist der erste echte RK Workspace Prozess. Sie startet den plattformneutralen Core, registriert eine lokale Arbeitsflaeche und gibt ihren Status sichtbar auf der Konsole aus.

Der Agent ist noch kein Windows-Service, kein macOS-Daemon und kein Linux-Systemdienst.

## Startbefehle

```powershell
.\tools\run-agent.ps1
.\tools\run-agent.ps1 -Once
.\tools\run-agent.ps1 -Status
.\tools\run-agent.ps1 -AgentId rkws-agent-b -WorkspaceName Workspace-B -Position Right -IpcServer rkws-b
.\tools\run-agent.ps1 -AgentId rkws-agent-a -WorkspaceName Workspace-A -Position Left -IpcClient rkws-b -TargetAgentId rkws-agent-b -Once
```

Direkte Agent-Optionen:

```text
--once
--status
--demo
--no-demo
--agent-id <id>
--workspace-name <name>
--position <Left|Right|Center>
--ipc-server <pipeName>
--ipc-client <pipeName>
--target-agent-id <id>
--ipc-stop-after-transfer
--help
```

PowerShell-Optionen:

```text
-AgentId
-WorkspaceName
-Position
-IpcServer
-IpcClient
-TargetAgentId
-IpcStopAfterTransfer
```

## Default-Konfiguration

| Feld | Wert |
| --- | --- |
| AgentId | `rkws-agent-local` |
| DisplayName | `RKWS Local Agent` |
| WorkspaceName | `RKWS Local Workspace` |
| WorkspaceType | `SmartDevice` |
| WorkspacePosition | `Center` |
| RunMode | `LocalOnly` |
| EnableDemoWorkspace | `true` |
| EnableConsoleStatus | `true` |
| HeartbeatIntervalSeconds | `5` |

## Dual-Agent-Konfigurationen

MA004.02 ergaenzt zwei vordefinierte LocalOnly-Konfigurationen:

| Agent | AgentId | Workspace | Position |
| --- | --- | --- | --- |
| Agent A | `rkws-agent-a` | `Workspace-A` | `Left` |
| Agent B | `rkws-agent-b` | `Workspace-B` | `Right` |

Beide Konfigurationen verwenden eigene `AgentRuntime`-Instanzen und erzeugen eigene `RuntimeEngine`-Instanzen. Es gibt keinen gemeinsamen Singleton-Zustand zwischen den Agenten.

## Local IPC

MA004.03 ergaenzt einen lokalen IPC-Modus ueber Named Pipes. MA004.04 legt diesen Modus unter die Transport Abstraction Layer:

- Agent B kann als IPC-Server gestartet werden.
- Agent A kann als IPC-Client gestartet werden.
- Nachrichten werden als `TransportMessage` ueber `ITransportClient` und `ITransportServer` uebertragen.
- `NamedPipeTransport` ist die aktuelle lokale Implementierung.
- Unterstuetzt werden AgentHello, AgentStatusRequest, AgentStatusResponse, WorkspaceAdvertisement, TransferRequest, TransferResponse, ShutdownRequest, ErrorResponse sowie vorbereitete Live-Workspace-Typen.
- Der IPC-Test startet zwei echte Agent-Prozesse ueber `tools/run-local-ipc.ps1`.

Die IPC bleibt lokal. Es werden keine TCP-/UDP-Ports, keine Firewall-Freigaben und keine Netzwerkkommunikation verwendet.

## Lokale Workspace

Bei aktivierter Demo-Workspace registriert der Agent:

- WorkspaceId aus AgentId abgeleitet.
- DisplayName aus `WorkspaceName`.
- WorkspaceType `SmartDevice`.
- Position `Center`.
- State `Available`.
- Trusted `true`.
- Capabilities: Display, Keyboard, Mouse, Clipboard, Encryption, Pairing, OfflineMode und Logging.

## Core-Anbindung

Der Agent verwendet echte Core-Komponenten:

- `RuntimeEngine`
- `WorkspaceRegistry`
- `CapabilityManager`
- `RuntimeDiagnostics`
- Workspace- und Capability-Modelle aus dem Core

## Nicht-Ziele

- kein Netzwerk
- keine echte Discovery
- keine Betriebssystemintegration
- kein Windows-Service
- kein macOS-Daemon
- kein Linux-Systemdienst
- keine GUI
- keine Firmware
- keine Hardware
- keine Cloud
- keine Persistenz

## Bekannte Einschraenkungen

- Der Agent ist LocalOnly.
- Es gibt keine Konfigurationsdatei; Defaults liegen im Code.
- Pro Agent wird genau eine lokale Workspace registriert.
- Transfers zwischen zwei AgentRuntime-Instanzen werden in MA004.02 nur logisch ueber den lokalen Dual-Agent-Harness simuliert.
- Transfers zwischen zwei echten Agent-Prozessen werden seit MA004.04 lokal ueber die TAL und aktuell per NamedPipeTransport simuliert.
- Ctrl+C wird sauber behandelt, aber noch nicht durch Systemdienst-Lifecycle ersetzt.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.3.0 | 2026-07-02 | MA004.04 TAL-Anbindung fuer Agent-IPC dokumentiert. |
| 1.2.0 | 2026-07-02 | Local-IPC-CLI und Named-Pipe-Zwei-Prozess-Modus dokumentiert. |
| 1.1.0 | 2026-07-02 | Dual-Agent-Konfigurationen und Harness-Einschraenkung ergaenzt. |
| 1.0.0 | 2026-07-02 | Workspace Agent Runtime fuer MA004.01 dokumentiert. |
