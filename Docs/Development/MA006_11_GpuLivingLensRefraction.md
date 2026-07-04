# MA006.11 GPU Living Lens Refraction

Dokument-ID: RKWS-MA006-11-GPU-LIVING-LENS-REFRACTION
Version: 1.16.0
Status: Accepted
Datum: 2026-07-04

## Ziel

MA006.11 reagiert auf das Owner-Feedback zur Living Lens:

- keine lila/cyan Flaeche hinter der Linse.
- echter Desktop muss durch die Linse sichtbar bleiben.
- die Linse gehoert an den Bildschirmrand.
- das Ding darf nicht automatisch einrasten.
- das Ding braucht vektorielle Antwort und Schatten.
- Bewegung und Linsenwirkung muessen fluessiger werden.

## Neuer Slice

```text
src/Shell/RKWorkspace.Shell.LivingLens.Gpu.Windows
```

Start:

```powershell
.\tools\run-gpu-lens.ps1
.\tools\run-gpu-lens.ps1 -SmokeTest
```

## Umsetzung

Der Slice ist bewusst separat vom bisherigen `RKWorkspace.Shell.LivingLens.Windows`.

Er nutzt:

- transparentes WPF-Overlay.
- DirectX-komponierte Zeichenflaechen.
- Desktop-Sampling per `CopyFromScreen`.
- Refraction-Map-Vorbereitung durch gebrochene Darstellung des Desktop-Samples innerhalb der Linse.
- Randlinse am rechten Bildschirmrand.
- etwa 85 bis 90 Prozent sichtbare Linse, weiterhin am Rand klebend.
- Edge-Continuation, damit der Rand als Durchgang wirkt.
- Linse erscheint direkt beim Greifen.
- kontrolliertes Loslassen statt automatischer Absorption.
- flaches Ablegen auf der Arbeitsflaeche ohne Trageschatten.
- Pull-out aus der Linse.
- klares zweidimensionales Rechteck als Test-Ding.
- rechteckig-perspektivischer Trageschatten.
- sanfte perspektivische Trapez-Neigung aus der Bewegungsrichtung.
- Schattenmodell nur unter dem getragenen Ding.
- weicher Schatten aus mehreren transparenten Projektionen.
- PortalPull: die linse-nahe Kante wird bereits vor dem Loslassen Richtung Tunnel gezogen.
- PortalEdgeSqueeze: die tunnelnahe Kante und ihre Ecken laufen aufeinander zu, statt das ganze Papier um die eigene Achse zu drehen.
- PortalEdgeApexSqueeze: die tunnelnahe Kante laeuft symmetrisch zu einer Spitze zusammen.
- NoTwistPortalFunnel: keine per-Ecke-Distanzverzerrung, die das Papier verdreht.
- TiltDampingNearTunnel: normale Trage-Neigung wird direkt am Tunnel stark gedaempft.
- Schatten-Sog bei der Linsenaufnahme.
- ShadowTunnelSuction: der Schatten wird mit zur Tunneloeffnung gezogen und perspektivisch komprimiert.
- CalmRestingObjectInTunnel: im Tunnel liegt ein kleines ruhiges Papierstueck statt eines verdrehten Restobjekts.
- zusaetzliche Tunnel-Tiefenschichten in der Linse.
- Premium-Tunnelgrafik mit mehr gebrochenen Desktop-Schichten, neutralen Tiefenringen, dunklerem innerem Schlund und ruhigen Spiegelkanten.
- PremiumTunnelRefraction mit ruhigen Refraction-Ribbons und neutralen Glas-/Caustic-Spuren.
- PremiumTunnelAperture mit feiner innerer Glas-/Tiefenschichtung.
- Acht-Tunnel-Testfeld: vier Ecken und vier Seitenmitten koennen als Sogziele getestet werden.
- digitales Test-Ding startet fuer diesen Versuch in der Mitte der Arbeitsflaeche.
- VectorSuctionCenter: jede Tunnelmitte ist ein eigener Sogmittelpunkt.
- die fuehrenden Ecken/Kanten des Rechtecks werden aus der Richtung zum aktiven Tunnel berechnet, nicht mehr aus einer festen Rechtsrand-Annahme.
- ThroatSuctionTarget: die sichtbare schwarze Tunneloeffnung ist das einzige Sogziel.
- NoApexOvershoot: die Papier-Spitze darf nicht ueber den schwarzen Schlund hinauslaufen.
- StableTunnelTargetLock: der aktive Tunnel wird mit Hysterese stabilisiert, damit kein unruhiges Ausloten zwischen Nachbartunneln entsteht.
- ThroatPointCollapse: wenn das Ding direkt ueber dem schwarzen Schlund liegt, kollabiert es staerker zu einem Punkt und der Schriftzug verschwindet frueher.
- drei sofort testbare Premium-Looks: `1` Glasblase, `2` Wurmloch, `3` Hybrid.
- Glasblase priorisiert Transparenz, echte Desktop-Durchsicht, feine Lichtkante und ruhige Glasringe.
- Wurmloch priorisiert Tiefe, inneren Schlund, Ribbons, Lichtstrahlen und staerkere Tunnelwirkung.
- Hybrid kombiniert Glasmaterial und raeumlichen Tunnel als aktueller Standard.
- kompakteres digitales Ding beim Greifen, damit es mehr wie ein genommenes Arbeitsobjekt und weniger wie ein grosses UI-Element wirkt.
- Look-spezifische Entfernung, Intensitaet, Refraction-Schichten, Aperture-Schichten und Carry-Skalierung.
- sichtbare Labor-Hilfe im Overlay: aktueller Look und Tasten `1`, `2`, `3`.
- CleanDesktopPlate: aktive Refraction verwendet vorgewärmte Desktop-Plates statt staendig das sichtbare Overlay neu einzusampeln.
- SelfSamplingEchoSuppression: Papier, Schatten und Linsenlicht sollen beim schnellen Rein-/Rausfahren nicht als langes Echo in der Linse stehenbleiben.
- SoftFresnelEdge: der harte weisse Blasenrand wird durch eine weichere Fresnel-Lichtkante ersetzt.
- Premiumblock-Profile: Glasblase ist transparenter und ringaermer, Wurmloch tiefer und dunkler, Hybrid ruhiger zwischen beiden.
- SmoothPickupScale: Beim Greifen wird das Ding ueber `PickProgress` fließend kompakter, statt sprunghaft kleiner zu werden.
- SmoothLensApproach: Die Blase reagiert ueber geglaettete Annaeherung, nicht durch ploetzliches Anspringen.
- UltraFineGlassOptics: Ringe, Lichtkante und Ereignishorizont werden nochmals feiner und schwächer gezeichnet.
- HighResolutionVectorOptics: Der aktuelle Slice nutzt vektorbasierte WPF-/DirectX-Komposition mit hochwertiger Skalierung als Zwischenstufe vor echtem Shader-Rendering.
- LiveDesktopRefraction: Die aktive Linse sampled den echten Hintergrund wieder live, damit weisse oder farbige Flaechen unmittelbar durch die Blase wirken.
- CaptureExclusion: Das Overlay wird fuer Screen-Capture ausgeschlossen, damit Live-Refraction nicht wieder Papier, Schatten oder die eigene Blase als Echo einfängt.
- LensCenterLock: Der aktive Tunnel-/Blasenpunkt besitzt staerkere Hysterese gegen sichtbares Hin- und Herspringen.
- MicroGlassHighlights: Sehr feine bewegliche Lichtpunkte erhoehen den Glascharakter ohne neue Zielscheibenringe.
- PhysicalGlassMaterial: Die Linse wird als transparenter Glas-/Tunnelkoerper behandelt, nicht als gezeichneter UI-Kreis.
- GlassThickness: Mehrere sehr feine Rand- und Volumenschichten geben der Linse sichtbare Glasdicke.
- ChromaticEdge: Minimal versetzte Rot-/Cyan-Kanten simulieren subtile optische Dispersion ohne lila/cyan Flaechen.
- LensContactShadow: Die Linse bekommt einen ruhigen Kontakt-Schatten auf dem Desktop, damit sie sich vom Hintergrund absetzt.
- GlassCaustics: Feine, langsam bewegte Lichtlinien geben dem Material mehr Tiefe, ohne technische Zielringe zu erzeugen.
- SpecularGlassSweeps: Breite, weiche Reflex-Baender laufen durch die Linse und erzeugen eine hochwertigere Glaswirkung.
- Portal-Handover-State mit 10 Sekunden Ruecknahmefenster nach Drop im Tunnel.
- erneutes Nehmen innerhalb dieses Fensters setzt den Handover-Timer zurueck.
- automatisches Tunnel-Schliessen nach unberuehrtem Ablauf.
- lokale Markierung, dass das Ding danach auf der Gegenseite abgelegt ist.
- HLSL-Shader-Vertrag fuer den Direct2D-/Win2D-Produktpfad.

