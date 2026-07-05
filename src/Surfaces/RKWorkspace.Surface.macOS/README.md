# RKWorkspace.Surface.macOS

Status: Prepared stub

macOS wird als native Surface vorbereitet. Der spaetere Fokus liegt auf Trackpad-/Maus-Gesten, transparentem Overlay, Frame-Presentation und RKWP-Eingabekanaelen.

MA007.00 implementiert noch keine macOS-APIs. Die macOS-Surface darf Ownership nicht selbst entscheiden, sondern muss `RKWorkspace.Protocol` und `RKWorkspace.Surface.Abstractions` verwenden.

Pflichtsemantik:

- Original bleibt beim Owner.
- Gast bekommt Frame oder Session-Handoff nach Policy.
- CopyOut/Fork/MoveOwnership brauchen explizite Entscheidung.

## Berechtigungen und Risiken

- Accessibility fuer globale Eingaben und Gesten pruefen.
- Screen Recording fuer sichtbare Frame-/Overlay-Faelle pruefen.
- Sandbox und security-scoped access respektieren.
- Trackpad und Haptik sind native Surface-Details, keine Protokollregeln.

## Naechster Plattformauftrag

macOS Surface Host mit Trackpad-Geste, FrameOnly Presenter und RKWP No File Ingress Proof skizzieren.
