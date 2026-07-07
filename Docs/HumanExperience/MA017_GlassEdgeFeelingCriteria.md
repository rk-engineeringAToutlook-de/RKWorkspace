# MA017 Glass Edge Feeling Criteria

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Dokument definiert die Gefuehlskriterien fuer Glass Edge im MA017-Pilot.

Glass Edge ist keine Dekoration. Glass Edge ist die ruhige Anzeige der naechsten Ablage im Arbeitsraum.

## Grundsatz

```text
Eine Kante. Eine naechste Ablage. Kein Geraete-Denken.
```

## Gruen-Kriterien

- Es ist immer nur die naechste Ablage aktiv.
- Die Kante wirkt glaesern, ruhig und raeumlich.
- Das Objekt wird in Richtung Kante gezogen, nicht teleportiert.
- Der Owner denkt an Ablegen, nicht an Senden.
- Die Kante verschwindet, wenn sie nicht gebraucht wird.

## Gelb-Kriterien

- Richtung ist klar, aber der Effekt wirkt noch technisch.
- Glas wirkt gezeichnet statt physisch.
- Aktivierung ist zu frueh, zu spaet oder zu stark.
- Die Kante konkurriert mit dem Objekt.

## Rot-Kriterien

- Mehrere Kanten buhlen um Aufmerksamkeit.
- Kante wirkt wie Button, Portal-Icon oder App-UI.
- Objekt springt statt zu gleiten.
- Owner fragt: Auf welches Geraet uebertrage ich?

## Timing

Glass Edge soll gleiten:

- sanft erscheinen
- bei Naehe praeziser werden
- nach Abbruch ruhig verschwinden
- nach Place kurz halten und dann schliessen

## Mindestnachweis

```text
GlassEdgeMode: NearestOnly
Activation: Smooth
ObjectMotion: PulledIntoEdge
DeviceLanguage: Hidden
RESULT: SUCCESS
```