## Human Experience

MA006.11 unterstuetzt:

- HX-000: Der Desktop bleibt der Arbeitsraum.
- HX-001: Das Ding gehoert zur Arbeit und bleibt kein UI-Button.
- HX-001A: Das Ding antwortet durch Neigung, Traegheit und Schatten.
- HX-002: Das Ding bleibt in der Hand, bis der Mensch es loslaesst.

## Grenzen

MA006.11 ist noch kein finaler Renderer.

Noch nicht enthalten:

- finaler HLSL-Shader.
- finale physikalische Glasbrechung im nativen Shader.
- Blur-/Chromatic-Aberration-Shader.
- echte Desktop-Objekterkennung.
- echte Payload.
- echte Tablet-Ausgabe auf anderer Ablage.

Die Stufe beweist den richtigen Pfad:

```text
echter Desktop
+
transparente Linse
+
GPU-komponierte Darstellung
+
Refraction-Map-Vorbereitung
```

Der naechste Qualitaetssprung ist Direct2D, Win2D oder HLSL.

## Backup

Vor dem Shader-Sprung wurde der getestete Stand eingefroren:

```text
Tag: gpu-living-lens-depth-freeze-v1
Branch: backup/gpu-living-lens-depth-freeze-v1
Commit: 997fee3fb801b62c603bb69446cc34683f604cb9
```

