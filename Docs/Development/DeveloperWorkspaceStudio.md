# Developer Workspace Studio

Dokument-ID: RKWS-DEV-DEVELOPER-STUDIO
Version: 1.2.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Das Developer Workspace Studio ist eine erste sichtbare Core-Testoberflaeche fuer RK Workspace. Es dient Entwicklung, Diagnose, Tests, Demonstration und Core-Visualisierung.

Das Studio ist keine Endanwender-GUI und kein Produktagent. Der interaktive Workspace-Prototyp ist ebenfalls nur ein Entwicklungsprototyp.

## Technologie

Die erste Version verwendet Windows Forms auf .NET 8:

- einfache Windows-faehige Desktop-Technologie
- direkte Projektanbindung an `src/Core/RKWorkspace.Core.csproj`
- keine Web-App
- keine Mobile-GUI
- keine komplexe UI-Architektur

## Start

```powershell
.\tools\run-studio.ps1
```

Automatisierter Smoke-Test ohne dauerhaft offene GUI:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

## Layout

Die Oberflaeche ist in fuenf Bereiche gegliedert:

- Oben: Interaktiver Workspace-Prototyp mit Workspace A links, Workspace B rechts und einer sichtbaren Textkarte.
- Mitte links: Workspace-Liste mit Name, Type, Position, State, Trusted und Priority.
- Mitte: Transfer Objects mit Object Type, Display Name, State, Source und Target.
- Mitte rechts: Agents und Diagnostics mit Runtime State, Plugin Count, Workspace Count, Transfer Object Count, Capabilities, Last Result und Last Error.
- Unten: Log und Transfer-History.

## Aktionen

Minimal verfuegbare Aktionen:

- Start Runtime
- Add Demo Workspaces
- Create Text Object
- Transfer Right
- Reset
- Run Full Demo
- Reset Interactive Demo
- Run Full Interactive Demo

`Run Full Demo` startet die Runtime, erzeugt `RKWS-Demo-Laptop` und `RKWS-Demo-Display-Right`, erzeugt ein Textobjekt `Hallo von RK Workspace`, fuehrt einen Transfer nach rechts aus und erwartet `SUCCESS`.

`Run Full Interactive Demo` initialisiert denselben interaktiven Zustand, simuliert Drag, Target Highlight und Drop auf Workspace B und erwartet `InteractiveDemo: SUCCESS`.

## Interaktiver Workspace-Prototyp

Der interaktive Bereich zeigt:

- `Workspace A / Laptop` links
- `Workspace B / Display Right` rechts
- `Text Object` mit `Hallo von RK Workspace`

Die Textkarte kann mit der Maus gegriffen und nach rechts gezogen werden. Beim Ziehen ueber Workspace B wird das Ziel hervorgehoben. Beim Loslassen ueber Workspace B erzeugt das Studio einen `TransferRequest` und fuehrt `TransferEngine.ExecuteLogicalTransfer()` aus. Nach Erfolg liegt die Karte sichtbar in Workspace B, Diagnostics zeigen `Last Result: SUCCESS`, und die History enthaelt den abgeschlossenen Core-Ablauf.

## Core-Anbindung

Das Studio verwendet echte Core-Komponenten:

- `RuntimeEngine`
- `WorkspaceRegistry`
- `CapabilityManager`
- `TransferObjectManager`
- `TransferEngine`
- Core-Modelle fuer Workspaces, Capabilities und Transfer Objects

Der Ablauf wird nicht als separate Studio-Logik dupliziert. Das Studio ruft den vorhandenen Core auf und visualisiert dessen Zustand.

## IPC-Hinweis

Developer Studio zeigt lokale Simulationen, Dual-Agent-Diagnose und den interaktiven Workspace-Prototyp. Die interaktive Demo verwendet keinen Local-IPC-Kanal und keine Netzwerkfunktion.

## Nicht-Ziele

- keine Netzwerkfunktion
- keine Betriebssystemintegration
- keine Firmwarefunktion
- keine Hardwarefunktion
- keine Cloud-Funktion
- keine produktive Endanwender-GUI
- keine stabilen UI-Automation-Tests

## Bekannte Einschraenkungen

- Das Studio ist ein Windows-Desktop-Tool.
- Der Smoke-Test prueft den Core-Ablauf ohne sichtbares Fenster.
- Der Smoke-Test prueft den interaktiven Demo-Ablauf viewmodelbasiert ohne echte UI-Automation.
- Es gibt noch keine Persistenz und keine gespeicherten Studio-Profile.
- Das Log ist nur eine In-Memory-Ansicht.
- Das Studio nutzt Demo-Daten und keine automatische Discovery.
- Das Studio spricht weiterhin nicht selbst mit dem Local-IPC-Kanal.
- Drag-and-Drop ist nur fuer das Textobjekt von Workspace A nach Workspace B vorgesehen.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.2.0 | 2026-07-02 | Interactive Workspace Prototype, Drag-and-Drop und erweiterten Smoke-Test dokumentiert. |
| 1.1.0 | 2026-07-02 | Hinweis zur spaeteren IPC-Anbindung ergaenzt. |
| 1.0.0 | 2026-07-02 | Developer Workspace Studio fuer MA003.08 dokumentiert. |
