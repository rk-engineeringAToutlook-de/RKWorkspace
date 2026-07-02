# Local IPC Two Process Test

Dokument-ID: RKWS-DEV-LOCAL-IPC-TWO-PROCESS
Version: 1.2.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Der Local IPC Two Process Test ist der erste Nachweis, dass zwei echte RK Workspace Agent-Prozesse auf demselben Rechner miteinander kommunizieren koennen.

Der Test ersetzt den MA004.02-Harness nicht. MA004.02 prueft zwei AgentRuntime-Instanzen im selben Prozess. MA004.03 prueft zwei getrennte Prozesse mit lokaler IPC. Ab MA004.04 laeuft diese IPC ueber die Transport Abstraction Layer (TAL).

## Architektur

```text
Agent Process A
  |
  | ITransportClient / TransportMessage
  v
Transport Abstraction Layer
  |
  | NamedPipeTransport
  v
Agent Process B
```

Agent B startet als IPC-Server. Agent A startet als IPC-Client. Beide Prozesse starten ihre eigene `RuntimeEngine` und registrieren jeweils ihre eigene lokale Workspace. Agent und Harness sprechen gegen `ITransport`, `ITransportClient`, `ITransportServer` und `TransportMessage`. `NamedPipeTransport` ist nur die aktuelle lokale Implementierung.

## IPC-Entscheidung

MA004.03 verwendet Named Pipes direkt. MA004.04 legt diese Logik unter die TAL.

Gruende:

- lokal auf demselben Rechner
- keine TCP-/UDP-Ports
- keine Firewall-Themen
- klare Client-/Server-Rollen
- geeignet fuer den naechsten Schritt zu echten Agent-Prozessen
- austauschbar gegen spaetere Transportarten, ohne den Agent-Ablauf umzubauen

## Nachrichtenformat

Die IPC nutzt ab MA004.04 neutrale Transport-JSON-Nachrichten mit diesen Feldern:

```text
MessageId
MessageType
SourceId
TargetId
Timestamp
Payload
CorrelationId
Headers
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
- LiveSessionEvent
- WorkspaceWindowFrame
- WorkspaceObjectUpdate
- InputEvent

Die Live-Workspace-Typen sind nur vorbereitet. MA004.04 implementiert noch keine Live-Sitzung, kein Streaming und keine Eingabeweiterleitung.

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

Fehler werden als `ErrorResponse` oder als fehlgeschlagenes `TransportResult` gemeldet. Die alte `LocalIpcResult`-Schicht bleibt fuer Kompatibilitaet erhalten und mappt intern auf `TransportResult`. Der Test besitzt zusaetzlich einen aeusseren 30-Sekunden-Timeout in `tools/run-tests.ps1`.

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

Nach MA004.04 wurde zuerst der Interactive Workspace Prototype im Developer Studio umgesetzt. Nach der gemeinsamen Bedienpruefung wird entschieden, ob die GUI weiter verbessert, Local Discovery gebaut oder echter Netzwerktransfer begonnen wird.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.2.0 | 2026-07-02 | Nachfolger-Hinweis nach Interactive Workspace Prototype aktualisiert. |
| 1.1.0 | 2026-07-02 | MA004.04 TAL-Anbindung und neutrales TransportMessage-Format dokumentiert. |
| 1.0.0 | 2026-07-02 | Local IPC Two Process Test fuer MA004.03 dokumentiert. |
