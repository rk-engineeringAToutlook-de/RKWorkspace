# Digital Physics Sprint DP-001

Dokument-ID: RKWS-DEV-DIGITAL-PHYSICS-DP-001
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Leitsatz

RK Workspace uebertraegt keine Daten. RK Workspace laesst Menschen digitale Dinge nehmen, tragen und ablegen.

Ab DP-001 wird jede UX-Entscheidung an diesem Leitsatz gemessen:

```text
Pick
Carry
Place
```

## Ziel

DP-001 baut keine neue Infrastruktur, keine Discovery, kein Netzwerk und keine Plattformintegration. Der Sprint veraendert ausschliesslich das Bediengefuehl im Developer Studio.

Der Benutzer soll glauben, dass sich digitale Objekte wie physische Gegenstaende verhalten.

## Greifen

Greifen bedeutet nicht markieren und nicht auswaehlen. Das Objekt loest sich sichtbar von der Arbeitsflaeche, bekommt Tiefe, Gewicht oder Grip und soll sich fuer einen Moment wie "mein Ding" anfuehlen.

Das Workspace Experience Lab enthaelt weiterhin 24 Greif-Generationen.

## Tragen

DP-001 fuehrt eine eigene Kategorie `Tragen` ein. Sie enthaelt 12 Generationen fuer:

- leichte Verzoegerung
- Traegheit
- kleine Feder
- sanfte Bewegung
- spuerbares Gewicht
- ruhige Handfuehrung

Im Single-Window-Prototyp folgt das Objekt dem Cursor nicht mehr perfekt. Es wird mit einer einfachen Feder-/Daempfungsphysik getragen. Beim Ablegen entscheidet weiterhin die aktuelle Handposition, damit das weiche Gefuehl nicht unpraezise wird.

## Durchgang

Der Rand ist kein Ziel. Der Rand ist ein Durchgang. Die sichtbaren Texte sprechen deshalb von Durchgang, weitertragen und annehmen statt von Edge, Drop oder Ziel.

## Kontinuitaet

Das Objekt soll am Rand nicht teleportieren. Im Multi-Window-Prototyp bleibt ein Ghost im Durchgang sichtbar, waehrend die andere Arbeitsflaeche das Objekt annimmt.

## Ablegen

Ablegen soll sich wie Aufsetzen anfuehlen. Die bestehende Ablege-Generation steuert weiterhin sanftes Aufsetzen, Nachfedern, Einrasten, Glow und kleine Impulse.

## Aufmerksamkeit

Beim Tragen werden Arbeitsflaechen ruhiger: weniger Kontrast, ruhigere Farben, weniger visuelle Dominanz. Es gibt keine Unschaerfe. Das Objekt wird Mittelpunkt, nicht der Cursor.

## Sprache

Sichtbare UX-Begriffe im Studio wurden naeher an DP-001 gebracht:

- Ding statt Transferobjekt
- Arbeitsflaeche statt Workspace
- nehmen statt ziehen
- tragen statt draggen
- Durchgang statt Randziel
- ablegen statt drop/transfer

Interne Core-Namen bleiben unveraendert, damit die Architektur stabil bleibt.

## Owner-Test

Nach jeder Variante zaehlen nur drei Fragen:

1. Habe ich das Gefuehl, dass ich etwas wirklich gegriffen habe?
2. Habe ich vergessen, dass ich in einer Test-App arbeite?
3. Hat sich das Objekt angefuehlt, als wuerde ich es tragen?

## Tests

Der Studio-Smoke-Test prueft zusaetzlich:

- 12 Trage-Generationen
- Live-Konfiguration im Lab
- Anwendung der Lab-Auswahl im Multi-Window-Kontext

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Signale:

```text
LabCarryVariants: 12
WorkspaceExperienceLab: SUCCESS
RESULT: SUCCESS
```

## Grenzen

- Keine echte Monitorerkennung.
- Kein OS-weites Drag-Ghost-Bild.
- Keine Discovery.
- Keine Netzwerkfunktion.
- Keine Aenderung an Core, Runtime, Transfer Engine, Agent, IPC oder Transport.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | DP-001 Digital Physics dokumentiert. |
