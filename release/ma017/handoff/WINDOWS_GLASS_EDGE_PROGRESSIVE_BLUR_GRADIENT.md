# Windows Glass Edge Progressive Blur Gradient

Status: immediate Windows Codex handoff  
Date: 2026-07-07

## Owner Correction

The current visible glass edge must not look like a hard block, bar, portal icon, or rectangular UI element.

The desired effect is a real glass edge that fades into the sharp desktop:

```text
sharp desktop -> soft progressive blur -> glass body -> fine light edge
```

This is the same visual idea as web `backdrop-filter: blur()` combined with
`mask-image: linear-gradient(...)`, but Windows is a native WPF overlay, not HTML/CSS.

## Web Reference

In CSS the desired edge would be:

```css
.glass-edge {
  position: fixed;
  background: rgba(255, 255, 255, 0.06);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  pointer-events: none;
}

.glass-right {
  right: 0;
  top: 0;
  width: 120px;
  height: 100vh;
  border-left: 1px solid rgba(255, 255, 255, 0.2);
  -webkit-mask-image: linear-gradient(to left, black 30%, transparent 100%);
  mask-image: linear-gradient(to left, black 30%, transparent 100%);
}
```

The important part is not the CSS syntax. The important part is the progressive alpha mask:

```text
opaque at the screen edge
smoothly transparent toward the desktop
```

## Native Windows Mapping

Implement the same concept in:

```text
src/Shell/RKWorkspace.Shell.NativeGlassOverlay.Windows/NativeGlassOverlaySurface.cs
src/Shell/RKWorkspace.Shell.NativeGlassOverlay.Windows/NativeGlassShaderLayer.cs
src/Shell/RKWorkspace.Shell.NativeGlassOverlay.Windows/NativeGlassMaterialEffect.cs
```

Windows/WPF equivalent:

| CSS/Web | Native Windows/WPF |
| --- | --- |
| `backdrop-filter: blur(20px)` | sampled desktop image + shader/blurred material layer |
| `mask-image: linear-gradient(...)` | `OpacityMask` / alpha gradient / shader fade |
| subtle white glass fill | low-alpha `LinearGradientBrush` over the edge band |
| transparent light border | gradient light stroke that fades toward the sharp desktop |
| `pointer-events: none` | `IsHitTestVisible = false` for glass-only layer |

## Required Visual Behavior

The glass edge must:

- be one edge only: nearest Ablage only.
- work on right, left, top, and bottom edges.
- have no hard rectangular boundary toward the desktop.
- fade over a wide band, approximately 120 to 180 px.
- keep the brightest/clearest glass at the physical screen edge.
- fade blur, fill, and light border together.
- not steal mouse input from the Desktop.
- stay quiet: no visible device UI, no button language.

## Recommended WPF Shape

For the right edge:

```text
edge band: x = screenWidth - 160, width = 160
mask: transparent at x = 0, full opacity by x = 112..160
light line: near x = screenWidth - 1, plus a softer inner catchlight
```

For the left edge, invert the mask.  
For top/bottom, rotate the gradient axis.

## Implementation Guidance For Lisa

1. Keep `DrawGlassPortalEdge(...)` as the visible entry point.
2. Replace any hard body rectangle with a progressive fade body:
   - alpha 0 at the desktop side.
   - alpha gradually reaches 40-80 near the screen edge.
   - do not draw a full-opacity rectangle.
3. Extend `NativeGlassShaderLayer` so it can render an edge band, not only the circular lens:
   - capture the band behind the edge.
   - apply the existing material shader or a light blur.
   - apply an opacity mask matching the edge direction.
4. Draw the fine light border as a gradient, not a uniform line:
   - strong near the edge mouth.
   - faded at top/bottom ends.
   - faded toward the desktop side.
5. Keep labels optional and quiet. The edge itself should carry the feeling.

## Acceptance Criteria

```text
GlassEdgeMode: NearestOnly
ProgressiveBlurGradient: SUCCESS
HardEdge: NO
DesktopFade: SMOOTH
BorderFade: SMOOTH
PointerBlocking: NO
RightEdge: SUCCESS
LeftEdge: SUCCESS
TopEdge: SUCCESS
BottomEdge: SUCCESS
RESULT: SUCCESS
```

## Anti-Goals

Do not ship:

- a rectangular frosted panel with a visible hard left/right boundary.
- a portal card, button, or block.
- a plain white vertical bar.
- a blur that stops abruptly.
- a full-screen blur.

## Summary

This is a visual material task, not a protocol task. The protocol remains RKWP frame-only. The glass edge is the human-visible direction cue for the nearest Ablage.
