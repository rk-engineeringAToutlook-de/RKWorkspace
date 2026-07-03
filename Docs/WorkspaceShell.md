# Workspace Shell

Dokument-ID: RKWS-WORKSPACE-SHELL
Version: 1.10.0
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
.\tools\run-shell.ps1 -NativeOverlayDemo
.\tools\run-shell.ps1 -NativeOverlaySmokeTest
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

## Native Spatial Overlay Slice

MA006.08 fuehrt den neuen nativen Gefuehlspfad ein:

```text
src/Shell/RKWorkspace.Shell.NativeOverlay.Windows
```

Der Slice ist kein Browser und kein WebView. Er ist ein randloses, transparentes, topmost Windows-Overlay ueber dem echten Desktop. Die Aktivierung erfolgt in V1 ueber `Ctrl+Alt+Space`.

Der Slice zeigt:

- Demo-Ding `Rechnung.pdf`.
- digitale Hand ueber optische Haptik.
- vektorbasierte Bewegung inklusive Diagonalen.
- periphere Bubble-Linsen nur bei aktivem Carry.
- Mini-Ablage im geoeffneten Portal.
- sicheres Ende ueber `Esc`.

Start:

```powershell
.\tools\run-native-overlay.ps1
.\tools\run-native-overlay.ps1 -SmokeTest
```

Der Web-/Browser-Prototyp bleibt technisches Experiment. Fuer das echte Gefuehl ist ab MA006.08 das native Overlay der primaere Pfad.

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
- `Version`
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

Ab der verfeinerten MA006.04-Fassung reagiert eine Zielablage mit `OpeningAblage`: Die Ablage oeffnet sich als ruhige Ablage-Linse, macht ihren Namen erst durch Naehe lesbar und zeigt `Hier ablegen` nur im aktiven Moment. `SpatialRoomState`, `SpatialAblage` und `SpatialThing` tragen dafuer Version und Metadata, ohne daraus technische Transportlogik zu machen.

Spatial Handover ist nur dokumentiert. Es gibt noch kein Handover-Protokoll, keine Discovery, kein Pairing und keine echte Payload.

MA006.05 verfeinert diesen Pfad als Tactile Mobile Carry Slice. Der Fokus liegt nicht auf neuen Funktionen, sondern auf dem einen Wahrnehmungsablauf: Ding von Ablage Handy nehmen, in der digitalen Hand halten, Ablage Monitor oeffnen lassen, das Ding hinein gleiten lassen und an einer relativen Position ablegen.

Die Shell-Modelle bleiben unveraendert plattformneutral. Der lokale Spatial Tray meldet dafuer zusaetzliche Wahrnehmungsdaten:

- Bewegungsneigung nach Vektor.
- kompakter gehaltenes Ding.
- Teilverdeckung und Kontaktflaeche.
- fast vollstaendig reduziertes Wabern.
- weiches Einrasten.
- Ghost auf Zielablage vor Place.
- gleitender Uebergang in die Ablage-Bubble.
- relative Zielposition auf der Ablage.

MA006.06 erweitert diesen Wahrnehmungspfad um Spatial Portal Carry. Eine Zielablage am Rand des wahrgenommenen Raums oeffnet sich als Portal. Das Ding wird nicht sofort auf die Zielablage gesetzt, sondern durchlaeuft eine `SpatialPortalTransition` mit Source-/Target-Progress:

- Quelle: Ding tritt optisch in die Oeffnung ein.
- Zwischenraum: `Progress` beschreibt den Weg durch den Raum.
- Ziel: Ghost erscheint, wird klarer und meldet `ReadyToPlace`.
- Erst `Place` setzt `CurrentAblageId` und speichert die Zielposition.

Auch dieser Schritt bleibt Shell-Wahrnehmung, nicht Transporttechnik. Es gibt keine Payload, keine Discovery, kein Pairing und keine echte Plattformkopplung.

MA006.07 korrigiert die sichtbare Richtung. Die MA006.06-Logik bleibt, aber die Radar-/Statusseiten-Darstellung wird nicht als Produktpfad fortgefuehrt. Die Surface wird auf eine ruhige Ablageflaeche reduziert:

- Empty zeigt keine Bubbles und keine Karte.
- Bubbles erscheinen erst bei aktiver Tragehandlung.
- Bubbles liegen peripher am Rand.
- sichtbare Abbruch- und Reset-Buttons werden aus dem Erlebnis entfernt.
- Web App Manifest und Standalone-Modus sind vorbereitet.

Damit folgt die Shell staerker dem Produktgrundsatz: Workspace Shell ist keine Anwendung, sondern der Raum, der nur in Erscheinung tritt, wenn Human Experience entsteht.

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

MA006.05 baut weiterhin nicht:

- Discovery
- Pairing
- LAN- oder Cloud-Kopplung
- Kamera, UWB oder Dongle
- echte Payload
- native Mobile-App
- finale Produkt-UI

MA006.06 baut weiterhin nicht:

- echte Portal- oder Handover-Technik
- echte Mehrgeraete-Kopplung
- Payload-Transport
- Discovery, Pairing oder Sicherheitsschicht
- native Mobile-App
- echte Raum- oder Monitorerkennung
- produktive Shell-Persistenz

MA006.07 baut weiterhin nicht:

- native Mobile-App
- echte transparente Mobile- oder Desktop-Overlay-Schicht
- OS-Hooks
- echte Raumvermessung
- echte Monitorerkennung
- Discovery, Pairing oder Payload-Transport

MA006.08 baut weiterhin nicht:

- globale OS-Hooks
- echte Desktop-Objekterkennung
- Explorer-, Browser- oder Office-Adapter
- echte Payload
- Discovery, Pairing oder Sicherheitsschicht
- finale Produktphysik

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.10.0 | 2026-07-03 | MA006.08 Native Spatial Overlay Slice als neuen primaeren Gefuehlspfad ohne Browser/WebView dokumentiert. |
| 1.9.0 | 2026-07-03 | MA006.07 Surface Overlay Reset als Korrektur von Radar-/Statusseite zu ruhiger Ablageflaeche mit Bubbles nur beim Tragen eingeordnet. |
| 1.8.0 | 2026-07-03 | MA006.06 Spatial Portal Carry als Shell-Wahrnehmungspfad mit Source-/Target-Progress und ReadyToPlace eingeordnet. |
| 1.7.0 | 2026-07-03 | MA006.05 Tactile Mobile Carry Slice mit taktilen Wahrnehmungsdaten und gleitendem Ablegen eingeordnet. |
| 1.6.0 | 2026-07-03 | MA006.04 Ablage-Linse, OpeningAblage, Version/Metadata und dokumentiertes Spatial Handover ergaenzt. |
| 1.5.0 | 2026-07-03 | MA006.04 Spatial Room Session, gleichberechtigte Surfaces und einheitlichen Raumzustand dokumentiert. |
| 1.4.0 | 2026-07-03 | MA006.03-A digitale Hand, optische Haptik, Ablage-Bubbles und freies Ablegen ergaenzt. |
| 1.3.0 | 2026-07-03 | MA006.03 Spatial Carry Tray als neuen Wahrnehmungspfad dokumentiert. |
| 1.2.0 | 2026-07-03 | MA006.02 Workspace Overlay Prototype und Overlay-States dokumentiert. |
| 1.1.0 | 2026-07-03 | MA006.01 Workspace Shell Runtime Host und Shell-Smoke dokumentiert. |
| 1.0.0 | 2026-07-03 | MA006.00 Workspace Shell Foundation dokumentiert. |
