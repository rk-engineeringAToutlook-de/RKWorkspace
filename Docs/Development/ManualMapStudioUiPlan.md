# Manual Map Studio UI Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP151 bereitet die sichtbare Manual-Map-Bedienung im Developer Studio vor, ohne die produktive Shell oder den Core zu veraendern.

Die Manual Map beschreibt, welche Ablage im Arbeitsraum relativ zur aktuellen Ablage liegt. Sie ist die Vorstufe zu BLE, WiFi, UWB und Dongle-Distanzmessung.

## Warum kein grosser GUI-Umbau in diesem Schritt

Das Developer Studio ist Labor und Diagnosewerkzeug. Der Produktpfad bleibt Workspace Shell. Deshalb wird die UI in diesem Block als klarer Plan und Smoke-Test-Pfad dokumentiert, waehrend die bestehende Manual-Map-CLI weiter die verbindliche Bedienung bleibt.

## Geplante Registerkarte

Name:

```text
Manual Map
```

Sichtbare Bereiche:

- aktuelle Ablage
- bekannte Ablagen
- Richtung
- Distanzklasse
- optionale Meterdistanz
- Confidence
- Plattform
- Verfuegbarkeit
- naechste Ablage
- daraus entstehende Glass Edge

## Bedienung

Der Owner kann spaeter:

- Ablage hinzufuegen
- Ablage entfernen
- Richtung aendern
- Distanzklasse aendern
- Confidence setzen
- Ablage kurzzeitig deaktivieren
- Karte importieren
- Karte exportieren
- Auswahl validieren

## Sprache

Die UI spricht nicht von Geraeten als Zentrum. Erlaubt:

- Ablage
- Arbeitsflaeche
- rechts
- links
- oben
- unten
- nah
- weiter entfernt
- naechste Moeglichkeit

Vermeiden:

- Send to device
- Transfer target
- Upload
- Download
- remote machine

## Bestehende Bedienung bis zur UI

```powershell
.\tools\run-manual-map.ps1 -Show
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near -Confidence 0.90
.\tools\run-manual-map.ps1 -Validate
.\tools\run-studio.ps1 -SmokeTest
```

## Smoke-Test-Regel

`run-studio.ps1 -SmokeTest` bleibt der Studio-Gate-Test. Er muss weiterhin gruen bleiben, waehrend Manual Map als Diagnose-/Konfigurationspfad separat ueber `run-manual-map.ps1` abgesichert wird.

## Nicht-Ziele

- keine echte Sensorik
- keine automatische Discovery
- keine Netzwerksuche
- kein Pairing
- kein Besitzwechsel
- keine Dateiuebertragung

## Uebergang zum Produktpfad

Wenn BLE, WiFi, UWB oder Dongle-Daten verfuegbar sind, muessen sie dieselbe `AblageProximitySnapshot`-Struktur liefern. Die Studio-UI bleibt dann nur noch Sichtfenster auf die Daten, nicht deren Quelle.
