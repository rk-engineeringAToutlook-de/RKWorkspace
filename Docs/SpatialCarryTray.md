# Spatial Carry Tray

Dokument-ID: RKWS-SPATIAL-CARRY-TRAY
Version: 1.5.0
Status: Accepted
Datum: 2026-07-03

## Wichtigster Satz

Der Mensch traegt das digitale Ding nicht im Fenster.

Er traegt es auf einem Geraet in seiner Hand durch seinen realen Raum.

Eine Ablage zeigt sich nicht sofort als Ziel.

Sie offenbart sich durch Naehe.

Erst wenn der Mensch nahe genug ist, erkennt er, was dort liegt oder moeglich ist.

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

## Einordnung Nach MA006.04

MA006.04 erweitert diesen Prototyp zur Spatial Room Session.

Damit sind `/tray` und `/ablage` nicht mehr zwei unterschiedliche Konzepte.

Kanonisch sind jetzt:

```text
/surface/handy
/surface/monitor
```

Alle Oberflaechen sehen denselben Raumzustand.

Das Ding existiert nur einmal:

- liegt initial auf Ablage Handy
- verschwindet dort beim Greifen
- befindet sich waehrend Carry in der digitalen Hand
- erscheint als Preview auf der Zielablage
- liegt nach Place auf der Zielablage
- kann von dort wieder genommen werden

Die Details stehen in `Docs/SpatialRoomSession.md`.

Die verfeinerte MA006.04-Fassung fuehrt `OpeningAblage` und die Ablage-Linse ein. Aktive Ablagen reagieren auf Naehe, werden lesbarer, zeigen eine innere Ablageflaeche und lassen erst dann `Hier ablegen` erscheinen. Mindestens fuenf Ablagen sind im Raum vorbereitet.

MA006.05 konzentriert diesen Pfad auf den Tactile Mobile Carry Slice: Handy oder Tablet soll weniger wie Webseite und staerker wie digitale Hand wirken. Das Ding loest sich mit kurzem Widerstand, wird kompakter, ist teilweise verdeckt, neigt sich nach Bewegungsvektor und gleitet in die geoeffnete Ablage-Bubble.

MA006.06 erweitert die geoeffnete Bubble zum Spatial Portal Carry. Die Ablage am Rand des Raums wirkt nicht mehr nur wie ein Ziel, sondern wie eine Oeffnung. Das Ding verschwindet teilweise auf der Quelle, erscheint teilweise als Ghost im Ziel und wird erst bei `Place` endgueltig dort abgelegt.

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

## Digitale Hand Und Optische Haptik

Ab MA006.03-A wirkt das Ding beim Greifen nicht mehr wie ein Objekt, das am Cursor klebt.

Stattdessen besitzt es eine subtile digitale Griffzone:

- ein Teil des Dings wird optisch ueberdeckt
- die Kontaktseite wird leicht abgedunkelt
- Schatten und Kontaktflaeche veraendern sich
- das Ding folgt weich und mit minimaler Traegheit
- Wabern ist stark reduziert und bleibt nur noch als ruhige Lebendigkeit erhalten

Das Ziel ist:

```text
Ich habe es gefasst.
Ein Teil liegt jetzt in meiner digitalen Hand.
```

Nicht:

```text
Das Icon haengt an meinem Finger.
```

Ab MA006.05 wird dieser Moment enger gefuehrt:

- `beruehren`: dezente Kontaktflaeche und Griffschatten.
- `Widerstand`: die ersten Pixel geben nur weich nach.
- `loesen`: das Ding hebt sich sichtbar aus der Ablage.
- `halten`: das Ding wird kompakter und teilweise verdeckt.
- `tragen`: die Neigung folgt dem Bewegungsvektor.

Das Ding darf nicht wabern. Es darf nur leicht leben: durch sanfte Traegheit, ruhigen Nachlauf und kurze Antwort beim Loesen.

Auf Handy oder Tablet wird `navigator.vibrate` subtil genutzt, wenn verfuegbar:

- beim Greifen.
- beim Loesen.
- beim Erreichen einer aktiven Ablage.
- beim Ablegen.

Wenn keine echte Haptik verfuegbar ist, uebernimmt die optische Haptik: Teilverdeckung, Schatten, Kontaktflaeche, Kompression und Stabilisierung.

## Release Und Cancel

Normales Loslassen bedeutet:

```text
Ich lege es hier hin.
```

Deshalb springt das Ding nach dem Greifen nicht automatisch auf die alte Position zurueck.

Nur ein explizites Zuruecklegen bedeutet:

```text
Ich will es doch nicht nehmen.
Zurueck auf das Tablett.
```

Der Prototyp trennt deshalb:

- `Release`: hier ablegen, auch im freien Raum
- `Cancel`: zuruecklegen

## Ablage-Kompass

Der Ablage-Kompass ersetzt Bildschirmrand-Denken als Hauptgefuehl.

V1 simuliert:

- rechts: Ablage Monitor
- vorne: Ablage Schreibtisch
- links: Ablage links
- links hinten: Ablage Fenster
- hinten: Ablage Ruhe

