# Native Spatial Overlay

Dokument-ID: RKWS-NATIVE-SPATIAL-OVERLAY
Version: 1.5.0
Status: Accepted
Datum: 2026-07-04

## Wichtigster Satz

Keine App.

Kein Browser.

Der echte Desktop bleibt da.

Der digitale Raum erscheint nur, wenn der Mensch etwas in der Hand hat.

## Ziel

MA006.08 fuehrt den ersten nativen transparenten Spatial-Overlay-Slice fuer Windows ein.

Der Web-/Browser-Prototyp bleibt ein technisches Experiment. Er war nuetzlich fuer Raumzustand, Portalphasen und Smoke-Tests, erzeugt aber nicht das benoetigte Gefuehl, weil Browser-Chrome, Seitenstruktur und Web-App-Wahrnehmung die Human Experience stoeren.

Der native Slice liegt ueber dem echten Desktop und zeigt nur:

- digitales Ding.
- digitale Hand ueber optische Haptik.
- periphere Ablage-Bubbles.
- Ablage-Portale.

## Projekt

```text
src/Shell/RKWorkspace.Shell.NativeOverlay.Windows
```

Windows-spezifische Darstellung bleibt in diesem Projekt isoliert. Der neutrale Shell-Core bleibt frei von WinForms-, Desktop- und Betriebssystemlogik.

## Start

```powershell
.\tools\run-native-overlay.ps1
```

Smoke-Test:

```powershell
.\tools\run-native-overlay.ps1 -SmokeTest
```

Optional ueber Shell Host:

```powershell
.\tools\run-shell.ps1 -NativeOverlayDemo
.\tools\run-shell.ps1 -NativeOverlaySmokeTest
```

## Aktivierung

V1 nutzt eine stabile Testgeste:

```text
Ctrl + Alt + Space
```

Danach erscheint das Demo-Ding `Rechnung.pdf` im transparenten Overlay. Die Bubbles erscheinen erst, wenn das Ding gehalten wird.

## Digitales Ding

Das Ding ist kein Web-Element und keine Karte.

Beim Greifen:

- wird es kompakter.
- wird es teilweise verdeckt.
- erhaelt es Griffschatten.
- reagiert es vektorbasiert auf Bewegung.
- bleibt es stabil, ohne Dauerwabern.

## Digitale Hand

Es gibt keine Handgrafik.

Die digitale Hand entsteht nur durch:

- Teilmaske.
- Kontaktflaeche.
- Griffschatten.
- leichte Kompression.
- optische Haptik.

## Vektorbewegung

Die Bewegung reagiert auf:

- rechts.
- links.
- oben.
- unten.
- diagonal oben rechts.
- diagonal unten rechts.
- diagonal oben links.
- diagonal unten links.

Der Smoke-Test prueft, dass diagonale Bewegung beide Achsen der visuellen Antwort beeinflusst.

## Ablage-Bubbles

Bubbles sind keine gruenen Punkte. Ab dem Visual-Reality-Referenzboard werden sie nicht mehr als Bubble-Zielbild weitergefuehrt, sondern als Ablage-Linsen mit optischer Materialitaet.

Sie sind als ruhige Glas-/Linsen-Portale gedacht:

- optisch materiell.
- transparent, aber nicht niedlich.
- leicht glaenzend.
- raeumlich.
- peripher am Desktop-Rand.
- nur sichtbar bei aktivem Carry.

## Portal Und Mini-Ablage

Wenn das Ding einer Bubble nahekommt:

- Bubble wird groesser.
- Bubble oeffnet sich.
- Mini-Ablage wird sichtbar.
- Ding gleitet hinein.
- Zielposition wird relativ gespeichert.

Das ist noch keine echte Payload-Uebertragung. Es ist ein lokaler Human-Experience-Slice.

## Visual Reality Erweiterung

MA006.09 baut auf diesem nativen Slice auf, verschiebt den Fokus aber von "Overlay existiert" zu "Linse wirkt echt".

Neuer isolierter Spike:

```text
src/Shell/RKWorkspace.Shell.VisualReality.Windows
```

Start:

```powershell
.\tools\run-visual-reality.ps1
.\tools\run-visual-reality.ps1 -SmokeTest
```

Der Visual-Reality-Slice prueft fuenf Living-Lens-Hypothesen in neuer Reihenfolge:

- Glasbrunnen-Portal.
- Glasmaterial.
- Raumbrunnen.
- Ruhiges Portal.
- Minimaler Raumriss.

Alle Varianten sind native Windows-Darstellung ohne Browser, WebView, HTML, Statusseite oder gruene Punkte.

Der erste konkrete WinForms/GDI+-Spike wurde nach Owner-Test als visuelle Richtung verworfen. Er bleibt als technischer Smoke-Test erhalten, ist aber nicht die Grundlage fuer die naechste Linsen-Iteration.

## Renderer-Einschaetzung

Das bestehende WinForms/GDI+-Rendering war fuer einen ersten nativen Smoke-Spike ausreichend.

Nach Owner-Test ist es fuer das Zielgefuehl hochwertiger lebendiger Linsen mit echter Brechung, Blur, Shadern und per-pixel genauer Transparenz nicht ausreichend.

Bewertung:

