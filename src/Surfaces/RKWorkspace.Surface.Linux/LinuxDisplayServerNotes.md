# Linux Display Server Notes

Status: Docs/stub only
Datum: 2026-07-05

## X11

X11 kann Overlays und globale Eingaben technisch leichter erlauben, ist aber sicherheitlich grober.

## Wayland

Wayland schuetzt globale Eingriffe staerker. Portals sind wahrscheinlich Pflicht fuer Capture und Screen-bezogene Funktionen.

## Portals

Portals muessen fuer Screenshot/Capture und Datei-/Dokumentauswahl geprueft werden.

## Entscheidung

V1 darf als Agent/FrameGuestSurface ohne finalen Overlay-Anspruch starten.
