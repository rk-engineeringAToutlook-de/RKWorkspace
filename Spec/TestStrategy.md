# RKWS-0290 Test Strategy

Dokument-ID: RKWS-SPEC-TEST-STRATEGY-001  
Version: 1.9.0
Status: Accepted  
Datum: 2026-07-02

## Zweck

Die Teststrategie definiert, wie RK Workspace langfristig pruefbar bleibt. Architektur, Dokumentation, Simulation, Core, Plattformagenten, Firmware und Hardware muessen reproduzierbar getestet werden.

## Testpyramide

```mermaid
flowchart TB
    Long["Langzeittest / Feldtest"] --> Hardware["Hardware und Firmware"]
    Hardware --> Security["Security und Recovery"]
    Security --> Integration["Integration / Mehrgeraete"]
    Integration --> Simulation["Simulation"]
    Simulation --> Unit["Unit Tests"]
```

## Testbereiche

| Bereich | Ziel | Beispiele |
| --- | --- | --- |
| Unit Tests | Core-Regeln isoliert pruefen. | Objektmodell, WorkspaceMap, TransferPlanner. |
| Integration Tests | Komponenten gemeinsam pruefen. | Agent + Core + Transport-Testdouble. |
| Simulation | Nutzerfluss ohne Plattformrisiko pruefen. | A nach B Texttransfer mit Log. |
| Mehrgeraetebetrieb | Mehrere Arbeitsflaechen und Ziele. | Mehrere rechte Ziele, stale Heartbeats. |
| Offlinebetrieb | Ohne Cloud und eingeschraenktes Netz. | Bereits gepairte lokale Transfers. |
| KVM | Wechselnde Host- und Display-Zuordnung. | KVM Node Trust-Trennung. |
| Industrie | Robuste Diagnose und Policy. | abgeschottete Netze, LAN-only. |
| Headless | Ziel ohne Display. | Ablage und Log ohne Preview. |
| Performance | Latenz, Chunking, Speicher. | Text sofort, Datei spaeter. |
| Security | Pairing, Auth, Revocation. | MITM, Replay, rogue Dongle. |
| Firmware | Boot, Update, Identity. | Recovery, signiertes Update. |
| Hardware | Strom, ESD, Thermik. | Labor- und Produktionstests. |
| Regression | Keine Rueckfaelle. | CI bei Push/PR. |
| Kompatibilitaet | Versionen und Plattformen. | Protocol minor/major. |
| Langzeittest | Stabilitaet ueber Zeit. | Heartbeat, Speicher, Logs. |
| Recovery | Fehler sauber beheben. | Rollback, Schluesselrotation. |

## Foundation-Check

`tools/run-tests.ps1` bleibt der Mindestcheck vor Commit und Push. Er baut den Core, fuehrt Unit-Tests aus, fuehrt Integration-Tests aus, startet die lokale Simulation und prueft den Core Demo Runner. Die Ausgabe ist in Build, Unit Tests, Integration Tests, Simulation und Demo Test getrennt.

Ab MA004.01 prueft `tools/run-tests.ps1` zusaetzlich den Agent Smoke-Test mit `tools/run-agent.ps1 -Once`.

Ab MA004.02 prueft `tools/run-tests.ps1` zusaetzlich den Dual-Agent-Harness mit `tools/run-dual-agent.ps1`.

Ab MA004.03 prueft `tools/run-tests.ps1` zusaetzlich den Local-IPC-Zwei-Prozess-Test mit `tools/run-local-ipc.ps1`. Dieser Teil ist durch einen aeusseren 30-Sekunden-Timeout gegen Haenger abgesichert.

Ab MA004.04 pruefen die Unit-Tests zusaetzlich die Transport Abstraction Layer und die NamedPipeTransport-Implementierung. Der Local-IPC-Zwei-Prozess-Test bleibt im Foundation-Check und laeuft intern ueber die TAL.

