# MA017 Owner Final Guide

Status: Pilot Ready
Datum: 2026-07-07

## Ziel

Dieser Guide fuehrt den Owner in den ersten echten MA017-Pilotlauf.

## Vorbereitete Testpfade

1. Windows lokal geschlossene PDF als Capsule testen.
2. Windows lokal geoeffnete PDF als OpenFrame testen.
3. No File Ingress pruefen.
4. Rueckgabe und Recovery pruefen.
5. Glass Edge als Richtung zur naechsten Ablage pruefen.
6. UWB/Manual-Map-Simulation pruefen.
7. macOS Handoff an separatem Codex/macOS vorbereiten.
8. iPad/iPhone Handoff ueber Xcode vorbereiten.

## Hauptbefehle

```powershell
.\tools\run-ma017-final-verification.ps1 -SkipHeavy
.\tools\run-ma017-windows-pdf-pilot.ps1 -SmokeTest
.\tools\run-ma017-windows-all-pdf-lifecycle.ps1 -SmokeTest
.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest
.\tools\run-ma017-final-cleanup-checkpoint.ps1 -SmokeTest -AllowDirty
```

## Wichtig

- Das Original bleibt beim Owner.
- Die Gastablage erhaelt keine PDF-Datei.
- macOS und iOS/iPadOS sind vorbereitet, aber native Hardwareausfuehrung muss auf den Zielgeraeten erfolgen.
- Security-Warnungen fuer Development/Pilot sind sichtbar und beabsichtigt.
- Go/No-Go entscheidet der Owner nach Testlauf, nicht der Build allein.

## Result

```text
MA017OwnerFinalGuide: READY
RESULT: SUCCESS
```
