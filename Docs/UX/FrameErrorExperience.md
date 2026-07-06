# Frame Error Experience

Status: Draft  
Datum: 2026-07-06

## Ziel

AP167 definiert menschliche Fehlertexte fuer Lease, Frame und Policy. Fehler muessen Vertrauen erhalten, nicht technische Angst erzeugen.

## Fehler und Sprache

Nicht erlaubt:

```text
Diese Ablage darf dieses Ding nicht zeigen.
```

Verbindung verloren:

```text
Die Verbindung zur Ablage ist verloren. Das Original bleibt sicher hier.
```

Zurueckgeholt:

```text
Das Ding wurde zurueckgeholt.
```

Nicht verfuegbar:

```text
Diese Ablage ist gerade nicht verfuegbar.
```

Sichere Verbindung fehlt:

```text
Diese Ablage ist noch nicht sicher verbunden.
```

## Verbotene UI-Woerter

- Lease
- Session
- Protocol
- PolicyDenied
- TLS
- Peer
- Handshake
- Transport failure

Diese Begriffe duerfen in Diagnostics und Audit stehen, nicht im sichtbaren Benutzerpfad.

## Verhalten

Bei Fehler:

- Ding springt nicht weg.
- Original bleibt sichtbar sicher.
- Kante schliesst ruhig.
- Frame wird nicht als freie Datei dargestellt.
- Rueckgabe/Recovery bleibt erreichbar.

## Erfolg

Der Owner soll verstehen:

```text
Es hat nicht geklappt, aber mein Ding ist sicher.
```
