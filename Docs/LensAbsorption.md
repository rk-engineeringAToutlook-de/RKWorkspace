# Lens Absorption

Dokument-ID: RKWS-LENS-ABSORPTION
Version: 1.2.0
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
3. Der Benutzer behaelt die Kontrolle, solange er nicht loslaesst.
4. Erst beim Loslassen wird das Ding vom Zentrum angezogen.
5. Vorderkante bewegt sich staerker zur Linse.
6. Rueckkante folgt verzoegert.
7. Ding wird kleiner und perspektivisch verzerrt.
8. Opacity sinkt erst spaet.
9. Schatten wird in die Linse gezogen.
10. Ding verschwindet in der Tiefe.
11. Target Ghost erscheint.
12. Ghost wird groesser und klarer.
13. Ding liegt auf der Zielablage.

## Rueckholen

Ein Ding, das in der Linse abgelegt wurde, darf nicht endgueltig verschwinden.

Der Benutzer muss es wieder herausnehmen koennen:

1. Hand naehert sich der Linse.
2. Ding loest sich von der Zielablage.
3. Ding wird langsam wieder groesser.
4. Ding wird wieder zum getragenen Gegenstand.
5. Linse beruhigt sich, wenn der Benutzer weggeht.
6. Freies Ablegen legt das Ding wieder auf die aktuelle Ablage.

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
- automatisches Einrasten beim Stillstehen.
- Absorption ohne Loslassen.
- farbige Hilfsflaeche hinter der Blase.
- Einbahnstrasse ohne Rueckholen.
- weisser Innenrahmen, der wie ein UI-Element wirkt.

## Smoke-Kriterien

Der Smoke-Test prueft:

- Absorption startet.
- Ding wird kleiner.
- Ding wird verzerrt.
- Ding verschwindet nicht sofort.
- Ghost erscheint.
- Ghost wird groesser und klarer.
- Timing-Varianten existieren.
- Absorption startet nicht automatisch durch Naehe.
- Die Linse beruhigt sich wieder, wenn der Benutzer weggeht.
- Pull-out aus der Linse ist vorbereitet.
