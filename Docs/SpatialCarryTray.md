# Spatial Carry Tray

Dokument-ID: RKWS-SPATIAL-CARRY-TRAY
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Wichtigster Satz

Der Mensch traegt das digitale Ding nicht im Fenster.

Er traegt es auf einem Geraet in seiner Hand durch seinen realen Raum.

## Ziel

MA006.03 testet, ob Handy oder Tablet staerker als digitales Tablett wirken als ein Vollbild-Overlay auf dem Desktop.

Das Ziel ist nicht technische Uebertragung. Das Ziel ist die Wahrnehmung:

```text
Ich habe etwas bei mir.
Ich sehe, wo ich es hinlegen kann.
Ich lege es dort ab.
```

## Grundentscheidung

```text
Handy / Tablet = digitale Hand / digitales Tablett
Desktop / Monitor = Ablage
Geraete = Orte im Raum
Ablagepunkte = wichtiger als Bildschirmraender
```

Das digitale Ding wird nicht aus einem Fenster geschoben. Es liegt auf dem mobilen Tablett und wird im realen Raum zu einer Ablage getragen.

## Projekt

Der lokale Web-Prototyp liegt in:

```text
src/Shell/RKWorkspace.Shell.SpatialTray
```

Start:

```powershell
.\tools\run-spatial-tray.ps1
```

Smoke-Test:

```powershell
.\tools\run-spatial-tray.ps1 -SmokeTest
```

Port waehlen:

```powershell
.\tools\run-spatial-tray.ps1 -Port 5099
```

## Lokaler Tray Server

Der Desktop startet einen lokalen HTTP-Server. Die angezeigte URL wird manuell auf dem Handy oder Tablet geoeffnet.

Beispiel:

```text
RK Workspace Spatial Carry Tray
Desktop-Ablage: aktiv
Mobile Tray oeffnen:
http://192.168.x.x:5099/tray
```

Wenn die lokale IP nicht erreichbar ist, kann am Desktop `http://localhost:5099/tray` verwendet werden. Fuer Handy oder Tablet muss die lokale Rechner-IP im selben WLAN genutzt werden.

## Keine Discovery

MA006.03 enthaelt bewusst keine automatische Suche:

- keine Discovery
- kein Pairing
- keine Kamera
- kein UWB
- keine Cloud
- keine native iOS- oder Android-App
- kein echtes Payload-System

Die URL wird manuell geoeffnet. Sicherheit folgt erst mit Pairing und Secure Session.

## Mobile Tray UI

Die mobile Weboberflaeche zeigt:

- ruhige Flaeche
- digitales Ding
- Ablage-Kompass
- Statushinweis

Sichtbare Sprache:

- Digitales Ding
- Rechnung.pdf
- Ablage Schreibtisch
- Ablage Monitor
- Hier ablegen
- Abgelegt

Nicht sichtbar verwendet werden sollen:

- Transfer
- Upload
- Download
- Datei senden
- Geraet
- Agent
- Workspace B
- IPC
- Server
- Client

## Digitales Ding

Das Demo-Ding ist:

```text
Rechnung.pdf
```

Es soll wirken wie:

```text
Das Ding liegt auf meinem digitalen Tablett.
```

Nicht wie:

```text
Eine Datei wartet auf Upload.
```

## Ablage-Kompass

Der Ablage-Kompass ersetzt Bildschirmrand-Denken als Hauptgefuehl.

V1 simuliert:

- rechts: Ablage Monitor
- vorne: Ablage Schreibtisch
- links: Ablage links

Es gibt keine echte Raumvermessung. Die Punkte sind Wahrnehmungsanker.

## Desktop Als Ablage

Der Desktop zeigt nur ein ruhiges Empfangssignal:

```text
Ablage Monitor bereit
```

Nach dem Ablegen:

```text
Hier liegt jetzt: Rechnung.pdf
```

Nicht:

```text
Transfer abgeschlossen
Datei empfangen
Upload erfolgreich
```

## Endpunkte

Minimal:

- `GET /tray`
- `GET /api/state`
- `POST /api/place`
- `GET /ablage`
- `GET /health`

Zusaetzlich fuer das mobile Verhalten:

- `POST /api/pick`
- `POST /api/near`

Diese Endpunkte dienen nur dem lokalen Prototyp. Sie sind keine finale Produkt-API.

## State Model

V1 verwendet:

- Idle
- ThingOnTray
- ThingPicked
- NearAblage
- Placed
- Cancelled
- Failed

Das Modell ist bewusst klein und beschreibt den Human-Experience-Pfad, nicht eine technische Uebertragung.

## Human Experience Referenz

Unterstuetzt:

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.
- HX-001A: Das Objekt antwortet mir.
- HX-002: Ich habe etwas in meiner Hand.

Vorgemerkt:

- HX-003: Ich sehe, wo ich es ablegen kann.

MA006.03 bereitet HX-003 vor, ohne sie vollstaendig normativ zu definieren.

## Owner-Test

Nach erfolgreicher Verifikation:

```powershell
.\tools\run-spatial-tray.ps1
```

Der Owner oeffnet die angezeigte URL auf Handy oder Tablet.

Testfrage:

```text
Fuehlt sich das mehr an wie:
Ich trage etwas im Raum und lege es auf eine Ablage
als der Desktop-Overlay-Test?
```

Bewertung:

```text
gruen / gelb / rot
```

Zusatzfragen:

1. Fuehlt sich das Handy oder Tablet wie eine digitale Hand oder ein digitales Tablett an?
2. Wirken die Ablagen wie Orte im Raum?
3. Denke ich an Geraete oder an Ablagen?
4. Fuehlt sich `Abgelegt` natuerlicher an als technische Abschluss-Sprache?
5. Ist das naeher an meiner Vision?

## Bekannte Grenzen

- Nur lokaler Web-Prototyp.
- Keine echte Payload.
- Keine Discovery.
- Kein Pairing.
- Keine Sicherheitsschicht.
- Keine echte Raumvermessung.
- Keine Kamera.
- Keine UWB-Logik.
- Keine native Mobile-App.
- Keine finale Produkt-UI.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | MA006.03 Spatial Carry Tray Prototype dokumentiert. |
