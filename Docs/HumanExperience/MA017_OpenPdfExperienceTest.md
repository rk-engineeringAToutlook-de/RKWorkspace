# MA017 Open PDF Experience Test

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Test prueft die Human Experience fuer eine geoeffnete PDF als Original-Owned Frame.

Der Owner soll nicht glauben, dass eine PDF-Datei verschoben wurde. Er soll erleben, dass eine Arbeitsdarstellung auf einer anderen Ablage erscheint, waehrend das Original owner-owned bleibt.

## HX-Bezug

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.
- HX-001A: Das Objekt antwortet mir.
- HX-002: Ich habe etwas in meiner Hand.

## Sollgefuehl

```text
Ich arbeite dort weiter, aber das Original bleibt hier.
```

## Testablauf

1. Owner oeffnet eine PDF auf Windows.
2. Owner nimmt den PDF-Frame, nicht die Datei.
3. Glass Edge zeigt die naechste Ablage.
4. Frame erscheint auf der Zielablage.
5. Eingaben laufen ueber RKWP zur Owner-Seite zurueck.
6. Guest erhaelt keine Originaldatei.

## Beobachtung

Gruen:

- Owner sagt: Ich arbeite weiter.
- Owner sieht den Frame als Darstellung, nicht als Kopie.
- Owner kann den Rueckweg erklaeren, ohne technische Begriffe zu nutzen.

Gelb:

- Owner erkennt die Darstellung, aber sie wirkt wie Remote Desktop.
- Owner fragt nach Bearbeitungsrechten.
- Owner benoetigt sichtbare Owner/Guest-Signale.

Rot:

- Owner glaubt, dass das Zielgeraet die PDF besitzt.
- Owner verliert die Orientierung, welche Ablage fuehrend ist.
- Owner verwechselt Frame mit Dateikopie.

## Mindestnachweis

```text
OpenPdfFrame: Active
OriginalOwnership: Owner
InputPath: RKWP
GuestIngress: None
RESULT: SUCCESS
```
