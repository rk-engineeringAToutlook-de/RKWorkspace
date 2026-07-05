# Spatial Room Session

Dokument-ID: RKWS-SPATIAL-ROOM-SESSION
Version: 1.8.0
Status: Accepted
Datum: 2026-07-04

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
- `Version`
- `Ablagen`
- `Things`
- `ActiveCarry`
- `ActivePortalTransition`
- `UpdatedAt`

Alle Oberflaechen lesen denselben Raumzustand.

## Gleichberechtigte Ablagen

MA006.04 fuehrt Oberflaechen ein:

```text
/surface/handy
/surface/monitor
/surface/tablet
/surface/desktop
/surface/beamer
```

Jede Surface ist eine Ablage im Raum.

Handy, Tablet, Monitor, Desktop und Beamer sind in der Wahrnehmung keine Geraete. Sie sind Orte, auf denen ein Ding liegen, ankommen oder genommen werden kann.

Ab MA006.13 werden diese Orte nicht mehr als mehrere Bubbles gleichzeitig angeboten. Der neue Hauptpfad zeigt genau eine naechste Ablage als gläserne Kante. Die Spatial Room Session bleibt das gemeinsame Raummodell; die Kante ist nur die ruhigere Darstellung der naechsten Moeglichkeit.

Ab dem Tablet-Testpfad ist `/surface/tablet` die Default-Ablage. Das erste digitale Ding liegt initial auf dem Tablet, damit der Owner den mobilen Gefuehlstest auf der groesseren Handflaeche fortsetzen kann. Handy bleibt eine normale Ablage im selben Raum.

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
- `Position`
- `Metadata`

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

`Metadata` beschreibt die menschliche Rolle des Dings, nicht technische Nutzdaten. Im Prototyp ist `Rechnung.pdf` ein Arbeitsding, das HX-001 unterstuetzt: `Das gehoert zu meiner Arbeit`.

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
- OpeningAblage
- PreviewingOnAblage
- Placed
- Cancelled

`OpeningAblage` beschreibt den Moment, in dem eine Ablage auf Naehe antwortet. Die Ablage wird nicht nur markiert; sie wird praesenter, lesbarer und zeigt eine innere Ablage-Linse.

## Ablage-Linse

Wenn der Mensch sich einer Ablage naehert, entsteht keine technische Zielmarkierung.

Stattdessen oeffnet sich die Ablage ruhig:

- Bubble wird praesent.
- Name wird lesbar.
- eine innere Ablage-Flaeche wird sichtbar.
- `Ablage oeffnet sich` beschreibt den Zustand.
- `Hier ablegen` erscheint erst bei aktiver Naehe.

Die Distanzsprache ist verbindlich:

- Far: Name nicht lesbar.
- Medium: Mikrotext.
- Near: Name lesbar.
- VeryNear: Name und `Hier ablegen`.

Die Ablage ist damit keine Schaltflaeche, keine Box und kein technisches Ziel. Sie ist eine Moeglichkeit im Raum.

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

Die Preview ist eine Ghost-Karte. Sie zeigt nicht, dass etwas uebertragen wurde, sondern nur:

