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
- etwa 85 bis 90 Prozent sichtbare Linse, weiterhin am Rand klebend.
- Edge-Continuation, damit der Rand als Durchgang wirkt.
- Linse erscheint direkt beim Greifen.
- kontrolliertes Loslassen statt automatischer Absorption.
- flaches Ablegen auf der Arbeitsflaeche ohne Trageschatten.
- Pull-out aus der Linse.
- staerkere perspektivische Trapez-Neigung aus der Bewegungsrichtung.
- Schattenmodell nur unter dem getragenen Ding.
- zusaetzliche Tunnel-Tiefenschichten in der Linse.

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
LensAppearsOnPick: OK
DropRequiresRelease: OK
PullOutFromLens: OK
PerspectiveTrapezoid: OK
CarryShadowOnly: OK
GpuLivingLensSmoke: SUCCESS
RESULT: SUCCESS
```

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.1.0 | 2026-07-04 | Owner-Feedback zu 85-90 Prozent sichtbarer Randlinse, Pick-Emergence, Trageschatten, Perspektiv-Trapez und Tunnel-Tiefe aufgenommen. |
| 1.0.0 | 2026-07-04 | MA006.11 GPU Living Lens Refraction Prototype dokumentiert. |
