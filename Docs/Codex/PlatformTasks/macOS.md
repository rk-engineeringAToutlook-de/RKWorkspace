# macOS Platform Tasks

Status: Prepared  
Datum: 2026-07-06

## Plattformziel

macOS ist die erste echte Gegenplattform zum Windows-Owner-Pfad. Die erste native macOS-Arbeit ist eine Frame Guest Surface.

Ziel ist nicht Dateiuebertragung. Ziel ist:

- Windows bleibt Owner der PDF.
- macOS wird als Ablage identifiziert.
- macOS zeigt einen RKWP Frame.
- macOS speichert keine freie PDF-Datei.
- macOS sendet Heartbeat und Return.
- No File Ingress ist beweisbar.

## Aktueller Handoff

MA013 Bootstrap:

```text
release/handoff/macOS_Codex_MA013_Bootstrap.md
```

MA013 RKWP Client:

```text
release/handoff/macOS_Codex_MA013_RKWPClient.md
```

MA013 First Real Test Gate:

```text
release/handoff/WindowsToMac_FirstRealTestGate.md
```

Direkt nutzbare Handoff-Datei:

```text
release/handoff/macOS_Codex_MA009_FrameGuestSurface.md
```

Windows Owner Handoff:

```text
release/handoff/WindowsToMac_MA009_Handoff.md
```

## Vor dem Bauen Lesen

- `Docs/Codex/CURRENT_CONTEXT.md`
- `Docs/Platform/macOS_RepositoryBootstrap.md`
- `release/handoff/macOS_Codex_MA013_Bootstrap.md`
- `release/handoff/macOS_Codex_MA013_RKWPClient.md`
- `Docs/Platform/macOS_FrameGuestUI.md`
- `Docs/Platform/macOS_NoFileIngressChecklist.md`
- `Docs/Platform/macOS_ReturnAndRecovery.md`
- `Docs/Platform/macOS_FrameRenderingStrategy.md`
- `Docs/Platform/macOS_DevAgentPackaging.md`
- `Docs/Platform/macOS_GestureAndGlassEdge.md`
- `Docs/Readiness/WindowsToMac_FirstRealTestGate.md`
- `contracts/macOS-guest/rkwp-macos-guest-contract-v0.1.json`
- `release/schema/rkwp-envelope-schema-v0.1.json`
- `Docs/Platform/macOS_SurfaceStarterKit.md`
- `Docs/Protocol/RKWP_ProtocolFoundation.md`
- `Docs/Protocol/RKWP_SecureSession.md`
- `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Protocol/RKWP_InputChannel.md`
- `Docs/Protocol/RKWP_ChangeSetAndReturn.md`
- `src/Surfaces/RKWorkspace.Surface.Abstractions`
- `src/Surfaces/RKWorkspace.Surface.macOS`

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceInputChannel`
- `ISurfaceHapticsProvider`
- `ISurfaceSecurityContext`
- `SurfacePlatform.MacOS`

## macOS Berechtigungen

Fuer den ersten Frame Guest Test:

- App Sandbox mit Network Client.
- Local Network / Firewall Prompt dokumentieren.
- keine freie Dateiablage fuer Owner-PDF.
- Logs fuer No File Ingress.

Spaeter:

- Accessibility fuer globale Gesten.
- Screen Recording fuer echte Desktop-/Fenstererkennung.
- Security-scoped resources fuer explizit gewaehlt Dateien.
- Trackpad/Force Touch Feedback.
- Notifications optional.

## Technologieoptionen

- Swift/AppKit fuer native Window-/Surface-Kontrolle.
- SwiftUI fuer schnelle Minimal-App.
- PDFKit nur, wenn keine freie Owner-PDF materialisiert wird.
- CoreAnimation/Metal spaeter fuer Glass Edge.
- .NET/MAUI nur pruefen, wenn Surface-Gefuehl und Berechtigungen nicht leiden.

## Kopierbarer Auftrag

Der vollstaendige kopierbare Auftrag steht in:

```text
release/handoff/macOS_Codex_MA009_FrameGuestSurface.md
```

Kernauftrag:

```text
Baue eine macOS RK Workspace Frame Guest Surface.
```

## Blocker

- macOS-Codex/Xcode-Umgebung fehlt in diesem Windows-Thread.
- netzwerkfaehiger DevTransport ist ab MA010.02 als DevLan-Lab-Profil vorbereitet; echter macOS-Client und Firewall-/Local-Network-Test fehlen noch.
- produktive TLS/mutual auth fehlt.
- nativer PDF-Renderer ist noch nicht final entschieden.

## MA010.02 DevLan Handoff

Windows kann fuer den ersten Lab-Test einen DevLan Owner starten:

```powershell
.\tools\run-rkwp-lan-owner.ps1 -BindAddress 0.0.0.0 -Port 57100 -AllowDevPairing
```

Fuer den PDF-Frame-Owner-Pfad:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -PdfPath samples/Objects/Rechnung.pdf -Port 57100 -AllowDevPairing
```

