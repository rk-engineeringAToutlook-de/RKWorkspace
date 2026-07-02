# Workspace Illusion Sprint

Dokument-ID: RKWS-DEV-WORKSPACE-ILLUSION-SPRINT
Version: 1.1.0
Status: Accepted
Datum: 2026-07-02

## Zweck

MA005.03 verbessert nicht den technischen Transfer, sondern die Illusion eines durchgehenden Arbeitsraums. Der Benutzer soll weniger das Gefuehl haben, ein Objekt in einer App zwischen zwei Kaestchen zu bewegen, und staerker das Gefuehl, ein Objekt festzuhalten und durch einen Bildschirmrand auf die naechste Arbeitsflaeche zu schieben.

Der Sprint bleibt lokal im Developer Studio und im Multi-Window-Prototyp. Er fuehrt keine Discovery, kein Pairing, kein LAN, kein Bluetooth, kein UWB, keine Hardware, keine Firmware, keine Cloud, keine echten OS-Hot-Zones und keine echte Monitorerkennung ein.

## Owner-Feedback

Der getestete MA005.02-Stand funktioniert technisch:

- zwei Fenster
- Drag-and-Drop
- Transfer Engine
- Ruecktransfer
- History und Diagnostics
- gruene Tests

Das Bediengefuehl war aber noch zu technisch. Die zentrale Rueckmeldung lautet: Es fuehlt sich noch wie eine App mit zwei Kaestchen an. Das Ziel ist ein Uebergang, der sich mehr wie Greifen, Randuebertritt und Weiterfuehren auf eine andere Arbeitsflaeche anfuehlt.

## Warum Kontinuitaet wichtiger ist

Klassisches Drag-and-Drop trennt den Ablauf oft in drei sichtbare Schritte: ziehen, loslassen, neu anzeigen. RK Workspace soll spaeter anders wirken: Das Objekt soll lebendig bleiben, der Rand soll ein Uebergang sein, und die Zielarbeitsflaeche soll reagieren, bevor der Benutzer loslaesst.

Der Prototyp bildet diese Kontinuitaet nur optisch ab. Der eigentliche Core bleibt plattformneutral und unveraendert.

UX-LAB-001 macht diese optischen Entscheidungen variierbar. Die Illusion aus MA005.03 ist damit nicht mehr eine einzelne feste Loesung, sondern eine von mehreren testbaren Kombinationen.

## Workspace Illusion

Workspace Illusion bedeutet in diesem Sprint:

- Beim Mausklick entsteht ein klarer Greifzustand.
- Das Objekt wird groesser, bekommt Glow/Rahmen und pulsiert leicht.
- Die Randzonen werden sichtbar und pulsieren.
- Die Quelle zeigt, wohin das Objekt geschoben werden kann.
- Das Ziel zeigt vor dem Drop einen Eintrittsbereich.
- Ein Ghost-Objekt liegt halb im Rand und deutet den Uebergang an.
- `WorkspaceSessionCandidate` zeigt den Zustand des vorbereiteten Uebergangs.

## Candidate-Zustaende

`WorkspaceSessionCandidate` ist weiterhin keine Live-Funktion. Fuer MA005.03 werden seine Zustaende sichtbar:

- `None`: Es gibt keinen Randkandidaten.
- `Candidate`: Ein Rand wurde erkannt und ein Ziel ist vorgeschlagen.
- `EdgeLocked`: Die Zielarbeitsflaeche uebernimmt visuell.
- `Completed`: Der Transfer oder die logische Positionsaktualisierung ist abgeschlossen.
- `Cancelled`: Der Greifvorgang wurde ohne gueltiges Ziel beendet.

Diese Zustaende bereiten spaetere Live-Workspace-Sessions vor, ohne sie zu implementieren.

## UX-Diagnostics

Die lokale UX-Diagnose zeigt zusaetzlich:

- Greifzeitpunkt
- Edge-Lock-Zeitpunkt
- aktive Richtung
- Kandidatenstatus
- Uebergangsdauer
- Ruecktransfer-Zaehler
- Fehlversuche

Die Werte dienen der Bewertung des Bediengefuehls und sind keine Telemetrie.

## Smoke-Test

Der Studio-Smoke-Test prueft den Illusionspfad viewmodelbasiert:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartete Zusatzsignale:

```text
IllusionGripState: SUCCESS
IllusionEdgeCandidate: SUCCESS
IllusionEdgeLocked: SUCCESS
IllusionForwardTransfer: SUCCESS
IllusionReturnTransfer: SUCCESS
IllusionCandidateCompleted: SUCCESS
WorkspaceIllusion: SUCCESS
RESULT: SUCCESS
```

## Bekannte Grenzen

- Kein echter Fenster- oder Monitoruebertritt ausserhalb der WinForms-Fenster.
- Kein OS-weites Drag-Ghost-Bild.
- Kein echter gemeinsamer Desktop.
- Keine automatische Fensterpositionierung fuer mehrere Monitore.
- Keine Live-Workspace-Session.
- Keine echte Payload-Uebertragung fuer Datei, PDF, Bild oder Link.
- Ruecktransfer abgeschlossener Demo-Objekte bleibt eine Studio-seitige logische Positionsaktualisierung.

## Naechste Bewertung

Nach MA005.03 bewertet der Owner nicht die Technik, sondern das Gefuehl:

- Fuehlt sich das Greifen klarer an?
- Wirkt der Rand mehr wie ein Uebergang?
- Reagiert die Zielarbeitsflaeche frueh genug?
- Ist das Ziel jederzeit verstaendlich?
- Wirkt der Prototyp weniger wie eine App mit zwei Kaestchen?

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-02 | Bezug zum Workspace Experience Lab ergaenzt. |
| 1.0.0 | 2026-07-02 | MA005.03 Workspace Illusion Sprint dokumentiert. |
