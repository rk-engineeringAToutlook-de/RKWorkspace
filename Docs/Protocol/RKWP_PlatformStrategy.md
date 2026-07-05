# RKWP Platform Strategy

Dokument-ID: RKWS-RKWP-PLATFORM-001
Status: Draft
Datum: 2026-07-05

## Ziel

Windows, macOS, iOS/iPadOS, Android und Linux sollen dieselbe RKWP-Semantik verwenden, aber unterschiedliche native Oberflaechen haben.

## Gemeinsamer Kern

Alle Plattformen teilen:

- `RKWorkspace.Protocol`
- Ownership-Regeln
- CarryLease
- FrameSession
- Surface-Abstraktionen
- Gesture- und Proximity-Vertraege

Die Surface-Abstraktionen sind ab MA007.02 als einzelne Contract-Dateien unter `src/Surfaces/RKWorkspace.Surface.Abstractions/` angelegt. Plattformadapter implementieren diese Verträge und duerfen keine neuen Ownership-Regeln erfinden.

## Plattformgrenzen

Die Surface-Projekte duerfen native APIs nutzen, aber nicht die Ownership-Regeln neu definieren. Sie uebersetzen nur:

- Geste zu Intent
- Ablage zu Surface
- Frame zu Darstellung
- Naehe zu Glass Edge
- Eingabe zu FrameInput

## Vorbereitete Stubs

```text
src/Surfaces/RKWorkspace.Surface.Abstractions
src/Surfaces/RKWorkspace.Surface.Windows
src/Surfaces/RKWorkspace.Surface.macOS
src/Surfaces/RKWorkspace.Surface.iOS
src/Surfaces/RKWorkspace.Surface.iOS_iPadOS
src/Surfaces/RKWorkspace.Surface.Android
src/Surfaces/RKWorkspace.Surface.Linux
```

## Regel

Wenn eine Plattform ein Ding zeigt, entscheidet nicht die Plattform, ob eine Datei kopiert wird. Das entscheidet RKWP Ownership.
