# Windows Platform Tasks

## Plattformziel

Windows bleibt der primaere aktive Testpfad fuer Workspace Shell, Native Overlay, Glass Edge und PDF FrameOnly.

## Aktueller Stand

- Native Glass Edge und mehrere Renderer-Spikes existieren.
- `src/Tools/RKWorkspace.PdfFrameOwner` und `src/Tools/RKWorkspace.FrameGuestSurface` beweisen Original-Owned PDF FrameOnly lokal.
- `src/Surfaces/RKWorkspace.Surface.Abstractions` liefert die gemeinsamen Contracts.
- `src/Agents/RKWorkspace.Agent.Windows` bereitet den Windows Agent Dev Host ohne produktive Installation vor.

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
- Windows Agent startet in MA010.08 ohne Admin-Rechte und ohne Service-Installation.

## Aktuelle Blocker

- noch keine produktive PDF-Page-Rendering-Engine
- noch keine echte OS-weite Objektquelle

## Naechster Codex-Auftrag

Windows PDF Frame Presenter mit Native Glass Edge verbinden, Windows Agent Dev Host stabilisieren und No File Ingress sichtbar nachweisen.

## MA010.08 Windows Agent Dev

Smoke:

```powershell
.\tools\run-windows-agent-dev.ps1 -SmokeTest
```

Vorbereitete Stubs:

```powershell
.\tools\install-windows-agent-dev.ps1 -SmokeTest
.\tools\uninstall-windows-agent-dev.ps1 -SmokeTest
```

Es erfolgt noch keine produktive Installation.

## GitHub und Context Pack

Nur auf Feature-Branch arbeiten. Context-Pack aus `tools/export-codex-context.ps1` vor Plattform-Handoff erzeugen. Kein Push ohne Owner-Freigabe.

## MA016 Aufgaben

- Windows Closed PDF Capsule als Standard-Pilot stabilisieren.
- Windows Open PDF Context als OpenFrame-Pilot vorbereiten.
- Owner Lock, Return, Recovery und verbotene sichtbare Sprache pruefen.
- No File Ingress fuer Capsule und OpenFrame beweisen.
- Pilot-Skripte fuer Windows lokal, Windows-to-macOS und Windows-to-iPad bereitstellen.
- DevMode-/Security-Warnungen sichtbar halten.
