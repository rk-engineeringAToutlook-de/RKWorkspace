# Workspace Shell

Dokument-ID: RKWS-WORKSPACE-SHELL
Version: 1.30.0
Status: Accepted
Datum: 2026-07-04

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

## Visual Reality Lab

MA006.09 fuehrt einen isolierten nativen Wahrnehmungsslice ein:

```text
src/Shell/RKWorkspace.Shell.VisualReality.Windows
```

Das Lab ist kein Produkt und kein Developer Studio. Es ist ein Visual-Reality-Spike fuer hochwertige Ablage-Linsen ueber dem echten Desktop.

Start:

```powershell
.\tools\run-visual-reality.ps1
.\tools\run-visual-reality.ps1 -SmokeTest
```

Das Lab prueft:

- fuenf Linsen-Hypothesen in der Richtung Glasbrunnen-Portal.
- Umschalten per `1` bis `5`.
- langsames Erscheinen der Linsen.
- subtile Lebendigkeit.
- digitale Griffwirkung.
- vektorielle Dingbewegung inklusive Diagonalen.
- geoeffnete Linse mit Mini-Ablage.
- Gleiten in die Linse.
- Ghost auf der Zielseite.

Der erste WinForms/GDI+-Spike hat den technischen Smoke-Test bestanden, wurde aber nach Owner-Test als visuelle Richtung verworfen:

```text
Totale grafische Katastrophe.
```

Die Shell-Grundregel wird dadurch geschaerft:

```text
Web-/Browser-Prototypen sind technische Tests.
Der aktuelle WinForms/GDI+-Spike ist ebenfalls nur noch ein technischer Test.
Der naechste gueltige HX-Schritt ist visuelle Richtung, Storyboard und Renderer-Entscheidung.
```

Das Owner-Referenzboard liegt in:

```text
Docs/Assets/VisualReality/
```

Die daraus abgeleitete Zielrichtung fuer die naechste visuelle Entscheidung ist:

```text
Glaslinse
+
Gravitationsbrunnen
+
ruhiges Portal
```

Diese Richtung darf nicht als Weltraumkulisse verstanden werden. Workspace Shell bleibt der reale Arbeitsraum; die Linse erzeugt nur eine glaubwuerdige Oeffnung innerhalb dieses Raums.

Der aktuelle Visual-Reality-Slice baut diese Richtung erstmals sichtbar:

- `1`: Glasbrunnen-Portal.
- `2`: Glasmaterial.
- `3`: Raumbrunnen.
- `4`: Ruhiges Portal.
- `5`: Minimaler Raumriss.

## Living Lens Renderer Reset

MA006.10R fuehrt einen eigenen isolierten Windows-Slice ein:

```text
src/Shell/RKWorkspace.Shell.LivingLens.Windows
```

Start:

```powershell
.\tools\run-living-lens.ps1
.\tools\run-living-lens.ps1 -SmokeTest
.\tools\run-living-lens.ps1 -ExportFrames
```

Der Slice setzt die verworfene flache Bubble-/UI-Kreis-Richtung sichtbar zurueck und prueft:

- Real Bubble Lens.
- Glass Lens als Startvariante.
- Water Surface Lens.
- Wormhole Lens.
- Gravity Lens.
- Per-Pixel-Alpha-Overlay ohne Magenta-/Color-Key-Hintergrund.
- echten Desktop als sichtbaren Raum unter der Linse.
- Primaerlinse direkt am Bildschirmrand.
- getaktetes Frame-Pacing statt Rendering pro Mausereignis.
- digitale Griffwirkung ohne stoerenden Rechteckcontainer.
- vektorbasierte Dingantwort inklusive Diagonalen.
- Linse oeffnet durch Naehe und beruhigt sich beim Wegbewegen.
- Absorption startet erst durch Loslassen oder explizites Replay.
- erneutes Herausziehen aus der Linse.
- weiche Tiefen-Andeutung statt weissem Innenrahmen.
- Lens Absorption.
- Target Emergence.
- Timing 600 ms, 1200 ms, 1800 ms.
- Visual Target Export nach `Docs/VisualTargets/MA00610R/`.

Die aktuelle Fassung ist noch kein finaler Glas-Shader. Sie beseitigt die farbige Zwischenflaeche und legt die Blase als transparente ARGB-Ebene ueber den Desktop. Die Performance ist fuer den Wahrnehmungstest verbessert, bleibt aber CPU/GDI-basiert. Echte Brechung des realen Desktop-Inhalts und hochpraezise Blasenbrillanz bleiben ein eigener GPU-/Shader-Renderer-Schritt.

Dieser Slice bleibt ein visueller Spike. Die Renderer-Entscheidung steht in `Docs/RenderingDecision_LivingLens.md`.

## GPU Living Lens Refraction Prototype

MA006.11 fuehrt einen separaten GPU-komponierten Windows-Slice ein:

```text
src/Shell/RKWorkspace.Shell.LivingLens.Gpu.Windows
```

Start:

