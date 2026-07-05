# Single Glass Edge Portal

Status: Accepted  
Datum: 2026-07-05

## Zweck

Der Single Glass Edge Portal ersetzt die vielen Bubbles als primaeren sichtbaren Uebergangshinweis.

## Renderer

Der Windows-Prototyp nutzt `GpuLivingLensSurface`, zeichnet aber im Hauptpfad nur noch eine Kante. Der alte Multi-Lens-Pfad bleibt als Laborcode erhalten, wird aber nicht mehr als Standard dargestellt.

## Bedienung

- `Ctrl+Alt+Space`: Ding an Mausposition nehmen
- Maus: Ding tragen
- `1`: Whole Edge
- `2`: Focus Point
- `3`: Directional Slot
- `A`: Absorption demonstrieren
- `R`: Reset
- `Esc`: Beenden

## Offene Grafikgrenze

Die Kante ist bewusst ruhiger als die bisherigen Bubbles. Fuer echten Premium-Glaslook bleibt ein staerkerer nativer Composition-/Direct2D-/Win2D-Shaderpfad der naechste Qualitaetssprung.
