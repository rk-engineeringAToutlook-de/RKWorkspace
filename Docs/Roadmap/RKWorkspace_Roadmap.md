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

MA010.09 liefert DevLan, Windows/macOS/iOS-Kompatibilitaet, PDF-Renderer-Abstraktion, FrameCachePolicy, Windows-Agent-Dev-Vorbereitung und ein zentrales Configuration-System. Windows kann lokal einen TCP-basierten Owner/Guest-Smoke mit AblageHello, DevPairing, Heartbeat, FrameUpdate und No File Ingress ausfuehren. Der PDF-Pfad nutzt noch den `MetadataPreviewDevRenderer` mit `IsPlaceholder: true`.

## Naechster Fokus

Nach MA010.09 liegt der Fokus auf MA010 Readiness, echtem Plattform-Handoff und danach dem ersten echten PDFium/MuPDF-Renderer-Spike.

## Entfernung und Ablage-Anker

Der Produktpfad fuer Entfernung ist:

1. Simulation fuer reproduzierbare Tests.
2. Manual Map fuer den ersten echten Raumaufbau.
3. BLE/WiFi als grobe Naeherung.
4. Ablage Anchor Dongle fuer Identitaet, Trust und Ankerfunktion.
5. UWB/SensorFusion fuer praezisere Distanz und Richtung.

Diese Stufen veraendern den Core nicht zu einem Sync-System. Sie helfen nur, die naechste Ablage im Arbeitsraum zu bestimmen.

GitHub bleibt die zentrale technische Synchronisation. Kein Plattformpfad wird als abgeschlossen betrachtet, solange er nicht ueber einen klaren Branch, Tests und Owner-Freigabe integrierbar ist.

## MA011 Secure Real Frame Cross-Device Foundation

MA011.01 startet den produktiveren Secure-Session-Pfad. `RkwpSecureSessionPath` trennt DevelopmentInsecure, DevelopmentAuthenticated, TestSecure, ProductionSecure und ProductionRequired. Production und SecureSessionRequired blockieren unsichere Sessions; Heartbeat und Revocation sind authentisierungspflichtig. Die naechsten Schritte sind Identity Store, SecureDevTransport und erster echter Windows-zu-macOS-Test.
