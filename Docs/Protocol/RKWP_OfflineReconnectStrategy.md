# RKWP Offline Reconnect Strategy

Status: Draft  
Datum: 2026-07-06

## Ziel

AP177 definiert, wie RKWP mit Offline, Reconnect und verlorenen Frames umgeht.

## Grundregel

Wenn Verbindung oder Session unsicher ist, bleibt das Original beim Owner sicher.

## Frame invalid

Ein Guest-Frame wird invalid, wenn:

- Lease ablaeuft
- Heartbeat fehlt
- Session revoked wird
- sichere Verbindung verloren ist
- Policy geaendert wurde

## Owner Recovery

Der Owner darf wiederherstellen:

- Guest Frame invalidieren
- Original entsperren
- Audit schreiben
- sichtbare Sprache: `wiederhergestellt`

## Guest Reconnect

Guest darf reconnecten, wenn:

- Identitaet unveraendert
- Policy unveraendert
- Lease noch gueltig oder erneuert
- Owner bestaetigt

## No stale lock

Ein verlorener Guest darf das Original nicht dauerhaft sperren. Recovery muss Vorrang haben.

## No file materialization

Offline bedeutet nicht:

- Datei lokal speichern
- PDF extrahieren
- Frame zu Original machen
- Ownership still uebernehmen

## UI-Sprache

Erlaubt:

- Verbindung verloren
- wiederhergestellt
- nicht verfuegbar
- wieder verfuegbar

Vermeiden:

- Session timeout
- lease expired
- peer disconnected
