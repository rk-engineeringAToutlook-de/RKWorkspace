# Workspace Overlay Prototype

Dokument-ID: RKWS-WORKSPACE-OVERLAY-PROTOTYPE
Version: 1.7.0
Status: Accepted
Datum: 2026-07-03

## Ziel

MA006.02 fuehrt erstmals eine sichtbare Workspace-Shell-Ebene ueber dem echten Desktop ein.

Der Prototyp ist keine App, kein Developer Studio und keine Panel-Oberflaeche. Er ist eine transparente Ebene ueber dem Desktop, die nur sichtbar wird, wenn ein Mensch ein digitales Ding nimmt, traegt oder ablegt.

## Einordnung Nach MA006.03

Der Vollbild-Desktop-Overlay-Prototyp bleibt ein technisches Experiment.

Er ist nicht mehr der Hauptpfad fuer das Human-Experience-Ziel. Das Raumgefuehl entsteht voraussichtlich staerker durch ein Geraet in der Hand: Handy oder Tablet als digitales Tablett, Desktop und Monitor als Ablagen im Raum.

MA006.03 verfolgt diesen neuen Testpfad im Dokument `Docs/SpatialCarryTray.md`.

MA006.03-A praezisiert, dass der Desktop-Overlay-Pfad nicht durch grelle Vollbildwelten gewinnen soll. Wenn Overlay-Ideen spaeter wieder aufgenommen werden, muessen sie die echte Desktop-Welt sichtbar lassen und optische Haptik nur als ruhige Kontakt-, Schatten- und Naeheantwort einsetzen.

Die fuer den Spatial Carry Tray erprobten Prinzipien gelten auch fuer spaetere Overlays:

- Teilverdeckung durch digitale Hand statt Comic-Hand.
- Weiches Nachgeben statt hartem Snapping.
- Ablagen offenbaren sich durch Naehe.
- Freies Ablegen ist erlaubt.
- Nur explizites Zuruecklegen kehrt zur alten Ablage zurueck.

MA006.04 fuegt hinzu:

- Jede sichtbare Flaeche ist eine Ablage im selben Raum.
- Ein Ding existiert nur einmal.
- Eine Zielablage sieht ein Ding als Preview, bevor es dort liegt.
- Der Desktop darf nicht nur eine passive Warteseite sein.

Wenn ein Overlay spaeter wieder Produktpfad wird, muss es denselben `SpatialRoomState` respektieren.

MA006.05 fuegt fuer spaetere Overlays eine taktile Regel hinzu:

- ein Ding muss sich aus der Ablage loesen, bevor es getragen wird.
- die Bewegung folgt dem Bewegungsvektor, nicht nur dem Cursor-Ort.
- Teilverdeckung, Kontaktflaeche und Schatten ersetzen fehlende physische Haptik.
- eine Ablage oeffnet sich als Ort.
- das Ding gleitet hinein und springt nicht.

Der aktuelle Overlay-Prototyp wird dadurch nicht erweitert. Die Regeln werden im Spatial Tray erprobt und spaeter auf echte Shell-Overlays uebertragen.

MA006.06 fuegt fuer spaetere Overlays eine weitere Regel hinzu:

- ein Bildschirmrand ist kein Ziel, sondern ein moeglicher Durchgang.
- eine Ablage am Rand darf sich als Portal oeffnen.
- das Ding darf nicht sofort im Ziel erscheinen.
- Quelle und Ziel zeigen eine kontinuierliche Wahrnehmung ueber Source-/Target-Progress.
- ein Ghost im Ziel ist noch kein finales Ablegen.

Der aktuelle Overlay-Prototyp bleibt unveraendert. Wenn Overlays spaeter wieder Produktpfad werden, muessen sie Spatial Portal Carry respektieren und duerfen nicht in klassisches Drag-and-Drop oder technische Drop-Zones zurueckfallen.

MA006.07 fuegt hinzu:

- ein Overlay darf keine Karte im Zentrum zeigen.
- ein Overlay darf keine Warteseite sein.
- Ablagen duerfen nicht dauerhaft als Ziele sichtbar sein.
- Moeglichkeiten erscheinen erst, wenn der Mensch ein Ding traegt.
- die reale oder ruhige Flaeche bleibt wichtiger als eine UI-Erklaerung.

Der lokale Browser-Prototyp bereitet dafuer Standalone-/Fullscreen-Nutzung vor. Das ist nur eine Zwischenloesung. Langfristig muss die Surface ueber der echten Umgebung liegen, nicht als eigene Web-App-Welt daneben.

MA006.08 zieht daraus die Konsequenz:

- der Browser-/Web-Prototyp bleibt technisches Experiment.
- das echte Desktop-Gefuehl benoetigt ein natives Overlay.
- der neue Slice liegt in `src/Shell/RKWorkspace.Shell.NativeOverlay.Windows`.
- der echte Desktop bleibt sichtbar.
- Bubbles sind transparente Linsen statt gruene Punkte.
- die Testgeste ist `Ctrl+Alt+Space`.

Damit wird der alte Overlay-Prototyp nicht geloescht. Er bleibt Referenz fuer transparente Fenstereigenschaften. Der neue Native Spatial Overlay Slice ist aber der aktuelle Human-Experience-Pfad.

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
| 1.7.0 | 2026-07-03 | MA006.08 Native Spatial Overlay Slice als neuen Gefuehlspfad ueber echtem Desktop eingeordnet. |
| 1.6.0 | 2026-07-03 | MA006.07 Surface Overlay Reset als Regel fuer keine Karte, keine Statusseite und Bubbles nur bei aktiver Tragehandlung eingeordnet. |
| 1.5.0 | 2026-07-03 | MA006.06 Spatial Portal Carry als spaetere Overlay-Regel fuer Durchgang, Ghost und No-Jump-Verhalten dokumentiert. |
| 1.4.0 | 2026-07-03 | MA006.05 taktile Carry-Regeln fuer spaetere Overlays eingeordnet. |
| 1.3.0 | 2026-07-03 | MA006.04 Spatial Room Session als Bedingung fuer spaetere Overlays eingeordnet. |
| 1.2.0 | 2026-07-03 | MA006.03-A Prinzipien fuer digitale Hand, optische Haptik und freie Ablage in Overlay-Einordnung aufgenommen. |
| 1.1.0 | 2026-07-03 | MA006.03 eingeordnet: Desktop-Overlay bleibt Experiment, Spatial Carry Tray wird neuer Wahrnehmungstest. |
| 1.0.0 | 2026-07-03 | MA006.02 Workspace Overlay Prototype dokumentiert. |
