# Glass Edge PDF Frame E2E

Status: Draft  
Datum: 2026-07-05

## Ziel

MA008.04 verbindet die gläserne Kante mit dem Original-Owned PDF Frame Ablauf. Die Glass Edge ist damit nicht mehr nur Visualisierung. Sie startet den FrameOnly-Pfad zur naechsten Ablage.

## Ausfuehren

```powershell
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -PdfPath "samples\Objects\Rechnung.pdf"
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -UseManualMap -TargetAblage macOS -PdfPath "samples\Objects\Rechnung.pdf"
```

## Ablauf

1. PDF liegt auf der Owner-Ablage.
2. `NearestAblageSelector` waehlt die naechste Ablage.
3. `GlassEdge` erscheint an der passenden Kante.
4. `ObjectEnteringEdge` erzeugt den RKWP-Frame-Pfad.
5. `CarryLease` wird aktiv.
6. `FrameSession` wird aktiv.
7. Guest sieht den PDF-Frame.
8. Guest erhaelt keine PDF-Datei, keinen Originalpfad und keine Originalbytes.
9. Rueckgabe und Recovery bleiben erfolgreich.

## MA009.06 Manual Map

Der E2E-Pfad kann jetzt die manuelle Raumkarte nutzen:

```powershell
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -UseManualMap -TargetAblage macOS -PdfPath "samples\Objects\Rechnung.pdf"
```

Im Smoke-Test wird eine reproduzierbare ManualMap erzeugt. Erwartet:

- `ProximitySource: ManualMap`
- `NearestAblage: Ablage macOS`
- `EdgeDirection: Right`
- `CarryLease: Active`
- `FrameSession: Active`
- `GuestHasPdfFile: NO`
- `NoFileIngress: SUCCESS`
- `Return: SUCCESS`
- `Recovery: SUCCESS`

## RKWP Event Flow

Der Smoke erzeugt folgende RKWP-Events:

- `GlassEdgeAppearing`
- `GlassEdgeActive`
- `ObjectEnteringEdge`
- `CarryLeaseRequested`
- `CarryLeaseGranted`
- `FrameSessionOpen`
- `FrameSessionReady`
- `ObjectInTransit`
- `ObjectEmerging`
- `ObjectPlaced`

## Visualisierung

Der aktuelle MA008.04-Smoke ist eine CLI-Simulation. Die native Tastensteuerung bleibt Aufgabe des visuellen Shell-Slices.

Vorgesehene manuelle Demo-Tasten:

- `Ctrl+Alt+Space`: Start / Ding nehmen
- `P`: Demo-Sequenz abspielen
- `R`: Reset
- `Esc`: Exit

## Akzeptanz

Der Smoke ist erfolgreich, wenn `NearestAblageSelected`, `GlassEdgeAppearing`, `GlassEdgeActive`, `ObjectEnteringEdge`, aktive CarryLease, aktive FrameSession, Guest Frame, No File Ingress, ObjectPlaced, Return, Recovery und `RESULT: SUCCESS` gemeldet werden.
