# Ablage Proximity And Distance

Status: Accepted  
Datum: 2026-07-05

## Zweck

MA006.13 fuehrt Entfernung und Richtung als eigene Shell-nahe Logik ein. Die erste Implementierung ist simuliert, aber providerfaehig.

MA007.13 vertieft diese Schicht: RK Workspace kann jetzt eine manuelle Raumkarte modellieren und daraus stabil genau eine naechste Ablage ableiten.

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
- `AblageProximitySource`
- `IAblageProximityProvider`
- `INearestAblageSelector`
- `SimulatedAblageProximityProvider`
- `ManualAblageMap`
- `ManualMapAblageProximityProvider`

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

MA007.13 bereitet `ManualMap` als zweite Quelle vor. BLE, UWB, Dongle, WiFi und SensorFusion bleiben dokumentierte Folgequellen und liefern spaeter dieselben Modelle.

MA009.05 macht ManualMap nutzbar:

- persistenter Store `config/manual-ablage-map.json`,
- Sample `config/samples/manual-ablage-map.sample.json`,
- Tool `tools/run-manual-map.ps1`,
- Serializer und Validator,
- Provider-Kette mit Prioritaet ManualMap vor Simulation.

MA011.07 erweitert die Manual Map zur vollstaendigen Lab-CLI. Der Owner kann Ablagen setzen, anzeigen, entfernen, importieren, exportieren, validieren und loeschen:

```powershell
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near -Confidence 0.9
.\tools\run-manual-map.ps1 -Set -Ablage iPad -Direction Up -Distance Medium -Confidence 0.7
.\tools\run-manual-map.ps1 -Set -Ablage iPhone -Direction Down -Distance Near -Confidence 0.86
.\tools\run-manual-map.ps1 -Set -Ablage Linux -Direction Left -Distance Far -Confidence 0.72
.\tools\run-manual-map.ps1 -Validate
```

Der `NearestAblageSelector` nutzt diese Map direkt. Trotz mehrerer Ablagen bleibt die Glass Edge bei genau einer naechsten Ablage.

## Manual Map

`ManualAblageMap` beschreibt eine Ablage relativ zur aktuellen Arbeitsflaeche:

- `AblageId`
- `DisplayName`
- `RelativeDirection`
- `DistanceClass`
- `DistanceMeters`
- `Confidence`
- `IsAvailable`
- `LastUpdated`
- `Source`

Die Owner-Raumkarte kann damit ohne Sensorik vorbereitet werden, zum Beispiel:

- macOS rechts, nah
- iPad oben, mittel
- iPhone unten, nah
- Monitor links, weit

Der `ManualMapAblageProximityProvider` erzeugt daraus ein `AblageProximitySnapshot`. Er ist noch keine echte Discovery und keine Kopplung. Er ist die Bruecke, um Entfernung und Richtung im Produktpfad zu testen.

Der lokale Pfad `config/manual-ablage-map.json` ist nutzerspezifisch und wird nicht versioniert. Die versionierte Sample-Datei liegt unter `config/samples/manual-ablage-map.sample.json`.

MA010.09 ergaenzt `RKWorkspace.Configuration`. Die zentrale Sample-Config verweist auf `config/manual-ablage-map.json`, waehrend echte lokale Configs unter `config/` nicht versioniert werden. `run-config-tool.ps1 -SmokeTest` prueft, dass die Manual-Map-Sample-Datei vorhanden ist und Eintraege enthaelt.

## Stabilitaet

Der Selector beruecksichtigt Confidence, bevorzugte Richtung, letzte Aktivitaet und Hysterese. Kleine Distanzschwankungen sollen die Kante nicht nervoes wechseln lassen.

Konkrete Regeln:

- Ziele unter `MinimumConfidence` werden ignoriert.
- Distanz und Confidence bilden den Score.
- `DistanceHysteresis` haelt die bisherige Kante bei kleinen Schwankungen.
- `EdgeSwitchDelay` verhindert schnelles Hin-und-Her-Schalten.
- Bei deutlich besserem Ziel darf die Kante trotzdem wechseln.
- Das Ergebnis beschreibt immer genau eine Zielablage oder gar keine.

Damit bleibt die Glass Edge ruhig: nicht mehrere Ziele, kein Radar, kein Flackern.

## Bedeutung fuer RKWP

Ab MA007.00 entscheidet Proximity nicht ueber Dateiuebertragung. Proximity entscheidet nur, welche Ablage als naechste sinnvolle Gegenflaeche angeboten wird.

Die anschliessende RKWP-Session entscheidet ueber:

- CarryLease
- FrameOnly oder spaeter andere Modi
- Rechte
- Rueckgabe
- Recovery

Entfernung und Richtung bleiben damit Wahrnehmungs- und Zielwahlkontext, nicht Besitzlogik.
