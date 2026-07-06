# macOS Codex Auftrag: MA013 Bootstrap

Status: Prepared  
Datum: 2026-07-06

## Ziel

Bereite macOS als erste echte Gegenplattform zum Windows Owner vor. Der erste Test ist eine macOS Frame Guest Surface. Es wird keine freie PDF-Datei auf macOS gespeichert.

## Repository

```bash
git clone git@github.com:rk-engineeringAToutlook-de/RKWorkspace.git
cd RKWorkspace
git fetch --all --tags
git checkout feature/ma013-real-cross-platform-frame-pilot
```

Falls der Branch nicht vorhanden ist, mit dem vom Owner gelieferten Branch arbeiten.

## Context Pack

```text
release/codex-context/RKWorkspace_Context_latest.zip
```

Pruefe, dass diese Dateien enthalten sind:

- `Docs/Platform/macOS_RepositoryBootstrap.md`
- `Docs/Codex/PlatformTasks/macOS.md`
- `release/schema/rkwp-envelope-schema-v0.1.json`
- `src/Surfaces/RKWorkspace.Surface.Abstractions/`
- `src/Surfaces/RKWorkspace.Surface.macOS/`

## Pflichtlekture

1. `Docs/Codex/CURRENT_CONTEXT.md`
2. `Docs/Platform/macOS_RepositoryBootstrap.md`
3. `Docs/Codex/PlatformTasks/macOS.md`
4. `release/schema/rkwp-envelope-schema-v0.1.json`
5. `Docs/Protocol/RKWP_ProtocolFoundation.md`
6. `Docs/Protocol/RKWP_SecureSession.md`
7. `Docs/Protocol/RKWP_AblageIdentityAndTrust.md`
8. `Docs/Protocol/RKWP_OwnershipAndLease.md`
9. `Docs/Protocol/RKWP_FrameSession.md`
10. `src/Surfaces/RKWorkspace.Surface.Abstractions`
11. `src/Surfaces/RKWorkspace.Surface.macOS`

## Erster Build

Bevorzugt:

```text
apps/macos/RKWorkspaceMacGuest/
```

Minimaler Funktionsumfang:

- AblageIdentity laden oder erzeugen.
- RKWP DevLan/SecureDev Client vorbereiten.
- `AblageHello` senden.
- `FrameSessionOpen` empfangen.
- Frame anzeigen.
- Heartbeat senden.
- Return senden.
- No File Ingress pruefen.

## Windows Owner

Windows startet fuer den Test:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

Smoke auf Windows:

```powershell
.\tools\run-windows-owner-for-mac.ps1 -SmokeTest
```

## Erfolg

```text
Platform: MacOS
FrameView: OK
AblageHello: OK
FrameSession: Active
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS
```

## Blocker Ehrlich Melden

- Netzwerk/Firewall/Local Network blockiert.
- Signing/Sandbox fehlt.
- DevTransport Client fehlt.
- Renderer ist nur Mock.
- produktive Security fehlt.
