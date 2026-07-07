# MA017 macOS Guest Experience Test

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Test beschreibt die erwartete Human Experience fuer einen macOS Guest im MA017-Pilot.

macOS darf nicht wie ein fremdes Zielgeraet wirken. Es soll wie eine weitere Ablage im Arbeitsraum wirken.

## HX-Bezug

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.
- HX-002: Ich habe etwas in meiner Hand.

## Sollgefuehl

```text
Die Ablage auf dem Mac gehoert zu meinem Arbeitsraum.
```

## Testablauf

1. Windows Owner stellt eine PDF-Kapsel oder einen PDF-Frame bereit.
2. macOS Guest nimmt die RKWP-Kapsel oder den Frame entgegen.
3. Guest zeigt keine Originaldatei im Dateisystem.
4. Owner erkennt, dass die macOS-Ablage nur Gast ist.
5. Rueckweg und Fehlerfall bleiben ruhig erklaerbar.

## Beobachtung

Gruen:

- Owner spricht von Ablage, nicht von Mac.
- Guest wirkt wie Teil desselben Arbeitsraums.
- No File Ingress bleibt glaubwuerdig.

Gelb:

- Owner versteht den Ablauf, sagt aber noch Mac oder Geraet.
- Guest UI wirkt noch zu technisch.
- Rollenmodell benoetigt sichtbare Unterstuetzung.

Rot:

- Owner denkt an AirDrop, Netzlaufwerk oder Dateiuebertragung.
- Owner glaubt, macOS besitze die PDF.
- Guest wirkt wie fremde Anwendung.

## Mindestnachweis

```text
macOSGuest: Ready
GuestRole: FrameOnly
OriginalOwnership: Owner
RESULT: SUCCESS
```
