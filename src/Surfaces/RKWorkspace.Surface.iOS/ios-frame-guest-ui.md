# iOS/iPadOS Frame Guest UI

Status: MA011.09 planning baseline  
Datum: 2026-07-06

## Ziel

Die App zeigt eine mobile Ablage, nicht eine Datei-App. Sichtbar ist ein Ding im Frame.

## Sichtbare Sprache

Erlaubt:

- Ablage.
- Ding.
- Frame.
- liegt hier im Frame.
- zurueckgeben.
- nicht verfuegbar.
- Verbindung verloren.

Nicht als primaere UI-Sprache:

- Transfer.
- Upload.
- Download.
- Sync.
- Server.
- Client.
- Device.
- Agent.

## Hauptansicht

Die erste SwiftUI-Ansicht besteht aus:

- ruhiger Ablage-Flaeche.
- Frame-Karte fuer das digitale Ding.
- Statuszeile fuer `liegt hier im Frame`.
- Button oder Geste `zurueckgeben`.
- schmalem Diagnosebereich nur im Dev-Modus.

## iPad Layout

iPad priorisiert:

- groesseren Frame.
- ruhige Ablage.
- spaetere Glass Edge Richtung.
- Haptik und Touch-Geste.

## iPhone Layout

iPhone priorisiert:

- kompakten Frame.
- klaren Zustand.
- einfache Return-Geste.
- keine ueberladene Diagnose.

## Frame Anzeige

Der erste Frame kann sein:

- gerenderte PNG/WebP-Seite.
- PDF-Seitenbild aus Windows Owner.
- MetadataPreview nur falls Renderer blockiert.

Die App zeigt niemals eine freie PDF-Datei im Files-Kontext.

## Fehlerzustaende

- Keine Verbindung: `Ablage wartet`.
- Frame entzogen: `nicht verfuegbar`.
- Owner verloren: `Verbindung verloren`.
- Return abgeschlossen: Frame ausblenden und Log schreiben.
