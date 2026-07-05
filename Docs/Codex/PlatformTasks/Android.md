# Android Platform Tasks

## Plattformziel

Android wird als Phone-/Tablet-Ablage vorbereitet, spaeter ueber Android Studio/Gradle.

## Aktueller Stand

- Stub: `src/Surfaces/RKWorkspace.Surface.Android`
- gemeinsame Contracts: `src/Surfaces/RKWorkspace.Surface.Abstractions`
- noch keine native Android-App

## Relevante Surface Contracts

- `ISurfaceHost`
- `ISurfaceGestureProvider`
- `ISurfaceFramePresenter`
- `ISurfaceHapticsProvider`
- `ISurfaceObjectAdapter`
- `ISurfaceSecurityContext`

## Build-Hinweise

Der Stub ist dokumentarisch und bricht den Windows-Build nicht. Native Android-Struktur folgt spaeter.

## Berechtigungen

- Sharesheet
- Intent
- Content URI
- Clipboard vorsichtig
- Haptik
- Accessibility nur spaeter und vorsichtig
- PWA als Uebergangsloesung moeglich

## Aktuelle Blocker

- keine Android Studio/Gradle-Struktur
- keine finale Touch-Geste
- keine produktive Content-URI-Policy

## Naechster Codex-Auftrag

Android Surface Host mit TouchHold, Glass Edge, FrameGuestSurface und No File Ingress planen.

## GitHub und Context Pack

Android-Arbeit laeuft ueber Feature-Branch, GitHub und Context-Pack. Keine Plattformsemantik duplizieren.
