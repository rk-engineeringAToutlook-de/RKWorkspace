# Accessibility And Non Intrusive Overlay

Status: Draft  
Datum: 2026-07-06

## Ziel

AP166 definiert, dass das Overlay niemals den Menschen blockieren darf. RK Workspace ist eine Faehigkeit, keine App, die den Bildschirm besitzt.

## Grundregeln

- `Esc` muss aus sichtbaren Overlay-Zustaenden herausfuehren.
- Kein Full-Screen-Trap.
- Keine unendlichen modalen Zustände.
- Keine Pflicht zur Maus.
- Keine verdeckten Systemdialoge.
- Keine erzwungene Animation bei Reduced Motion.

## Screen Reader

Produkt-Overlay und Studio muessen spaeter semantische Statusmeldungen liefern:

- Ding genommen
- naechste Ablage bereit
- Frame liegt hier
- Rueckgabe abgeschlossen
- Handlung nicht erlaubt

Technische Protokolltexte bleiben verborgen.

## Reduced Motion

Wenn Reduced Motion aktiv ist:

- Kante erscheint kuerzer und ruhiger.
- Absorption wird als klare Positionsaenderung statt langer Animation dargestellt.
- Haptik darf helfen, wenn verfuegbar.

## Kontrast

Glas darf nicht bedeuten, dass nichts lesbar ist. Die Kante bleibt peripher, das Ding bleibt kontrollierbar, und Frame-Status muss ohne Farbe allein verstehbar bleiben.

## Keyboard Fallback

Mindestpfad:

- Fokus auf Ding
- Nehmen
- naechste Ablage bestaetigen
- Ablegen
- Zurueckgeben
- Abbrechen

## Nicht-Stoeren

Wenn der Mensch nicht traegt, ist RK Workspace nahezu unsichtbar. Keine permanente Zielwerbung, kein Radar, keine App-Leiste.