```text
Etwas kommt in meinen Raum.
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

Ab Version 1.1 oeffnen sich aktive Bubbles als Ablage-Linse. Damit wird Naehe nicht nur ueber Farbe oder Groesse gezeigt, sondern ueber Verhalten.

## Spatial Handover

Spatial Handover ist nur dokumentiert, noch nicht implementiert.

Gemeint ist der spaetere Moment, in dem ein Ding von einer Ablage in eine andere Raumzone weitergereicht wird, ohne wie ein Versand, Upload oder Synchronisationsvorgang zu wirken.

MA006.04 legt dafuer nur die Begriffe und Zustandsgrenzen an:

- ein gemeinsamer Raumzustand.
- ein Ding existiert genau einmal.
- eine aktive Carry Session.
- eine oeffnende Ablage.
- eine Preview, bevor abgelegt wird.

Es gibt noch kein echtes Handover-Protokoll, keine Discovery, kein Pairing und keine Payload.

## Tactile Mobile Carry Slice

MA006.05 reduziert den Prototyp bewusst auf einen einzigen Ablauf:

```text
Ablage Tablet
Ding nehmen
Ding in digitaler Hand halten
Ablage Monitor oeffnet sich
Ding gleitet hinein
Ding liegt auf Ablage Monitor
```

Der Slice fuegt keine neue Infrastruktur hinzu. Er verfeinert nur die Wahrnehmung:

- kurzer Widerstand beim ersten Ziehen.
- sichtbares Loesen aus der Ablage.
- kompakteres Ding waehrend des Haltens.
- Teilverdeckung durch digitale Hand.
- optische Haptik ueber Kontaktflaeche, Griffschatten und Tiefe.
- optionale mobile Haptik ueber `navigator.vibrate`, wenn verfuegbar.
- Neigung nach Bewegungsvektor, nicht nach absoluter Bildschirmposition.
- fast kein Wabern.
- weiches Einrasten als Einladung, nicht als aggressive Magnetik.
- Ghost auf der Zielablage vor dem eigentlichen Ablegen.
- gleitender Uebergang in die Ablage-Bubble.
- relative Zielposition auf der Ablage wird gespeichert.

Die Surface-State-Daten melden dafuer:

- `motion.tiltSource = movement-vector`
- `motion.heldCompactScale`
- `motion.initialResistanceDistancePx`
- `motion.glideIntoBubbleMs`
- `haptics.partialOcclusion`
- `haptics.contactShadow`
- `transition.targetGhostBeforePlace`
- `transition.glideIntoBubble`
- `transition.targetPositioning = relative-on-ablage`

Damit bleibt die Regel erhalten:

```text
Release = hier ablegen
Cancel = zurueck zur Quelle
```

Freies Ablegen bleibt gueltig.

## Spatial Portal Carry

MA006.06 macht aus der oeffnenden Ablage erstmals einen ruhigen Durchgang.

Nicht:

```text
Ding springt auf den Monitor.
```

Sondern:

```text
Ablage Handy
Ding nehmen
digitale Hand
Ablage Monitor oeffnet sich als Portal
Ding gleitet in das Portal
Ding erscheint auf Monitor
Ding wird dort oertlich abgelegt
```

Das Portal ist keine technische Verbindung. Es ist eine Wahrnehmungsbruecke zwischen zwei Ablagen desselben Raums. Es gibt weiterhin kein echtes Handover-Protokoll, keine Discovery, kein Pairing und keine Payload.

### Portalphasen

Eine Ablage-Bubble kann ab MA006.06 folgende Portalphasen melden:

- `DistantBubble`
- `ApproachingBubble`
- `ReadableBubble`
- `OpeningPortal`
- `PortalOpen`
- `ObjectEntering`
- `ObjectEmerging`
- `Placed`

Diese Phasen beschreiben, wie die Ablage fuer den Menschen sichtbar wird. Sie beschreiben nicht, ob Daten kopiert wurden.

### SpatialPortalTransition

Eine laufende Portalbewegung wird durch `SpatialPortalTransition` beschrieben:

- `TransitionId`
- `ThingId`
- `SourceAblageId`
- `TargetAblageId`
- `State`
- `Progress`
- `StartedAt`
- `UpdatedAt`
- `SourceVisualProgress`
- `TargetVisualProgress`

Zustaende:

- `None`
- `Entering`
- `InBetween`
- `Emerging`
- `ReadyToPlace`
- `Placed`
- `Cancelled`

`Progress` beschreibt den wahrgenommenen Weg durch den Raum:

- `0.0`: Ding ist noch sichtbar auf der Quelle.
- `0.5`: Ding befindet sich im Zwischenraum.
- `1.0`: Ding ist final auf der Zielablage abgelegt.

Vor dem finalen Ablegen bleibt das Ding im Zustand `PreviewOnAblage`. Die Zielablage zeigt einen Ghost, aber `CurrentAblageId` bleibt leer. Dadurch ist abgesichert:

```text
Es gibt keinen Sofortsprung.
```

`SourceVisualProgress` steuert, wie stark das Ding auf der Quelle optisch in das Portal eintritt. `TargetVisualProgress` steuert, wie klar und gross es auf der Zielablage als Ghost erscheint. Erst `Place` setzt das Ding final auf die Zielablage und speichert die dortige Position.

### Rand Des Raums

Die Zielablage wird im Prototyp bewusst am Rand des wahrgenommenen Raums positioniert. Das ist noch keine echte Monitor- oder Raumerkennung. Es ist eine kapselbare Wahrnehmungslogik:

- Monitor rechts.
- Handy links.
- Tablet oben rechts.
- Tisch links.
- Wand oben.

Spaeter kann diese Logik durch echte Raumdaten ersetzt werden, ohne den Human-Experience-Ablauf zu aendern.

## Surface Overlay Reset

MA006.07 verwirft die MA006.06-Radar-/Statusdarstellung als Produktpfad.

Die Logik bleibt:

- Spatial Room Session.
- Spatial Portal Carry.
- Portalphasen.
- Source-/Target-Progress.
- Ghost vor Place.
- gespeicherte Zielposition.

Die sichtbare Grunddarstellung wird zurueckgesetzt:

- keine zentrale Statuskarte.
- keine zentrale Raumkarte.
- keine permanenten Bubbles.
- keine sichtbaren technischen Aktionsbuttons.
- Empty bedeutet: die Surface wirkt leer, nicht wartend.
- Bubbles erscheinen erst bei aktiver Tragehandlung.
- Bubbles liegen peripher am Rand der aktuellen Surface.

Der Raum wird dadurch nicht mehr als Karte gezeigt. Er zeigt sich nur noch als Moeglichkeit, wenn der Mensch ein Ding traegt.

## Native Spatial Overlay Slice

MA006.08 nutzt die in der Spatial Room Session erarbeiteten Regeln, verlaesst aber den Browser als primaeren Gefuehlspfad.

Der Web-Prototyp bleibt nuetzlich fuer:

- Raumzustand.
- Portalphasen.
- Source-/Target-Progress.
- Smoke-Tests.

Das gewuenschte Gefuehl benoetigt jedoch:

- echten Desktop im Hintergrund.
- transparentes natives Overlay.
- keine Browserleiste.
- keine Web-App-Struktur.
- Bubbles nur bei aktivem Carry.

Der native Slice implementiert noch keinen echten Mehrgeraete-Raumzustand. Er uebernimmt die Wahrnehmungsregeln als lokalen Windows-Slice.

## Visual Reality Lab

MA006.09 trennt technische Raumtests und Human-Experience-Tests noch schaerfer.

Die Spatial Room Session bleibt der richtige Ort fuer:

- Raumzustand.
- Ablagen.
- Things.
- Portalphasen.
- Source-/Target-Progress.
- Smoke-Tests.

Sie ist aber kein gueltiger Gefuehlspfad mehr, wenn sie im Browser betrachtet wird.

Fuer die Frage, ob eine Ablage-Linse echt, lebendig und raeumlich wirkt, gilt ab MA006.09:

```text
Visual Reality Lab
```

Das Lab prueft native lebendige Linsen ueber dem echten Desktop. Die Web-Surfaces bleiben technische Hilfsmittel.

Ab MA006.10R wird der sichtbare Uebergang als `Lens Absorption` geschaerft. Die Spatial Room Session beschreibt weiterhin Raumzustand, Portalphasen und Source-/Target-Progress. Der Living-Lens-Slice prueft dazu isoliert, wie das Ding visuell in eine Linse hineingezogen, verzerrt, verkleinert und als Target Ghost wieder sichtbar wird.

```text
Spatial Room Session = Raumzustand.
Living Lens = sichtbare Material- und Absorptionswahrnehmung.
```

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

- `/tray` -> `/surface/tablet`
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
/surface/tablet
```

