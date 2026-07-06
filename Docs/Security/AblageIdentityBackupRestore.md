# Ablage Identity Backup Restore

Status: Draft  
Datum: 2026-07-06

## Ziel

AP178 beschreibt Backup und Restore fuer Ablage-Identitaeten.

## Public Export

Erlaubt:

- AblageId
- DisplayName
- Platform
- PublicKey/Fingerprint spaeter
- Trust-Hinweis

## Private Backup

Private Schluessel oder Secrets duerfen nur optional und verschluesselt gesichert werden.

Anforderungen:

- lokale Verschluesselung
- klares Restore-Risiko
- kein Chat-/Clipboard-Transfer von Secrets
- keine unverschluesselte Handoff-Datei

## Rotate

Rotation benoetigt:

- neue Identitaet oder neuen Schluessel
- altes Material widerrufen
- Peers informieren
- Audit schreiben

## Revoke

Bei verlorener Ablage:

- Identity revoked
- offene Leases invalidieren
- Frames schliessen
- Owner Recovery ausloesen

## Lost Device

Wenn ein Geraet verloren ist:

- keine stillen Guest Frames
- kein Trust behalten
- keine Materialisierung erlauben
- neue Pairing-Entscheidung erzwingen

## Dongle

Ein Dongle kann Identity stabilisieren, aber Restore nicht ersetzen. Dongle-Verlust ist wie Ablage-Verlust zu behandeln.
