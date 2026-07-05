# Ablage Proximity And Distance

Status: Accepted  
Datum: 2026-07-05

## Zweck

MA006.13 fuehrt Entfernung und Richtung als eigene Shell-nahe Logik ein. Die erste Implementierung ist simuliert, aber providerfaehig.

## Stufen

1. Simulierte Raumkarte
2. Manuelle Raumkarte
3. BLE-RSSI / lokale Naeherung
4. Dongle als Ablage-Anker
5. UWB-Ranging / Richtung
6. Sensorfusion / Kamera / AR / raeumliche Kalibrierung

## Modelle

- `AblageDistance`
- `AblageDirection`
- `AblagePose`
- `AblageProximitySnapshot`
- `IAblageProximityProvider`
- `INearestAblageSelector`
- `SimulatedAblageProximityProvider`

## Quellen

Vorbereitet sind:

- `Simulated`
- `ManualMap`
- `BLE`
- `UWB`
- `Dongle`
- `WiFi`
- `SensorFusion`
- `Unknown`

MA006.13 nutzt ausschliesslich `Simulated`.

## Stabilitaet

Der Selector beruecksichtigt Confidence, bevorzugte Richtung, letzte Aktivitaet und Hysterese. Kleine Distanzschwankungen sollen die Kante nicht nervoes wechseln lassen.
