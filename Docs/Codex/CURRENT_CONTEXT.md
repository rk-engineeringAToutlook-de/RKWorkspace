# Current Codex Context

Datum: 2026-07-05

## Branch

```text
feature/ma008-rkwp-devtransport-e2e-frame
```

## Aktueller Auftrag

MA008-Folgepaket baut den ersten RKWP Dev-Transport, Trust/Pairing-Grundlagen und Windows-End-to-End-Frame-Tests auf. Aktueller Stand: MA008.10 Readiness, Next Actions und Integrations-Gate in Arbeit.

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
- `src/Protocol/RKWorkspace.Protocol/Identity`
- `src/Tools/RKWorkspace.WindowsLocalFrameE2E`
- `src/Tools/RKWorkspace.GlassEdgePdfFrameE2E`
- `src/Frame/RKWorkspace.Frame.Pdf/PdfFrameInteractionService.cs`
- `tools/run-windows-owner-for-mac.ps1`
- `src/Tools/RKWorkspace.RkwpDiagnostics`
- `tools/run-rkwp-diagnostics.ps1`
- `src/Protocol/RKWorkspace.Protocol/Security`

## Semantik

Ein PDF wird nicht auf die Gastablage kopiert. Der Owner erzeugt eine FrameSession. Die Gastablage sieht eine Frame-Repräsentation ohne Originalpfad und ohne Originalbytes.

Input ist policygebunden. Aenderungen laufen als ChangeSet zur Owner-Entscheidung. Ownership Transfer ist kein Default und materialisiert nur bei Approved-Decision.

Eine Ablage wird nicht vertraut, nur weil sie technisch erreichbar ist. `AblageIdentity`, `AblageTrustPolicy` und `AblageTrustGate` entscheiden vor Lease und Frame, ob eine Zielablage ueberhaupt berechtigt ist.

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
- `Docs/Readiness/iPad_iPhone_Surface_TestPlan.md`
- `release/handoff/iOS_iPadOS_MA008_Handoff.md`

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

## Ablage Identity / Trust

MA008.02 liefert:

- `AblageIdentity`
- `AblageIdentityId`
- `AblagePublicKey`
- `AblageTrustLevel`
- `AblagePairingState`
- `AblagePairingRequest`
- `AblagePairingDecision`
- `AblageTrustPolicy`
- `AblageTrustGate`
- `DevAblagePairingService`
- `AblageIdentityMessageFactory`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Protocol/RKWP_Pairing.md`

Regel: Unknown, Untrusted, Revoked, Denied und Pending blockieren Lease und Frame. DevTrusted ist nur fuer lokale Dev-Tests gedacht. SecureSessionRequired blockiert `DevelopmentInsecure`.

## Windows Local Frame E2E

MA008.03 liefert:

- `src/Tools/RKWorkspace.WindowsLocalFrameE2E/`
- `tools/run-windows-local-frame-e2e.ps1`
- `Docs/Development/WindowsLocalFrameE2E.md`

Smoke:

```powershell
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
```

Geprueft werden Windows Owner, Windows Guest, DevPairing, NamedPipeDev, echte Sample-PDF, OriginalOwned FrameOnly, aktive CarryLease, Owner-Lock, aktive FrameSession, Guest Frame, No File Ingress, Return und Recovery.

## Glass Edge PDF Frame E2E

MA008.04 liefert:

- `src/Tools/RKWorkspace.GlassEdgePdfFrameE2E/`
- `tools/run-glass-edge-pdf-frame-e2e.ps1`
- `Docs/Development/GlassEdgePdfFrameE2E.md`

Smoke:

```powershell
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -SmokeTest
```

Geprueft werden NearestAblage, GlassEdgeAppearing, GlassEdgeActive, ObjectEnteringEdge, ObjectInTransit, ObjectEmerging, ObjectPlaced, FrameSessionOpen, FrameSessionReady, aktive CarryLease, aktive FrameSession, Guest Frame, No File Ingress, Return und Recovery.

## PDF Frame Interaction

MA008.05 liefert:

- `PdfFrameInteractionService`
- `PdfAnnotationDraft`
- `PdfFrameAnnotationChangeSetResult`
- `Docs/Development/PdfFrameInteraction.md`

Geprueft werden Scroll, Zoom, Annotation als `ChangeSetOperationKind.AnnotationAdded`, ViewOnly-Ablehnung, ungueltige Lease, Apply/Accept als Applied, Reject ohne Originalaenderung und No File Ingress.

## Windows to macOS Handoff

MA008.06 liefert:

- `Docs/Readiness/WindowsToMac_DevTransportPlan.md`
- `release/handoff/WindowsToMac_MA008_Handoff.md`
- `tools/run-windows-owner-for-mac.ps1`

Windows Owner Info:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -InfoOnly
```

