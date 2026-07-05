# Current Codex Context

Datum: 2026-07-05

## Branch

```text
feature/ma007-followup-original-owned-frame-platforms
```

## Aktueller Auftrag

MA007-Folgepaket fuehrt Original-Owned Frame weiter und bereitet Plattform-Handoffs vor. Aktueller Stand: MA007.11 iOS/iPadOS Surface Starter Kit.

## Implementierte Schichten

- `src/Protocol/RKWorkspace.Protocol`
- `src/Protocol/RKWorkspace.Protocol/Ownership`
- `src/Frame/RKWorkspace.Frame.Pdf`
- `src/Surfaces/RKWorkspace.Surface.Abstractions`
- `src/Surfaces/RKWorkspace.Surface.Windows`
- `src/Surfaces/RKWorkspace.Surface.macOS`
- `src/Surfaces/RKWorkspace.Surface.iOS`
- `src/Surfaces/RKWorkspace.Surface.iOS_iPadOS`
- `src/Surfaces/RKWorkspace.Surface.Android`
- `src/Surfaces/RKWorkspace.Surface.Linux`
- `tests/Unit/RKWorkspace.Protocol.Tests`
- `src/Tools/RKWorkspace.PdfFrameOwner`
- `src/Tools/RKWorkspace.FrameGuestSurface`
- `src/Adapters/RKWorkspace.ObjectAdapter.Windows`

## Semantik

Ein PDF wird nicht auf die Gastablage kopiert. Der Owner erzeugt eine FrameSession. Die Gastablage sieht eine Frame-Repräsentation ohne Originalpfad und ohne Originalbytes.

Input ist policygebunden. Aenderungen laufen als ChangeSet zur Owner-Entscheidung. Ownership Transfer ist kein Default und materialisiert nur bei Approved-Decision.

## Neuer Smoke

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
```

## Surface Foundation

MA007.02 legt die Surface-Vertraege als einzelne Dateien an: `ISurfaceHost`, `ISurfaceOverlay`, `ISurfaceGestureProvider`, `ISurfaceHapticsProvider`, `ISurfaceProximityProvider`, `ISurfaceFramePresenter`, `ISurfaceInputChannel`, `ISurfacePlacementAdapter`, `ISurfaceObjectAdapter`, `ISurfaceSecurityContext`, `GestureType`, `GestureState`, `SurfaceGestureEvent`, `SurfaceCapabilities`, `SurfacePlatform` und `SurfaceException`.

## macOS Handoff

MA007.10 liefert:

- `Docs/Codex/PlatformTasks/macOS.md`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/SurfaceHostStub.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/FrameGuestSurfacePlan.md`
- `src/Surfaces/RKWorkspace.Surface.macOS/macOSPermissions.md`

## iOS/iPadOS Handoff

MA007.11 liefert:

- `Docs/Codex/PlatformTasks/iOS_iPadOS.md`
- `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/iOSSurfacePlan.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/iOSPermissions.md`
- `src/Surfaces/RKWorkspace.Surface.iOS/iOSXcodeHandoff.md`
