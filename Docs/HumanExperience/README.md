# Human Experience

Dokument-ID: RKWS-HX-README
Version: 1.1.0
Status: Accepted
Datum: 2026-07-03

## Zweck

Dieses Verzeichnis sammelt die Human-Experience-Fuehrung fuer RK Workspace.

Ab HX-LAB-001 wird RK Workspace nicht mehr primaer feature-getrieben entwickelt. Jede neue sichtbare Interaktion muss zuerst einer Human Experience zugeordnet werden. Code ist danach nur das Mittel, um diese Wahrnehmung pruefbar zu machen.

## Aktive Human Experiences

Der aktuelle Entwicklungsstand arbeitet mit dieser Timeline:

```text
HX-000
Ich bin in meinem Arbeitsraum.

v

HX-001
Das gehoert zu meiner Arbeit.

v

HX-001A
Das Objekt antwortet mir.

v

HX-002
Ich habe etwas in meiner Hand.

v

HX-003
Ich trage etwas.
```

HX-000, HX-001 und HX-001A liegen als normative Spezifikationen in `Spec/`. HX-002 und HX-003 werden im Lab als fuehrende Wahrnehmungen aus ES-001 und ES-002 gefuehrt, bis eigene HX-Spezifikationen geschrieben werden.

## Validierung

Human Experiences werden nicht mit Prozentwerten oder Sternen bewertet. Der Owner bewertet jedes Experiment nur mit:

- Gruen: Das fuehlt sich richtig an.
- Gelb: Fast.
- Rot: Nein.

Zu jedem Experiment speichert das Studio lokal:

- HX
- Experiment
- Datum
- Bewertung
- Kommentar
- Dauer
- Wiederholungen

Es gibt keine Cloud, keine Telemetrie und keine Netzwerkfunktion.

## Lokaler Speicher

Das Human Experience Lab speichert seine Beobachtungen lokal:

```text
%LOCALAPPDATA%\RKWorkspace\human-experience-lab.json
```

Experimente werden nicht geloescht und nicht ueberschrieben. Neue Bewertungen erzeugen gezielt ein Folgeexperiment aus dem besten passenden Ausgangspunkt.

## Entwicklungsregel

Codex darf kuenftig keine UX-Komponente entwickeln, ohne dass sie genau einer Human Experience zugeordnet ist.

Jede Animation, Bewegung und Interaktion muss beantworten:

```text
Welche HX prueft diese Aenderung?
```

Wenn keine HX unterstuetzt wird, wird nicht implementiert.

## Grenzen

Das Human Experience Lab veraendert nur das Developer Studio. Es veraendert nicht:

- Core
- Runtime
- Transfer Engine
- Agent
- IPC
- Transport
- Discovery

## Verweise

- `Spec/HumanExperienceSpecification_HX000.md`
- `Spec/HumanExperienceSpecification_HX001.md`
- `Spec/HumanExperienceSpecification_HX001A.md`
- `Spec/EmotionSpecification_ES001.md`
- `Spec/EmotionSpecification_ES002.md`
- `Docs/Development/HumanExperienceLab.md`