## MA003.05 Core Integration Tests

MA003.05 fuehrt das erste separate Integration-Test-Projekt ein: `tests/Integration/RKWorkspace.Core.IntegrationTests/`.

Diese Tests verwenden erstmals alle vier produktiven Core-Komponenten gemeinsam:

- Plugin Manager
- Capability Manager
- Workspace Registry
- Transfer Object Manager

Der erste Szenariosatz prueft einen logischen Texttransfer von Workspace A nach Workspace B, Zielauswahl ueber Position und Capabilities, Fehlerfall ohne passendes Ziel, Auswahl bei genau einem Ziel, verbotene Capabilities und Priority-Tie-Breaks.

Die Integration-Tests verwenden keine Netzwerkkommunikation, keine Betriebssystem-APIs, keine GUI, keine Persistenz, keine Cloud und keine Firmware- oder Hardwarelogik.

## MA003.06 Core Demo Runner Test

MA003.06 fuehrt `src/Demo/RKWorkspace.Core.Demo/` und `tools/run-demo.ps1` ein. Der Demo Runner zeigt den aktuellen Core-Ablauf sichtbar in der Konsole und endet bei Erfolg mit `RESULT: SUCCESS`.

`tools/run-tests.ps1` startet den Demo Runner als Demo Test und prueft:

- Demo-Projekt baut ohne Warnungen und Fehler.
- Demo-Projekt laeuft mit Exitcode 0.
- Ausgabe enthaelt `RESULT: SUCCESS`.

Der Demo Runner ist kein Produktagent, keine GUI, kein Netzwerkdienst, keine Persistenzschicht und kein Plattformadapter.

## MA003.07 Transfer Engine Runtime Tests

MA003.07 erweitert die Unit-Tests um die Transfer Engine Runtime. Geprueft werden TransferRequest, TransferPlan, Planvalidierung, Zielauswahl fuer Right, Left und Any, Required/Forbidden Capabilities, Prepare, Complete, History, fehlende Quelle, fehlendes Ziel, fehlendes Objekt, Cancel, Fail, ExecuteLogicalTransfer und Plattformneutralitaet.

Die Core-Integrationstests verwenden ab MA003.07 die Transfer Engine statt manueller Orchestrierung. Der Szenariosatz bleibt gleich:

- Texttransfer nach rechts.
- Fehlerfall ohne passendes Ziel.
- Auswahl bei genau einem Ziel.
- Ablehnung verbotener Capabilities.
- Priority-Tie-Break bei mehreren passenden Zielen.

Der Demo Runner nutzt ebenfalls `TransferEngine.ExecuteLogicalTransfer`. Damit laufen Unit-Tests, Integration-Tests und sichtbare Demo ueber denselben logischen Core-Pfad.

## MA003.07 Core Runtime Orchestrator Tests

Der Core Runtime Orchestrator wird in Unit-Tests und Integrationstests geprueft. Die Unit-Tests decken RuntimeState, RuntimeConfiguration, Start, Stop, Pause, Resume, Shutdown, Diagnostics, ungueltige Uebergaenge, Initialisierungsfehler und Plattformneutralitaet ab.

Die Integrationstests verwenden die Runtime Engine fuer alle bisherigen Core-Szenarien. Zusaetzlich pruefen sie:

- Runtime initialisiert Plugin Manager, Capability Manager, Workspace Registry und Transfer Object Manager.
- Runtime stoppt aktivierte Plugins sauber.
- Runtime Diagnostics melden RuntimeState, Runtime-Version, Startzeit und Komponentenzaehler korrekt.

Der Demo Runner startet ab diesem Stand zuerst `RuntimeEngine.Start()` und verwendet danach nur die von der Runtime bereitgestellten Manager.

## MA003.08 Developer Workspace Studio Tests

