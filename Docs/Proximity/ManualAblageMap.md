# Manual Ablage Map

Status: Accepted  
Datum: 2026-07-06

## Ziel

Solange BLE, UWB und Ablage Anchor Dongle noch nicht real messen, beschreibt die manuelle Raumkarte die naechsten Ablagen im Arbeitsraum.

Beispiel:

- macOS steht rechts und nah.
- iPad liegt oben und mittelweit.
- iPhone liegt unten und nah.
- Linux steht links und weit.

Die Karte ist kein Pairing, keine Dateiuebertragung und keine Ownership-Entscheidung. Sie liefert nur Richtung und Entfernung fuer die eine Glass Edge.

## Lokaler Pfad

Nutzerspezifische Karte:

```text
config/manual-ablage-map.json
```

Dieser Pfad ist absichtlich in `.gitignore`.

Sample im Repository:

```text
config/samples/manual-ablage-map.sample.json
```

## Tool

```powershell
.\tools\run-manual-map.ps1 -List
.\tools\run-manual-map.ps1 -Show
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near
.\tools\run-manual-map.ps1 -Set -Ablage iPad -Direction Up -Distance Medium
.\tools\run-manual-map.ps1 -Remove -Ablage iPad
.\tools\run-manual-map.ps1 -Clear
.\tools\run-manual-map.ps1 -Import config/samples/manual-ablage-map.sample.json
.\tools\run-manual-map.ps1 -Export config/manual-map-lab.json
.\tools\run-manual-map.ps1 -Validate
.\tools\run-manual-map.ps1 -SmokeTest
```

AP047 macht diese Modi fuer den Owner-Lab-Aufbau verbindlich:

- `-List`: kompakte Ausgabe der lokalen Karte.
- `-Show`: Karte plus Selector-/Validierungsstatus.
- `-Set`: Ablage setzen oder aktualisieren.
- `-Remove`: Ablage aus der lokalen Karte entfernen.
- `-Clear`: lokale Karte loeschen.
- `-Import`: JSON-Karte importieren und validieren.
- `-Export`: lokale Karte als JSON exportieren.
- `-Validate`: Karte gegen Modell und Selector pruefen.
- `-SmokeTest`: alle Modi in einem isolierten Log-Pfad testen.

## Modell

- `ManualAblageMap`
- `ManualAblageMapEntry`
- `ManualAblageMapStore`
- `ManualAblageMapSerializer`
- `ManualAblageMapValidator`
- `ManualMapAblageProximityProvider`
- `AblageProximityProviderChain`

## Felder

- `AblageId`
- `DisplayName`
- `RelativeDirection`
- `DistanceClass`
- `DistanceMeters`
- `Confidence`
- `IsAvailable`
- `LastUpdated`
- `Source`
- `Platform`

## Richtung

Erlaubt:

- `Left`
- `Right`
- `Up`
- `Down`
- `UpLeft`
- `UpRight`
- `DownLeft`
- `DownRight`
- `Front`
- `Back`

`Unknown` wird fuer manuelle Ziele abgelehnt.

## Entfernung

Erlaubt:

- `VeryNear`
- `Near`
- `Medium`
- `Far`
- `VeryFar`

`Unknown` wird fuer manuelle Ziele abgelehnt.

## Prioritaet

Die Proximity-Quelle ist vorbereitet als:

1. ManualMap, wenn Eintraege vorhanden sind.
2. Simulated.
3. spaeter BLE/UWB/Dongle.

Der `NearestAblageSelector` bleibt fuer Hysterese und NoFlicker zustaendig. Auch mit mehreren Eintraegen wird nur eine Ablage gewaehlt.

## Smoke-Test

Der Smoke-Test prueft:

- Set.
- List.
- Export.
- Import.
- Remove.
- Clear.
- Validate.
- Auswahl aus ManualMap.
- naechste Ablage.
- Edge-Richtung.
- ungueltige Werte werden abgelehnt.

## Verbindung Zum PDF Frame E2E

MA009.06 verbindet die Karte mit dem PDF-Frame-Pfad:

```powershell
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near
.\tools\run-glass-edge-pdf-frame-e2e.ps1 -UseManualMap -TargetAblage macOS -PdfPath "samples\Objects\Rechnung.pdf"
```

Dadurch entstehen:

- ManualMap als Proximity-Quelle.
- macOS als naechste Ablage.
- rechte Glass Edge.
- CarryLease.
- FrameSession.
- No File Ingress.
- Return und Recovery.
