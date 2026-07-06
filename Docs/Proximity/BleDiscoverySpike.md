# BLE Discovery Spike

Status: Draft  
Datum: 2026-07-06

## Ziel

AP152 beschreibt BLE als grobe Anwesenheits- und Naehequelle fuer Ablagen. BLE ist nicht der Datenkanal und nicht der Beweis, dass ein digitales Ding uebergeben werden darf.

## Rolle im Workspace

BLE beantwortet nur:

```text
Welche Ablage ist wahrscheinlich in der Naehe?
```

BLE beantwortet nicht:

- Wem gehoert das Objekt?
- Darf ein Frame geoeffnet werden?
- Welche Nutzdaten werden transportiert?
- Ist der Peer vertrauenswuerdig?

Diese Fragen bleiben RKWP, Policy und Trust vorbehalten.

## Geplantes Signal

Ein BLE-Beacon kann spaeter liefern:

- AblageId oder kurzlebiger Alias
- PlatformHint
- RSSI
- letzter Sichtzeitpunkt
- optional Dongle-/Surface-Klasse
- optional Pairing-Status-Hinweis

## Grenzen

BLE-RSSI ist unruhig. Menschen, Metall, Gehaeuse, Wandnaehe, Antennenausrichtung und Betriebssystem-Scanning veraendern das Signal.

Deshalb darf BLE in V1 nur Confidence erhoehen oder senken. Es darf nicht allein die Glass Edge nervoes wechseln lassen.

## Mapping auf Shell-Modell

BLE wird spaeter als `AblageProximitySource.BLE` in diese Modelle uebersetzt:

- `AblageSurface`
- `AblageDistance`
- `AblagePose`
- `AblageProximitySnapshot`

Die Richtung kommt nicht verlaesslich aus BLE. Wenn keine weitere Quelle existiert, muss Richtung aus Manual Map oder letzter stabiler Position kommen.

## Hysterese

BLE benoetigt:

- MinimumConfidence
- geglaetteten RSSI-Mittelwert
- Wechselverzoegerung
- Stale-Timeout
- starke Hysterese gegen Flackern

## Smoke-Idee

Ein spaeterer BLE-Smoke erzeugt drei simulierte Beacon-Signale:

1. macOS rechts, starkes Signal.
2. iPad oben, mittleres Signal.
3. Android links, schwaches Signal.

Erwartung:

- genau eine naechste Ablage
- keine Mehrfachkante
- schwache Ziele werden ignoriert
- Glass Edge bleibt stabil

## Nicht-Ziele

- kein Bluetooth-Payload-Transfer
- kein BLE-Mesh
- keine Hintergrundpflicht fuer iOS/Android im ersten Schritt
- keine Security-Entscheidung durch RSSI