MA003.08 fuehrt `src/Tools/RKWorkspace.DeveloperStudio/` und `tools/run-studio.ps1` ein. Das Studio ist ein Developer-Werkzeug fuer Diagnose, Tests, Demonstration und Core-Visualisierung, keine Endanwender-GUI.

Da stabile UI-Automation zu diesem Zeitpunkt nicht erzwungen wird, gilt fuer MA003.08:

- Das Studio-Projekt muss mit 0 Warnungen und 0 Fehlern bauen.
- `tools/run-studio.ps1 -SmokeTest` muss den vollstaendigen Demo-Pfad ausfuehren und `RESULT: SUCCESS` liefern.
- Bestehende Unit Tests, Integration Tests, Simulation und Demo Runner bleiben gruen.
- Der Core bleibt plattformneutral; die Windows-Desktop-Abhaengigkeit liegt ausschliesslich im separaten Developer-Tool.

## MA004.01 Workspace Agent Runtime Tests

MA004.01 fuehrt `src/Agents/RKWorkspace.Agent/` und `tools/run-agent.ps1` ein. Der Agent ist ein LocalOnly-Konsolenprozess und kein Betriebssystemdienst.

Der Agent Smoke-Test in `tools/run-tests.ps1` fuehrt aus:

```powershell
.\tools\run-agent.ps1 -Once
```

Der Smoke-Test erwartet:

- Exitcode 0.
- Ausgabe enthaelt `RK Workspace Agent`.
- Ausgabe enthaelt `State: Running`.
- Ausgabe enthaelt `stopped cleanly`.

Zusaetzlich bleiben `tools/run-demo.ps1` und `tools/run-studio.ps1 -SmokeTest` als separate Abschlusspruefungen erhalten.

## MA004.02 Dual Local Agent Simulation Tests

MA004.02 fuehrt `tools/DualAgentHarness/` und `tools/run-dual-agent.ps1` ein. Der Harness startet zwei LocalOnly-Agenten im selben Testprozess, ohne Netzwerk, IPC, Discovery oder Betriebssystemdienst.

Der Dual-Agent-Harness prueft:

- paralleler Start von Agent A und Agent B.
- paralleler Stop von Agent A und Agent B.
- unterschiedliche AgentIds.
- unterschiedliche WorkspaceIds.
- getrennte RuntimeEngine-Instanzen.
- getrennte WorkspaceRegistry-, CapabilityManager- und TransferObjectManager-Instanzen.
- TransferRequest-Erzeugung in Agent A.
- logischer Transfer zu Agent B ueber den Harness.
- TransferResult mit erfolgreichem Source/Target-Abschluss.
- TransferObject existiert nur im TransferObjectManager von Agent A.
- History enthaelt Created, Validated, MetadataUpdated, Prepared und Completed.
- beide Agent-Runtimes bleiben waehrend des Transfers stabil.

`tools/run-tests.ps1` prueft fuer den Harness:

- Exitcode 0.
- Ausgabe enthaelt `RK Workspace Dual Agent Harness`.
- Ausgabe enthaelt `Agent A: rkws-agent-a`.
- Ausgabe enthaelt `Agent B: rkws-agent-b`.
- Ausgabe enthaelt `Transfer Result: SUCCESS`.
- Ausgabe enthaelt `RESULT: SUCCESS`.

## MA004.03 Local IPC Two Process Tests

MA004.03 fuehrt `src/Communication/RKWorkspace.LocalIpc/`, `tools/LocalIpcHarness/` und `tools/run-local-ipc.ps1` ein. Der Test startet zwei echte Agent-Prozesse auf demselben Rechner und verwendet Named Pipes als lokale IPC-Technik.

Der Local-IPC-Harness prueft:

