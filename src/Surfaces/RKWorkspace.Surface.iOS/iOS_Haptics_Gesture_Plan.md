# iOS/iPadOS Haptics and Gesture Plan

Status: Prepared handoff  
Datum: 2026-07-06

## Ziel

Haptik und Gesten sollen die Human Experience unterstuetzen: Ding wahrnehmen, nehmen, halten, ablegen und zurueckgeben.

## Gesten

Zu testen:

- TouchHold fuer Ding nehmen.
- LongPress als Fallback.
- Drei-Finger-Langdruck als Hypothese, falls iOS/iPadOS ihn nicht durch Systemgesten blockiert.
- Drag zur Glass Edge.
- Cancel durch Loslassen ausserhalb erlaubter Flaechen.
- Return durch sichtbare, ruhige Aktion.

## Haptik

Nur subtile Rueckmeldung:

- Geste erkannt: kurzer weicher Impuls.
- Ding genommen: ruhige Bestaetigung.
- Frame angekommen: leichter Ankunftsimpuls.
- Glass Edge aktiv: sehr feines Signal.
- Rueckgabe: kurzer Abschluss.
- Denied/Fehler: kurzer, trockener Hinweis.

Keine starke oder spielerische Haptik.

## Owner-Testfragen

- Fuehlt sich das Ding wie Teil meiner Arbeit an?
- Habe ich das Gefuehl, dass es auf mich reagiert?
- Fuehlt sich die Ablage wie mein Arbeitsraum an?
- Denke ich an Dateien oder an ein Ding?

## Blocker

- Drei-Finger-Geste kann mit iOS/iPadOS-Systemgesten kollidieren.
- Haptik muss auf echten Geraeten bewertet werden.
- iPad und iPhone koennen sich deutlich anders anfuehlen.
