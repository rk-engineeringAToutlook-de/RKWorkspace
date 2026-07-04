# MA006.12 Real3D Lens Renderer

Dokument-ID: RKWS-MA006-12-REAL3D-LENS-RENDERER
Version: 1.1.0
Status: Accepted
Datum: 2026-07-04

## Ziel

MA006.12 reagiert auf das Owner-Feedback:

```text
Das ist technisch ein Shader-Schritt, aber visuell kein Videospiel-Stil.
```

Der bisherige WPF-/ShaderEffect-Pfad bleibt ein technischer Overlay-Schritt, ist aber nicht die Zieloptik fuer den Premium-Look.

MA006.12 fuehrt deshalb einen separaten Real3D-Look-Slice ein:

```text
src/Shell/RKWorkspace.Shell.Real3D.Lens.Web
```

Start:

```powershell
.\tools\run-real3d-lens.ps1
.\tools\run-real3d-lens.ps1 -Open
.\tools\run-real3d-lens.ps1 -SmokeTest
```

## Umsetzung

Der Slice nutzt:

- WebGLRenderer.
- Three.js.
- echte 3D-Szene.
- PerspectiveCamera.
- MeshPhysicalMaterial.
- Transmission.
- IOR.
- Thickness.
- Clearcoat.
- PMREM Environment.
- RoomEnvironment.
- ACES Filmic Tone Mapping.
- Soft Shadows.
- 3D-Tunnelgeometrie ueber TubeGeometry.
- Glasringe ueber TorusGeometry.
- Caustic-Lichtlinien.
- deformierendes digitales Ding.
- Schatten unter dem getragenen Ding.
- optionales Desktop-Livebild ueber `getDisplayMedia`.

## Warum separat

Der Real3D-Slice ist kein Produktpfad fuer Overlay-Integration.

Er ist ein Look-Labor.

Ziel ist nicht:

```text
Wie bekommen wir WPF noch etwas huebscher?
```

Sondern:

```text
Welche echte 3D-Materialwirkung fuehlt sich richtig an?
```

Wenn der Owner eine Richtung akzeptiert, wird daraus spaeter der native Produktpfad fuer Windows abgeleitet.

Nach Owner-Feedback vom 2026-07-04 ist diese Grenze verbindlich verschaerft: Der Real3D-Slice darf nicht als Produktprototyp gestartet oder bewertet werden. Er zeigt eine eigene 3D-Buehne und erzeugt dadurch nicht das Shell-Gefuehl des echten Arbeitsraums. Der korrigierte Produktpfad ist ab MA006.13 `NativeGlassOverlay.Windows`: ein natives transparentes Desktop-Overlay ohne Browser, WebView oder synthetische Buehne.

## Owner-Test

Der Owner testet:

- wirkt das Glas wie ein Koerper?
- hat der Tunnel echte Tiefe?
- fuehlt sich das digitale Ding beim Ziehen dreidimensional an?
- reagiert das Ding in Richtung Linse?
- wirkt der Schatten glaubwuerdig?
- hilft Desktop-Livebild, den realen Arbeitsraum in der Linse zu spueren?

## Grenzen

Noch nicht enthalten:

- native Windows-Overlay-Integration.
- echte transparente Desktop-Fensterintegration.
- echte Payload.
- Tablet-Handover.
- finaler Produktrenderer.

## Verifikation

Pflicht:

```powershell
.\tools\run-real3d-lens.ps1 -SmokeTest
.\tools\run-tests.ps1
```

Erwartung:

```text
RK Workspace Real3D Lens Smoke Test
WebGLRenderer: OK
ThreeJS: OK
PhysicalGlass: OK
EnvironmentLighting: OK
SoftShadows: OK
Real3DTunnel: OK
DesktopLiveTexture: OK
DeformingDigitalThing: OK
PremiumUiShell: OK
Real3DLensSmoke: SUCCESS
RESULT: SUCCESS
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-04 | Owner-Korrektur ergaenzt: Real3D bleibt Look-Labor, MA006.13 Native Glass Overlay ist der Produktpfad. |
| 1.0.0 | 2026-07-04 | Real3D Lens Renderer als separaten WebGL-/Three.js-Look-Slice dokumentiert. |
