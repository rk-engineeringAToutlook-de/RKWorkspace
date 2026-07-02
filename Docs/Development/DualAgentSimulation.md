# Dual Local Agent Simulation

Dokument-ID: RKWS-DEV-DUAL-AGENT-SIMULATION
Version: 1.0.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Die Dual Local Agent Simulation ist der erste Nachweis, dass zwei RK Workspace Agenten gleichzeitig betrieben werden koennen. Beide Agenten laufen noch im selben Testprozess, werden aber als getrennte Runtime-Instanzen behandelt.

Der Harness bereitet MA004.03 Local IPC vor. Erst dort kommunizieren zwei echte Prozesse miteinander.

## Architektur

Der Harness erzeugt:

- Agent A mit `rkws-agent-a`, `Workspace-A` und Position `Left`.
- Agent B mit `rkws-agent-b`, `Workspace-B` und Position `Right`.

Jeder Agent besitzt:

- eigene `RuntimeEngine`
- eigene `WorkspaceRegistry`
- eigenen `CapabilityManager`
- eigenen `TransferObjectManager`
- eigene Runtime Diagnostics

Der Harness verwendet keinen gemeinsamen Singleton zwischen den Agenten.

## Logische Transfer-Simulation

Der Transfer laeuft in MA004.02 bewusst noch nicht ueber Netzwerk oder IPC:

```text
Agent A
  |
  | TransferRequest
  v
Dual Agent Harness
  |
  | logische Zieluebergabe
  v
Agent B Workspace
  |
  v
TransferResult
```

Das TransferObject wird nur im TransferObjectManager von Agent A angelegt. Agent B erhaelt keine Objektkopie. Der Harness erzeugt fuer die Zielauflosung einen lokalen Transfer-Kontext mit den Workspace-Snapshots beider Agenten.

## Startbefehl

```powershell
.\tools\run-dual-agent.ps1
```

Erwartete Kernausgabe:

```text
RK Workspace Dual Agent Harness
Agent A: rkws-agent-a
Agent B: rkws-agent-b
Transfer Result: SUCCESS
RESULT: SUCCESS
```

## Gepruefte Regeln

- Zwei Agenten starten parallel.
- Zwei Agenten stoppen parallel.
- AgentIds sind verschieden.
- WorkspaceIds sind verschieden.
- RuntimeEngine-Instanzen sind verschieden.
- Core-Manager-Instanzen sind verschieden.
- TransferRequest wird fuer Agent A erzeugt.
- Ziel ist Agent B.
- TransferObject existiert nur einmal.
- FinalState ist `Completed`.
- History enthaelt Created, Validated, MetadataUpdated, Prepared und Completed.
- Beide Agent-Runtimes bleiben waehrend des Transfers `Running`.

## Einschraenkungen

- keine Netzwerkkommunikation
- keine IPC
- keine automatische Discovery
- kein Windows-Service, macOS-Daemon oder Linux-Systemdienst
- keine Persistenz
- keine Cloud
- keine Firmware
- keine Hardware

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | Dual Local Agent Simulation fuer MA004.02 dokumentiert. |
