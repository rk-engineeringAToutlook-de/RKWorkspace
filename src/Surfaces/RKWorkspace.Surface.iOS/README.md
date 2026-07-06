# RKWorkspace.Surface.iOS

Status: Prepared handoff  
Datum: 2026-07-06

Dieses Verzeichnis beschreibt die geplante native iOS/iPadOS Surface App. In diesem Windows-Thread wird keine Xcode-App gebaut.

## Ziel

Die App wird eine mobile Ablage fuer RKWP Frames. Sie zeigt digitale Dinge als Frame an, ohne die Owner-Datei zu besitzen oder als freie Datei zu speichern.

## Relevante Abstractions

- `ISurfaceHost`
- `ISurfaceOverlay`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceInputChannel`
- `ISurfaceHapticsProvider`
- `ISurfaceSecurityContext`

## Geplanter Ansatz

Der erste iOS/iPadOS-Pfad ist keine globale OS-Magie. Er startet mit:

- eigener RK Workspace Surface App.
- Share Extension.
- bewusst begrenztem Pasteboard.
- Document Picker.
- FrameOnly View.
- No File Ingress Logs.

## Handoff-Dateien

- `ios-xcode-project-layout.md`
- `ios-rkwp-client-flow.md`
- `ios-frame-guest-ui.md`
- `ios-haptics-plan.md`
- `ios-gesture-plan.md`
- `ios-usb-test-plan.md`
- `ios-sandbox-sources.md`
- `iOS_SurfaceApp_Design.md`
- `iOS_Haptics_Gesture_Plan.md`
- `iOS_Xcode_USB_TestPlan.md`
- `iOS_Sandbox_ObjectSources.md`
- `release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md`

## MA011.09 Xcode-Testplan

MA011.09 konkretisiert den ersten echten iPad/iPhone-Test. macOS-Codex soll daraus eine native SwiftUI/Xcode-App aufbauen, die eine `AblageIdentity` erzeugt, sich per RKWP DevLan/SecureDev mit Windows Owner verbindet, einen PDF-Frame anzeigt, Haptik vorbereitet, Return senden kann und keine PDF-Datei speichert.

## Offene Punkte

- Xcode-Projekt anlegen.
- iPhone/iPad per USB testen.
- Local Network Permission pruefen.
- DevTransport Mobile Client bauen.
- finale Touch-Geste festlegen.
- native FramePresenter-Strategie klaeren.
