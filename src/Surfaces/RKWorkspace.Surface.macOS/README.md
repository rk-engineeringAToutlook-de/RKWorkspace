# RKWorkspace.Surface.macOS

Status: Prepared handoff  
Datum: 2026-07-06

## Zweck

Dieses Verzeichnis enthaelt die Windows-seitig vorbereitete macOS-Surface-Uebergabe. Es ist noch keine buildbare macOS-App.

macOS-Codex/Xcode baut spaeter die native App.

## Ziel

Baue eine macOS RK Workspace Frame Guest Surface:

- native macOS Ablage.
- RKWP DevTransport Client.
- AblageIdentity.
- DevPairing.
- FrameSession empfangen.
- PDF Frame anzeigen.
- No File Ingress beweisen.
- Heartbeat und Return senden.

## Dateien

- `SurfaceHostStub.md`
- `FrameGuestSurfacePlan.md`
- `macOSPermissions.md`
- `macOS_FrameGuestSurface_Design.md`
- `macOS_Permissions_Checklist.md`
- `macOS_Build_Notes.md`
- `macos-project-layout.md`
- `macos-rkwp-client-flow.md`
- `macos-rkwp-client-architecture.md`
- `macos-rkwp-message-flow.md`
- `macos-rkwp-devtransport-client.md`
- `macos-rkwp-securedev-client.md`
- `macos-frame-guest-ui.md`
- `macos-frame-guest-ui-spec.md`
- `macos-permissions-checklist.md`
- `macos-build-commands.md`
- `macos-test-plan.md`

## MA011.08 Build-Layout

Fuer den ersten echten Test ist eine native Swift/Xcode-App bevorzugt. Die konkrete Zielstruktur liegt in `macos-project-layout.md`. `.NET MAUI` oder Avalonia bleiben Option B, falls macOS-Codex damit schneller einen stabilen Host liefern kann.

## MA013.02 RKWP Client

Der macOS-Client wird als reine Frame Guest Surface vorbereitet. Die Architektur liegt in `macos-rkwp-client-architecture.md`; Nachrichtenfluss, DevTransport und SecureDevTransport sind in den zugehoerigen MA013-Dateien beschrieben. Der Client darf keine PDF-Datei speichern, sondern zeigt ausschliesslich Frame-Daten aus einer Owner-gehaltenen Session an.

## MA013.04 bis MA013.10 Testpfad

Der erste echte Windows-zu-macOS-Test nutzt:

- `macos-frame-guest-ui-spec.md`
- `Docs/Platform/macOS_FrameGuestUI.md`
- `Docs/Platform/macOS_NoFileIngressChecklist.md`
- `Docs/Platform/macOS_ReturnAndRecovery.md`
- `Docs/Platform/macOS_FrameRenderingStrategy.md`
- `Docs/Platform/macOS_DevAgentPackaging.md`
- `Docs/Platform/macOS_GestureAndGlassEdge.md`
- `Docs/Readiness/WindowsToMac_FirstRealTestGate.md`

## Pflichtregeln

- keine Originaldatei auf macOS speichern.
- keine Ownership-Entscheidung lokal erfinden.
- FrameOnly respektieren.
- Input nur ueber RKWP Input Channel und Policy.
- ChangeSets sind Vorschlaege.
- Return und Recovery muessen nachvollziehbar sein.