Der Windows-Pfad bleibt lokal ueber `NamedPipeDev` verifiziert. Fuer den echten macOS-Test ist ein netzwerkfaehiges DevTransport-Profil der offene Blocker. Ziel bleibt: Windows Owner, macOS Guest, PDF FrameOnly, DevPairing, AblageIdentity, No File Ingress, Heartbeat, Return und Recovery.

## iPad/iPhone Handoff

MA008.07 liefert:

- `Docs/Readiness/iPad_iPhone_Surface_TestPlan.md`
- `release/handoff/iOS_iPadOS_MA008_Handoff.md`

Der mobile Zielpfad ist eine native iOS/iPadOS Surface App ueber macOS-Codex und Xcode. PWA bleibt nur Uebergang. Der minimale Test ist: Windows besitzt PDF, iPad zeigt PDF-Frame, iPad bekommt keine PDF-Datei, Windows bleibt Owner, iPad gibt zurueck.

## RKWP Diagnostics

MA008.08 liefert:

- `src/Tools/RKWorkspace.RkwpDiagnostics/`
- `tools/run-rkwp-diagnostics.ps1`
- `Docs/Development/RKWP_Diagnostics.md`
- `logs/rkwp-audit/` als Development JSONL-Speicher

Smoke:

```powershell
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
```

Geprueft werden LeaseGranted, FrameOpened, Heartbeat, PolicyDenied, FrameReturned, RecoveredByOwner und No File Ingress.

## RKWP Security Gate

MA008.09 liefert:

- `RkwpSecurityConfiguration`
- `RkwpSecurityEnvironmentMode`
- `RkwpSecurityGate`
- `RkwpSecurityGateDecision`
- `Docs/Security/RKWP_SecurityGate.md`

Production lehnt `DevelopmentInsecure`, fehlenden Audit, fehlenden Replay-Schutz und fehlendes Policy Binding ab. Development/Test duerfen unsichere Modi nur sichtbar als nicht-produktiv verwenden.

## MA008 Readiness

MA008.10 liefert:

- `Docs/Readiness/MA008_ReadinessReview.md`
- `Docs/Readiness/MA008_NextActions.md`
- `Docs/Readiness/MA008_TestMatrix.md`
- `release/MA008_READINESS_SUMMARY.md`

Die Readiness trennt Done, Partial, Planned und Blocked fuer DevTransport, AblageIdentity/Trust, Windows Local E2E, Glass Edge PDF Frame, Interactive Input, ChangeSet, OwnershipTransfer, Windows Object Adapter, macOS/iOS Handoff, Android/Linux Starter, Proximity/ManualMap, Dongle Roadmap, Diagnostics und Security Gate.

Gate-Aussage: Struktur und Protokoll stehen fuer Development-Testing. Produktiv fehlen weiterhin netzwerkfaehiger DevTransport, native macOS/iOS Guest Surfaces, produktive Verschluesselung/Mutual Auth, Trust Store, echter Gast-PDF-Renderer und echte Proximity-Hardware.

## MA009 Secure Cross-Device Foundation

MA009.01 liefert den Secure-Session-Spike:

- `RkwpSecureSession`
- `RkwpHandshake`
- `RkwpDevCertificate`
- `RkwpSessionKey`
- `RkwpSecurityPolicy`
- `tools/init-dev-rkwp-identity.ps1`
- `Docs/Protocol/RKWP_SecureSession.md`
- `Docs/Security/RKWP_DevCertificates.md`

MA009.02 bereitet den Windows Owner fuer macOS vor:

- `tools/run-windows-owner-for-mac.ps1 -SmokeTest`
- `release/handoff/WindowsToMac_MA009_Handoff.md`
- DevTransport URL Format `dev+namedpipe://rkws-windows-owner-macos`
- geplanter LAN-Port `43707`
- No File Ingress bleibt Pflicht.

## Context Pack

Stabiler Pfad:

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Finaler AP020-Export:

```text
release/codex-context/RKWorkspace_Context_20260705-235625.zip
```

Der Zeitstempel-Pfad wird bei jedem Export zusaetzlich ausgegeben.
