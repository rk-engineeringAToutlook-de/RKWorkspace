# App Window Interactive Frame

Status: MA013.45  
Datum: 2026-07-06

## Ziel

App-Fenster sollen spaeter als interaktive Frames auf einer anderen Ablage sichtbar und bedienbar sein, ohne Fensterbesitz oder Anwendungskontext automatisch zu uebertragen.

## Grundsatz

Ein App Window ist kein Dateiobjekt. RK Workspace zeigt eine kontrollierte Frame-Sicht auf das Originalfenster.

```text
Owner App Window
  -> Capture / UIA / Frame Broker
  -> InteractiveFrame
  -> RKWP Input Policy
  -> Guest Ablage
```

## UIA

UI Automation ist ein moeglicher Pfad fuer Metadaten, Titel, Rollen und ausgewaehlte Interaktionen. UIA darf aber nicht blind als Vollzugriff verstanden werden.

## Input Forwarding

Eingaben sind nur erlaubt, wenn:

- FramePolicy Input erlaubt.
- Pointer/Keyboard/Annotation einzeln erlaubt sind.
- SettingsWindow nicht durch kritische Policy blockiert ist.
- Audit aktiv ist.

## Capture

Erster Produktpfad:

- Snapshot/Frame statt Besitzwechsel.
- Fensterhandle planned.
- Titel und Bounds als Metadaten.
- Reale Capture-API erst nach Plattformfreigabe.

## Permissions

Windows benoetigt voraussichtlich Berechtigungen/Policies fuer Fenster-Capture, UIA und ggf. geschuetzte Fenster. macOS braucht Accessibility/Screen Recording. Mobile Plattformen koennen App-Fenster anderer Apps nicht frei capturen.

## Security

- Kein Ownership Transfer fuer SettingsWindow.
- Kein verstecktes Keyboard Forwarding.
- Kein System Shortcut Forwarding im kritischen Profil.
- Keine Guest-Datei.
- Audit bei jeder abgelehnten Eingabe.

## Limitations

- Geschuetzte Inhalte koennen schwarz oder blockiert sein.
- Admin-/Security-Fenster werden nicht interaktiv geteilt.
- Cross-Platform-Verhalten ist nicht einheitlich.
- Latenz und Cursor-Feedback muessen spaeter UX-validiert werden.

## No Ownership Transfer

Das Fenster bleibt beim Owner. Guest sieht und bedient nur einen Frame, wenn Policy und Berechtigungen dies erlauben.
