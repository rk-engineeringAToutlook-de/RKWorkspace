# Workspace Shell

Dokument-ID: RKWS-WORKSPACE-SHELL
Version: 1.5.0
Status: Accepted
Datum: 2026-07-03

## Wichtigster Satz

Workspace Shell ist keine Anwendung.

Workspace Shell ist der digitale Raum, in dem sich der Mensch bewegt.

## Zweck

Workspace Shell ist die kuenftige unsichtbare Produktebene von RK Workspace. Sie wird spaeter staendig vorhanden sein und nur dann in Erscheinung treten, wenn Human Experience entsteht:

- greifen
- tragen
- ablegen

Der Benutzer startet Workspace Shell spaeter nicht. Workspace Shell ist einfach vorhanden.

## Abgrenzung zum Developer Studio

Developer Studio bleibt erhalten, aber nur als:

- Entwicklungswerkzeug
- Diagnosewerkzeug
- Human Experience Playground

Developer Studio ist nicht das Produkt.

## Architektur-Fundament

Die Foundation liegt in:

```text
src/Shell/RKWorkspace.Shell
```

Aktuelle Architekturdateien:

- `WorkspaceShell.cs`
- `WorkspaceShellState.cs`
- `WorkspaceShellConfiguration.cs`
- `WorkspaceShellRuntime.cs`
- `IWorkspaceShellRuntime.cs`
- `WorkspaceShellRuntimeState.cs`
- `WorkspaceShellDiagnostics.cs`
- `WorkspaceShellRuntimeException.cs`
- `WorkspaceOverlayState.cs`
- `WorkspaceOverlayStateMapper.cs`
- `WorkspaceOverlay.cs`
- `WorkspaceOverlayManager.cs`
- `WorkspaceSession.cs`
- `WorkspaceCarryState.cs`
- `WorkspaceObject.cs`

Die Foundation enthaelt nur neutrale Architekturmodelle. Sie enthaelt keine Betriebssystemintegration, keine Fensterlogik, keine Netzwerkfunktion, keine Discovery, kein Pairing und keine Transportlogik.

## Runtime Host

MA006.01 fuehrt erstmals einen eigenen Shell-Prozess ein:

```text
src/Shell/RKWorkspace.Shell.Host
```

Der Host ist noch keine App und zeigt kein Hauptfenster. Er laeuft in V1 ausschliesslich als Konsolen-/Statusmodus, damit die unsichtbare Produktebene reproduzierbar gestartet, diagnostiziert und gestoppt werden kann.

Startskript:

```powershell
.\tools\run-shell.ps1
.\tools\run-shell.ps1 -Once
.\tools\run-shell.ps1 -Status
.\tools\run-shell.ps1 -OverlayDemo
.\tools\run-shell.ps1 -OverlaySmokeTest
```

Der Smoke-Pfad zeigt:

```text
RK Workspace Shell
State: Running
Session: Active
CarryState: Empty
Overlay: Inactive
ProductMode: Shell
RESULT: SUCCESS
```

Damit ist sichtbar:

- Workspace Shell ist der Produktpfad.
- Eine Workspace Session ist aktiv.
- Der menschliche Carry State ist `Empty`.
- Das Overlay ist vorbereitet, aber inaktiv.
- Es gibt weiterhin keine OS-Hooks, keine Adapter, keine Discovery und keine Netzwerkfunktion.

## Workspace Overlay Prototype

MA006.02 fuehrt den ersten sichtbaren Shell-Schritt ein:

```text
src/Shell/RKWorkspace.Shell.Overlay.Windows
```

Der Prototyp ist Windows-spezifisch und bewusst von `src/Shell/RKWorkspace.Shell` getrennt. Der neutrale Shell-Core kennt nur die Overlay-States und deren Kompatibilitaet zu `WorkspaceCarryState`; die transparente Desktop-Ebene, Mausinteraktion und Darstellung liegen im Windows-Projekt.

Der Demo-Modus:

```powershell
.\tools\run-shell.ps1 -OverlayDemo
```

Der Smoke-Test:

```powershell
.\tools\run-shell.ps1 -OverlaySmokeTest
```

Der Prototyp zeigt eine transparente Ebene ueber dem echten Desktop, ein einzelnes digitales Ding und zwei Ablagen am linken und rechten Bildschirmrand. `Esc` beendet die Ebene sicher.

Ab MA006.03 bleibt dieser Desktop-Overlay-Prototyp erhalten, ist aber nicht mehr der Hauptpfad fuer das Raumgefuehl.

## Spatial Carry Tray Prototype

MA006.03 fuehrt den neuen Wahrnehmungspfad ein:

```text
src/Shell/RKWorkspace.Shell.SpatialTray
```

Der lokale Web-Prototyp wird auf dem Desktop gestartet und auf Handy oder Tablet im Browser geoeffnet. Das mobile Geraet wird als digitale Hand oder digitales Tablett gedacht. Desktop und Monitor erscheinen nur als Ablagen im Raum.

Start:

```powershell
.\tools\run-spatial-tray.ps1
```

Smoke-Test:

```powershell
.\tools\run-spatial-tray.ps1 -SmokeTest
```

MA006.03 enthaelt keine Discovery, kein Pairing, keine Cloud, keine native Mobile-App und kein echtes Payload-System.

MA006.03-A verfeinert diesen Pfad:

- Digitale Hand: ein Teil des Dings wird optisch umfasst.
- Optische Haptik: Schatten, Kontaktflaeche und weiches Nachgeben ersetzen physische Haptik auf Desktop und Laptop.
- Mobile Haptik: iPhone, iPad, Android Phone und Android Tablet sind konzeptionell vorbereitet, aber noch nicht verpflichtend.
- Ablage-Bubbles: Ablagen erscheinen als Moeglichkeiten im Raum und werden erst durch Naehe lesbar.
- Release und Cancel sind getrennt: normales Loslassen legt ab, nur explizites Zuruecklegen kehrt zur alten Ablage zurueck.
- Freier Raum ist gueltig: ein Ding darf liegen bleiben, auch wenn keine Ablage aktiv ist.

