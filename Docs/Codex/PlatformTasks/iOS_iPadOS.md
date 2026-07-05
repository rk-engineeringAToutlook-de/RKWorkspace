# iOS and iPadOS Platform Tasks

## Plattformziel

iPhone und iPad werden als mobile Ablagen vorbereitet. Sie haben keinen eigenen Codex; Entwicklung laeuft ueber macOS-Codex und Xcode.

Die erste native App soll RKWP Frames anzeigen, Haptik vorbereiten und eine Glass Edge am Rand simulieren, ohne eine freie Datei zu speichern.

## Aktueller Stand

- Stubs: `src/Surfaces/RKWorkspace.Surface.iOS` und `src/Surfaces/RKWorkspace.Surface.iOS_iPadOS`
- Starter Kit: `Docs/Platform/iOS_iPadOS_SurfaceStarterKit.md`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- noch keine native App

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceHapticsProvider`
- `ISurfaceObjectAdapter`
- `ISurfaceSecurityContext`
- `ISurfaceInputChannel`
- `SurfacePlatform.IOS`
- `SurfacePlatform.IPadOS`

## Build-Hinweise

Die iOS/iPadOS-Verzeichnisse sind Handoff-Stubs und brechen den Windows-Build nicht. Test spaeter ueber Mac, Xcode und USB-angeschlossene iPhones/iPads.

## Berechtigungen und Grenzen

- iOS/iPadOS kann nicht beliebige App-Inhalte global greifen.
- erste sichere Quellen: RK Workspace App, Share Extension, Pasteboard, Document Picker
- Sandbox-Grenzen respektieren
- Haptik nur als menschliche Rueckmeldung
- Drei-Finger-Langdruck muss gegen OS-Gesten validiert werden
- Browser/PWA ist nur Uebergang, nicht finaler Gefuehlspfad

## Native Zielrichtung

Finales Ziel ist eine native iOS/iPadOS Surface App.

PWA darf fuer fruehes Prototyping genutzt werden, aber Browser-Chrome stoert die Human Experience. Der Produktpfad ist Xcode/native App.

## Kopierbarer naechster macOS-/Xcode-Codex-Auftrag

```text
RK Workspace iOS/iPadOS Codex Auftrag:

Baue eine iOS/iPadOS RK Workspace Surface App in Xcode, die RKWP Frames anzeigen kann, Haptik vorbereitet und eine Glass Edge am Rand simuliert.

Vorgehen:

1. macOS-Codex liest Context Pack und GitHub-Branch.
2. Xcode-Projekt anlegen oder vorbereiten.
3. iPhone/iPad per USB testen.
4. Einfache mobile Ablage anzeigen.
5. RKWP Frame anzeigen, aber keine Originaldatei speichern.
6. No File Ingress pruefen.
7. Haptik bei Frame-Ankunft und Pick/Place vorbereiten.
8. Drei-Finger-Geste pruefen; TouchHold als Fallback.
9. Glass Edge am Rand simulieren.
10. Erste Quellen nur sicher vorbereiten:
    - RK Workspace App
    - Document Picker
    - Share Extension
    - Pasteboard bewusst und begrenzt

Nicht bauen:

- keine globale Erfassung beliebiger App-Inhalte als erste Annahme
- kein Ownership Transfer als Default
- keine Dateiuebertragung als Erfolgspfad
- PWA nur als Uebergang dokumentieren
```

## Aktuelle Blocker

- keine Xcode-Umgebung im Windows-Thread
- keine finale Geste
- noch kein nativer FramePresenter
- DevTransport zu mobilen Geraeten folgt spaeter

## GitHub und Context Pack

iOS/iPadOS nutzt GitHub und Context-Pack aus dem Windows-Repo. Plattformarbeit erfolgt auf eigenem Feature-Branch und wird per Owner-Freigabe integriert.
