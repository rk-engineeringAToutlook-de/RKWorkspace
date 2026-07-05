# RKWorkspace.Surface.Windows

Status: Prepared stub

Windows ist der erste native Surface-Pfad fuer RK Workspace. Die Surface darf spaeter transparente Overlays, Desktop-Sampling, globale Gesten und Frame-Presentation verwenden.

MA007.00 implementiert hier noch keine native Technik. Die Windows-Surface muss spaeter die neutralen Vertraege aus `src/Surfaces/RKWorkspace.Surface.Abstractions` erfuellen.

Pflichtsemantik:

- RKWP Ownership bleibt fuehrend.
- FrameOnly zeigt keine Originaldatei auf dem Gast.
- Glass Edge zeigt nur die naechste Ablage.
- Entfernung und Confidence kommen aus Proximity-Providern.

## Berechtigungen und Hinweise

- Global Hooks sind spaeter gesondert zu pruefen.
- Touchpad, Touchscreen, Pen und Maus liefern nur neutrale Gesten.
- Desktop-Sampling darf die eigene Overlay-Ebene nicht als Echo einsampeln.

## Naechster Plattformauftrag

PDF FrameOnly Presenter mit Native Glass Edge verbinden.
