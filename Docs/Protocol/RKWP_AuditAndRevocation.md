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
- `Heartbeat`
- `NoFileIngressChecked`

## Persistenter Development Audit Log

MA008.08 fuehrt ein persistierbares Development-Format ein:

- `RkwpAuditLogRecord`
- `RkwpAuditSeverity`
- `JsonlRkwpAuditLogStore`
- `RkwpSessionDiagnostics`

Dev-Speicherort:

```text
logs/rkwp-audit/
```

Format:

```text
JSONL, eine Zeile pro Audit Event
```

Dieses Format ist noch nicht manipulationssicher. Es dient Debugging, Smoke Tests und Handoff-Diagnose. Produktive Auditierung braucht spaeter Signatur, Verschluesselung, Retention-Policy und Clock-Strategie.

MA008.09 macht Audit fuer Production zur Gate-Regel. Eine Production-Konfiguration ohne `RequireAudit` wird von `RkwpSecurityGate` abgelehnt.

Das CLI-Tool liegt unter:

```text
src/Tools/RKWorkspace.RkwpDiagnostics/
```

Start:

```powershell
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
```

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