Der wichtigste Satz:

```text
Eine Ablage zeigt sich nicht sofort als Ziel.
Sie offenbart sich durch Naehe.
Erst wenn der Mensch nahe genug ist, erkennt er, was dort liegt oder moeglich ist.
```

## Spatial Room Session

MA006.04 erweitert den Spatial Carry Tray zur gemeinsamen Raum-Session.

Das zentrale Modell ist:

```text
SpatialRoomState
```

Es beschreibt:

- `RoomId`
- `Ablagen`
- `Things`
- `ActiveCarry`
- `UpdatedAt`

Kanonische Oberflaechen:

```text
/surface/handy
/surface/monitor
/surface/tablet
/surface/desktop
```

Alle Ablagen sind gleichberechtigt.

Das Ding existiert nur einmal. Wenn es genommen wird, ist `CurrentAblageId = null` und `CurrentCarryId` verweist auf die aktive Carry Session. Dadurch liegt es nicht mehr auf der Ursprungslage.

Eine Zielablage sieht das Ding bereits als Preview, bevor es endgueltig abgelegt wird.

Details: `Docs/SpatialRoomSession.md`

## Human Experience Referenz

Workspace Shell verweist ausdruecklich auf:

- HX-000: Ich bin in meinem Arbeitsraum.
- HX-001: Das gehoert zu meiner Arbeit.
- HX-001A: Das Objekt antwortet mir.
- HX-002: Ich habe etwas in meiner Hand.

Die Richtung ist wichtig:

```text
Workspace Shell folgt HX.
HX folgt nicht Workspace Shell.
```

## Workspace Carry State

`WorkspaceCarryState` beschreibt ausschliesslich den Menschen und seine Wahrnehmung:

- Empty
- Candidate
- Picked
- Carried
- NearSurface
- Placed
- Cancelled
- Lost

Diese Zustaende sind keine Computer-Transferzustaende. Sie beschreiben nicht, ob eine Datei kopiert, gesendet oder synchronisiert wurde.

## Workspace Session

`WorkspaceSession` beschreibt den aktuellen Arbeitsraum des Benutzers.

Eine Session beschreibt nicht:

- Geraete
- Betriebssysteme
- Anwendungen
- Fenster

Eine Session beschreibt:

- den aktuellen digitalen Raum
- digitale Dinge in diesem Raum
- den menschlichen Carry State

## Workspace Object

Workspace Objects gehoeren kuenftig nicht:

- Anwendungen
- Geraeten
- Betriebssystemen

Workspace Objects gehoeren Workspace Sessions.

Adapter koennen spaeter Objekte aus Anwendungen uebersetzen, aber sie besitzen diese Objekte nicht.

## Workspace Overlay

Das Overlay ist spaeter die einzige sichtbare Shell-Ebene.

Es besitzt keine:

- Fenster
- Menues
- Dialoge
- Werkzeugleisten

Es erscheint nur, wenn Human Experience entsteht.

Ab MA006.02 existieren dafuer die neutralen Overlay-Zustaende:

- Inactive
- Listening
- CarryCandidate
- Picked
- Carried
- NearAblage
- Placed
- Cancelled
- Failed

## Nicht-Ziele

MA006.00 baut noch nicht:

- Betriebssystemintegration
- globale Hot Zones
- Explorer-Integration
- Browser-Integration
- Overlay-Fenster
- Netzwerk
- Discovery
- Pairing
- Transport
- produktive Adapter

MA006.01 baut zusaetzlich noch nicht:

- Tray-Integration
- transparente Desktop-Overlays
- globale Eingabe-Hooks
- Explorer-, Browser- oder Office-Adapter
- persistente Shell-Sessions

MA006.02 baut noch nicht:

- echte Desktop-Objekterkennung
- Explorer-, Browser- oder Office-Adapter
- globale Eingabe-Hooks
- echte Monitor- oder Rand-Erkennung
- produktive Overlay-Persistenz
- Netzwerk, Discovery oder Pairing

MA006.03 baut noch nicht:

- Discovery
- Pairing
- Sicherheitsschicht
- echtes Payload-System
- native iOS- oder Android-App
- Kamera- oder UWB-Raumvermessung
- finale Produkt-UI
- erzwungene echte Mobile-Haptik
- finale Distanzschwellen fuer Ablage-Bubbles

MA006.04 baut zusaetzlich noch nicht:

- echte Mehrgeraete-Discovery
- Pairing
- sichere Raumsitzung
- echte Payload
- native Mobile- oder Desktop-App
- WebSocket-Pflicht
- echte Raumvermessung

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.5.0 | 2026-07-03 | MA006.04 Spatial Room Session, gleichberechtigte Surfaces und einheitlichen Raumzustand dokumentiert. |
| 1.4.0 | 2026-07-03 | MA006.03-A digitale Hand, optische Haptik, Ablage-Bubbles und freies Ablegen ergaenzt. |
| 1.3.0 | 2026-07-03 | MA006.03 Spatial Carry Tray als neuen Wahrnehmungspfad dokumentiert. |
| 1.2.0 | 2026-07-03 | MA006.02 Workspace Overlay Prototype und Overlay-States dokumentiert. |
| 1.1.0 | 2026-07-03 | MA006.01 Workspace Shell Runtime Host und Shell-Smoke dokumentiert. |
| 1.0.0 | 2026-07-03 | MA006.00 Workspace Shell Foundation dokumentiert. |
