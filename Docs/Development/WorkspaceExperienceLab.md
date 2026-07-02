# Workspace Experience Lab

Dokument-ID: RKWS-DEV-WORKSPACE-EXPERIENCE-LAB
Version: 1.0.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Das Workspace Experience Lab ist ein internes Experimentierlabor im Developer Studio. Es dient nicht dazu, sofort die finale UX zu bauen, sondern viele Bedienvarianten live miteinander vergleichbar zu machen.

Der Owner entscheidet nach Gefuehl. Nicht die technisch erste Loesung gewinnt, sondern die Variante, die sich am natuerlichsten anfuehlt.

Leitsatz:

```text
Nicht uebertragen.
Nicht ziehen.
Nehmen. Tragen. Ablegen.
```

## Registerkarte

Das Developer Studio enthaelt eine neue Registerkarte:

```text
Workspace Experience Lab
```

Dort koennen Varianten zur Laufzeit umgeschaltet werden. Offene Multi-Window-Arbeitsflaechen lesen dieselben Lab-Einstellungen und reagieren ohne Neukompilieren.

## Varianten

### Greifen

- A - Normales Drag
- B - Objekt schwebt
- C - Objekt wird kleiner
- D - Objekt hebt sich
- E - Objekt bekommt Traegheit
- F - Objekt pulsiert
- G - Eingesammelt
- H - Kombination

### Rand

- A - Glow
- B - Pulsieren
- C - Lauflicht
- D - Oeffnender Rand
- E - Magnetischer Rand
- F - Unsichtbarer Rand
- G - Grosser Zielbereich
- H - Kombination

### Uebergang

- A - Objekt verschwindet
- B - Objekt gleitet
- C - Ghost erscheint
- D - Halb sichtbar
- E - Wird uebernommen
- F - Kontinuierlich
- G - Soft Fade
- H - Kombination

### Ablegen

- A - Normales Drop
- B - Sanftes Aufsetzen
- C - Kleiner Bounce
- D - Magnetisches Einrasten
- E - Leichtes Vergroessern
- F - Glow
- G - Objekt richtet sich aus
- H - Kombination

### Preview

- A - Keine Vorschau
- B - Ghost
- C - Workspace Preview
- D - Miniatur
- E - Nur Richtungspfeil

## Bewertung

Jede Variante kann bewertet werden:

- Gefaellt mir
- Neutral
- Gefaellt mir nicht

Die Bewertung wird lokal gespeichert. Es gibt keine Cloud, keine Telemetrie und keine Netzwerkfunktion.

Speicherort:

```text
%LOCALAPPDATA%\RKWorkspace\workspace-experience-lab.json
```

## Performance und Grenzen

Das Lab veraendert ausschliesslich Darstellung und Timing im Developer Studio. Es veraendert nicht:

- Transfer Engine
- Workspace Registry
- Capability Manager
- Runtime
- IPC
- Transport
- Core-Modelle

Das Lab ist kein Produkt und kein Endanwenderwerkzeug.

## Smoke-Test

Der Studio-Smoke-Test prueft:

- mindestens 8 Greifvarianten
- mindestens 8 Randvarianten
- mindestens 8 Uebergangsvarianten
- mindestens 8 Ablegevarianten
- mindestens 5 Previewvarianten
- Live-Wechsel der Greifvariante
- lokale Bewertung
- Anwendung der Lab-Auswahl im Multi-Window-Kontext

Ausfuehrung:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Zusatzsignale:

```text
LabLiveSwitch: SUCCESS
LabRating: SUCCESS
LabAppliedToMultiWindow: SUCCESS
WorkspaceExperienceLab: SUCCESS
RESULT: SUCCESS
```

## Spaetere Nutzung

Nach manueller Bewertung kann eine Gewinnerkombination in den produktnahen Multi-Window-Prototyp uebernommen werden. Bis dahin bleiben alle Varianten experimentell.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | UX-LAB-001 Workspace Experience Lab dokumentiert. |