Es gibt keine echte Raumvermessung. Die Punkte sind Wahrnehmungsanker.

## Ablage-Bubbles

Ab MA006.03-A sind Ablagen keine sofort lesbaren Schaltflaechen mehr.

Sie erscheinen als ruhige Moeglichkeiten im Raum:

- sehr weit: kleiner Punkt, Name nicht lesbar
- weit: Bubble sichtbar, Name nicht lesbar
- mittel: Name als Mikrotext angedeutet
- nah: Name lesbar
- sehr nah / aktiv: Name und `Hier ablegen`

Damit entsteht Entfernung ueber Groesse, Lesbarkeit und Naehe.

Ab MA006.05 ist die aktive Bubble ausdruecklich eine Oeffnung:

- die Ablage-Linse wird sichtbar.
- die Innenflaeche deutet einen Ort an.
- das Ding kann hinein gleiten.
- die Zielablage zeigt vorher einen Ghost.

Der Uebergang darf nicht wie ein Sprung wirken. Auf der tragenden Ablage gleitet das Ding in die Bubble; auf der Zielablage wird es als Ghost sichtbar und danach an einer relativen Position abgelegt.

Die konkreten Schwellen bleiben konfigurierbar:

- `NameRevealThreshold`
- `ActivationThreshold`
- `BubbleScaleFactor`
- `MicroTextOpacity`
- `SoftSnapStrength`

Das Einrasten ist weich. Die Ablage zieht das Ding nicht weg, sondern zeigt nur ruhig:

```text
Hier kannst du es hinlegen.
```

## Spatial Portal Carry

Ab MA006.06 ist die aktive Ablage-Bubble ein ruhiges Portal.

Der Ablauf:

```text
Ablage Handy
Ding nehmen
digitale Hand
Ablage Monitor oeffnet sich als Portal
Ding gleitet in das Portal
Ding erscheint auf Monitor
Ding wird dort oertlich abgelegt
```

Wichtig:

- kein Sofortsprung auf die Zielablage.
- kein Teleport.
- kein Senden-Gefuehl.
- kein sichtbarer Technikbegriff.
- kein finales Ablegen vor `Place`.

Stattdessen entsteht eine zweigeteilte Wahrnehmung:

- Quelle: `SourceVisualProgress` laesst das Ding optisch in die Oeffnung eintreten.
- Ziel: `TargetVisualProgress` laesst einen Ghost klarer und groesser werden.

Die Zielablage meldet vor dem Ablegen `ReadyToPlace`. Das bedeutet:

```text
Hier kannst du es ablegen.
```

Nicht:

```text
Es wurde uebertragen.
```

Die Position wird erst beim finalen Ablegen auf der Zielablage gespeichert.

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
- `POST /api/carry`
- `POST /api/near`
- `POST /api/release`
- `POST /api/cancel`

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

Zusaetzlich meldet der State:

- `carryState`: OnTray, Picked, Carried, Placed, Cancelled, Failed
- `bubbles`: mindestens fuenf Ablage-Bubbles mit Distanz, Lesbarkeit, Groesse und Aktivzustand
- `motion`: Bewegungsneigung nach Vektor, Widerstand, Loesetiefe und Glide-Dauer
- `haptics`: vorbereitete mobile Haptik, Teilverdeckung und optische Haptik
- `transition`: Ghost vor Place, Gleiten in die Bubble und relative Zielposition
- `portalTransition`: Source-/Target-Progress, Portalzustand und ReadyToPlace
- `placement`: Ablage oder freier Raum

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
6. Verschwindet ein Teil des Dings glaubwuerdig in meiner digitalen Hand?
7. Offenbaren sich Ablagen erst durch Naehe?
8. Fuehlt sich freies Ablegen besser an als automatisches Zurueckspringen?
9. Wirkt die Monitor-Ablage wie ein Durchgang statt wie eine Schaltflaeche?
10. Fuehlt sich das Auftauchen im Ziel eher kontinuierlich als sprunghaft an?

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
- Keine erzwungene echte Mobile-Haptik.
- Keine finale Produkt-UI.
- Keine echte Portal- oder Handover-Technik.
- Kein echtes Senden zwischen zwei Geraeten.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.5.0 | 2026-07-03 | MA006.06 Spatial Portal Carry mit ruhigem Ablage-Portal, Source-/Target-Progress, ReadyToPlace und No-Jump-Regel dokumentiert. |
| 1.4.0 | 2026-07-03 | MA006.05 Tactile Mobile Carry Slice mit Widerstand, Teilverdeckung, Vektor-Neigung, optionaler Haptik und gleitendem Ablegen dokumentiert. |
| 1.3.0 | 2026-07-03 | MA006.04 Ablage-Linse, OpeningAblage und fuenf vorbereitete Ablagen eingeordnet. |
| 1.2.0 | 2026-07-03 | MA006.04 Spatial Room Session als Erweiterung eingeordnet. |
| 1.1.0 | 2026-07-03 | MA006.03-A digitale Hand, optische Haptik, freie Ablage und Ablage-Bubbles dokumentiert. |
| 1.0.0 | 2026-07-03 | MA006.03 Spatial Carry Tray Prototype dokumentiert. |
