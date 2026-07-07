# MA017 iPad Haptic Experience Test

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Test beschreibt die erwartete iPad-Haptik fuer MA017.

Das iPad soll nicht wie eine App antworten. Es soll wie eine Ablage antworten, die ein digitales Ding annimmt, haelt und wieder freigibt.

## HX-Bezug

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001A: Das Objekt antwortet mir.
- HX-002: Ich habe etwas in meiner Hand.

## Haptische Momente

### Ankommen

Subtiles Signal, wenn Kapsel oder Frame auf der iPad-Ablage erscheinen.

### Greifen

Kurze ruhige Bestaetigung, wenn der Owner das Objekt nimmt.

### Ablegen

Sanfter Impuls, wenn das Objekt platziert wird.

### Rueckgabe

Ruhiges Signal, wenn der Frame zur Owner-Ablage zurueckgeht.

## Bewertung

Gruen:

- Haptik bestaetigt Kontrolle.
- Haptik ist ruhig und nicht spielerisch.
- Owner sagt: Die Ablage antwortet.

Gelb:

- Haptik ist richtig gemeint, aber Timing oder Staerke passen noch nicht.
- Haptik wirkt wie UI-Bestaetigung.

Rot:

- Haptik wirkt wie Benachrichtigung.
- Haptik stoert den Arbeitsfluss.
- Haptik fuehlt sich technisch statt raeumlich an.

## Mindestnachweis

```text
iPadHaptics: Mapped
HapticPurpose: HumanExperience
NoFileIngress: Preserved
RESULT: SUCCESS
```
