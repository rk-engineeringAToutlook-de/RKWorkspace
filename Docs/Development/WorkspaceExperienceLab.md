# Workspace Experience Lab

Dokument-ID: RKWS-DEV-WORKSPACE-EXPERIENCE-LAB
Version: 1.1.0
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

## UX Evolution Lab Sprint 1

Mit Sprint 1 wird das Lab zur Evolutionsplattform. Varianten werden nicht mehr als A/B/C benannt, sondern als Generationen:

```text
Generation 01
Generation 02
Generation 03
...
```

Nach jeder Bewertung waehlt das Lab automatisch eine nahe Folgegeneration. Die Auswahl ist deterministisch und orientiert sich an der besten lokalen Bewertung, nicht an Zufall.

## Varianten

### Greifen

Es existieren 24 Greif-Generationen. Variiert werden unter anderem Groesse, Schatten, Glow, Schweben, Traegheit, Pulsieren, Einrasten, Geschwindigkeit, Transparenz, Rotation und Tiefeneindruck.

### Rand

Es existieren 24 Rand-Generationen. Variiert werden Licht, Magnetismus, Wellen, Einziehen, Oeffnen, Projektion, Bewegung, Pfeile und Farbdynamik.

### Uebergang

Es existieren 24 Uebergangs-Generationen. Variiert werden Gleiten, Magnetzug, Traegheit, Halb-Sichtbarkeit, Ghost, Schatten, Perspektive, Schrumpfen, Ausblenden und Portalwirkung.

### Ablegen

Es existieren 20 Ablege-Generationen. Variiert werden normales Ablegen, sanftes Aufsetzen, Einrasten, Nachfedern, Schweben, Magnetwirkung, Aufsetzen, Zielglow und Ausrichtung.

### Preview

Es existieren 8 Vorschau-Generationen: keine Vorschau, Ghost, Workspace Preview, Miniatur, Richtungspfeil, kompakte Zielvorschau, Richtung plus Zielname und minimaler Randhinweis.

## Bewertung

Jede Variante kann bewertet werden:

- Gruen: Das fuehlt sich richtig an.
- Gelb: Fast.
- Rot: Fuehlt sich falsch an.

Die Bewertung wird lokal gespeichert. Es gibt keine Cloud, keine Telemetrie und keine Netzwerkfunktion.

Das Lab speichert zusaetzlich lokale Statistik:

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

- mindestens 20 Greifvarianten
- mindestens 20 Greifvarianten
- mindestens 20 Randvarianten
- mindestens 20 Uebergangsvarianten
- mindestens 20 Ablegevarianten
- mindestens 5 Previewvarianten
- Live-Wechsel der Greifvariante
- lokale Bewertung
- automatische Folgegeneration
- Anwendung der Lab-Auswahl im Multi-Window-Kontext

Ausfuehrung:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Zusatzsignale:

```text
LabLiveSwitch: SUCCESS
LabRating: SUCCESS
LabEvolution: SUCCESS
LabAppliedToMultiWindow: SUCCESS
WorkspaceExperienceLab: SUCCESS
RESULT: SUCCESS
```

## Spaetere Nutzung

Nach manueller Bewertung kann eine Gewinnerkombination in den produktnahen Multi-Window-Prototyp uebernommen werden. Bis dahin bleiben alle Varianten experimentell.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-02 | UX-EVO Sprint 1 mit Generationen, Evolutionsbewertung und Statistik dokumentiert. |
| 1.0.0 | 2026-07-02 | UX-LAB-001 Workspace Experience Lab dokumentiert. |
