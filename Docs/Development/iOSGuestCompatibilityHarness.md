# iOS/iPadOS Guest Compatibility Harness

Status: MA010.05

## Zweck

Der iOS/iPadOS Guest Compatibility Harness ist eine Windows-lauffaehige Spezifikation fuer die spaetere native Xcode-App. Er baut keine iOS-App, aber er prueft den Vertrag, den iPad und iPhone spaeter erfuellen muessen.

## Start

Smoke:

```powershell
.\tools\run-ios-guest-compat.ps1 -SmokeTest
```

Replay:

```powershell
.\tools\run-ios-guest-compat.ps1 -ReplaySample
```

## Modellierte Capabilities

- FrameView true.
- TouchInput geplant.
- Haptics geplant.
- GlassEdge geplant.
- NoFileIngress true.
- ShareExtension geplant.
- DocumentPicker geplant.
- Pasteboard bewusst und begrenzt.
- GlobalAppCapture false.
- OwnershipTransfer standardmaessig aus.

## Xcode-Bruecke

Der native Pfad bleibt macOS-Codex + Xcode + echtes iPhone/iPad per USB.

Der Handoff liegt hier:

```text
release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md
```

Pflichtpunkte fuer den echten Build:

- Development Signing.
- Developer Mode auf dem Geraet.
- Local Network Permission.
- RKWP Frame anzeigen.
- Haptik vorbereiten.
- Drei-Finger-Langdruck gegen Systemgesten testen.
- keine globale App-Erfassung.

## No File Ingress

Der Harness verlangt:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
AppContainerOriginalPdf: NO
FilesAppOriginalPdf: NO
NoFileIngress: SUCCESS
```

## Offene Punkte

- echtes Xcode-Projekt fehlt in diesem Windows-Thread.
- echte USB-Hardwarepruefung fehlt.
- nativer FramePresenter ist noch zu bauen.
- Local Network Permission muss auf echter Hardware bestaetigt werden.
