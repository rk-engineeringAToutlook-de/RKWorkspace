# Ablage Anchor Dongle MVP

Status: Draft  
Datum: 2026-07-06

## Leitsatz

Der Dongle ist kein Transferstick. Der Dongle ist ein Ablage-Anker.

## Ziel

AP155 definiert den ersten Hardware-MVP fuer einen physischen Anker im Arbeitsraum. Er macht eine Ablage auffindbar, aber speichert keine Originaldaten und transportiert keine Nutzdaten.

## MVP-Faehigkeiten

Mindestumfang:

- stabile DongleId
- zugeordnete AblageId
- BLE-Beacon oder aehnliches Presence-Signal
- USB-Stromversorgung
- lokaler Status fuer Laborbetrieb
- Firmware-Version
- Reset-/Recovery-Pfad
- optional Secure-Element-Vorbereitung

Optional spaeter:

- UWB-Ranging
- USB-Control-Kanal
- LED/Haptik nur fuer Pairing-Status
- signiertes Firmware-Update

## Nicht-Ziele

- keine Dateiablage
- kein Massenspeicher
- kein Copy-Stick
- keine Cloud-Kopplung
- keine Ownership-Entscheidung
- keine Frame-Daten im Dongle

## Rolle im Produktpfad

Der Dongle repraesentiert einen Ort, nicht zwingend einen Computer. Er kann an einem Monitor, KVM-Platz, Industriepanel oder Arbeitsplatz liegen und sagen:

```text
Hier ist eine Ablage.
```

## Datenmodell

Ein Dongle darf spaeter in die Shell liefern:

- `AblageId`
- `AblageSurfacePlatform.Unknown` oder Plattform-Hinweis
- Distanzklasse
- optionale Distanz in Metern
- Confidence
- ProximitySource `Dongle`
- letzter Sichtzeitpunkt

## Erste Laborentscheidung

Vor Hardwarebestellung bleibt Manual Map verbindlich. Der Dongle-MVP startet erst, wenn:

- Single Glass Edge stabil ist
- Manual Map Owner-Test bestanden ist
- RKWP FrameOnly stabil ist
- Security Gate fuer Pairing/Trust nicht verletzt wird
