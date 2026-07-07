# macOS Pilot Runbook - MA017

Status: final
Datum: 2026-07-07

## Windows Vorbereitung

~~~powershell
.\tools\run-ma017-macos-handoff.ps1 -SmokeTest
.\tools\run-pilot-windows-to-mac-closed-pdf.ps1 -SmokeTest -Policy CriticalInfrastructure
.\tools\run-pilot-windows-to-mac-open-pdf.ps1 -SmokeTest -Policy CriticalInfrastructure
~~~

## macOS Vorbereitung

1. Repository auf macOS klonen.
2. Xcode oeffnen.
3. SwiftUI Minimal App anlegen.
4. `release/ma017/packages/apple-rkwp-swift-bundle/RKWPModels.swift` einbinden.
5. `release/ma017/config/macos-guest.sample.json` als lokale Dev-Konfiguration uebernehmen.
6. Sample Messages in lokale Unit Tests laden.

## Pilotfluss

1. Windows Owner haelt `Rechnung.pdf`.
2. macOS Guest meldet `AblageHello`.
3. Windows sendet Capsule oder OpenFrame.
4. macOS zeigt `liegt hier im Frame`.
5. macOS beweist NoFileIngress.
6. macOS sendet `zurueckgeben`.
7. Windows bestaetigt Rueckgabe oder Recovery.

## Abbruch

Abbrechen, wenn:

- macOS eine PDF-Datei, Originalbytes oder einen Originalpfad bekommt.
- sichtbare technische Sprache auftaucht.
- Return nicht reproduzierbar ist.
- Recovery nicht reproduzierbar ist.
