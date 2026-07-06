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
- `macos-frame-guest-ui.md`
- `macos-permissions-checklist.md`
- `macos-build-commands.md`
- `macos-test-plan.md`

## MA011.08 Build-Layout

Fuer den ersten echten Test ist eine native Swift/Xcode-App bevorzugt. Die konkrete Zielstruktur liegt in `macos-project-layout.md`. `.NET MAUI` oder Avalonia bleiben Option B, falls macOS-Codex damit schneller einen stabilen Host liefern kann.

## Pflichtregeln

- keine Originaldatei auf macOS speichern.
- keine Ownership-Entscheidung lokal erfinden.
- FrameOnly respektieren.
- Input nur ueber RKWP Input Channel und Policy.
- ChangeSets sind Vorschlaege.
- Return und Recovery muessen nachvollziehbar sein.
