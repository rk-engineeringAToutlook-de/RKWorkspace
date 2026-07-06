# MA016 Recovery Feeling Test

Status: Draft
Datum: 2026-07-07

## Zweck

Recovery wird in MA016 nicht nur technisch getestet. Der Owner muss spuerbar verstehen, dass ein Objekt nicht verloren ist.

## Szenarien

### Abbruch vor dem Oeffnen

Der Ablauf wird gestoppt, bevor die Gastablage den Frame zeigt.

Erwartetes Gefuehl:

- Das Objekt bleibt beim Owner.
- Kein Verlustgefuehl.
- Wiederholen ist plausibel.

### Abbruch waehrend OpenFrame

Die Gastablage verliert die Session, waehrend der Frame offen ist.

Erwartetes Gefuehl:

- Der Owner erkennt den Rueckweg.
- Die Arbeit wirkt nicht zerstoert.
- Die Session wirkt beendet statt kaputt.

### Timeout nach abgelegter Kapsel

Die Kapsel bleibt kurz liegen und wird danach finalisiert.

Erwartetes Gefuehl:

- Der Abschluss wirkt ruhig.
- Das Objekt wirkt abgelegt, nicht verschwunden.
- Die Kante schliesst verstaendlich.

## Bewertung

Gruen:

- Der Owner bleibt ruhig und versteht den Zustand ohne technische Erklaerung.

Gelb:

- Der Zustand ist richtig, aber die Rueckmeldung ist noch zu technisch.

Rot:

- Der Owner glaubt, dass etwas verloren oder doppelt vorhanden ist.

## Protokollfelder

- Szenario
- erster Owner-Satz
- sichtbarer Zustand
- Bewertung
- naechste Recovery-Aenderung

