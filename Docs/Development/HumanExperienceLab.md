# Human Experience Lab

Dokument-ID: RKWS-DEV-HUMAN-EXPERIENCE-LAB
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Zweck

Das Human Experience Lab erweitert das bisherige UX-Labor im Developer Studio. Es testet nicht mehr einzelne Animationen, sondern menschliche Wahrnehmung.

Das Lab ist kein Produkt, kein Endanwenderwerkzeug und keine technische Telemetrie. Es ist ein lokales Validierungslabor fuer den Owner.

## Registerkarte

Das Developer Studio enthaelt die Registerkarte:

```text
Human Experience Lab
```

Die Registerkarte ersetzt die sichtbare `Workspace Experience Lab`-/`Digitale Physik`-Registerkarte als fuehrenden Experimentierort. Die vorhandene Digital-Physics-Logik bleibt als Darstellungsgrundlage erhalten, aber neue sichtbare Experimente werden ueber HX gefuehrt.

## Aktive HX

Oben ist immer sichtbar:

```text
Aktive Human Experience

HX-000
Ich bin in meinem Arbeitsraum.

HX-001
Das gehoert zu meiner Arbeit.

HX-002
Ich habe etwas in meiner Hand.

HX-003
Ich trage etwas.
```

Die aktive HX wird markiert. Jedes Experiment gehoert genau einer HX.

## Experiment-Modus

Experimente heissen nicht Variante A/B, sondern:

```text
Experiment 001
Experiment 002
Experiment 003
...
```

Ein Experiment enthaelt:

- HX-Zuordnung
- Titel
- Ziel
- Evolution-Hinweis
- Quelle, wenn es aus einem frueheren Experiment entstanden ist
- Erstellzeit

## Owner-Bewertung

Das Lab verwendet nur drei Bewertungen:

- Gruen: Das fuehlt sich richtig an.
- Gelb: Fast.
- Rot: Nein.

Nach jeder Bewertung wird ein neues Folgeexperiment erzeugt. Bei Gruen und Gelb entwickelt das Lab das bewertete Experiment weiter. Bei Rot sucht es innerhalb derselben HX nach einem besseren Anker, falls bereits ein gruenes oder gelbes Experiment existiert.

## Beobachtungsprotokoll

Das Beobachtungsprotokoll speichert lokal:

- HX
- Experiment
- Datum
- Bewertung
- Kommentar
- Dauer in Sekunden
- Wiederholungen

Speicherort:

```text
%LOCALAPPDATA%\RKWorkspace\human-experience-lab.json
```

Experimente und Beobachtungen bleiben erhalten. Es gibt keine Loesch- oder Ueberschreiblogik.

## Timeline

Die Timeline zeigt:

```text
HX-000

v

HX-001

v

HX-002

v

HX-003
```

Jede HX erhaelt einen Status:

- offen
- in Pruefung
- bestaetigt

## Dashboard

Das Dashboard zeigt lokal:

- getestete HX
- offene HX
- bestaetigte HX
- verworfene Experimente
- aktuelle Evolution
- Anzahl Protokolle

## Smoke-Test

Der Studio-Smoke-Test prueft das Human Experience Lab viewmodelbasiert:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Zusatzsignale:

```text
HumanExperienceLabStarted: SUCCESS
HumanExperienceLabExperiment: SUCCESS
HumanExperienceLabObservation: SUCCESS
HumanExperienceLabEvolution: SUCCESS
HumanExperienceLabDashboard: SUCCESS
HumanExperienceLab: SUCCESS
RESULT: SUCCESS
```

Der Smoke-Test nutzt einen transienten Lab-State und schreibt keine echten Owner-Bewertungen in die lokale Datei.

## Grenzen

- Keine Aenderung an Core, Runtime, Agent, IPC, Transport oder Discovery.
- Keine Cloud.
- Keine echte externe Studie.
- Keine automatische Entscheidung, welche HX final gewinnt.
- Keine Produkt-GUI.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | HX-LAB-001 Human Experience Validation Lab dokumentiert. |
