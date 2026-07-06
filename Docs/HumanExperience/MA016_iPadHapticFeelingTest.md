# MA016 iPad Haptic Feeling Test

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Test beschreibt, wie iPad-Haptik spaeter die Human Experience unterstuetzen soll. Er ist ein Testplan fuer die native iPad-Umsetzung, keine Windows-Implementierung.

## Zielgefuehl

```text
Das iPad ist keine App. Es ist eine Ablage, die antwortet.
```

## Haptische Momente

### Objekt erscheint

Subtiles Signal, wenn ein Frame oder eine Kapsel auf der iPad-Ablage verfuegbar wird.

Bewertung:

- Gruen: fuehlt sich wie Ankommen an.
- Gelb: wahrnehmbar, aber noch technisch.
- Rot: erschreckt oder lenkt ab.

### Objekt wird genommen

Kurzes, leichtes Feedback bei Pick.

Bewertung:

- Gruen: bestaetigt Kontrolle.
- Gelb: Timing noch falsch.
- Rot: fuehlt sich wie Button an.

### Objekt wird platziert

Ruhiger Impuls bei Place.

Bewertung:

- Gruen: fuehlt sich wie Ablegen an.
- Gelb: Feedback zu schwach oder zu spaet.
- Rot: wirkt wie Benachrichtigung.

## Grenzen

Keine Haptik darf eine technische Meldung ersetzen. Haptik bestaetigt nur Wahrnehmung.

## Native Umsetzung

Die konkrete iOS/iPadOS-API-Entscheidung gehoert in den nativen Surface-Task. Dieses Dokument definiert nur das erwartete Gefuehl.

