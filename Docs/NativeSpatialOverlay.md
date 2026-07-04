# Native Spatial Overlay

Dokument-ID: RKWS-NATIVE-SPATIAL-OVERLAY
Version: 1.2.0
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

Bubbles sind keine gruenen Punkte.

Sie sind als transparente Seifenblasen-/Linsen-Portale gedacht:

- weich.
- transparent.
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

Der Visual-Reality-Slice prueft fuenf Living-Lens-Hypothesen:

- Seifenblase.
- Wasserlinse.
- Glaslinse.
- Portal-Linse.
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

## Sicherheit

Pflicht:

- `Esc` beendet das Overlay.
- Smoke-Test laeuft ohne haengenden Prozess.
- kein Browser.
- kein WebView.
- keine blockierende Vollbildfalle.

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

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.2.0 | 2026-07-04 | Owner-Bewertung des ersten WinForms/GDI+-Visual-Reality-Spikes als visuellen Fehlschlag und Renderer-Grenze dokumentiert. |
| 1.1.0 | 2026-07-04 | MA006.09 Visual Reality Lab und Renderer-Einschaetzung fuer lebendige Ablage-Linsen ergaenzt. |
| 1.0.0 | 2026-07-03 | MA006.08 Native Spatial Overlay Slice dokumentiert. |
