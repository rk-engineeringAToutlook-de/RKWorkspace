# Operations Readiness

Status: Draft  
Datum: 2026-07-06

## Ziel

AP180 fasst den Betriebsstatus fuer den naechsten Labor- und Real-Pilot zusammen.

## Config

Config-Tool kann:

- List
- Show
- Validate
- UseProfile
- CreateLocal
- Redact

## Logs

RKWP Diagnostics kann:

- Logs lesen
- Sessions zusammenfassen
- Leases sehen
- Violations hervorheben
- Markdown exportieren

## Identity

Ablage Identity ist vorbereitet. Backup/Restore-Regeln sind dokumentiert. Private Secrets bleiben lokal und duerfen nicht unverschluesselt exportiert werden.

## Transport

DevTransport und SecureDev sind testbar. Produktiver TLS-/mTLS-Pfad bleibt Folgeschritt.

## Policy

Policy Profiles existieren. Critical Infrastructure Pack ist als Sample vorhanden. Editor ist geplant.

## Recovery

Owner Recovery ist fachlich definiert: kein stale lock, kein File Materialization, Guest Frame wird invalid.

## Diagnostics

Dashboard ist geplant. CLI ist jetzt die Vorstufe fuer Audit, Sessions, Leases, Violations und Markdown-Export.

## Offene Punkte

- produktiver Transport
- echte macOS/iPad Surfaces
- echter Policy Editor
- signierte Audit-/Exportkette
- Identity Backup mit Plattform-Keychain
