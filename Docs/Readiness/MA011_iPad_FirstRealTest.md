# MA011 iPad/iPhone First Real Test

Status: Draft  
Datum: 2026-07-06

## Ziel

iPad oder iPhone wird mobile Ablage. Das Geraet zeigt einen RKWP Frame von Windows Owner, speichert keine PDF-Datei und kann Return senden.

## Voraussetzungen

- Mac mit Xcode.
- iPad oder iPhone per USB.
- Apple Developer Signing lokal bereit.
- Windows Owner im gleichen Dev-Netz.
- Context Pack `release/codex-context/RKWorkspace_Context_latest.zip`.

## Ablauf

1. macOS-Codex baut Xcode-App anhand:

```text
src/Surfaces/RKWorkspace.Surface.iOS/ios-xcode-project-layout.md
src/Surfaces/RKWorkspace.Surface.iOS/ios-rkwp-client-flow.md
src/Surfaces/RKWorkspace.Surface.iOS/ios-frame-guest-ui.md
src/Surfaces/RKWorkspace.Surface.iOS/ios-haptics-plan.md
src/Surfaces/RKWorkspace.Surface.iOS/ios-gesture-plan.md
src/Surfaces/RKWorkspace.Surface.iOS/ios-usb-test-plan.md
src/Surfaces/RKWorkspace.Surface.iOS/ios-sandbox-sources.md
```

2. App per USB auf iPad/iPhone installieren.
3. Windows Owner starten:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

4. iOS/iPadOS App starten.
5. Local Network Permission bestaetigen.
6. App sendet `AblageHello`.
7. Windows Owner oeffnet FrameSession.
8. iPad/iPhone zeigt PDF-Frame.
9. Haptik bei FrameReady pruefen.
10. No File Ingress pruefen.
11. Return per Touch-Geste ausloesen.
12. Windows Owner bestaetigt Return/Recovery.

## Erfolgskriterien

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

## Haptik

V1 nutzt ruhige Haptik:

- FrameReady: kurzes Ankommen.
- Return: kurzer Abschluss.
- Verbindungsverlust: zunaechst nur sichtbarer Status.

## Geste

V1 nutzt eine einfache Touch-Geste oder Button-Fallback fuer Return. Drei-Finger-Gesten werden nur geprueft, weil iOS/iPadOS Systemgesten kollidieren koennen.

## Blocker

- iPhone/iPad haben keinen eigenen Codex.
- Xcode Signing/Provisioning kann blockieren.
- Local Network Permission muss aktiv sein.
- echter RKWP Mobile Client fehlt noch.
- finale iOS FrameRenderer-Strategie ist offen.
