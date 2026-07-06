# RKWP Certificate Provisioning

Status: MA013.23 roadmap  
Datum: 2026-07-06

## Ziel

Ablage-Zertifikate brauchen Provisioning, Rotation, Revocation und Recovery.

## Pfade

| Pfad | Zweck |
| --- | --- |
| Dev Certificates | lokale Entwicklung und Lab |
| User Pairing | persoenlicher Trust zwischen Ablagen |
| Enterprise Provisioning | verwaltete Ablagen |
| Dongle Provisioning | optionale Hardware-Identitaet |

## Provisioning

Dev:

- lokal erzeugt.
- klar als Development markiert.
- keine Produktfreigabe.

User:

- Pairing startet Trust.
- Nutzer bestaetigt Ablage.
- Fingerprint/QR/Dongle optional.

Enterprise:

- Zertifikat/Trust wird per Management verteilt.
- Policy entscheidet erlaubte Ablagen.

## Rotation

- neue Keys vor Ablauf erzeugen.
- altes und neues Zertifikat kurz parallel akzeptieren.
- Audit schreibt Rotation.
- alte Keys nach Grace-Window sperren.

## Revocation

- kompromittierte Ablage sofort revoked.
- aktive Leases werden beendet.
- Frames auf Guests werden invalid.
- Owner entsperrt Original.

## Backup und Verlustfall

- Private Keys werden nicht in Git/Context Packs kopiert.
- Backup nur verschluesselt und plattformkonform.
- Verlust einer Ablage fuehrt zu Revocation und neuem Pairing.
