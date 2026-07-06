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

- `iOS_SurfaceApp_Design.md`
- `iOS_Haptics_Gesture_Plan.md`
- `iOS_Xcode_USB_TestPlan.md`
- `iOS_Sandbox_ObjectSources.md`
- `release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md`

## Offene Punkte

- Xcode-Projekt anlegen.
- iPhone/iPad per USB testen.
- Local Network Permission pruefen.
- DevTransport Mobile Client bauen.
- finale Touch-Geste festlegen.
- native FramePresenter-Strategie klaeren.
