# Owner Guest Frame State Machine

Status: Draft  
Datum: 2026-07-06

## Ziel

AP163 definiert die menschliche Sprache fuer Owner-/Guest-Frame-Zustaende. Die State Machine muss fuer den Owner lesbar sein, ohne technische RKWP-Woerter zu zeigen.

## Zustaende

`verfuegbar`:

- Das Original liegt beim Owner.

`genommen`:

- Der Mensch haelt das Ding.

`ausgeliehen`:

- Das Original ist gesperrt, weil ein Frame auf einer anderen Ablage entsteht.

`liegt im Frame`:

- Die Gastablage zeigt das Ding als Frame.

`zurueckgegeben`:

- Der Frame ist beendet und das Original ist wieder frei.

`Verbindung verloren`:

- Der Frame ist nicht mehr verlaesslich erreichbar.

`recovered`:

- Der Owner hat das Ding sicher wiederhergestellt.

`denied`:

- Die Handlung war nicht erlaubt.

## Sichtbare Sprache

Erlaubt:

- liegt hier im Frame
- wartet auf Rueckgabe
- wieder verfuegbar
- nicht verfuegbar
- Verbindung verloren
- wiederhergestellt

Vermeiden:

- Lease expired
- session revoked
- protocol denied
- transfer failed
- remote peer

## Owner/Guest-Regel

Owner sieht Besitz und Rueckgabe. Guest sieht nur, ob der Frame hier nutzbar ist.

Der Guest darf nie den Eindruck bekommen, dass er eine freie Originaldatei bekommen hat.
