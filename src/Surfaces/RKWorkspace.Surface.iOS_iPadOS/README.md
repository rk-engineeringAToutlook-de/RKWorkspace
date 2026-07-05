# RKWorkspace.Surface.iOS_iPadOS

Status: Prepared stub

iOS und iPadOS sind zentrale Zieloberflaechen fuer Tablet- und Phone-Ablagen. Sie sollen digitale Dinge als Frames zeigen, nicht als Dateikopien.

MA007.00 implementiert noch keine native App. Vorbereitet sind nur die neutralen Surface-Vertraege. Entwicklung laeuft spaeter ueber macOS/Xcode; auf iPhone oder iPad laeuft kein eigener Codex.

Pflichtsemantik:

- TouchHold oder LongPress wird zu neutraler SurfaceGesture.
- FramePresenter zeigt nur erlaubte Frames.
- Gast erhaelt keinen Originalpfad und keine Originalbytes.
- Gegenkante oeffnet passend zur naechsten Ablage auf Windows/macOS/Linux.

## Erste Quellen

- RK Workspace App Surface
- Share Extension
- Pasteboard nur bewusst
- Document Picker
- eigene Surface, nicht globales Greifen beliebiger App-Inhalte
