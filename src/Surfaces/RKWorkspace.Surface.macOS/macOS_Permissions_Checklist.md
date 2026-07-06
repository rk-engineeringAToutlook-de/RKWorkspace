# macOS Permissions Checklist

Status: Prepared handoff  
Datum: 2026-07-06

## Fuer Den Ersten Frame Guest Test

- App Sandbox pruefen.
- Network Client erlauben.
- Local Network / Firewall Prompt dokumentieren.
- lokale Logs schreiben duerfen.
- keine freie Owner-PDF speichern.

## Spaeter Fuer Produktnaehere Surface

- Accessibility fuer globale Gesten.
- Input Monitoring fuer spaetere Eingaben.
- Screen Recording fuer echte Desktop-/Fenstererkennung.
- Security-scoped resources fuer explizit gewaehlt Dateien.
- Notifications optional fuer Recovery/Return.
- Trackpad/Force Touch Feedback pruefen.

## Nicht Als Abkuerzung Nutzen

Berechtigungen duerfen No File Ingress nicht umgehen. Auch mit Dateizugriff bleibt FrameOnly Default.

## Testnachweis

macOS-Codex muss im Bericht nennen:

- welche Berechtigungen aktiv waren.
- welche Prompts erschienen.
- ob Sandbox aktiv war.
- ob irgendeine PDF-Datei geschrieben wurde.
- ob No File Ingress PASS war.
