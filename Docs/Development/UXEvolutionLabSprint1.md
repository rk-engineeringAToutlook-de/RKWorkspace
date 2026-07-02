# UX Evolution Lab Sprint 1

Dokument-ID: RKWS-DEV-UX-EVOLUTION-LAB-SPRINT-1
Version: 1.0.0
Status: Accepted
Datum: 2026-07-02

## Zweck

Sprint 1 macht das Workspace Experience Lab zur Evolutionsplattform. Ziel ist nicht, sofort die richtige UX zu bauen. Ziel ist, viele nahe Varianten live zu vergleichen, lokal zu bewerten und daraus eine natuerliche Standardinteraktion abzuleiten.

Der Owner entscheidet ausschliesslich nach Gefuehl.

```text
Nicht uebertragen.
Nicht ziehen.
Nehmen. Tragen. Ablegen.
```

## Generationen

Varianten werden als Generationen gefuehrt:

- Greifen: 24 Generationen
- Rand: 24 Generationen
- Uebergang: 24 Generationen
- Ablegen: 20 Generationen
- Vorschau: 8 Generationen

Die Generationen variieren nur Darstellung, Animation und Timing im Developer Studio. Core, Transfer Engine, Agent, IPC, Transport und Discovery bleiben unveraendert.

## Live-Konfiguration

Alle Parameter koennen zur Laufzeit im Tab `Workspace Experience Lab` geaendert werden:

- Greifen
- Rand
- Uebergang
- Ablegen
- Vorschau
- Animation aktiv
- Geschwindigkeit

Offene Multi-Window-Fenster lesen dieselben Lab-Einstellungen und reagieren ohne Neukompilierung.

## Bewertung

Die Bewertung besteht nur aus drei Gefuehlsentscheidungen:

- Gruen: Das fuehlt sich richtig an.
- Gelb: Fast.
- Rot: Fuehlt sich falsch an.

Nach jeder Bewertung waehlt das Lab automatisch eine nahe Folgegeneration. Die Auswahl ist deterministisch und nutzt die vorhandenen Generationen als lokalen Suchraum.

## Statistik

Das Lab speichert lokal:

- getestete Varianten
- gruene Bewertungen
- gelbe Bewertungen
- rote Bewertungen
- Evolutionsschritt
- zuletzt bewertete Kombinationen

Speicherort:

```text
%LOCALAPPDATA%\RKWorkspace\workspace-experience-lab.json
```

Es gibt keine Cloud, keine Telemetrie und keine Netzwerkfunktion.

## Grenzen

- Keine echte Monitorerkennung.
- Kein OS-weites Drag-Ghost-Bild.
- Keine Discovery.
- Keine Aenderung am Core.
- Keine Aenderung an Transfer Engine, Agent, IPC oder Transport.
- Keine finale UX-Entscheidung innerhalb dieses Sprints.

## Smoke-Test

Der Studio-Smoke-Test prueft:

- Variantenzahlen
- Live-Wechsel
- lokale Bewertung
- automatische Folgegeneration
- Anwendung im Multi-Window-Kontext

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Zusatzsignale:

```text
LabGripVariants: 24
LabEdgeVariants: 24
LabTransitionVariants: 24
LabDropVariants: 20
LabPreviewVariants: 8
LabEvolution: SUCCESS
WorkspaceExperienceLab: SUCCESS
RESULT: SUCCESS
```

## Abschluss

Dieser Sprint endet mit einer Owner-Entscheidung: Welche Generation oder Kombination fuehlt sich so natuerlich an, dass der Benutzer vergisst, dass er zwischen zwei Arbeitsflaechen arbeitet?

Diese Entscheidung kann spaeter als neue Standardinteraktion in RK Workspace uebernommen werden.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | UX Evolution Lab Sprint 1 dokumentiert. |
