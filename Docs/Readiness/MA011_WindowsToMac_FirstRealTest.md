# MA011 Windows-to-macOS First Real Test

Status: Draft  
Datum: 2026-07-06

## Ziel

Der erste echte Test verbindet Windows Owner mit einer nativen macOS Guest Surface. Windows besitzt die echte PDF. macOS zeigt nur einen Frame und speichert keine freie PDF-Datei.

## Voraussetzungen

- Windows Branch `feature/ma011-secure-real-frame-cross-device-foundation`.
- Context Pack `release/codex-context/RKWorkspace_Context_latest.zip`.
- Mac im gleichen Dev-Netz.
- macOS-Codex oder Xcode/Swift-Umgebung auf Mac.
- Local Network/Firewall fuer DevLan geoeffnet.

## Ablauf

1. Windows Branch aktualisieren.
2. Context Pack erzeugen:

```powershell
.\tools\export-codex-context.ps1
```

3. macOS-Codex liest:

```text
Docs/Codex/CURRENT_CONTEXT.md
release/handoff/macOS_Codex_MA009_FrameGuestSurface.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-project-layout.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-client-flow.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-frame-guest-ui.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-permissions-checklist.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-build-commands.md
src/Surfaces/RKWorkspace.Surface.macOS/macos-test-plan.md
```

4. Windows Owner starten:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

5. macOS Guest starten.
6. macOS sendet `AblageHello`.
7. Windows prueft Identity, Pairing und Policy.
8. Windows erzeugt CarryLease und FrameSession.
9. macOS zeigt PDF-Frame.
10. macOS sendet Heartbeat.
11. No File Ingress pruefen.
12. macOS sendet Return.
13. Windows gibt Owner-PDF logisch frei.
14. Recovery/Verbindungsverlust testen.

## Erfolgskriterien

```text
AblageHello: OK
FrameSession: Active
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS
Recovery: SUCCESS
```

## Blocker

- echter RKWP DevLan Client auf macOS fehlt noch.
- Signing/Sandbox/Firewall koennen den Start blockieren.
- PDF-Renderer auf macOS kann initial nur Frame-Bild anzeigen.
- produktive Security ist noch nicht final.

## Empfehlung

Zuerst mit einer nativen Swift/Xcode Minimal-App testen. Wenn der RKWP-Client dort zu langsam entsteht, darf macOS-Codex kurzfristig einen minimalen .NET/Avalonia Host bauen, solange die No-File-Ingress-Regeln identisch bleiben.
