# RKWP Audit And Revocation

Dokument-ID: RKWS-RKWP-AUDIT-REVOCATION-001
Status: Draft
Datum: 2026-07-05

## Zweck

Audit und Revocation machen Original-Owned Frames nachvollziehbar und widerrufbar. Ein digitales Ding darf auf einer Gastablage sichtbar sein, aber die Sicherheitsentscheidung bleibt an Session, Lease, Policy und Owner gebunden.

## Audit

`RkwpAuditEvent` beschreibt sicherheitsrelevante Entscheidungen. `RkwpAuditTrail` sammelt Ereignisse lokal, `IRkwpAuditSink` kapselt spaetere persistente Ziele.

Mindestereignisse:

- `SessionStarted`
- `LeaseRequested`
- `LeaseGranted`
- `LeaseDenied`
- `FrameOpened`
- `FrameInput`
- `FrameReturned`
- `LeaseRevoked`
- `LeaseExpired`
- `RecoveredByOwner`
- `OwnershipTransferRequested`
- `OwnershipTransferDenied`
- `OwnershipTransferApproved`
- `SecurityViolation`
- `ReplayDetected`
- `PolicyDenied`

## Revocation

Revocation wird durch `RkwpRevocationRequest` ausgeloest. Gruende:

- `OwnerRequested`
- `PolicyChanged`
- `HeartbeatLost`
- `SecurityViolation`
- `Timeout`
- `UserCancelled`
- `GuestDisconnected`
- `Unknown`

Ein erfolgreicher Revocation-Lauf muss:

- die CarryLease auf `Revoked` setzen,
- die FrameSession auf `Revoked` setzen,
- den Guest Frame ungueltig machen,
- das Ding beim Owner logisch entsperren,
- ein Audit-Ereignis `LeaseRevoked` schreiben.

## Replay und Policy

Replay-Schutz schreibt `ReplayDetected`. Policy-Verletzungen schreiben `PolicyDenied`. Beide Ereignisse sind keine UX-Zustaende, sondern Schutzsignale fuer kritische Umgebungen.

## Recovery

Recovery unterscheidet:

- `LeaseExpired`
- `RecoveredByOwner`
- `Revoked`
- `ConnectionLost`
- `Returned`

Heartbeat-Verlust durchlaeuft HeartbeatMissing, GracePeriod und danach `RecoveredByOwner`. Security-Verletzungen sollen nicht als normaler Timeout behandelt werden, sondern ueber Revocation enden.

## Produktive offene Punkte

- produktive Key-Aushandlung,
- AEAD-Verschluesselung,
- sichere Identitaet der Ablagen,
- persistenter manipulationssicherer Audit-Speicher,
- definierte Policy-Migration mit Zustimmung,
- sichere Clock-/Timestamp-Strategie.
