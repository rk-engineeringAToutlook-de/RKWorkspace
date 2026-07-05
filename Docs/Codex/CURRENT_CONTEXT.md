# Current Codex Context

Datum: 2026-07-05

## Branch

```text
feature/ma008-rkwp-devtransport-e2e-frame
```

## Aktueller Auftrag

MA008-Folgepaket baut den ersten RKWP Dev-Transport, Trust/Pairing-Grundlagen und Windows-End-to-End-Frame-Tests auf. Aktueller Stand: MA008.01 RKWP Dev Transport.

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
- `src/Communication/RKWorkspace.Transport.Dev`
- `src/Tools/RKWorkspace.RkwpTransportHarness`

## Semantik

Ein PDF wird nicht auf die Gastablage kopiert. Der Owner erzeugt eine FrameSession. Die Gastablage sieht eine Frame-Repräsentation ohne Originalpfad und ohne Originalbytes.

Input ist policygebunden. Aenderungen laufen als ChangeSet zur Owner-Entscheidung. Ownership Transfer ist kein Default und materialisiert nur bei Approved-Decision.

## Neuer Smoke

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
.\tools\run-rkwp-transport.ps1 -SmokeTest
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

## Android/Linux Handoff

MA007.12 liefert:

- `Docs/Codex/PlatformTasks/Android.md`
- `Docs/Platform/Android_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Android/AndroidSurfacePlan.md`
- `src/Surfaces/RKWorkspace.Surface.Android/AndroidPermissions.md`
- `src/Surfaces/RKWorkspace.Surface.Android/AndroidObjectSources.md`
- `Docs/Codex/PlatformTasks/Linux.md`
- `Docs/Platform/Linux_SurfaceStarterKit.md`
- `src/Surfaces/RKWorkspace.Surface.Linux/LinuxSurfacePlan.md`
- `src/Surfaces/RKWorkspace.Surface.Linux/LinuxPermissions.md`
- `src/Surfaces/RKWorkspace.Surface.Linux/LinuxDisplayServerNotes.md`

## Proximity / Manual Map

MA007.13 liefert:

- `AblageProximitySource`
- `ManualAblageMap`
- `ManualMapAblageProximityProvider`
- stabileren `NearestAblageSelector` mit Confidence, Hysterese und EdgeSwitchDelay
- `ProximityTests: SUCCESS` im Glass-Edge-Smoke
- aktualisierte Dongle-/Transport-/Roadmap-Dokumentation

## Readiness / Cross Device

MA007.14 liefert:

- `Docs/Readiness/MA007_ReadinessReview.md`
- `Docs/Readiness/CrossDeviceTestPlan_Windows_macOS_iPad.md`
- `Docs/Readiness/NextCodexActions.md`
- `release/MA007_READINESS_SUMMARY.md`

## RKWP Dev Transport

MA008.01 liefert:

- `src/Communication/RKWorkspace.Transport/Rkwp/`
- `src/Communication/RKWorkspace.Transport.Dev/`
- `src/Tools/RKWorkspace.RkwpTransportHarness/`
- `tools/run-rkwp-transport.ps1`
- `Docs/Protocol/RKWP_DevTransport.md`

Gewaehlter Dev-Transport: `NamedPipeDev`.

Smoke:

```powershell
.\tools\run-rkwp-transport.ps1 -SmokeTest
```

Geprueft werden `AblageHello`, `AblageCapabilities`, SessionId, `CarryLeaseHeartbeat`, `FrameUpdate`, Error Message, Timeout, Disconnect und No-Hang-Verhalten.

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Der Zeitstempel-Pfad wird bei jedem Export zusaetzlich ausgegeben.
