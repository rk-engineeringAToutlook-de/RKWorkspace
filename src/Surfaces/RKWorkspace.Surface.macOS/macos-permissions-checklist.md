# macOS Permissions Checklist

Status: MA011.08 handoff  
Datum: 2026-07-06

## Pflicht fuer den ersten Test

- Code Signing fuer lokalen Test aktivieren.
- App Sandbox bewusst konfigurieren.
- Local Network Zugriff pruefen, falls DevLan genutzt wird.
- Eingehende Verbindungen nur nutzen, wenn der Testaufbau es verlangt.
- Logs unter `Application Support/RKWorkspace/logs/` schreiben.
- AblageIdentity unter `Application Support/RKWorkspace/identity.json` speichern.

## Nicht erforderlich fuer V1

- Full Disk Access.
- Screen Recording.
- Accessibility.
- globale Dateisystemueberwachung.
- Finder-Integration.

## No File Ingress Check

Nach einem Test pruefen:

- keine `.pdf` im App Container.
- keine `.pdf` in Downloads.
- keine Originalbytes in Logs.
- kein Windows Originalpfad als lokale Datei.
- nur Frame-/Diagnoseinformationen persistiert.

## Offene Blocker melden

macOS-Codex soll Blocker explizit melden:

- Local Network Permission fehlt.
- Xcode Signing blockiert.
- DevLan/SecureDev Verbindung nicht erreichbar.
- PDF-Frame kann nur als Mock angezeigt werden.
