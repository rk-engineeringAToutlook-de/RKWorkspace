# Lens Absorption

Dokument-ID: RKWS-LENS-ABSORPTION
Version: 1.0.0
Status: Accepted
Datum: 2026-07-04

## Wichtigster Satz

Das digitale Ding wird nicht uebertragen.

Die Linse nimmt es auf.

## Ziel

Lens Absorption beschreibt den sichtbaren Moment, in dem ein digitales Ding in eine geoeffnete Ablage-Linse hineingeht, in der Tiefe verschwindet und auf der Zielablage wieder als Ghost herauskommt.

Der Benutzer soll denken:

```text
Das Ding geht dort hinein.
```

Nicht:

```text
Das Ding wurde gesendet.
```

## Ablauf

1. Ding naehert sich der geoeffneten Linse.
2. Linse reagiert und vertieft sich.
3. Ding wird vom Zentrum angezogen.
4. Vorderkante bewegt sich staerker zur Linse.
5. Rueckkante folgt verzoegert.
6. Ding wird kleiner und perspektivisch verzerrt.
7. Opacity sinkt erst spaet.
8. Schatten wird in die Linse gezogen.
9. Ding verschwindet in der Tiefe.
10. Target Ghost erscheint.
11. Ghost wird groesser und klarer.
12. Ding liegt auf der Zielablage.

## Timing

MA006.10R prueft:

- 600 ms.
- 1200 ms.
- 1800 ms.

Taste:

```text
T = Timing wechseln
A = Absorption erneut abspielen
```

## Verboten

- Sofort weg.
- harter Fade.
- Teleport.
- Beamen.
- lineares Drag-and-drop.
- ploetzliches Empfangen.
- Sprung auf Ziel.

## Smoke-Kriterien

Der Smoke-Test prueft:

- Absorption startet.
- Ding wird kleiner.
- Ding wird verzerrt.
- Ding verschwindet nicht sofort.
- Ghost erscheint.
- Ghost wird groesser und klarer.
- Timing-Varianten existieren.

