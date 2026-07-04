# Rendering Decision Living Lens

Dokument-ID: RKWS-RENDERING-DECISION-LIVING-LENS
Version: 1.0.0
Status: Accepted
Datum: 2026-07-04

## Entscheidungskontext

MA006.10R baut einen isolierten Living-Lens-Spike. Ziel ist nicht UI-Design, sondern die Frage, ob eine Ablage-Linse wie lebendiges Material wirken kann.

## Bewertung

| Renderer | Alpha / Overlay | Hintergrund sichtbar | Brechung / Verzerrung | Schatten / Reflex | Masken | GPU / Produktpfad | Bewertung |
| --- | --- | --- | --- | --- | --- | --- | --- |
| WinForms/GDI+ | Begrenzt ueber TransparencyKey | Ja, aber keine echte per-pixel Komposition | Nur simuliert | Einfach moeglich | Begrenzt | CPU-basiert | Ausreichend fuer Spike und Export, nicht final. |
| WPF | Besser als WinForms | Ja | Begrenzt, Effekte moeglich | Gut | Gut | Teilweise GPU | Moeglicher naechster Prototyp. |
| Win2D / Direct2D | Sehr gut | Ja | Shader-/Effektpfad moeglich | Sehr gut | Sehr gut | GPU | Starker Kandidat fuer Windows-Prototyp. |
| Windows Composition API | Sehr gut | Ja | Effekte/Blur/Layering gut | Sehr gut | Gut | GPU | Starker Kandidat fuer Produkt-Overlay. |
| SkiaSharp | Gut | Ja | Simulierbar, Shader moeglich | Gut | Gut | GPU je nach Backend | Guter plattformnaher Prototypkandidat. |
| Unity | Gut | Ja, aber Overlay-Integration aufwendig | Sehr gut | Sehr gut | Sehr gut | GPU | Gut fuer Motion-Prototyp, schwerer Produktpfad. |
| Unreal | Sehr gut | Ja, aber schwergewichtig | Sehr gut | Sehr gut | Sehr gut | GPU | Zu schwer fuer aktuellen Produktpfad. |
| Shader-basierter Renderer | Sehr gut | Ja | Sehr gut | Sehr gut | Sehr gut | GPU | Langfristig wahrscheinlich noetig fuer echte Materialwirkung. |

## Ehrliche Einschaetzung

```text
WinForms/GDI+ reicht fuer MA006.10R als isolierten visuellen Spike.
WinForms/GDI+ reicht fuer das finale Zielgefuehl nicht aus.
```

Der aktuelle Spike darf beweisen:

- Variantenlogik.
- Transparenzpfad.
- Randverankerung.
- subtile Materialbewegung.
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

Naechster Renderer-Kandidat fuer reine visuelle Wahrnehmungsstudien:

```text
Shader-basierter Motion-Prototyp, optional Unity.
```

Die Vision wird nicht reduziert, nur weil WinForms/GDI+ begrenzt ist.