Damit kann der Stand mit sauberer Randlinse, Pick-Emergence, Perspektiv-Trapez und Tunnel-Tiefe jederzeit wiederhergestellt werden.

## HLSL-Vertrag

Der Shader-Vertrag liegt in:

```text
src/Shell/RKWorkspace.Shell.LivingLens.Gpu.Windows/Shaders/LivingLensRefraction.hlsl
```

Er definiert die Parameter fuer den spaeteren Direct2D-/Win2D-Pfad:

- Desktop-Input.
- LensCenter.
- LensRadius.
- TunnelDepth.
- Absorption.
- TimeSeconds.
- ShadowSuction.
- PortalPull.
- EdgeContact.
- EdgeSqueeze.
- ApexSqueeze.
- HandoverProgress.
- TunnelClosing.
- ShadowTunnelSuction.
- PremiumRefraction.
- TunnelAperture.
- PhysicalGlass.
- GlassThickness.
- ChromaticEdge.
- LensContactShadow.
- GlassCaustics.
- SpecularGlassSweeps.
- ObjectMotion.

Die aktuelle sichtbare WPF-/GPU-Komposition spiegelt diese Logik in C# wider. Der HLSL-Vertrag ist damit die Uebergangsform zum nativen Shader-Renderer.

## Verifikation

Pflicht:

```powershell
.\tools\run-gpu-lens.ps1 -SmokeTest
.\tools\run-tests.ps1
```

Erwartung:

