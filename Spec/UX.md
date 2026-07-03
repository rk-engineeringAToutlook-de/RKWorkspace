# RKWS UX Specification

Dokument-ID: RKWS-SPEC-UX-001  
Version: 0.3.0
Status: Accepted  
Datum: 2026-07-02

## Principle

Die normale Benutzerinteraktion soll gestenbasiert und unaufdringlich sein. Die GUI ist fuer Einrichtung, Pairing, Diagnose, Logs, Firmware, Hardware und Tests vorgesehen.

## Emotion Specifications

Emotion Specifications definieren die Zielgefuehle von RK Workspace. Sie sind fuehrende UX-Dokumente und duerfen nicht direkt als finale Loesung interpretiert werden.

ES-001 definiert das erste zentrale Gefuehl:

```text
Ich habe etwas in meiner Hand.
```

Aus ES-001 folgen Experimente, keine direkten Produktentscheidungen. Codex erzeugt Varianten; der Owner bewertet das Gefuehl.

## Directional Intent

Eine Geste wird im Core als Richtung modelliert. V1 nutzt explizite Richtungen wie `Right`, um die Logik testbar zu machen.

## Feedback

Spaetere Agenten muessen Aktivierung, Zielrand, moegliche Ziele, Ablehnung und erfolgreichen Abschluss sichtbar oder haptisch rueckmelden.

## Diagramm

```mermaid
flowchart LR
    Select["Objekt markieren"] --> Gesture["Geste"]
    Gesture --> Feedback["Feedback"]
    Feedback --> Target["Ziel anzeigen"]
    Target --> Release["Loslassen"]
    Release --> Transfer["Transfer"]
```

## Querverweise

- `Spec/EmotionSpecification_ES001.md`
- `Spec/GestureModel.md`
- `Spec/StateMachine.md`
- `Docs/05_UX_Gestures.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 0.3.0 | 2026-07-03 | Emotion Specification ES-001 als fuehrendes UX-Dokument referenziert. |
| 0.2.0 | 2026-07-02 | Dokumentstandard und Gestenverweise ergaenzt. |
| 0.1.0 | 2026-07-02 | UX-Spezifikation angelegt. |