```text
Technisch lauffaehig.
Visuell nicht glaubwuerdig genug.
Nicht weiter polieren.
```

Moegliche naechste Renderer sind in `Docs/VisualRealityBlueprint.md` dokumentiert.

Das Owner-Referenzboard fuer die naechste visuelle Richtung liegt in:

```text
Docs/Assets/VisualReality/
```

Die Referenzen korrigieren die naechste Richtung auf:

```text
Glaslinse
+
Gravitationsbrunnen
+
ruhiges Portal
```

Dabei bleibt der echte Desktop der Raum. Die Referenzen liefern Material, Tiefe und Oeffnung, aber keine Weltraumkulisse.

## Living Lens Renderer Reset

MA006.10R verlaesst die flache Bubble-/UI-Kreis-Richtung und fuehrt einen eigenen Living-Lens-Slice ein:

```text
src/Shell/RKWorkspace.Shell.LivingLens.Windows
```

Der Slice prueft:

- Real Bubble Lens.
- Glass Lens.
- Water Surface Lens.
- Wormhole Lens.
- Gravity Lens.
- Per-Pixel-Alpha statt Color-Key-Transparenz.
- keine lila/cyan Artefaktflaeche hinter der Linse.
- primaere Linse direkt am Bildschirmrand.
- weiche Tiefe statt weissem Innenrahmen.
- Lens Absorption.
- keine automatische Absorption beim Stillstehen.
- Relax der Linse beim Wegbewegen.
- Pull-out aus der Linse.
- Target Emergence.
- Visual Target Export.

Start:

```powershell
.\tools\run-living-lens.ps1
.\tools\run-living-lens.ps1 -SmokeTest
.\tools\run-living-lens.ps1 -ExportFrames
```

## Sicherheit

Pflicht:

- `Esc` beendet das Overlay.
- Smoke-Test laeuft ohne haengenden Prozess.
- kein Browser.
- kein WebView.
- keine blockierende Vollbildfalle.

Die naechste visuelle Stufe ist ein GPU-/Shader-Pfad fuer echte Desktop-Brechung. Die aktuelle Fassung ist eine saubere transparente Overlay-Schicht mit simulierter Glaswirkung und verbessertem Frame-Pacing.

MA006.11 beginnt diesen naechsten Pfad als separaten Slice:

```powershell
.\tools\run-gpu-lens.ps1
.\tools\run-gpu-lens.ps1 -SmokeTest
```

Der GPU Living Lens Refraction Prototype nutzt WPF-/DirectX-Komposition und Desktop-Sampling unter der Randlinse. Dadurch bleibt der echte Desktop durch die Linse sichtbar, und die farbige Artefaktflaeche ist nicht mehr Teil des Erlebnisses. Die finale physikalische Brechung bleibt ein spaeterer HLSL-/Direct2D-/Win2D-Schritt.

## Nicht-Ziele

MA006.08 baut noch nicht:

- globale OS-Hooks.
- echte Objekt-Erkennung auf dem Desktop.
- Explorer-, Browser- oder Office-Adapter.
- echte Payload.
- Discovery.
- Pairing.
- Sicherheitsschicht.
- finale Produktphysik.

MA006.09 baut weiterhin nicht:

- finalen Shader-Renderer.
- echte Brechung.
- echte Desktop-Objekterkennung.
- echte Payload.
- Discovery, Pairing oder Sicherheitsschicht.
- echte mobile Haptik.

MA006.10R baut weiterhin nicht:

- finalen Shader-Renderer.
- echte Brechung des Desktop-Hintergrunds.
- echte Payload.
- Discovery, Pairing oder Sicherheitsschicht.
- finale Produktphysik.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.8.0 | 2026-07-04 | MA006.11 GPU Living Lens Refraction Prototype als naechsten Renderer-Slice eingeordnet. |
| 1.7.0 | 2026-07-04 | Living Lens Owner-Video-Feedback mit Randlinse, Pull-out, weicher Tiefe und Frame-Pacing eingeordnet. |
| 1.6.0 | 2026-07-04 | Living Lens Per-Pixel-Alpha, Color-Key-Entfernung und kontrolliertes Loslassen eingeordnet. |
| 1.5.0 | 2026-07-04 | MA006.10R Living Lens Renderer Reset mit Real Bubble Lens, Lens Absorption und Visual Target Export eingeordnet. |
| 1.4.0 | 2026-07-04 | Visual-Reality-Implementierung auf Glasbrunnen-Portal, Glasmaterial, Raumbrunnen, ruhiges Portal und minimalen Raumriss korrigiert. |
| 1.3.0 | 2026-07-04 | Owner-Referenzboard fuer naechste Visual-Reality-Richtung verlinkt und Ziel auf Glaslinse, Gravitationsbrunnen und ruhiges Portal korrigiert. |
| 1.2.0 | 2026-07-04 | Owner-Bewertung des ersten WinForms/GDI+-Visual-Reality-Spikes als visuellen Fehlschlag und Renderer-Grenze dokumentiert. |
| 1.1.0 | 2026-07-04 | MA006.09 Visual Reality Lab und Renderer-Einschaetzung fuer lebendige Ablage-Linsen ergaenzt. |
| 1.0.0 | 2026-07-03 | MA006.08 Native Spatial Overlay Slice dokumentiert. |
