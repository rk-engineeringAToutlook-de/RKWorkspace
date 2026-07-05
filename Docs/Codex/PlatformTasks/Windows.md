# Windows Platform Tasks

## Plattformziel

Windows bleibt der primaere aktive Testpfad fuer Workspace Shell, Native Overlay, Glass Edge und PDF FrameOnly.

## Aktueller Stand

- Native Glass Edge und mehrere Renderer-Spikes existieren.
- `src/Tools/RKWorkspace.PdfFrameOwner` und `src/Tools/RKWorkspace.FrameGuestSurface` beweisen Original-Owned PDF FrameOnly lokal.
- `src/Surfaces/RKWorkspace.Surface.Abstractions` liefert die gemeinsamen Contracts.

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceProximityProvider`
- `ISurfaceSecurityContext`

## Build-Hinweise

Windows-Projekte muessen den normalen `dotnet build` und `tools/run-tests.ps1` bestehen. Native Windows-Funktionen bleiben in Windows-Projekten, nicht im Protocol.

## Berechtigungen

- globale Hooks spaeter gesondert pruefen
- Desktop-Capture mit Overlay-Exclusion
- Touchpad/Touchscreen/Pen als neutrale Gesten

## Aktuelle Blocker

- noch keine produktive PDF-Page-Rendering-Engine
- noch keine echte OS-weite Objektquelle

## Naechster Codex-Auftrag

Windows PDF Frame Presenter mit Native Glass Edge verbinden und No File Ingress sichtbar nachweisen.

## GitHub und Context Pack

Nur auf Feature-Branch arbeiten. Context-Pack aus `tools/export-codex-context.ps1` vor Plattform-Handoff erzeugen. Kein Push ohne Owner-Freigabe.
