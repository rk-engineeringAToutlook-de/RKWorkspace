# iOS/iPadOS USB Device Test Plan

Status: MA011.09 planning baseline  
Datum: 2026-07-06

## Ziel

Der erste echte iPad/iPhone-Test laeuft ueber Xcode und USB. Windows bleibt Owner. iOS/iPadOS wird mobile Ablage und zeigt einen Frame.

## Voraussetzungen

- Mac mit Xcode.
- iPad oder iPhone per USB.
- Apple Developer Signing lokal verfuegbar.
- Windows Owner im gleichen Netzwerk oder erreichbarem DevLan.
- Context Pack `release/codex-context/RKWorkspace_Context_latest.zip`.

## Build

```bash
open apps/ios/RKWorkspaceIOSSurface/RKWorkspaceIOSSurface.xcodeproj
xcodebuild -scheme RKWorkspaceIOSSurface -destination 'generic/platform=iOS' build
```

## Testablauf

1. Windows Owner starten:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

2. iPad/iPhone per USB anschliessen.
3. Xcode Target auf echtes Geraet setzen.
4. App starten.
5. Local Network Permission bestaetigen, falls iOS fragt.
6. App sendet `AblageHello`.
7. Windows Owner erstellt CarryLease und FrameSession.
8. App zeigt PDF-Frame.
9. Haptik bei FrameReady pruefen.
10. No File Ingress pruefen.
11. Return ausloesen.
12. Windows Owner bestaetigt Recovery/Return.

## Erwartete Ausgabe

```text
SurfacePlatform: IOS oder IPadOS
SurfaceRole: FrameGuestSurface
AblageHello: OK
FrameSession: Active
HapticsPrepared: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
Return: SUCCESS
```

## Logs

Zu sichern:

- Xcode Console.
- RK Workspace Surface Log.
- Windows Owner Log.
- No File Ingress Probe.

## Blocker

- Signing/Provisioning fehlt.
- Local Network Permission blockiert DevLan.
- echter RKWP Mobile Client fehlt.
- Renderer nutzt noch Mock/Metadata.
