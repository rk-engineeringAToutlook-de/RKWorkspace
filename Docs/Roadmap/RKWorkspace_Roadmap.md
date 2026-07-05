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

MA008.06 bereitet den ersten Windows-zu-macOS DevTransport-Test vor. Windows liefert Handoff, Context Pack, Owner-Startscript und klare Blocker; PDF Frame Interaction aus MA008.05 bleibt policygeprueft und ohne File Ingress.

## Naechster Fokus

Nach MA008.06 liegt der Fokus auf iPad/iPhone Surface Testplan mit macOS/Xcode, anschliessend RKWP Audit Viewer und Dev Security Gate. Offen bleiben macOS FrameGuestSurface, netzwerkfaehiger DevTransport und echtes PDF-Seitenrendering.

## Entfernung und Ablage-Anker

Der Produktpfad fuer Entfernung ist:

1. Simulation fuer reproduzierbare Tests.
2. Manual Map fuer den ersten echten Raumaufbau.
3. BLE/WiFi als grobe Naeherung.
4. Ablage Anchor Dongle fuer Identitaet, Trust und Ankerfunktion.
5. UWB/SensorFusion fuer praezisere Distanz und Richtung.

Diese Stufen veraendern den Core nicht zu einem Sync-System. Sie helfen nur, die naechste Ablage im Arbeitsraum zu bestimmen.

GitHub bleibt die zentrale technische Synchronisation. Kein Plattformpfad wird als abgeschlossen betrachtet, solange er nicht ueber einen klaren Branch, Tests und Owner-Freigabe integrierbar ist.