```powershell
.\tools\run-gpu-lens.ps1
.\tools\run-gpu-lens.ps1 -SmokeTest
```

Der Slice nutzt ein transparentes WPF-Overlay und DirectX-komponierte Zeichenflaechen. Die Linse tastet den echten Desktop unter dem rechten Bildschirmrand ab und verwendet dieses Bildmaterial als Refraction-Map-Vorbereitung. Damit verschwindet der falsche lila/cyan Raum vollstaendig: hinter der Linse bleibt der reale Desktop sichtbar.

Geprueft werden:

- kein Browser.
- kein WebView.
- GPU-Kompositionspfad vorbereitet.
- Desktop-Sampling unter der Linse.
- transparente Randlinse als Durchgang.
- etwa 85 bis 90 Prozent sichtbare Randlinse.
- Edge-Continuation statt harter Bildschirmwand.
- Linse erscheint direkt beim Greifen.
- kontrolliertes Loslassen statt automatischem Einrasten.
- flaches Ablegen ohne Trageschatten.
- Pull-out aus der Linse.
- klares zweidimensionales Rechteck als digitales Test-Ding.
- rechteckig-perspektivischer Trageschatten.
- sanfte vektorielle Trapez-Neigung des Dings.
- Schattenmodell nur fuer raeumliches Tragen.
- weicher Schatten aus mehreren transparenten Projektionen.
- PortalPull an der linse-nahen Kante vor der eigentlichen Aufnahme.
- Schatten-Sog in Richtung Linse bei der Aufnahme.
- zusaetzliche Tunnel-Tiefenschichten in der Linse.
- HLSL-Shader-Vertrag als Uebergang zum Direct2D-/Win2D-Produktpfad.

MA006.11 ist noch kein finaler HLSL-Shader. Die Stufe beweist den naechsten Produktpfad: reale Desktopdaten werden in die Linsenkomposition einbezogen. Fuer maximale Brillanz, physikalisch glaubwuerdige Verzerrung, Blur und Reflexe bleibt ein Direct2D-/Win2D-/HLSL-Renderer die naechste technische Stufe.

Vor dem Shader-Sprung wurde der akzeptierte Stand mit `gpu-living-lens-depth-freeze-v1` und `backup/gpu-living-lens-depth-freeze-v1` eingefroren.

Der Portal-Handover bleibt in MA006.11 eine lokale Wahrnehmungssimulation, keine echte Uebertragung. Nach dem Ablegen im Tunnel startet ein 10-Sekunden-Fenster. Innerhalb dieses Fensters kann der Mensch das Ding wieder aus dem Tunnel nehmen; dadurch wird der Timer verworfen und beim naechsten Drop neu gestartet. Wenn das Ding 10 Sekunden unberuehrt im Tunnel bleibt, gilt es lokal als auf der Gegenseite abgelegt, die Linse schliesst sich und oeffnet sich nicht automatisch neu.

Die Tunneloptik wird in dieser Stufe ebenfalls bewusst veredelt: mehr gebrochene Desktop-Schichten, neutrale Tiefenringe, ein dunklerer innerer Schlund und ruhige Spiegelkanten sollen Tiefe erzeugen, ohne eine farbige UI-Flaeche oder einen Sci-Fi-Hintergrund einzufuehren.

Die Premium-Portal-Iteration korrigiert ausserdem die Objektphysik am Tunnel. Das digitale Ding soll nicht wie eine Vektorgrafik um seine eigene Mitte rotieren. Stattdessen zieht der Tunnel lokal an der tunnelnahen Kante: die beiden naechsten Ecken laufen symmetrisch zu einer Spitze zusammen, die Gegenseite bleibt laenger stabil und der Schatten wird als `ShadowTunnelSuction` mit in die Oeffnung gezogen. Direkt am Tunnel wird die normale Neigung gedaempft, damit keine Verdrehung entsteht. Im Tunnel liegt nur ein kleines ruhiges Papierstueck.

Fuer den aktuellen Wahrnehmungstest kann die GPU Living Lens acht Tunnel gleichzeitig zeigen: vier an den Ecken und vier in den Seitenmitten. Das digitale Ding startet in der Mitte der Arbeitsflaeche. Der aktive Tunnel wird ueber die naechste Tunnelmitte bestimmt; die Papier-Geometrie zieht ihre fuehrenden Ecken/Kanten vektorbasiert zu diesem Sogzentrum. Damit laesst sich pruefen, ob ein Ablegen nach oben, unten, links, rechts und in die Ecken gleich natuerlich wirkt.

Die sichtbare schwarze Tunneloeffnung ist dabei das einzige Sogziel. Papier und Schatten orientieren sich nicht mehr an der geometrischen Linsenmitte, sondern an diesem Schlundpunkt. Die Spitze des Papiers wird dort gekappt; direkt ueber dem Schlund kollabiert das Ding staerker zu einem Punkt und der aktive Tunnel besitzt eine kleine Hysterese, damit die Wahrnehmung nicht zwischen benachbarten Tunneln springt.

