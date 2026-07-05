# RK Workspace Gesture Strategy

Dokument-ID: RKWS-GESTURE-STRATEGY-001
Status: Draft
Datum: 2026-07-05

## Ziel

Gesten muessen plattformuebergreifend als menschliche Absicht verstanden werden. Die konkrete OS-Geste ist nur ein Adapterdetail.

## Neutrale Gesten

- ThreeFingerHold
- LongPress
- MouseLongPress
- KeyboardActivation
- TouchHold
- PenHold

## Aktive Regel

Eine Geste darf erst dann RKWP ausloesen, wenn eine Human Experience unterstuetzt wird:

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.
- HX-001A: Das Objekt antwortet mir.
- HX-002: Ich habe etwas in meiner Hand.

## Plattformhinweise

Windows kann mit Maus, Tastatur, Pen und spaeter globalen Hooks arbeiten. iOS/iPadOS und Android starten mit TouchHold/LongPress in der Surface. macOS startet mit Trackpad/Maus-Gesten. Linux startet minimal mit MouseLongPress und KeyboardActivation.

## Nicht-Ziel

Die Strategie ist kein globaler Hook-Plan. Globale OS-Integration folgt erst nach stabiler Semantik.
