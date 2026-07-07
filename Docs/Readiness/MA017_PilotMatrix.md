# MA017 Pilot Matrix

Status: Draft
Datum: 2026-07-07

## Zweck

Diese Matrix definiert die ersten MA017 Pilotlaeufe.

| Pilot | Owner | Guest | Objekt | Pfad | Erwartung |
| --- | --- | --- | --- | --- | --- |
| P1 | Windows | Windows lokal | Rechnung.pdf | Closed PDF Capsule | gruen, Referenz |
| P2 | Windows | Windows lokal | Rechnung.pdf | Open PDF Frame | gruen, Referenz |
| P3 | Windows | macOS | Rechnung.pdf | Closed PDF Capsule | erster realer Cross-Device-Pfad |
| P4 | Windows | macOS | Rechnung.pdf | Open PDF Frame | Frame Presenter laeuft nativ |
| P5 | Windows | iPad | Rechnung.pdf | Closed PDF Capsule | iPad als Ablage |
| P6 | Windows | iPad | Rechnung.pdf | Open PDF Frame | Haptik/Touch pruefen |
| P7 | Windows | iPhone | Rechnung.pdf | Compact Frame | kompakte Surface pruefen |
| P8 | Windows | macOS + iPad | Rechnung.pdf | Nearest Ablage | Proximity-Auswahl pruefen |
| P9 | Windows | simulated UWB/Dongle | Rechnung.pdf | Glass Edge + nearest Ablage | genau eine naechste Ablage |
| P10 | Windows | Manual Map + UWB Fusion | Rechnung.pdf | Fusion | keine springende Kante |

## Pflichtbeobachtung

Jeder Pilot dokumentiert:

- No File Ingress.
- Owner Lock.
- Return.
- Recovery.
- aktive Glass Edge.
- Direction, Distance und Confidence bei Proximity-Piloten.
- Owner-Bewertung: Gruen, Gelb oder Rot.
