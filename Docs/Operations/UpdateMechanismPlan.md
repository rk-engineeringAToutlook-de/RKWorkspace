# Update Mechanism Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP189 beschreibt sichere Updates fuer Agenten und mobile Oberflaechen.

## Signed Updates

Updates muessen signiert und verifizierbar sein:

- Manifest
- Version
- Hash
- Signatur
- Mindestversion

## Rollback

Rollback ist erlaubt, solange:

- Security Policy es erlaubt
- keine bekannte unsichere Version reaktiviert wird
- Audit geschrieben wird

## Policy

Critical Infrastructure:

- manuelle Freigabe
- kurze Wartungsfenster
- AuditRequired
- keine stillen Updates

Development:

- manuelle lokale Updates
- klare Warnung

## Agent Update

Windows/macOS/Linux Agent:

- Dienst/Host stoppen
- neue Version pruefen
- altes Paket behalten
- Start pruefen
- Rollback bei Fehler

## Mobile Update

iOS:

- TestFlight/App Store Mechanismus

Android:

- Play/App Distribution oder MDM spaeter

## Nicht-Ziele

- kein Auto-Updater in AP189
- keine Cloud-Infrastruktur
- keine erzwungenen Updates
