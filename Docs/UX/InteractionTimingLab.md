# Interaction Timing Lab

Status: Draft  
Datum: 2026-07-06

## Ziel

AP165 haelt fest, dass Timing ein Wahrnehmungsparameter ist. Die richtige Funktion kann falsch wirken, wenn sie zu schnell springt oder zu lange klebt.

## Parameter

`edgeAppearMs`:

- Zeit, bis die gläserne Kante sichtbar wird.

`absorptionMs`:

- Zeit, in der das Ding in die Kante eintritt.

`frameArriveMs`:

- Zeit, bis der Frame auf der Zielablage wahrnehmbar ankommt.

`returnMs`:

- Zeit, bis Rueckgabe als abgeschlossen wahrgenommen wird.

`hapticDelayMs`:

- Verzögerung zwischen sichtbarem Moment und Haptik.

## Erste Zielwerte

```text
edgeAppearMs: 180-320
absorptionMs: 280-520
frameArriveMs: 180-420
returnMs: 220-420
hapticDelayMs: 0-45
```

## Regel

Timing muss gleiten, nicht springen. Die Shell darf nicht den Eindruck erzeugen, dass ein Objekt ohne menschliche Kontrolle einrastet.

## Testfragen

- Fuehlt sich die Kante erwartbar an?
- Hat das Ding noch mir gehoert?
- Kommt der Frame schnell genug an?
- War die Haptik zu spaet?
- War die Bewegung nervoes?

## Optionales Config-Modell

Ein spaeteres `InteractionTimingProfile` soll Timing pro Laborprofil erfassen, ohne Core, RKWP oder Transport zu veraendern.
