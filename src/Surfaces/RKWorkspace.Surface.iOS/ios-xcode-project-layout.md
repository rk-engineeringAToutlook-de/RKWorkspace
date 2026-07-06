# iOS/iPadOS Xcode Project Layout

Status: MA011.09 planning baseline  
Datum: 2026-07-06

Dieses Dokument beschreibt das konkrete Build-Layout fuer den ersten echten iPad/iPhone-Test. Windows-Codex baut kein Xcode-Projekt, liefert aber macOS-Codex eine klare Struktur.

## Ziel

Die iOS/iPadOS App ist eine mobile Ablage fuer RKWP Frames. Sie zeigt ein digitales Ding als Frame, ohne die Original-PDF, den Windows-Pfad oder Originalbytes als Datei zu speichern.

## Empfohlener Projektpfad

```text
apps/ios/RKWorkspaceIOSSurface/
  RKWorkspaceIOSSurface.xcodeproj
  RKWorkspaceIOSSurface/
    App/
      RKWorkspaceIOSSurfaceApp.swift
      SurfaceAppState.swift
    Identity/
      AblageIdentity.swift
      AblageIdentityStore.swift
    RKWP/
      RkwpEnvelope.swift
      RkwpDevTransportClient.swift
      RkwpSecureDevSession.swift
      RkwpMessageCodec.swift
    Frame/
      FrameSessionModel.swift
      PdfFrameView.swift
      FrameReturnController.swift
    Haptics/
      SurfaceHaptics.swift
    Gestures/
      SurfaceGestureController.swift
    Diagnostics/
      SurfaceLogStore.swift
      NoFileIngressProbe.swift
  RKWorkspaceIOSSurfaceTests/
    NoFileIngressTests.swift
    RkwpEnvelopeTests.swift
```

## Native App

Bevorzugt ist eine native SwiftUI-App, weil sie auf echtem iPad/iPhone die wichtigsten Systemthemen direkt sichtbar macht:

- Local Network Permission.
- App Sandbox.
- Device Signing.
- Touch und Haptik.
- spater Share Extension und Document Picker.

## Minimale Runtime

Die App startet mit:

- SurfaceId.
- lokal gespeicherter AblageIdentity.
- RKWP DevLan/SecureDev Client.
- FrameGuestSurface Rolle.
- einem FramePresenter fuer PDF-Frames.
- No-File-Ingress-Pruefung.
- lokalem Diagnose-Log.

## Keine Produktillusion

Dies ist noch kein finales iOS-Produkt. Es ist der erste echte Geraete-Test, damit RK Workspace sehen kann, ob ein iPad/iPhone als Ablage fuer ein Original-Owned Frame funktioniert.
