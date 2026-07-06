# ADR MA016 Proximity Fusion

Dokument-ID: RKWS-ADR-MA016-PROXIMITY-FUSION
Version: 1.0.0
Status: Accepted
Datum: 2026-07-07

## Problemstellung

RK Workspace soll beim Tragen eines digitalen Dings nur die naechste sinnvolle Ablage anzeigen. Mehrere Bubbles, Radar-Optik oder technische Zielauswahl zerstoeren das Arbeitsraumgefuehl.

## Moegliche Alternativen

1. Alle moeglichen Ablagen gleichzeitig anzeigen.
2. Den Benutzer ein Ziel aus einer Liste auswaehlen lassen.
3. Manual Map, UWB-Simulation und spaetere Sensoren zu einer Proximity-Fusion zusammenfuehren.

## Bewertung der Alternativen

Alle Ziele gleichzeitig erzeugen visuelle Unruhe und erinnern an Geraeteauswahl.

Eine Liste ist technisch eindeutig, aber keine Human Experience.

Proximity Fusion erlaubt, dass RK Workspace genau eine Glass Edge aktiviert: die naechste Ablage mit ausreichender Confidence.

## Getroffene Entscheidung

MA016 verwendet Proximity Fusion als Zielauswahlregel fuer Glass Edge:

- genau eine aktive Glass Edge,
- Richtung aus Naehe und Raumkarte,
- Confidence als Stabilitaetsfilter,
- ManualMap als nachvollziehbarer Lab-Pfad,
- UWB-Simulation als vorbereiteter Hardware-Pfad.

## Konsequenzen

Die UX zeigt nicht alle verfuegbaren Geraete. Der Produktpfad zeigt nur die naechste Ablage. Fallbacks muessen menschlich erklaerbar bleiben.

## Risiken

- Falsche Naehe fuehrt zu falscher Kante.
- Zu hohe Instabilitaet laesst die Kante springen.
- Zu spaete Stabilisierung fuehlt sich traege an.

## Offene Punkte

- echte UWB-Hardwarevalidierung,
- Sensor-Fusion mit BLE/WiFi/Dongle,
- Owner-Override fuer falsche Naehe,
- native Visualisierung auf macOS, iOS/iPadOS, Android und Windows.

## Querverweise

- `Docs\AblageProximityAndDistance.md`
- `Docs\GlassEdgeNearestAblage.md`
- `Docs\Proximity\SensorFusionRoadmap.md`
- `Docs\HumanExperience\MA016_ProximityFeelingTest.md`

