# Sensor Fusion Roadmap

Status: Draft  
Datum: 2026-07-06

## Ziel

AP158 beschreibt, wie Manual Map, BLE, WiFi, UWB, Dongle und spaeter weitere Signale zu einer ruhigen Ablage-Auswahl zusammengefuehrt werden.

## Fuehrungsregel

Die Fusion hat nur ein sichtbares Ergebnis:

```text
genau eine naechste Ablage oder keine Ablage
```

Sie darf kein Radar, keine Mehrfachziele und keine nervoes wechselnden Kanten erzeugen.

## Quellen

- Manual Map: stabile Richtung und Owner-Lab-Aufbau.
- BLE: grobe Naehe und Anwesenheit.
- WiFi: Reachability und gleicher lokaler Raum.
- Dongle: stabile Ablage-Identitaet und Anker.
- UWB: praezise Distanz und optional Richtung.
- SensorFusion: geglaettetes Gesamtergebnis.

## Gewichtung

Fruehe Reihenfolge:

1. Owner Manual Override, falls aktiv.
2. UWB, falls stabil und vertrauenswuerdig.
3. Dongle Identity mit ManualMap-Richtung.
4. Manual Map.
5. BLE/WiFi als Confidence-Signal.
6. Simulation fuer Tests.

## Stabilitaet

Fusion benoetigt:

- Source Confidence
- Messalter
- Hysterese
- EdgeSwitchDelay
- Stale-Timeout
- Plausibilitaetscheck gegen Manual Map
- Fallback bei fehlender Richtung

## Fehlerfaelle

Wenn Signale widerspruechlich sind:

- keine harte Umschaltung
- bisherige stabile Kante halten
- Confidence senken
- Diagnose schreiben
- bei starkem Konflikt keine Kante anbieten

## Produktwirkung

Die Fusion darf fuer den Menschen nur ruhiger wirken:

- eine gläserne Kante
- klare Richtung
- keine technischen Quellen sichtbar
- keine Geraete-Liste im Produktpfad

## Nicht-Ziele

- keine perfekte Indoor-Navigation
- keine Karte des Raums
- keine Personenortung
- keine Cloud-Auswertung
- keine automatische Ownership-Entscheidung
