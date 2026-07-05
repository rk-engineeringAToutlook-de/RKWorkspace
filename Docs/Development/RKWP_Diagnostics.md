# RKWP Diagnostics

Status: Prepared
Datum: 2026-07-05

## Ziel

RKWP Diagnostics ist ein Entwicklungs- und Audit-Werkzeug. Es ist keine Endnutzer-UX.

Das Tool macht sichtbar:

- Sessions
- Leases
- FrameSessions
- Heartbeats
- Recovery
- Policy Denials
- Security Violations
- No File Ingress
- Ownership Transfer Requests
- Revocations

## Tool

Projekt:

```text
src/Tools/RKWorkspace.RkwpDiagnostics/
```

Script:

```powershell
.\tools\run-rkwp-diagnostics.ps1
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
.\tools\run-rkwp-diagnostics.ps1 -ReadLog "logs\rkwp-audit\<file>.jsonl"
```

## Audit Log

Dev-Speicherort:

```text
logs/rkwp-audit/
```

Format:

```text
JSONL
```

Jede Zeile ist ein `RkwpAuditLogRecord`.

Pflichtfelder:

- EventId
- Timestamp
- EventType
- SessionId
- LeaseId optional
- FrameSessionId optional
- ThingId optional
- SourceAblageId
- TargetAblageId
- Severity
- Message
- Metadata

## Smoke Test

Der Smoke erzeugt ein frisches JSONL-Log und liest es wieder ein.

Erzeugte Ereignisse:

- `SessionStarted`
- `LeaseGranted`
- `FrameOpened`
- `NoFileIngressChecked`
- `Heartbeat`
- `PolicyDenied`
- `FrameReturned`
- `RecoveredByOwner`

Erwartete Ausgabe:

```text
RkwpDiagnosticsSmoke: SUCCESS
RESULT: SUCCESS
```

## No File Ingress

No File Ingress gilt im Diagnostics-Kontext als erfolgreich, wenn ein `NoFileIngressChecked`-Event mit Metadata `status=success` vorhanden ist.

Erwartete Metadata:

```text
guestHasPdfFile=false
guestHasOriginalPath=false
originalFileBytes=false
```

## Grenzen

- JSONL ist Development-Speicher, noch kein manipulationssicherer Audit-Speicher.
- Keine produktive Signatur oder Verschluesselung der Logs.
- Keine UI.
- Keine zentrale Log-Sammlung.
- Keine Cloud.
