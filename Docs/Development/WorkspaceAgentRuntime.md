# Workspace Agent Runtime

Dokument-ID: RKWS-DEV-WORKSPACE-AGENT-RUNTIME
Version: 1.0.0
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
```

Direkte Agent-Optionen:

```text
--once
--status
--demo
--no-demo
--help
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
- Es wird genau eine lokale Workspace registriert.
- Es gibt noch keinen Transfer zwischen Agent-Instanzen.
- Ctrl+C wird sauber behandelt, aber noch nicht durch Systemdienst-Lifecycle ersetzt.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | Workspace Agent Runtime fuer MA004.01 dokumentiert. |
