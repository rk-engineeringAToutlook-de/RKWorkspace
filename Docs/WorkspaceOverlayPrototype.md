# Workspace Overlay Prototype

Dokument-ID: RKWS-WORKSPACE-OVERLAY-PROTOTYPE
Version: 1.0.0
Status: Accepted
Datum: 2026-07-03

## Ziel

MA006.02 fuehrt erstmals eine sichtbare Workspace-Shell-Ebene ueber dem echten Desktop ein.

Der Prototyp ist keine App, kein Developer Studio und keine Panel-Oberflaeche. Er ist eine transparente Ebene ueber dem Desktop, die nur sichtbar wird, wenn ein Mensch ein digitales Ding nimmt, traegt oder ablegt.

## Projekt

Der Windows-spezifische Prototyp liegt in:

```text
src/Shell/RKWorkspace.Shell.Overlay.Windows
```

Die Windows-Abhaengigkeit bleibt dort isoliert. Der neutrale Shell-Core enthaelt nur das plattformneutrale Overlay-State-Modell und das Mapping zu `WorkspaceCarryState`.

## Start

```powershell
.\tools\run-shell.ps1 -OverlayDemo
```

Automatisierter Smoke-Test:

```powershell
.\tools\run-shell.ps1 -OverlaySmokeTest
```

## Kein Normales App-Fenster

Das Overlay ist:

- randlos
- titelbarlos
- transparent ueber dem Desktop
- topmost
- ohne Menues
- ohne Werkzeugleisten
- ohne klassische Controls
- per `Esc` sicher beendbar

Das Ziel ist nicht, eine neue Anwendung zu zeigen. Das Ziel ist, den Desktop kurz so wirken zu lassen, als haette er eine neue Faehigkeit bekommen.

## Sichtbares Demo-Ding

Der Prototyp zeigt genau ein digitales Ding:

```text
Digitales Ding
Rechnung.pdf
```

Die Sprache bleibt bewusst menschlich:

- Ding nehmen
- Weitertragen
- Hier ablegen
- Abgelegt

Nicht verwendet werden:

- Datei uebertragen
- Transfer starten
- Device
- Agent
- Pairing

## Ablagen

Links und rechts am Bildschirmrand erscheinen angedeutete Ablagen:

- Ablage links
- Ablage rechts

Sie sind Orte im Raum, keine Geraete. Es wird kein Laptop, Monitor, Windows-PC oder anderes technisches Objekt dargestellt.

## Digitale Antwort

HX-001A verlangt, dass das Objekt nicht tot wirkt und nicht perfekt am Cursor klebt.

Der Prototyp nutzt deshalb:

- leichtes Nachgeben
- minimale Traegheit
- sanfte Stabilisierung
- kleine Ausrichtung durch Bewegung
- sichtbares Reagieren der Ablage

Das ist noch keine finale Physik. Es ist ein erster Wahrnehmungstest.

## Overlay States

Der neutrale Shell-Core definiert:

- Inactive
- Listening
- CarryCandidate
- Picked
- Carried
- NearAblage
- Placed
- Cancelled
- Failed

Diese States bleiben kompatibel zu `WorkspaceCarryState`.

## Human Experience Referenz

- HX-000: Der Desktop bleibt sichtbar und wirkt als Arbeitsraum.
- HX-001: Das Demo-Ding erscheint als Teil der aktuellen Arbeit.
- HX-001A: Das Ding antwortet beim Greifen und Tragen.
- HX-002: Der Zustand `Picked` bereitet das Gefuehl vor, etwas in der Hand zu haben.

## Sicherheit

Das Overlay darf den Desktop nicht dauerhaft blockieren.

Pflicht fuer diesen Stand:

- `Esc` beendet die Ebene.
- `-OverlaySmokeTest` laeuft ohne manuelle Bedienung.
- Keine haengenden Prozesse.
- Kein Vollbildmodus mit Fensterrahmen.
- Kein Netzwerk, keine Discovery, kein Pairing.

## Bekannte Grenzen

- Nur Windows-Prototyp.
- Keine echten Desktop-Objekte.
- Keine Explorer-, Browser-, Office- oder Mail-Adapter.
- Keine globale Greifgeste.
- Keine echten OS-Hooks.
- Keine echte Monitor- oder Rand-Erkennung.
- Keine Persistenz.
- Kein produktives Overlay-Verhalten.

## Naechster Schritt

Der Owner entscheidet nach dem manuellen Test:

```powershell
.\tools\run-shell.ps1 -OverlayDemo
```

Die einzige Frage lautet:

```text
Fuehlt sich das eher wie mein Desktop mit neuer Faehigkeit an
oder immer noch wie eine App?
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-03 | MA006.02 Workspace Overlay Prototype dokumentiert. |
