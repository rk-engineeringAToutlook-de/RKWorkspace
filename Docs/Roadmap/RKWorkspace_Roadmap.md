# RK Workspace Roadmap

Dokument-ID: RKWS-ROADMAP-001
Status: Draft
Datum: 2026-07-05

## Nordrichtung

RK Workspace ist keine App. RK Workspace ist der digitale Raum, in dem Menschen Dinge nehmen, tragen und ablegen.

## Phasen

| Phase | Ziel |
| --- | --- |
| 0 | Architecture Baseline und Core Foundation |
| 1 | Human Experience Lab und Developer Studio |
| 2 | Workspace Shell Foundation |
| 3 | Native Overlay und Glass Edge Hauptpfad |
| 4 | RKWP Original-Owned Frame Protocol |
| 5 | PDF FrameOnly Slice mit Owner/Guest |
| 6 | Cross-Platform Surfaces fuer Windows, macOS, iOS/iPadOS, Android, Linux |
| 7 | Proximity und Entfernung ueber lokale Provider |
| 8 | Ablage Anchor Dongle und Sensorfusion |
| 9 | Produktive Security, Pairing, Trust und Revocation |
| 10 | Erste echte Workspace Adapter: Explorer, PDF, Browser |
| 11 | Private Alpha mit zwei bis drei Plattformen |
| 12 | Hardware-/Dongle-Prototyp und Feldtest |

## Aktueller Stand

MA007.08 modelliert Ownership Transfer und Materialization als eigenes Gate. Default bleibt OriginalOwned + FrameOnly. CopyOut/ForkVersion/MoveOwnership brauchen Policy, Objektartregeln und bei MoveOwnership starke Bestaetigung. Denied/NotSupported erzeugen keine neue Quelle der Wahrheit.

## Naechster Fokus

Nach MA007.08 liegt der Fokus auf echten Windows-Objektquellen, Plattform-Handoff fuer macOS/iOS/iPadOS/Android/Linux und Proximity/ManualMap. Echte PDF-Seitenrendering-Pfade bleiben ein separater Renderer-Blocker.

GitHub bleibt die zentrale technische Synchronisation. Kein Plattformpfad wird als abgeschlossen betrachtet, solange er nicht ueber einen klaren Branch, Tests und Owner-Freigabe integrierbar ist.
