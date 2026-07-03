# Surface Overlay Reset

Dokument-ID: RKWS-DEV-SURFACE-OVERLAY-RESET
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Ziel

MA006.07 reduziert die Spatial Surface radikal.

Nicht:

```text
Ich sehe eine Web-App mit einer Raumkarte.
```

Sondern:

```text
Ich sehe meine Ablage.
Ich nehme ein Ding.
Erst dann zeigt der Raum Moeglichkeiten.
```

## Grundsatz

Der Raum wird nicht als Karte dargestellt.

Der Raum reagiert.

Ablagen sind nicht dauerhaft sichtbar. Sie erscheinen erst, wenn der Mensch ein digitales Ding traegt.

## Verworfen

Nicht mehr Produktpfad:

- zentrale Statuskarte.
- zentrale Warteflaeche.
- Radar-Kreis.
- dauerhafte Bubble-Karte.
- sichtbarer Zuruecklege-Button.
- erklaerende Web-App-Struktur.

## Surface

Die Surface ist eine ruhige Ablageflaeche.

Wenn nichts dort liegt, wirkt sie leer.

Nicht:

```text
wartend
empfangsbereit
technisch aktiv
```

Wenn ein Ding dort liegt, liegt es direkt auf der Flaeche. Nicht in einer Liste, nicht in einem Panel und nicht in einer Statuskarte.

## Bubbles

Bubbles sind nur sichtbar, wenn eine Tragehandlung aktiv ist.

Bei Empty:

- keine Bubbles.
- keine Karte.
- keine Ziele.
- keine Raumgrafik.

Bei Carried:

- Bubbles erscheinen peripher.
- Bubbles orientieren sich am Rand der Surface.
- Bubbles wirken wie Moeglichkeiten.
- aktive Bubbles oeffnen sich als Portal.

## Fullscreen Und PWA

V1 bleibt technisch ein lokaler Browser-Prototyp.

Fuer den Gefuehlstest soll Browser-Chrome trotzdem moeglichst verschwinden:

- Web App Manifest.
- Standalone-/Fullscreen-Vorbereitung.
- Hinweis im Terminal: auf Handy oder Tablet zum Startbildschirm hinzufuegen oder im Vollbildmodus oeffnen.

Das ist keine native App und keine App-Store-Implementierung.

## Langfristige Richtung

Die Surface soll spaeter ueber der echten Handy-, Tablet- oder Desktop-Oberflaeche liegen.

Der reale Hintergrund muss langfristig erhalten bleiben, weil RK Workspace keine neue App-Welt zeigen soll. RK Workspace soll den bestehenden Arbeitsraum um eine Faehigkeit erweitern.

## Smoke-Test

Der Smoke-Test prueft ab MA006.07:

- keine zentrale Statuskarte.
- keine Radarstruktur.
- keine sichtbaren Bubbles bei Empty.
- Bubbles erst bei aktiver Tragehandlung.
- periphere Bubble-Positionen.
- kein sichtbares `Zuruecklegen`.
- Standalone-Manifest.
- Portal, Ghost und No-Jump-Regel bleiben erhalten.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | Surface Overlay Reset fuer MA006.07 dokumentiert. |
