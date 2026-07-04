# Rendering Decision Living Lens

Dokument-ID: RKWS-RENDERING-DECISION-LIVING-LENS
Version: 1.2.0
Status: Accepted
Datum: 2026-07-04

## Entscheidungskontext

MA006.10R baut einen isolierten Living-Lens-Spike. Ziel ist nicht UI-Design, sondern die Frage, ob eine Ablage-Linse wie lebendiges Material wirken kann.

## Bewertung

| Renderer | Alpha / Overlay | Hintergrund sichtbar | Brechung / Verzerrung | Schatten / Reflex | Masken | GPU / Produktpfad | Bewertung |
| --- | --- | --- | --- | --- | --- | --- | --- |
| WinForms/GDI+ mit Per-Pixel-Alpha Layer | Echt per-pixel fuer Overlay-Elemente | Ja, ohne Magenta-/Color-Key-Artefakte | Nur simuliert | Einfach moeglich | Begrenzt | CPU-basiert | Ausreichend fuer den korrigierten Spike und Export, nicht final. |
| WinForms/GDI+ mit TransparencyKey | Begrenzt ueber TransparencyKey | Visuell fehleranfaellig | Nur simuliert | Einfach moeglich | Begrenzt | CPU-basiert | Verworfen fuer sichtbare Living Lens, weil lila/cyan Artefakte entstehen. |
| WPF mit Desktop-Sampling | Gut fuer transparentes Overlay | Ja, echter Desktop wird unter der Linse abgetastet | Erste Refraction-Map-Vorbereitung, noch kein HLSL | Gut | Gut | GPU-komponiert | MA006.11 Slice: richtiger naechster Produktpfad, aber noch nicht finale Shader-Qualitaet. |
| Win2D / Direct2D | Sehr gut | Ja | Shader-/Effektpfad moeglich | Sehr gut | Sehr gut | GPU | Starker Kandidat fuer Windows-Prototyp. |
| Windows Composition API | Sehr gut | Ja | Effekte/Blur/Layering gut | Sehr gut | Gut | GPU | Starker Kandidat fuer Produkt-Overlay. |
| SkiaSharp | Gut | Ja | Simulierbar, Shader moeglich | Gut | Gut | GPU je nach Backend | Guter plattformnaher Prototypkandidat. |
| Unity | Gut | Ja, aber Overlay-Integration aufwendig | Sehr gut | Sehr gut | Sehr gut | GPU | Gut fuer Motion-Prototyp, schwerer Produktpfad. |
| Unreal | Sehr gut | Ja, aber schwergewichtig | Sehr gut | Sehr gut | Sehr gut | GPU | Zu schwer fuer aktuellen Produktpfad. |
| Shader-basierter Renderer | Sehr gut | Ja | Sehr gut | Sehr gut | Sehr gut | GPU | Langfristig wahrscheinlich noetig fuer echte Materialwirkung. |

## Ehrliche Einschaetzung

```text
WinForms/GDI+ mit Per-Pixel-Alpha reicht fuer MA006.10R als isolierten visuellen Spike.
WinForms/GDI+ reicht fuer das finale Zielgefuehl nicht aus.
```

Der aktuelle Spike darf beweisen:

- Variantenlogik.
- echter Overlay-Transparenzpfad ohne Color-Key-Artefakte.
- Randverankerung.
- primaere Randlinse als Durchgang am Bildschirmrand.
- besseres Frame-Pacing im CPU-Prototyp.
- subtile Materialbewegung.
- kontrolliertes Loslassen statt automatischer Absorption.
- erneutes Herausziehen aus der Linse.
- Lens Absorption.
- Target Emergence.
- ExportFrames.

Er darf nicht als Beweis gelten fuer:

- echte Brechung des realen Desktops.
- finale Glas-/Wasserphysik.
- finalen Shader-Look.
- Produkt-Overlay-Performance.

## Empfehlung

Naechster Renderer-Kandidat fuer den Windows-Pfad:

```text
Windows Composition API oder Win2D / Direct2D.
```

Der naechste Qualitaetssprung ist nicht eine weitere Farbpolitur. Er ist echte Hintergrundaufnahme plus Shader-/Effektpfad, damit der reale Desktop durch die Blase gebrochen, gestaucht und optisch glaubwuerdig verdichtet werden kann.

MA006.11 setzt diesen Sprung erstmals als separaten WPF-/DirectX-komponierten Slice um. Er tastet den realen Desktop unter der Linse ab, entfernt lila/cyan Artefaktflaechen aus dem Erlebnis und bereitet Refraction-Maps vor. Er ist bewusst noch kein finaler HLSL-/Direct2D-Shader.

Der erste Shader-Sprung fuegt einen HLSL-Vertrag hinzu, der die spaeteren Direct2D-/Win2D-Parameter definiert: Desktop-Input, LensCenter, LensRadius, TunnelDepth, Absorption, TimeSeconds, ShadowSuction und ObjectMotion. Der aktuelle sichtbare Renderer spiegelt diese Gleichungen in C# wider, bis der native Shaderpfad aktiviert wird.

Owner-Video-Feedback vom 2026-07-04 bestaetigt diese Grenze: Die Blase ist in der aktuellen Richtung richtig, aber fuer "mega" Brillanz, echte Spiegelung, perfekte Fluessigkeit und glaubwuerdige dreidimensionale Materialtiefe sollte der naechste Sprint einen GPU-Pfad pruefen. Der CPU/GDI-Slice bleibt Wahrnehmungs- und Ablaufprototyp, nicht Endrenderer.

Naechster Renderer-Kandidat fuer reine visuelle Wahrnehmungsstudien:

```text
Shader-basierter Motion-Prototyp, optional Unity.
```

Die Vision wird nicht reduziert, nur weil WinForms/GDI+ begrenzt ist.

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 1.3.0 | 2026-07-04 | HLSL-Shader-Vertrag und ShadowSuction als Uebergang zum nativen Direct2D-/Win2D-Renderer ergaenzt. |
| 1.2.0 | 2026-07-04 | MA006.11 GPU Living Lens Refraction Prototype mit Desktop-Sampling und Renderer-Grenze eingeordnet. |
| 1.1.0 | 2026-07-04 | Per-Pixel-Alpha-Layer und Owner-Video-Grenze ergaenzt. |
