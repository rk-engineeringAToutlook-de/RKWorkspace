# Human Experience Playground HX-P001

Dokument-ID: RKWS-DEV-HUMAN-EXPERIENCE-PLAYGROUND-HXP001
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Mission

HX-P001 besitzt genau ein Ziel:

```text
Der Benutzer soll fuer einen kurzen Moment glauben,
dass sich ein digitales Objekt wirklich in seiner Hand befindet.
```

Wenn dieser Moment nicht entsteht, ist der Sprint nicht erfolgreich.

## Ort im Studio

Das Developer Studio enthaelt eine Registerkarte:

```text
Human Experience Playground
```

Der Playground ist kein Produktmodus. Er ist eine isolierte Wahrnehmungsbuehne fuer den Owner.

## Fuenf Hypothesen

HX-P001 baut keine zwanzig Varianten und keine Evolution. Es testet fuenf radikal unterschiedliche Hypothesen.

### Hypothese A: Das Objekt loest sich

Das Ding loest sich wie ein Blatt Papier an einer Ecke vom Untergrund. Es soll nicht schweben und nicht springen.

### Hypothese B: Die digitale Hand

Eine fast unsichtbare Greifbewegung deutet eine Hand an. Keine Comic-Hand, kein Mauszeiger, keine starke Illustration.

### Hypothese C: Das Objekt verschwindet teilweise

Ein Teil des Dings wird verdeckt, als wuerde es von einer Hand umfasst. Das Gehirn soll den Rest ergaenzen.

### Hypothese D: Das Objekt antwortet

Keine zusaetzlichen Effekte. Nur Verhalten: Ausrichtung, Stabilisierung, minimales Nachgeben und Traegheit.

### Hypothese E: Die Welt reagiert

Nicht das Ding wird lauter. Die Umgebung tritt zurueck, Arbeitsflaechen werden ruhiger und Ablagen wirken empfangsbereit.

## Owner-Test

Der Owner bewertet jede Hypothese nur nach Gefuehl:

- Gruen: Ich glaube fuer einen Moment, dass ich es halte.
- Gelb: Fast.
- Rot: Es bleibt nur Software.

## Human Experience Log

Das Log speichert lokal:

- Variante
- Wahrnehmung
- Owner-Bewertung
- freier Kommentar

Es speichert keine Punkte und keine Scores.

Speicherort:

```text
%LOCALAPPDATA%\RKWorkspace\human-experience-playground.json
```

## Architekturregel

HX-P001 optimiert keine Animationen.

HX-P001 testet Wahrnehmungshypothesen.

Der Sprint veraendert nicht:

- Core
- Runtime
- Agenten
- Discovery
- IPC
- Transport
- Transfer Engine

## Abbruchkriterium

Wenn keine der fuenf Hypothesen den Moment erzeugt, dass das Objekt in der Hand ist, wird nicht weiter optimiert. Dann wird der Ansatz verworfen und ein neuer Ansatz entwickelt.

## Smoke-Test

Der Studio-Smoke-Test prueft:

- fuenf Hypothesen A bis E
- isolierte Hypothesen statt Generationen
- lokales Human-Experience-Log
- keine Scoring-Logik

Ausfuehrung:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Zusatzsignale:

```text
HumanExperiencePlaygroundHypotheses: 5
HumanExperiencePlaygroundIsolation: SUCCESS
HumanExperiencePlaygroundLog: SUCCESS
HumanExperiencePlaygroundNoScoring: SUCCESS
HumanExperiencePlayground: SUCCESS
RESULT: SUCCESS
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | HX-P001 Human Experience Playground fuer den ersten Magic Moment dokumentiert. |
