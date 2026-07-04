# MA006.11 GPU Living Lens Refraction

Dokument-ID: RKWS-MA006-11-GPU-LIVING-LENS-REFRACTION
Version: 1.0.0
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
- Edge-Continuation, damit der Rand als Durchgang wirkt.
- kontrolliertes Loslassen statt automatischer Absorption.
- Pull-out aus der Linse.
- vektorielle Ding-Neigung.
- Schattenmodell unter dem getragenen Ding.

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
- echte physikalische Glasbrechung.
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
EdgeContinuation: OK
DropRequiresRelease: OK
PullOutFromLens: OK
GpuLivingLensSmoke: SUCCESS
RESULT: SUCCESS
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.0.0 | 2026-07-04 | MA006.11 GPU Living Lens Refraction Prototype dokumentiert. |
