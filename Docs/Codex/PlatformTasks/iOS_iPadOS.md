# iOS and iPadOS Platform Tasks

## Plattformziel

iPhone und iPad werden als eigene Ablagen vorbereitet. Sie haben keinen eigenen Codex; Entwicklung laeuft ueber macOS-Codex und Xcode.

## Aktueller Stand

- Stubs: `src/Surfaces/RKWorkspace.Surface.iOS` und `src/Surfaces/RKWorkspace.Surface.iOS_iPadOS`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- noch keine native App

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceHapticsProvider`
- `ISurfaceObjectAdapter`
- `ISurfaceSecurityContext`

## Build-Hinweise

Die iOS/iPadOS-Verzeichnisse sind Handoff-Stubs und brechen den Windows-Build nicht. Test spaeter ueber Mac, Xcode und USB-angeschlossene iPhones/iPads.

## Berechtigungen und Grenzen

- iOS/iPadOS kann nicht beliebige App-Inhalte global greifen.
- erste sichere Quellen: RK Workspace App, Share Extension, Pasteboard, Document Picker
- Sandbox-Grenzen respektieren
- Haptik nur als menschliche Rueckmeldung
- Drei-Finger-Langdruck muss gegen OS-Gesten validiert werden

## Aktuelle Blocker

- keine Xcode-Umgebung im Windows-Thread
- keine finale Geste
- noch kein nativer FramePresenter

## Naechster Codex-Auftrag

iOS-/iPadOS-Xcode-Handoff starten: TouchHold, Drei-Finger-Langdruck, FrameOnly View und No File Ingress planen.

## GitHub und Context Pack

iOS/iPadOS nutzt GitHub und Context-Pack aus dem Windows-Repo. Plattformarbeit erfolgt auf eigenem Feature-Branch und wird per Owner-Freigabe integriert.
