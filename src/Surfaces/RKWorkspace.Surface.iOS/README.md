# RKWorkspace.Surface.iOS

Status: Prepared stub

iOS wird als Phone-Ablage vorbereitet. Auf dem iPhone laeuft spaeter kein Codex; Entwicklung und Test erfolgen ueber macOS/Xcode und spaeter per USB/TestFlight.

## Relevante Abstractions

- `ISurfaceHost`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceHapticsProvider`
- `ISurfaceSecurityContext`

## Geplanter Ansatz

Der erste iOS-Pfad ist keine globale OS-Magie. Er startet mit einer eigenen RK Workspace Surface, Share Extension, Pasteboard, Document Picker und FrameOnly View.

## Berechtigungen

- Files/Document Picker nur nach User-Auswahl
- Pasteboard nur bewusst und sparsam
- Haptik nur als Rueckmeldung fuer Pick/Place
- keine Annahme, dass beliebige App-Inhalte global gegriffen werden koennen

## Build-Hinweise

Dieses Stub-Verzeichnis ist absichtlich kein Windows-buildbares iOS-Projekt. Native Umsetzung folgt spaeter ueber Xcode.

## Offene Punkte

- App-Sandbox
- Share Extension Grenzen
- FrameOnly View
- Gegenkante auf Zielablage
- RKWP Session Security

## Naechster Plattformauftrag

Ersten iOS Surface Host mit TouchHold, FrameOnly View und No File Ingress Proof planen.
