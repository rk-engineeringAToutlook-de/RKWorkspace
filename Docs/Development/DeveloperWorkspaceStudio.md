# Developer Workspace Studio

Dokument-ID: RKWS-DEV-DEVELOPER-STUDIO
Version: 1.3.0
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
- Separat: Multi Window Prototype mit zwei echten Workspace-Fenstern, gemeinsamem Core-Kontext, Objektlisten, History, Diagnostics und Log je Fenster.

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
- Open Multi Window Prototype

`Run Full Demo` startet die Runtime, erzeugt `RKWS-Demo-Laptop` und `RKWS-Demo-Display-Right`, erzeugt ein Textobjekt `Hallo von RK Workspace`, fuehrt einen Transfer nach rechts aus und erwartet `SUCCESS`.

`Run Full Interactive Demo` initialisiert denselben interaktiven Zustand, simuliert Drag, Target Highlight und Drop auf Workspace B und erwartet `InteractiveDemo: SUCCESS`.

`Open Multi Window Prototype` oeffnet zwei echte Windows-Forms-Fenster fuer `Window A / Laptop` und `Window B / Display Right`. Beide Fenster teilen sich denselben `MultiWindowWorkspaceContext` und aktualisieren sich bei Core-Aenderungen gegenseitig.

## Interaktiver Workspace-Prototyp

Der interaktive Bereich zeigt:

- `Workspace A / Laptop` links
- `Workspace B / Display Right` rechts
- `Text Object` mit `Hallo von RK Workspace`

Die Textkarte kann mit der Maus gegriffen und nach rechts gezogen werden. Beim Ziehen ueber Workspace B wird das Ziel hervorgehoben. Beim Loslassen ueber Workspace B erzeugt das Studio einen `TransferRequest` und fuehrt `TransferEngine.ExecuteLogicalTransfer()` aus. Nach Erfolg liegt die Karte sichtbar in Workspace B, Diagnostics zeigen `Last Result: SUCCESS`, und die History enthaelt den abgeschlossenen Core-Ablauf.

## Multi Window Workspace-Prototyp

Der Multi Window Prototype zeigt zwei echte OS-Fenster:

- `Window A / Laptop`
- `Window B / Display Right`

Beide Fenster verwenden denselben laufenden Core-Kontext mit `RuntimeEngine`, `WorkspaceRegistry`, `CapabilityManager`, `TransferObjectManager` und `TransferEngine`. Window A enthaelt mehrere Transferobjekte: zwei Texte, ein PDF, ein Bild und einen Link. Window B ist das logische Ziel rechts.

Die Objektkarten in Window A koennen per Maus aus dem Fenster herausgezogen und ueber Window B losgelassen werden. Window B zeigt waehrend des Drag-Vorgangs eine Ziel-Hervorhebung. Beim Drop wird kein Studio-Sonderpfad verwendet, sondern `TransferEngine.ExecuteLogicalTransfer()`. Nach Erfolg wird das Objekt in Window B angezeigt, beide Fenster schreiben Logeintraege, die History zeigt den Core-Ablauf, und eine einfache Animation visualisiert den eingehenden Transfer.

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

Developer Studio zeigt lokale Simulationen, Dual-Agent-Diagnose, den interaktiven Workspace-Prototyp und den Multi Window Workspace Prototype. Die interaktiven Demos verwenden keinen Local-IPC-Kanal und keine Netzwerkfunktion.

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
- Der Smoke-Test prueft den Multi-Window-Ablauf viewmodelbasiert ohne echte UI-Automation.
- Es gibt noch keine Persistenz und keine gespeicherten Studio-Profile.
- Das Log ist nur eine In-Memory-Ansicht.
- Das Studio nutzt Demo-Daten und keine automatische Discovery.
- Das Studio spricht weiterhin nicht selbst mit dem Local-IPC-Kanal.
- Drag-and-Drop im Single-Window-Prototyp ist nur fuer das Textobjekt von Workspace A nach Workspace B vorgesehen.
- Drag-and-Drop im Multi-Window-Prototyp ist fuer Demo-Transferobjekte von Window A nach Window B vorgesehen.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.3.0 | 2026-07-02 | Multi Window Workspace Prototype im Developer Studio dokumentiert. |
| 1.2.0 | 2026-07-02 | Interactive Workspace Prototype, Drag-and-Drop und erweiterten Smoke-Test dokumentiert. |
| 1.1.0 | 2026-07-02 | Hinweis zur spaeteren IPC-Anbindung ergaenzt. |
| 1.0.0 | 2026-07-02 | Developer Workspace Studio fuer MA003.08 dokumentiert. |