```text
GpuComposition: READY
DesktopSampling: OK
DesktopRefraction: OK
HlslShaderContract: OK
NoPaperAxisSpin: OK
RectangularThing: OK
RectangularShadow: OK
GentleCarryTilt: OK
SoftShadow: OK
PortalEdgePull: OK
PortalEdgeSqueeze: OK
PortalEdgeApexSqueeze: OK
NoTwistPortalFunnel: OK
TiltDampingNearTunnel: OK
TunnelDepthLayers: OK
PremiumTunnelVisual: OK
PremiumTunnelRefraction: OK
PremiumTunnelAperture: OK
EightTunnelField: OK
CornerAndEdgeTunnels: OK
CenterStartObject: OK
VectorSuctionCenter: OK
ThroatSuctionTarget: OK
NoApexOvershoot: OK
StableTunnelTargetLock: OK
ThroatPointCollapse: OK
ThreePremiumLensLooks: OK
GlassBubbleLook: OK
WormholeLook: OK
HybridLook: OK
LiveLookSwitch: OK
CompactCarryCard: OK
CleanDesktopPlate: OK
SelfSamplingEchoSuppression: OK
SoftFresnelEdge: OK
SmoothPickupScale: OK
SmoothLensApproach: OK
UltraFineGlassOptics: OK
HighResolutionVectorOptics: OK
LiveDesktopRefraction: OK
CaptureExclusion: OK
LensCenterLock: OK
MicroGlassHighlights: OK
PhysicalGlassMaterial: OK
GlassThickness: OK
ChromaticEdge: OK
LensContactShadow: OK
GlassCaustics: OK
SpecularGlassSweeps: OK
EdgeContinuation: OK
LensAppearsOnPick: OK
DropRequiresRelease: OK
PullOutFromLens: OK
PerspectiveTrapezoid: OK
ShadowModel: OK
ShadowSuction: OK
ShadowTunnelSuction: OK
CalmRestingObjectInTunnel: OK
CarryShadowOnly: OK
TransitTimeoutMs: 10000
TransitCountdown: OK
RetakeResetsTransitTimer: OK
RemotePlacement: OK
TunnelAutoClose: OK
TunnelClosedAfterTransit: OK
RemoteGestureRequired: OK
TransitState: Closed
GpuLivingLensSmoke: SUCCESS
RESULT: SUCCESS
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.16.0 | 2026-07-04 | Physical Glass Material mit Glasdicke, chromatischer Kante, Kontakt-Schatten, Caustics und Specular-Sweeps dokumentiert. |
| 1.15.0 | 2026-07-04 | Live-Desktop-Refraction, Capture-Ausschluss, stabileren Lens-Lock und Mikro-Highlights dokumentiert. |
| 1.14.0 | 2026-07-04 | Fließendes Nehmen, geglaettete Blasenannaeherung, feinere Glasoptik und hochaufloesende Vektoroptik dokumentiert. |
| 1.13.0 | 2026-07-04 | Premiumblock mit CleanDesktopPlate, Echo-Unterdrueckung, weicher Fresnel-Kante und entschärften Look-Profilen dokumentiert. |
| 1.12.0 | 2026-07-04 | Drei sofort testbare Premium-Looks Glasblase, Wurmloch und Hybrid mit Live-Umschaltung dokumentiert. |
| 1.11.0 | 2026-07-04 | Staerkeren Punkt-Kollaps direkt ueber dem schwarzen Schlund und frueheres Ausblenden des Papiertextes dokumentiert. |
| 1.10.0 | 2026-07-04 | Schwarze Tunneloeffnung als einziges Sogziel, Apex-Kappung und stabiler Tunnel-Lock dokumentiert. |
| 1.9.0 | 2026-07-04 | Acht-Tunnel-Testfeld mit Vektor-Sogmitte fuer Ecken und Seitenmitten dokumentiert. |
| 1.8.0 | 2026-07-04 | No-Twist-Funnel mit Apex-Squeeze, Neigungsdaempfung, ruhigem Tunnelobjekt und Aperture-Schichtung dokumentiert. |
| 1.7.0 | 2026-07-04 | Premium-Portal-Iteration mit PortalEdgeSqueeze, NoPaperAxisSpin, ShadowTunnelSuction und PremiumTunnelRefraction dokumentiert. |
| 1.6.0 | 2026-07-04 | Premium-Tunnelgrafik mit zusaetzlichen Desktop-Schichten, Tiefenringen, innerem Schlund und Spiegelkanten dokumentiert. |
| 1.5.0 | 2026-07-04 | Portal-Handover mit 10-Sekunden-Ruecknahmefenster, Timer-Reset bei erneutem Nehmen und Tunnel-Schliessen dokumentiert. |
| 1.4.0 | 2026-07-04 | Carry-Neigung beruhigt, Schatten weich geschichtet und PortalPull fuer linse-nahe Kanten dokumentiert. |
| 1.3.0 | 2026-07-04 | Digital Thing fuer den Physiktest auf klares Rechteck und rechteckig-perspektivischen Schatten umgestellt. |
| 1.2.0 | 2026-07-04 | Backup-Tag, HLSL-Shader-Vertrag und ShadowSuction fuer den Direct2D-/Win2D-Pfad dokumentiert. |
| 1.1.0 | 2026-07-04 | Owner-Feedback zu 85-90 Prozent sichtbarer Randlinse, Pick-Emergence, Trageschatten, Perspektiv-Trapez und Tunnel-Tiefe aufgenommen. |
| 1.0.0 | 2026-07-04 | MA006.11 GPU Living Lens Refraction Prototype dokumentiert. |
