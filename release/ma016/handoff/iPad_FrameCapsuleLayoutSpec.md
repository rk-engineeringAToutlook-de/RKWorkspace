# iPad FrameCapsule Layout Spec - MA016

## Goal

iPad shows the capsule as a calm working surface, not as a file receiver.

## Layout

- Primary area: frame preview.
- Edge area: nearest Ablage direction.
- Bottom action: `zurueckgeben`.
- Status text: `liegt hier im Frame`, `Verbindung verloren`, `wiederhergestellt`.
- Diagnostics hidden behind lab gesture.

## Behavior

- FrameCapsule can be opened.
- OpenFrame can be shown.
- Haptics remain subtle.
- Return clears memory-only cache.
- No File Ingress indicator is available in diagnostics.

## Forbidden UI

- Download button.
- Save PDF button.
- Share original button.
- File path display.