Testfragen:

1. Fuehlt es sich weniger wie Senden und mehr wie Tragen an?
2. Fuehlt sich der Monitor jetzt wie eine echte Ablage an?
3. Ist klar, dass das Ding nicht mehr auf dem Tablet liegt, sobald ich es nehme?
4. Ist es gut, dass der Monitor das Ding schon kommen sieht?
5. Fuehlt sich freies Ablegen richtig an?
6. Denke ich noch an Geraete oder schon an Ablagen im Raum?

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.7.0 | 2026-07-04 | MA006.10R Lens Absorption als sichtbaren Materialpfad zur Spatial Room Session eingeordnet. |
| 1.6.0 | 2026-07-04 | MA006.09 Visual Reality Lab als nativen HX-Testpfad eingeordnet; Browser-Surfaces bleiben technische Raumtests. |
| 1.5.1 | 2026-07-04 | Tablet als Default-Ablage fuer den mobilen Gefuehlstest dokumentiert; Handy bleibt weitere Ablage. |
| 1.5.0 | 2026-07-03 | MA006.08 Native Spatial Overlay Slice als nativen Gefuehlspfad auf Basis der Spatial-Room-Regeln eingeordnet. |
| 1.4.0 | 2026-07-03 | MA006.07 Surface Overlay Reset mit Empty-ohne-Bubbles, peripheren Moeglichkeiten und verworfener Radar-/Statusdarstellung dokumentiert. |
| 1.3.0 | 2026-07-03 | MA006.06 Spatial Portal Carry mit Portalphasen, SpatialPortalTransition, Source-/Target-Progress und No-Jump-Regel dokumentiert. |
| 1.2.0 | 2026-07-03 | MA006.05 Tactile Mobile Carry Slice mit digitaler Hand, Vektor-Neigung, Ghost, Glide und Zielposition dokumentiert. |
| 1.1.0 | 2026-07-03 | Ablage-Linse, OpeningAblage, Metadata, Version und Spatial Handover als dokumentierten Zukunftsschritt ergaenzt. |
| 1.0.0 | 2026-07-03 | MA006.04 Spatial Room Session dokumentiert. |