Fuer den Premium-Vergleich kann der GPU-Lens-Slice live zwischen drei visuellen Raumhypothesen wechseln: `1` Glasblase, `2` Wurmloch und `3` Hybrid. Die Umschaltung aendert nur die Darstellung der Shell-Linse: Transparenz, Refraction-Schichten, Glasringe, Aperture, Tunnelribbons, Lichtkanten, Entfernungsskala und kompakte Carry-Skalierung. Core, Runtime, Agent, IPC, Transport und Discovery bleiben unveraendert.

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

MA006.09 baut weiterhin nicht:

- finalen Renderer
- echte Shader- oder Brechungsphysik
- echte Desktop-Objekterkennung
- echte Payload
- Discovery, Pairing oder Sicherheitsschicht
- finale Produktphysik

MA006.10R baut weiterhin nicht:

- finalen Shader-Renderer
- echte Desktop-Brechung
- echte Payload
- Discovery, Pairing oder Sicherheitsschicht
- globale OS-Hooks
- finale Produktphysik

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.30.0 | 2026-07-04 | Drei live testbare GPU-Living-Lens-Premium-Looks Glasblase, Wurmloch und Hybrid eingeordnet. |
| 1.29.0 | 2026-07-04 | Punkt-Kollaps direkt ueber dem schwarzen Schlund fuer die GPU Living Lens eingeordnet. |
| 1.28.0 | 2026-07-04 | Schwarze Tunneloeffnung als Sogziel, Apex-Kappung und stabiler Tunnel-Lock eingeordnet. |
| 1.27.0 | 2026-07-04 | GPU Living Lens Acht-Tunnel-Testfeld mit vektorbasiertem Sogzentrum eingeordnet. |
| 1.26.0 | 2026-07-04 | GPU Living Lens No-Twist-Funnel mit Apex-Squeeze, Neigungsdaempfung, ruhigem Tunnelobjekt und Aperture-Schichtung dokumentiert. |
| 1.25.0 | 2026-07-04 | GPU Living Lens Premium-Portal mit PortalEdgeSqueeze, NoPaperAxisSpin, ShadowTunnelSuction und PremiumTunnelRefraction eingeordnet. |
| 1.24.0 | 2026-07-04 | GPU Living Lens Tunnelgrafik mit Premium-Tiefenringen, innerem Schlund und ruhigen Spiegelkanten ergaenzt. |
| 1.23.0 | 2026-07-04 | GPU Living Lens Portal-Handover mit Ruecknahmefenster, Timer-Reset und automatischem Tunnel-Schliessen eingeordnet. |
| 1.22.0 | 2026-07-04 | GPU Living Lens Carry beruhigt, Schatten weich geschichtet und PortalPull an linse-naher Kante ergaenzt. |
| 1.21.0 | 2026-07-04 | GPU Living Lens Digital Thing auf Rechteck-Geometrie und rechteckig-perspektivischen Schatten fuer den Physiktest umgestellt. |
| 1.20.0 | 2026-07-04 | GPU Living Lens um Backup-Referenz, HLSL-Shader-Vertrag und Schatten-Sog beim Einsaugen erweitert. |
| 1.19.0 | 2026-07-04 | GPU Living Lens um Pick-Emergence, 85-90 Prozent sichtbare Randlinse, Perspektiv-Trapez, CarryShadowOnly und Tunnel-Tiefe ergaenzt. |
| 1.18.0 | 2026-07-04 | MA006.11 GPU Living Lens Refraction Prototype mit Desktop-Sampling, Rand-Durchgang und Shader-Grenze dokumentiert. |
| 1.17.0 | 2026-07-04 | Living Lens Randlinse, Pull-out, Frame-Pacing, Brillanz und weiche Tiefe nach Owner-Video-Feedback ergaenzt. |
| 1.16.0 | 2026-07-04 | Living Lens mit Per-Pixel-Alpha, ohne Color-Key-Artefakte, Relax und kontrolliertem Loslassen ergaenzt. |
| 1.15.0 | 2026-07-04 | MA006.10R Living Lens Renderer Reset mit eigenem Slice, Lens Absorption, Timing-Varianten und Visual Target Export ergaenzt. |
| 1.14.0 | 2026-07-04 | Visual-Reality-Slice in Richtung Glasbrunnen-Portal implementiert und aktuelle Linsenreihenfolge dokumentiert. |
| 1.13.0 | 2026-07-04 | Owner-Referenzboard fuer Visual Reality und neue Zielrichtung Glaslinse plus Gravitationsbrunnen plus ruhiges Portal verankert. |
| 1.12.0 | 2026-07-04 | Owner-Bewertung des ersten Visual-Reality-Spikes als visuellen Fehlschlag dokumentiert und naechsten HX-Schritt auf Blueprint, Storyboard und Renderer-Entscheidung korrigiert. |
| 1.11.0 | 2026-07-04 | MA006.09 Visual Reality Lab als nativen HX-Testpfad fuer lebendige Ablage-Linsen ergaenzt. |
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
