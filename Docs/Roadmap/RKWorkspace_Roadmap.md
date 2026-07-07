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

MA010.09 liefert DevLan, Windows/macOS/iOS-Kompatibilitaet, PDF-Renderer-Abstraktion, FrameCachePolicy, Windows-Agent-Dev-Vorbereitung und ein zentrales Configuration-System. Windows kann lokal einen TCP-basierten Owner/Guest-Smoke mit AblageHello, DevPairing, Heartbeat, FrameUpdate und No File Ingress ausfuehren. Ab MA011.04 kann der PDF-Pfad im Development-Modus ueber `PopplerPdfFrameRenderer` die erste Seite als PNG-Frame rendern. Ab MA011.06 kann der Windows PDF Pilot den FrameOnly-Pfad ueber `-UseGlassEdge -PlaySequence` lokal ausloesen und erzeugt dafuer GlassEdge-/Object-/FrameSession-Events.

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

MA011.01 startet den produktiveren Secure-Session-Pfad. `RkwpSecureSessionPath` trennt DevelopmentInsecure, DevelopmentAuthenticated, TestSecure, ProductionSecure und ProductionRequired. Production und SecureSessionRequired blockieren unsichere Sessions; Heartbeat und Revocation sind authentisierungspflichtig.

MA011.02 fuehrt den lokalen Ablage Identity Store ein. Private Dev-Keys bleiben unter `.rkworkspace-dev/` und werden nicht in Git oder Context Packs aufgenommen.

MA011.03 fuehrt den SecureDevTransport-Spike ein. Er verbindet lokale Ablage-Identitaeten, Identity Exchange, SecureDev Handshake, aktive Secure Session, Heartbeat und FrameUpdate in einem reproduzierbaren Smoke-Test. TLS bleibt ein markierter Blocker; der aktuelle Pfad ist `SecureDev/NamedPipeDevFallback` mit `DevelopmentAuthenticated`.

MA011.04 rendert die erste PDF-Seite im Development-Pfad als PNG-Frame. MA011.05 stabilisiert den Windows-Pilot fuer Owner-/Gast-Testanzeigen. MA011.06 verbindet diesen Pilot mit einer lokalen Glass-Edge-PlaySequence.

Die naechsten Schritte sind Manual Map UI/CLI fuer echte Lab-Aufbauten und danach die konkreten macOS- und iOS/iPadOS-Gastpfade.

## MA016 Real Platform Execution

MA016 verschiebt den Fokus vom vorbereiteten Readiness-Stand in den Real-Platform-Pilot. Windows bleibt die erste aktive Owner-Ablage. macOS wird als echte Gegenplattform angebunden, iPad/iPhone werden ueber macOS/Xcode installierbar und testbar vorbereitet.

Der MA016-Produktpfad ist:

1. geschlossene PDF auf Windows als Frame-Kapsel nehmen.
2. Kapsel auf eine andere Ablage legen.
3. Kapsel im kontrollierten Frame oeffnen.
4. geoeffnete PDF als OpenFrame weiterfuehren.
5. No File Ingress fuer Capsule und OpenFrame beweisen.
6. Return und Recovery beweisen.
7. Manual Map und UWB-Simulator als Naehequellen fuer Glass Edge nutzen.
8. macOS- und iOS-Handoffs so exportieren, dass externe Plattform-Codex-Arbeit starten kann.

MA016 ist kein Sync- oder Dateiuebertragungs-Sprint. Die Originalablage bleibt Eigentuemerin, bis eine explizite, erlaubte und auditierte Besitzuebernahme stattfindet.

## Pilot-Lab Go/No-Go

Nach MA016 muss ein Go/No-Go fuer groessere Tests vorliegen. Go ist nur moeglich, wenn Windows lokal Closed PDF und Open PDF testen kann, No File Ingress gruen ist, macOS/iOS-Handoffs vollstaendig sind, Proximity/UWB simulierbar ist und das Context Pack alle Runbooks, Schemas, Configs und Reports enthaelt.

## MA017 Real Pilot Execution

MA017 startet nach dem finalen MA016-Commit. Der Fokus wechselt von Readiness zu echter Ausfuehrung:

1. Windows bleibt Owner-Referenz.
2. macOS wird erster nativer Guest-Pilot.
3. iPad/iPhone folgen als mobile Ablagen.
4. Closed PDF Capsule und Open PDF Frame werden Standardtests.
5. Proximity entscheidet genau eine naechste Ablage.
6. Owner-Testfragen entscheiden ueber Human Experience.

MA017 darf keine MA016-Sicherheitsregel abschwaechen. No File Ingress, Owner Lock, Return und Recovery bleiben Gate-Kriterien.
