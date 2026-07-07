# MA017 Owner Short Guide

Status: Draft
Datum: 2026-07-07

## Worum es geht

MA017 testet den ersten realeren RK Workspace Pilotpfad:

```text
Ein digitales Ding bleibt original-owned und erscheint als Kapsel oder Frame auf einer anderen Ablage.
```

Der Pilot ist noch kein Produkt.

## Was du testen kannst

- Geschlossene PDF-Kapsel.
- Geoeffnete PDF als Frame.
- Windows Owner.
- macOS Guest Handoff.
- iPad/iPhone Guest Handoff.
- UWB/Dongle-Proximity Simulation.
- Glass Edge als naechste Ablage.

## Was nicht passieren darf

- Keine freie PDF-Datei auf der Guest-Ablage.
- Kein Originalpfad auf der Guest-Ablage.
- Keine PDF-Bytes im Guest-Dateisystem.
- Kein Besitzuebergang im Pilotpfad.

## Wichtigster Satz

```text
Das Original bleibt beim Owner.
```

## Startbefehle

Windows Pilot:

```powershell
.\tools\run-ma017-windows-pdf-pilot.ps1 -SmokeTest
```

macOS Handoff:

```powershell
.\tools\run-ma017-macos-handoff.ps1 -SmokeTest
```

iOS/iPad Handoff:

```powershell
.\tools\run-ma017-ios-handoff.ps1 -SmokeTest
```

UWB/Dongle:

```powershell
.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest
```

Gesamt:

```powershell
.\tools\run-ma017-smoke.ps1 -SkipHeavy
```

## Owner-Frage

Nach jedem Test nur diese Frage beantworten:

```text
Hatte ich das Gefuehl, dass alles in meinem Arbeitsraum passiert?
```
