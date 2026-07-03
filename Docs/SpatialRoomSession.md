# Spatial Room Session

Dokument-ID: RKWS-SPATIAL-ROOM-SESSION
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Wichtigster Satz

Alle Ablagen sehen denselben Raum.

Das Ding existiert nur einmal.

Wenn der Mensch es nimmt, ist es in seiner Hand - nicht mehr auf der Ablage.

## Ziel

MA006.04 ersetzt das alte Sender/Empfaenger-Gefuehl durch eine gemeinsame Spatial Room Session.

Nicht:

```text
Handy sendet an Monitor.
```

Sondern:

```text
Der Mensch nimmt ein digitales Ding aus einer Ablage,
traegt es im Raum
und legt es auf einer anderen Ablage ab.
```

## Raumzustand

Der Raumzustand wird durch `SpatialRoomState` beschrieben.

Er enthaelt:

- `RoomId`
- `Ablagen`
- `Things`
- `ActiveCarry`
- `UpdatedAt`

Alle Oberflaechen lesen denselben Raumzustand.

## Gleichberechtigte Ablagen

MA006.04 fuehrt Oberflaechen ein:

```text
/surface/handy
/surface/monitor
/surface/tablet
/surface/desktop
```

Jede Surface ist eine Ablage im Raum.

Handy, Tablet, Monitor, Desktop und Beamer sind in der Wahrnehmung keine Geraete. Sie sind Orte, auf denen ein Ding liegen, ankommen oder genommen werden kann.

## Ablage-Modell

Eine Ablage wird durch `SpatialAblage` beschrieben:

- `AblageId`
- `DisplayName`
- `SurfaceType`
- `RelativePosition`
- `Distance`
- `IsAvailable`
- `IsActive`
- `CanReceive`
- `CanProvide`
- `LastSeen`

Beispiele:

- Ablage Handy
- Ablage Monitor
- Ablage Tablet
- Ablage Desktop
- Ablage Beamer

## Digitales Ding

Ein Ding wird durch `SpatialThing` beschrieben.

Es besitzt immer genau einen Zustand:

- `RestingOnAblage`
- `Picked`
- `Carried`
- `ApproachingAblage`
- `PreviewOnAblage`
- `PlacedOnAblage`
- `FreePlaced`
- `Cancelled`
- `Lost`

Ein Ding darf nicht gleichzeitig auf einer Ablage liegen und in der digitalen Hand sein.

Beim Greifen gilt:

```text
CurrentState = Carried
CurrentAblageId = null
CurrentCarryId = CarryId
```

Damit ist sichtbar:

```text
Das Ding liegt nicht mehr auf der alten Ablage.
Es ist in der digitalen Hand.
```

## Carry Session

Eine laufende Tragehandlung wird durch `SpatialCarrySession` beschrieben:

- `CarryId`
- `ThingId`
- `SourceAblageId`
- `CarrierAblageId`
- `TargetCandidateAblageId`
- `State`
- `StartedAt`
- `UpdatedAt`

Zustaende:

- None
- Candidate
- Picked
- Carried
- NearAblage
- PreviewingOnAblage
- Placed
- Cancelled

## Remote Preview

Wenn sich ein Ding einer Ablage naehert, sieht die Zielablage das Ding bereits kommen.

Beispiel:

```text
Rechnung.pdf kommt an
```

Das ist noch kein Ablegen.

Es ist die Wahrnehmung:

```text
Diese Ablage erkennt, dass etwas in ihren Raum kommt.
```

## Ablegen

Beim Ablegen auf einer Ablage gilt:

1. `CarrySession.State = Placed`
2. `Thing.CurrentState = PlacedOnAblage`
3. `Thing.CurrentAblageId = Zielablage`
4. `Thing.PositionOnAblage` wird gesetzt
5. Quelle bleibt leer
6. Ziel zeigt das Ding als dort liegend

Sprache:

```text
Abgelegt
Liegt jetzt hier
```

## Freies Ablegen

Wenn keine Ablage aktiv ist, bleibt das Ding dort liegen, wo der Mensch es loslaesst.

Das ist kein Fehler.

Das erzeugt Raumgefuehl:

```text
Ein Gegenstand springt nicht automatisch zurueck.
```

Nur explizites Zuruecklegen ist `Cancel`.

## Ablage-Bubbles

Jede Surface zeigt andere Ablagen als Bubbles im Raum.

Die Bubbles zeigen Entfernung ueber:

- Groesse
- Lesbarkeit
- Mikrotext
- aktive Naehe

Eine Ablage zeigt sich nicht sofort als Ziel.

Sie offenbart sich durch Naehe.

Erst wenn der Mensch nahe genug ist, erkennt er, was dort liegt oder moeglich ist.

## Endpunkte

MA006.04 stellt bereit:

- `GET /surface/{ablageId}`
- `GET /api/state`
- `POST /api/pick`
- `POST /api/move`
- `POST /api/approach`
- `POST /api/place`
- `POST /api/cancel`
- `GET /health`

Alte Einstiege bleiben nur als Komfort-Weiterleitungen:

- `/tray` -> `/surface/handy`
- `/ablage` -> `/surface/monitor`

## Nicht-Ziele

MA006.04 baut noch nicht:

- Discovery
- Pairing
- Sicherheitsschicht
- echte Payload
- native Mobile-App
- WebSocket-Pflicht
- Kamera- oder UWB-Raumvermessung
- finale Produkt-UI

Polling ist fuer V1 ausreichend, weil das Verhalten wichtiger ist als die perfekte Technik.

## Owner-Test

Der Owner oeffnet:

```text
/surface/handy
/surface/monitor
```

Testfragen:

1. Fuehlt es sich weniger wie Senden und mehr wie Tragen an?
2. Fuehlt sich der Monitor jetzt wie eine echte Ablage an?
3. Ist klar, dass das Ding nicht mehr auf dem Handy liegt, sobald ich es nehme?
4. Ist es gut, dass der Monitor das Ding schon kommen sieht?
5. Fuehlt sich freies Ablegen richtig an?
6. Denke ich noch an Geraete oder schon an Ablagen im Raum?

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | MA006.04 Spatial Room Session dokumentiert. |
