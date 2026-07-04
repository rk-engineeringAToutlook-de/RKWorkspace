# Rendering Decision Living Lens

Dokument-ID: RKWS-RENDERING-DECISION-LIVING-LENS
Version: 1.1.0
Status: Accepted
Datum: 2026-07-04

## Entscheidungskontext

MA006.10R baut einen isolierten Living-Lens-Spike. Ziel ist nicht UI-Design, sondern die Frage, ob eine Ablage-Linse wie lebendiges Material wirken kann.

## Bewertung

| Renderer | Alpha / Overlay | Hintergrund sichtbar | Brechung / Verzerrung | Schatten / Reflex | Masken | GPU / Produktpfad | Bewertung |
| --- | --- | --- | --- | --- | --- | --- | --- |
| WinForms/GDI+ mit Per-Pixel-Alpha Layer | Echt per-pixel fuer Overlay-Elemente | Ja, ohne Magenta-/Color-Key-Artefakte | Nur simuliert | Einfach moeglich | Begrenzt | CPU-basiert | Ausreichend fuer den korrigierten Spike und Export, nicht final. |
| WinForms/GDI+ mit TransparencyKey | Begrenzt ueber TransparencyKey | Visuell fehleranfaellig | Nur simuliert | Einfach moeglich | Begrenzt | CPU-basiert | Verworfen fuer sichtbare Living Lens, weil lila/cyan Artefakte entstehen. |
| WPF | Besser als WinForms | Ja | Begrenzt, Effekte moeglich | Gut | Gut | Teilweise GPU | Moeglicher naechster Prototyp. |
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
- subtile Materialbewegung.
- kontrolliertes Loslassen statt automatischer Absorption.
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

Naechster Renderer-Kandidat fuer reine visuelle Wahrnehmungsstudien:

```text
Shader-basierter Motion-Prototyp, optional Unity.
```

Die Vision wird nicht reduziert, nur weil WinForms/GDI+ begrenzt ist.