macOS soll spaeter als Guest gegen diese URL verbinden:

```text
rkwp+tcp-dev://<windows-ip>:57100
```

Der Windows-Loopback-Smoke steht in:

```powershell
.\tools\run-rkwp-lan-smoke.ps1
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
```

## MA010.04 Compatibility Harness

Windows stellt einen macOS-Guest-Kompatibilitaets-Harness bereit:

```powershell
.\tools\run-mac-guest-compat.ps1 -SmokeTest
.\tools\run-mac-guest-compat.ps1 -ReplaySample
.\tools\run-mac-guest-compat.ps1 -ConnectToOwner rkwp+tcp-dev://<windows-ip>:57100
```

Dieser Harness ist kein nativer Mac-Client. Er definiert und prueft aber den erwarteten macOS-Guest-Flow:

- `Platform: MacOS`
- FrameView aktiv.
- FrameInput zuerst aus.
- Haptics und Glass Edge geplant.
- No File Ingress aktiv.
- OwnershipTransfer standardmaessig aus.
- Heartbeat und Return vorhanden.

Der native macOS-Codex soll diese Ausgabe als Kompatibilitaetsvertrag verwenden.

## MA013.01 Repository Bootstrap

macOS-Codex startet ab MA013 mit:

```text
Docs/Platform/macOS_RepositoryBootstrap.md
release/handoff/macOS_Codex_MA013_Bootstrap.md
release/schema/rkwp-envelope-schema-v0.1.json
```

Pflichtpruefung:

- Repository klonen oder aktualisieren.
- Branch `feature/ma013-real-cross-platform-frame-pilot` pruefen.
- Context Pack lesen.
- RKWP Protocol und Schema lesen.
- Surface Abstractions lesen.
- Swift/Xcode als bevorzugten ersten Pfad pruefen.
- .NET/MAUI/Avalonia nur als Fallback bewerten.
- No File Ingress als hartes Gate fuer den ersten macOS Guest Test behandeln.

## MA013.02 bis MA013.10 Real-Test-Vorbereitung

macOS-Codex muss den RKWP Client, die Frame Guest UI, No File Ingress, Return/Recovery, Frame Rendering, Packaging und Glass Edge als einen zusammenhaengenden ersten Testpfad behandeln.

Relevante Dateien:

```text
src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-client-architecture.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-message-flow.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-devtransport-client.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-securedev-client.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-frame-guest-ui-spec.md
Docs/Platform/macOS_FrameGuestUI.md
Docs/Platform/macOS_NoFileIngressChecklist.md
Docs/Platform/macOS_ReturnAndRecovery.md
Docs/Platform/macOS_FrameRenderingStrategy.md
Docs/Platform/macOS_DevAgentPackaging.md
Docs/Platform/macOS_GestureAndGlassEdge.md
Docs/Readiness/WindowsToMac_FirstRealTestGate.md
release/handoff/WindowsToMac_FirstRealTestGate.md
```

Windows-seitige Verifikation:

```powershell
.\tools\run-mac-guest-contract.ps1
.\tools\export-rkwp-schema.ps1
.\tools\run-windows-pdf-owner-securedev.ps1 -SmokeTest
```

## MA016 Aufgaben

- `release/ma016/handoff/macOS_START_HERE.md` als Einstieg verwenden.
- RKWP Client fuer AblageHello, Identity, Capsule, OpenFrame, Heartbeat, Return und Recovery bauen.
- Frame anzeigen, aber keine PDF-Datei, keine Originalbytes und keinen Originalpfad speichern.
- Return/Recovery sichtbar mit menschlicher Sprache darstellen.
- Build-/Permissions- und First-Pilot-Runbook abarbeiten.
