# Local IPC Two Process Test

Dokument-ID: RKWS-DEV-LOCAL-IPC-TWO-PROCESS
Version: 1.0.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Der Local IPC Two Process Test ist der erste Nachweis, dass zwei echte RK Workspace Agent-Prozesse auf demselben Rechner miteinander kommunizieren koennen.

Der Test ersetzt den MA004.02-Harness nicht. MA004.02 prueft zwei AgentRuntime-Instanzen im selben Prozess. MA004.03 prueft zwei getrennte Prozesse mit lokaler IPC.

## Architektur

```text
Agent Process A
  |
  | Named Pipe
  v
Agent Process B
```

Agent B startet als IPC-Server. Agent A startet als IPC-Client. Beide Prozesse starten ihre eigene `RuntimeEngine` und registrieren jeweils ihre eigene lokale Workspace.

## IPC-Entscheidung

MA004.03 verwendet Named Pipes.

Gruende:

- lokal auf demselben Rechner
- keine TCP-/UDP-Ports
- keine Firewall-Themen
- klare Client-/Server-Rollen
- geeignet fuer den naechsten Schritt zu echten Agent-Prozessen

## Nachrichtenformat

Die IPC nutzt einfache JSON-Nachrichten mit diesen Feldern:

```text
MessageId
MessageType
SourceAgentId
TargetAgentId
Timestamp
Payload
```

Unterstuetzte MessageTypes:

- AgentHello
- AgentStatusRequest
- AgentStatusResponse
- WorkspaceAdvertisement
- TransferRequest
- TransferResponse
- ShutdownRequest
- ErrorResponse

## Startbefehle

Agent B als IPC-Server:

```powershell
.\tools\run-agent.ps1 -AgentId rkws-agent-b -WorkspaceName Workspace-B -Position Right -IpcServer rkws-b
```

Agent A als IPC-Client:

```powershell
.\tools\run-agent.ps1 -AgentId rkws-agent-a -WorkspaceName Workspace-A -Position Left -IpcClient rkws-b -TargetAgentId rkws-agent-b -Once
```

Automatisierter Zwei-Prozess-Test:

```powershell
.\tools\run-local-ipc.ps1
```

Erwartete Kernausgabe:

```text
RK Workspace Local IPC Harness
Agent B started
Agent A started
AgentHello: OK
StatusRequest: OK
TransferRequest: OK
TransferResponse: SUCCESS
Agent A stopped
Agent B stopped
RESULT: SUCCESS
```

## Fehlerfaelle

Der Harness prueft:

- Server nicht erreichbar
- ungueltige Nachricht
- unbekannter MessageType
- falscher TargetAgentId
- TransferRequest ohne Payload
- Timeout

Fehler werden als `ErrorResponse` oder als fehlgeschlagenes `LocalIpcResult` gemeldet. Der Test besitzt zusaetzlich einen aeusseren 30-Sekunden-Timeout in `tools/run-tests.ps1`.

## Grenzen

- keine Netzwerkkommunikation
- keine Remote-Kommunikation
- keine echte Discovery
- keine Betriebssystemdienst-Installation
- kein GUI-Kommunikationspartner
- keine Hardware
- keine Cloud
- keine Persistenz

## Naechster Schritt

MA004.04 fuehrt eine lokale Discovery-Simulation ein. Agenten sollen sich dann nicht mehr fest ueber Pipe-Namen kennen muessen, sondern ueber lokale Agent-Announcement- und Lookup-Mechanismen gefunden werden.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | Local IPC Two Process Test fuer MA004.03 dokumentiert. |
