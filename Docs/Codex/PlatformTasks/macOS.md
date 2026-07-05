# macOS Platform Tasks

## Plattformziel

macOS bekommt eine eigene native Surface ueber macOS-Codex, GitHub und Xcode.

## Aktueller Stand

- Stub: `src/Surfaces/RKWorkspace.Surface.macOS`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- noch keine native macOS-App

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceGestureProvider`
- `ISurfaceOverlay`
- `ISurfaceFramePresenter`
- `ISurfaceHapticsProvider`
- `ISurfaceSecurityContext`

## Build-Hinweise

Der Stub bricht den Windows-Build nicht. Native Umsetzung wird spaeter ueber Xcode/Swift oder gepruefte .NET/MAUI-Optionen bewertet.

## Berechtigungen

- Accessibility fuer globale Gesten und Eingaben
- Screen Recording / Capture Permissions fuer sichtbare Oberflaechen
- Sandbox
- security-scoped access
- Trackpad-Gesten
- Force Touch / Trackpad Feedback als spaeterer Haptikpfad

## Aktuelle Blocker

- keine macOS-Entwicklungsumgebung in diesem Windows-Thread
- noch keine Entscheidung Swift vs. .NET/MAUI
- keine produktive Capture-/Overlay-Berechtigungsstrategie

## Naechster Codex-Auftrag

macOS-Codex-Thread mit Context-Pack starten, Xcode-Projekt skizzieren und FrameOnly View ohne Originaldatei planen.

## GitHub und Context Pack

macOS arbeitet ueber GitHub-Synchronisation und Context-Pack. Kein lokaler Windows-Pfad wird als macOS-Implementierung ausgegeben.
