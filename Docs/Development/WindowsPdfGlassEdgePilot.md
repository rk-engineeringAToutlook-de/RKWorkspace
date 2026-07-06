# Windows PDF Glass Edge Pilot

Status: MA011.06  
Datum: 2026-07-06

## Ziel

Der Windows PDF Frame Pilot kann den FrameOnly-Ablauf jetzt ueber eine lokale Glass-Edge-Sequenz ausloesen. Der Ablauf bleibt eine Simulation fuer Owner-Tests, nutzt aber dieselbe Semantik wie der Produktpfad:

1. Die PDF liegt auf der Originalablage.
2. Die naechste Ablage wird bestimmt.
3. Eine gläserne Kante erscheint.
4. Das Ding geht in die Kante.
5. Auf der Gastablage oeffnet sich ein Frame.
6. Die PDF-Datei wird nicht auf die Gastablage gelegt.

## Start

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1 -UseGlassEdge -PlaySequence
.\tools\run-windows-pdf-frame-pilot.ps1 -UseGlassEdge -PlaySequence -SmokeTest
.\tools\run-windows-pdf-frame-pilot.ps1 -UseGlassEdge -UseManualMap -PlaySequence
.\tools\run-windows-pdf-frame-pilot.ps1 -UseGlassEdge -PlaySequence -OwnerVisible
```

## Lokale Simulation

Ohne Manual Map erzeugt der Pilot eine lokale Test-Raumkarte:

- aktuelle Ablage: `Ablage Windows Owner`
- naechste Ablage: `Ablage Windows Guest`
- Richtung: `Right`

Mit `-UseManualMap` wird eine vorhandene Manual Map gelesen. Im Smoke-Test oder bei leerer Manual Map nutzt der Pilot eine reproduzierbare Testkarte, damit der Befehl ohne Lab-Vorbereitung stabil laeuft.

## Eventflow

`-PlaySequence` erzeugt folgende RKWP Events:

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

Die Abnahmepunkte fuer MA011.06 fokussieren diese Events:

- `GlassEdgeAppearing`
- `GlassEdgeActive`
- `ObjectEnteringEdge`
- `ObjectInTransit`
- `ObjectEmerging`
- `ObjectPlaced`
- `FrameSessionOpen`
- `FrameSessionReady`

## Sicherheitsstatus

Der Glass-Edge-Pilot veraendert den Ownership-Pfad nicht:

- `CarryLease: Active`
- `FrameSession: Active`
- Gastablage zeigt `liegt hier im Frame`
- `GuestHasPdfFile: NO`
- `GuestHasOriginalPath: NO`
- `GuestHasCopiedPdfBytes: NO`
- `NoFileIngress: SUCCESS`
- Rueckgabe und Recovery bleiben erfolgreich

## Grenzen

Dieser Pilot ist keine finale visuelle Glass-Edge-UX. Er ist eine lokale CLI-/Owner-Test-Sequenz. Native Kantensteuerung, echte Pointer-Gesten und echte plattformuebergreifende Anzeige bleiben in den Shell-/Surface-Slices.
