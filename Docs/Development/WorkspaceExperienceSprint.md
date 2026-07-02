# Workspace Experience Sprint

Dokument-ID: RKWS-DEV-WORKSPACE-EXPERIENCE-SPRINT
Version: 1.0.0
Status: Accepted
Datum: 2026-07-02

## Zweck

MA005.02 verschiebt den Fokus vom technischen Nachweis zur Bedienerfahrung. Der Benutzer soll im Multi-Window-Prototyp erstmals das Gefuehl bekommen, ein Objekt zwischen Arbeitsflaechen zu verschieben, nicht eine Datei an ein Geraet zu senden.

Der Sprint bleibt ein Developer-Studio-Prototyp. Er fuehrt keine Discovery, keine Netzwerkfunktion, keine Hardware, keine Firmware, kein Pairing und keine Cloud ein.

## UX-Ziele

- Arbeitsflaechen sollen als erkennbare Workspaces wirken.
- Transferobjekte sollen wie greifbare Karten mit Inhaltshinweis wirken.
- Drag-and-Drop soll unmittelbarer und verstaendlicher werden.
- Der Fensterrand soll die spaetere Monitor- und Raumlogik vorbereiten.
- Erfolg und Fehler sollen ohne Blick in den Code sichtbar werden.
- Ruecktransfer von Workspace B nach Workspace A soll moeglich sein.

## Designentscheidungen

Die Workspace-Fenster erhalten eine einfache Monitor-Optik mit Rahmen, Titel, Typ, Position und Status. Das ist noch kein finales Produktdesign, gibt aber sofort Orientierung.

Transferobjekte werden als Karten dargestellt. Jede Karte zeigt Symbol, Titel, Vorschau und Status. Die Demo-Daten enthalten Text, PDF, Bild und Link. Es werden weiterhin keine echten Payloads uebertragen.

Beim Drag wird die aktive Karte visuell hervorgehoben. Der Kontext schreibt `Objekt wird gezogen`, misst die Drag-Dauer und legt intern einen `WorkspaceSessionCandidate` an. Dieser Kandidat ist nur vorbereitet und besitzt noch keine Live-Funktion.

Die Randlogik bleibt bewusst GUI-intern:

- rechter Fensterrand schlaegt Workspace B vor
- linker Fensterrand schlaegt Workspace A vor
- keine echte Monitorerkennung
- keine OS-weiten Hot-Zones

Bei einem Randvorschlag zeigt die Workspace Preview Zielname, Objektanzahl und Status. Beim Drop wird die Zielarbeitsflaeche markiert, eine kurze Transferanimation abgespielt und der Zielworkspace leuchtet kurz auf.

## Bedienkonzept

1. Developer Studio starten.
2. `Open Multi Window Prototype` oeffnen.
3. Objektkarte in Workspace A greifen.
4. An den rechten Fensterrand ziehen oder direkt ueber Workspace B loslassen.
5. Workspace B zeigt Zielhinweis, Preview und Highlight.
6. Nach Drop erscheint das Objekt in Workspace B.
7. Dasselbe Objekt kann wieder in Workspace B gegriffen und nach Workspace A zurueckgezogen werden.

Ungueltige Drops brechen ohne Absturz ab. Das Objekt bleibt an seiner Ausgangsarbeitsflaeche, das Log meldet `Kein gueltiges Ziel`.

## UX-Diagnose

Der Multi-Window-Prototyp zeigt eine lokale UX-Diagnose mit:

- Drag Start
- Ziel erkannt
- Drop
- Drag-Dauer
- Transferzeit
- Erfolgsquote
- erfolgreicher Transferanzahl
- Fehleranzahl
- aktuellem `WorkspaceSessionCandidate`

Diese Werte sind keine Telemetrie. Sie dienen nur der lokalen Entwicklung und der manuellen Bewertung des Bediengefuehls.

## Live-Workspace-Vorbereitung

`WorkspaceSessionCandidate` ist als internes Konzept angelegt. Es beschreibt, welches Objekt von welcher Arbeitsflaeche zu welcher Zielarbeitsflaeche bewegt werden soll und welche Richtung daraus folgt.

Noch nicht implementiert:

- Live Workspace
- Live Window
- Workspace Node
- bidirektionale Synchronisation
- echte Session-Aushandlung

Der Kandidat schafft nur einen sauberen Anknuepfungspunkt fuer spaetere Live-Workspace-Sessions.

## Smoke-Test

Der Studio-Smoke-Test prueft jetzt zusaetzlich:

- Multi-Window-Kontext startet.
- `Run Full Demo` transferiert ein Objekt nach Workspace B.
- EdgeTarget-Logik liefert links Workspace A, rechts Workspace B und in der Mitte kein Ziel.
- Roundtrip A -> B -> A funktioniert.
- Nach dem Roundtrip liegen wieder alle fuenf Objekte in Workspace A.
- UX-Diagnose meldet Drag Starts, Zielerkennung, Drops und 100 Prozent Erfolgsquote.
- `WorkspaceSessionCandidate` ist vorbereitet, aber nicht live.

Ausfuehrung:

```powershell
.\tools\run-studio.ps1 -SmokeTest
```

Erwartet:

```text
RoundTrip: SUCCESS
UxDiagnostics: SUCCESS
WorkspaceSessionCandidate: READY
RESULT: SUCCESS
```

## Bekannte Einschraenkungen

- Noch keine echte Monitorerkennung.
- Noch keine Betriebssystem-Randbindung ausserhalb der beiden Fenster.
- Keine transparente Drag-Ghost-Darstellung ueber Fenstergrenzen hinweg.
- Keine persistierten Workspace-Layouts.
- Keine echten Dateien, Bilder, PDFs oder Links als Payload.
- Ruecktransfer nach Abschluss eines Core-Transfers wird im Studio als logische Positionsaktualisierung dargestellt, ohne den Core-Terminalzustand aufzubrechen.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-02 | MA005.02 Workspace Experience Sprint dokumentiert. |
