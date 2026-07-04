# MA006.13 Native Glass Overlay Foundation

Dokument-ID: RKWS-MA006-13-NATIVE-GLASS-OVERLAY
Version: 1.0.0
Status: Accepted
Datum: 2026-07-04

## Ziel

MA006.13 korrigiert den Produktpfad nach dem Real3D-Look-Slice.

Der Real3D-Slice bleibt als Materiallabor gueltig. Er ist aber kein Produkt-Overlay, weil er eine eigene Web-/3D-Buehne zeigt und dadurch nicht den echten Arbeitsraum des Menschen trifft.

MA006.13 fuehrt deshalb einen nativen Windows-Slice ein:

```text
src/Shell/RKWorkspace.Shell.NativeGlassOverlay.Windows
```

Start:

```powershell
.\tools\run-native-glass-overlay.ps1
.\tools\run-native-glass-overlay.ps1 -SmokeTest
```

## Entscheidung

Der echte Desktop bleibt die Buehne.

Das Overlay darf nur sichtbar machen:

- digitale Ding-Karte
- Trageschatten
- Glas-/Tunnel-Linse
- dezente Absorptions- und Handover-Zustaende

Es darf nicht sichtbar machen:

- kuenstlichen Raum
- Web-Demo-Flaeche
- farbigen Hintergrund
- lila/cyan Artefaktflaechen
- technische UI-Kaesten

## Umsetzung

Der Slice nutzt:

- randloses topmost WPF-Overlay.
- transparenten Hintergrund.
- keinen Browser.
- kein WebView.
- keinen synthetischen Desktop.
- Screen-Capture-Ausschluss fuer das Overlay.
- Live-Desktop-Sampling unter der Linse.
- WPF `ShaderEffect` mit dem bestehenden kompilierten Materialshader.
- zweidimensionales Rechteck als digitales Ding.
- fließendes Kleinerwerden beim Greifen.
- sanfte vektorielle Neigung.
- weichen perspektivischen Trageschatten.
- Sog zur sichtbaren Linsenmitte.
- Apex-Squeeze ohne Verdrehung.
- Schatten-Sog in Richtung Linse.
- 10-Sekunden-Handover-Fenster nach Drop im Tunnel.

## Wahrnehmungsregel

Der Mensch soll nicht denken:

```text
Ich sehe eine 3D-Demo.
```

Sondern:

```text
Mein Desktop bleibt da.
Etwas Glasiges oeffnet sich darueber.
Ich halte das Ding bis zum Loslassen.
```

## Grenzen

MA006.13 ist noch nicht:

- finaler Direct2D-/Win2D-Renderer.
- echte Desktop-Objekterkennung.
- globale OS-Hook-Integration.
- echte Payload.
- echte Tablet-/iPhone-Gegenseite.
- finaler Videospiel-Renderer.

Der Slice ist eine native Overlay-Foundation. Er macht den richtigen Produktpfad startbar und testbar. Die finale visuelle Qualitaet erfordert danach einen vollstaendigen Direct2D-/Win2D-/DirectComposition-Renderer.

## Verifikation

Pflicht:

```powershell
.\tools\run-native-glass-overlay.ps1 -SmokeTest
.\tools\run-tests.ps1
```

Erwartung:

```text
RK Workspace Native Glass Overlay Smoke Test
NativeOverlay: READY
BrowserSurface: NONE
SyntheticStage: NONE
TransparentDesktop: OK
DesktopSampling: OK
DesktopRefraction: OK
RealDesktopOnly: OK
NativeGlassOverlaySmoke: SUCCESS
RESULT: SUCCESS
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-04 | Native Glass Overlay Foundation als Produktpfad nach dem Real3D-Look-Labor dokumentiert. |
