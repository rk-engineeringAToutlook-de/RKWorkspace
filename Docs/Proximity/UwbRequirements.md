# UWB Requirements

Status: Draft  
Datum: 2026-07-06

## Ziel

AP154 beschreibt UWB als spaetere Praezisionsquelle fuer Entfernung und moeglichst Richtung. UWB ist hilfreich fuer die eine gläserne Kante, aber keine Voraussetzung fuer den ersten Produktpfad.

## Nutzen

UWB kann spaeter liefern:

- deutlich bessere Distanz als BLE/WiFi
- moegliche Richtung oder Winkelinformation
- stabileres Nahe-/Fern-Verhalten
- robustere Auswahl bei mehreren Ablagen im selben Raum

## Anforderungen

Eine UWB-Integration muss liefern:

- AblageId oder gekoppelte DongleId
- Distanz in Metern
- Confidence
- Zeitstempel
- Stale-Status
- optional Richtung/Winkel
- Fehlergrund bei nicht verfuegbarer Messung

## Mapping

UWB-Daten werden auf `AblageDistance.FromSource(..., AblageProximitySource.UWB)` abgebildet.

Wenn Richtung verfuegbar ist:

```text
UWB angle -> AblagePose -> EdgeHint
```

Wenn Richtung fehlt:

```text
UWB distance + ManualMap direction -> AblagePose -> EdgeHint
```

## Plattformrealitaet

UWB ist stark plattform- und hardwareabhaengig. iOS, Android und externe Dongles haben unterschiedliche APIs, Berechtigungen und Kopplungsmodelle.

Deshalb gilt:

- UWB zuerst als Hardware-/Dongle-Spike.
- Kein Produktversprechen vor realem Labortest.
- Manual Map bleibt Fallback.

## Sicherheitsgrenzen

UWB bestaetigt Naehe. Es bestaetigt keine Trust-Beziehung und keinen Besitz.

RKWP entscheidet weiterhin ueber:

- Secure Session
- CarryLease
- FrameSession
- Return
- Recovery

## Abnahmekriterien fuer spaeteren Spike

- Messwert bleibt ueber 30 Sekunden stabil.
- Glass Edge springt nicht zwischen Zielen.
- Entfernen einer Ablage fuehrt zu sauberem `NoTarget`.
- UWB-Ausfall faellt auf Manual Map oder BLE/WiFi zurueck.