- Agent B startet als eigener Prozess und oeffnet einen lokalen Named-Pipe-Server.
- Agent A startet als eigener Prozess und verbindet sich als Named-Pipe-Client.
- AgentHello funktioniert.
- AgentStatusRequest und AgentStatusResponse funktionieren.
- TransferRequest funktioniert.
- TransferResponse liefert `SUCCESS`.
- Agent A und Agent B stoppen sauber.
- nicht erreichbarer Server wird sauber gemeldet.
- ungueltige Nachricht wird sauber gemeldet.
- unbekannter MessageType wird sauber gemeldet.
- falsche TargetAgentId wird sauber gemeldet.
- TransferRequest ohne Payload wird sauber gemeldet.
- Timeout wird sauber gemeldet.

`tools/run-tests.ps1` prueft fuer den Harness:

- Exitcode 0.
- Ausgabe enthaelt `RK Workspace Local IPC Harness`.
- Ausgabe enthaelt `AgentHello: OK`.
- Ausgabe enthaelt `StatusRequest: OK`.
- Ausgabe enthaelt `TransferRequest: OK`.
- Ausgabe enthaelt `TransferResponse: SUCCESS`.
- Ausgabe enthaelt `RESULT: SUCCESS`.
- externer Timeout maximal 30 Sekunden.

## MA004.04 Transport Abstraction Layer Tests

MA004.04 fuehrt `src/Communication/RKWorkspace.Transport/` und `src/Communication/RKWorkspace.Transport.NamedPipes/` ein. Die Tests pruefen die neutrale Transportschicht ohne TCP, UDP, Discovery, Pairing, Hardware, Firmware oder Cloud.

Die Unit-Tests pruefen:

- `TransportMessage`-Erstellung mit den Live-Workspace-Nachrichtentypen.
- `CorrelationId` fuer Request/Response-Beziehungen.
- `TransportEndpoint`-Validierung fuer Named-Pipe-Endpunkte.
- NamedPipeTransport Client/Server Roundtrip.
- Request/Response ueber `ITransportClient.RequestAsync`.
- Timeout-Verhalten.
- Fehlerantwort bei falscher `TargetId`.

Der Local-IPC-Harness prueft weiterhin den vollstaendigen Zwei-Prozess-Pfad:

- Agent und Harness sprechen ueber `ITransport`, `ITransportClient`, `ITransportServer` und `TransportMessage`.
- `NamedPipeTransport` ist nur die lokale Implementierung.
- Die alte LocalIpc-Schicht bleibt als Kompatibilitaetsschicht erhalten und mappt intern auf `TransportResult`.
- Bestehende CLI-Optionen `--ipc-server`, `--ipc-client`, `--target-agent-id` und `--ipc-stop-after-transfer` bleiben kompatibel.

## Querverweise

- `Docs/07_TestPlan.md`
- `Spec/TestSpecification.md`
- `Spec/StateMachine.md`
- `Spec/SecurityModel.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.9.0 | 2026-07-02 | MA004.04 Transport Abstraction Layer Tests dokumentiert. |
| 1.8.0 | 2026-07-02 | MA004.03 Local IPC Two Process Tests dokumentiert. |
| 1.7.0 | 2026-07-02 | MA004.02 Dual Local Agent Simulation Tests dokumentiert. |
| 1.6.0 | 2026-07-02 | MA004.01 Workspace Agent Runtime Smoke-Test dokumentiert. |
| 1.5.0 | 2026-07-02 | MA003.08 Developer Workspace Studio Tests dokumentiert. |
| 1.4.0 | 2026-07-02 | Core Runtime Orchestrator Tests dokumentiert. |
| 1.3.0 | 2026-07-02 | MA003.07 Transfer Engine Runtime Tests dokumentiert. |
| 1.2.0 | 2026-07-02 | MA003.06 Core Demo Runner Test dokumentiert. |
| 1.1.0 | 2026-07-02 | MA003.05 Core Integration Tests und getrennte Testausgabe dokumentiert. |
| 1.0.0 | 2026-07-02 | Vollstaendige Teststrategie fuer RKWS-0290 definiert. |
