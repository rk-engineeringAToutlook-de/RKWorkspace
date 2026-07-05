# RKWorkspace.Surface.Android

Status: Prepared stub

Android wird als Phone- und Tablet-Surface vorbereitet. Android darf spaeter lokale Gesten, Haptik und Frame-Presentation bereitstellen.

MA007.00 implementiert noch keine Android-APIs. Der Surface-Adapter muss spaeter dieselbe RKWP-Semantik wie iOS/iPadOS nutzen.

Pflichtsemantik:

- Keine stille Dateiuebernahme.
- Keine lokale PDF-Kopie im FrameOnly-Modus.
- Distanz und naechste Ablage kommen aus Proximity-Providern.
- Surface-Haptik bestaetigt nur menschliche Handlung, nicht technischen Transfer.

## Android-Pfade

- Sharesheet
- Intent
- Content URI mit expliziter Berechtigung
- Haptik
- Accessibility nur vorsichtig als spaeterer Spezialpfad

## Naechster Plattformauftrag

Android Surface mit TouchHold, FrameOnly View und Content-URI-Grenzen planen.

## MA007.12 Starter Kit

Dieses Verzeichnis enthaelt ab MA007.12:

- `AndroidSurfacePlan.md`
- `AndroidPermissions.md`
- `AndroidObjectSources.md`

Die native Umsetzung erfolgt spaeter ueber Android Studio/Gradle. PWA bleibt nur Uebergang; native App ist Ziel.
