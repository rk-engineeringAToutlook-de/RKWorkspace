# Developer Workspace Studio

Dokument-ID: RKWS-DEV-DEVELOPER-STUDIO
Version: 1.0.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Das Developer Workspace Studio ist eine erste sichtbare Core-Testoberflaeche fuer RK Workspace. Es dient Entwicklung, Diagnose, Tests, Demonstration und Core-Visualisierung.

Das Studio ist keine Endanwender-GUI und kein Produktagent.

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

Die Oberflaeche ist in vier Bereiche gegliedert:

- Links: Workspace-Liste mit Name, Type, Position, State, Trusted und Priority.
- Mitte: Transfer Objects mit Object Type, Display Name, State, Source und Target.
- Rechts: Diagnostics mit Runtime State, Plugin Count, Workspace Count, Transfer Object Count, Capabilities, Last Result und Last Error.
- Unten: Log mit Zeit, Aktion, Ergebnis und Fehler.

## Aktionen

Minimal verfuegbare Aktionen:

- Start Runtime
- Add Demo Workspaces
- Create Text Object
- Transfer Right
- Reset
- Run Full Demo

`Run Full Demo` startet die Runtime, erzeugt `RKWS-Demo-Laptop` und `RKWS-Demo-Display-Right`, erzeugt ein Textobjekt `Hallo von RK Workspace`, fuehrt einen Transfer nach rechts aus und erwartet `SUCCESS`.

## Core-Anbindung

Das Studio verwendet echte Core-Komponenten:

- `RuntimeEngine`
- `WorkspaceRegistry`
- `CapabilityManager`
- `TransferObjectManager`
- `TransferEngine`
- Core-Modelle fuer Workspaces, Capabilities und Transfer Objects

Der Ablauf wird nicht als separate Studio-Logik dupliziert. Das Studio ruft den vorhandenen Core auf und visualisiert dessen Zustand.

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
- Es gibt noch keine Persistenz und keine gespeicherten Studio-Profile.
- Das Log ist nur eine In-Memory-Ansicht.
- Das Studio nutzt Demo-Daten und keine automatische Discovery.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | Developer Workspace Studio fuer MA003.08 dokumentiert. |
