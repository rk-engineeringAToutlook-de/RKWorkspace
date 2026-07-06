# iOS/iPadOS Gesture Plan

Status: MA011.09 planning baseline  
Datum: 2026-07-06

## Ziel

Die erste iOS/iPadOS Surface App braucht eine einfache, testbare Geste fuer Return und spaeter eine natuerlichere Carry-Geste.

## V1 Gesten

### Return Fallback

```text
lange auf Frame druecken
  -> Return vorbereiten
  -> bestaetigen durch Loslassen oder Button
```

Dieser Fallback ist bewusst schlicht, damit der erste Geraete-Test nicht an Gesten scheitert.

### Frame Focus

```text
einfach antippen
  -> Frame fokussiert
  -> Status sichtbar
```

### Scroll / Zoom

Nur falls Policy erlaubt:

- Scroll fuer Seite.
- Pinch fuer Zoom.

## Drei-Finger-Geste

Die Drei-Finger-Geste bleibt ein spaeteres Experiment, weil iOS/iPadOS eigene Systemgesten besitzt. macOS-Codex soll sie nur pruefen, nicht als harte Voraussetzung bauen.

## Konflikte

Zu pruefen:

- iPadOS Multitasking-Gesten.
- Textauswahl-Gesten.
- Screenshot- und AssistiveTouch-Konflikte.
- Pencil-Eingaben.

## Erfolg

V1 ist erfolgreich, wenn:

- Frame stabil angezeigt wird.
- Return ausloesbar ist.
- keine freie Datei entsteht.
- Geste nicht versehentlich Systemfunktionen ausloest.
