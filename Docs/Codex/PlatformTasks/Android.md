# Android Platform Tasks

## Plattformziel

Android wird als Phone-/Tablet-Ablage vorbereitet, spaeter ueber Android Studio/Gradle.

Die erste Android Surface App zeigt RKWP Frames, bereitet Touch/Haptik und Glass Edge vor und respektiert No File Ingress.

## Aktueller Stand

- Stub: `src/Surfaces/RKWorkspace.Surface.Android`
- Starter Kit: `Docs/Platform/Android_SurfaceStarterKit.md`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- noch keine native Android-App

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceHapticsProvider`
- `ISurfaceObjectAdapter`
- `ISurfaceSecurityContext`
- `ISurfaceInputChannel`
- `SurfacePlatform.Android`

## Build-Hinweise

Der Stub ist dokumentarisch und bricht den Windows-Build nicht. Native Android-Struktur folgt spaeter.

## Quellen und Berechtigungen

- Sharesheet
- Intent
- Content URI
- Clipboard vorsichtig
- Haptik
- Accessibility nur spaeter und vorsichtig
- PWA als Uebergangsloesung moeglich
- native App als Ziel

## RKWP-Pflicht

- FrameOnly als sicherer Default
- No File Ingress
- OriginalOwned respektieren
- keine stille Dateiuebernahme
- Input nur policygebunden

## Aktuelle Blocker

- keine Android Studio/Gradle-Struktur
- keine finale Touch-Geste
- keine produktive Content-URI-Policy
- DevTransport zu Android folgt spaeter

## Naechster Codex-Auftrag

Android Surface Host mit TouchHold, Glass Edge, FrameGuestSurface, Haptik und No File Ingress planen.

## GitHub und Context Pack

Android-Arbeit laeuft ueber Feature-Branch, GitHub und Context-Pack. Keine Plattformsemantik duplizieren.
