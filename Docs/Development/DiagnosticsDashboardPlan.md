# Diagnostics Dashboard Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP175 plant ein spaeteres Dashboard fuer Diagnose und Pilotbetrieb. Es ist kein Produkt-UI, sondern ein Labor- und Admin-Werkzeug.

## Panels

Sessions:

- aktive Sessions
- letzte Session
- Security Mode
- Peer-Ablage

Leases:

- aktive Leases
- Heartbeat
- Rueckgabe
- Recovery

Frames:

- aktive Frames
- FrameOnly
- GuestHasPdfFile
- No File Ingress

Security:

- PolicyDenied
- SecurityViolations
- revoked
- untrusted

Policy:

- aktives Profil
- CopyOut
- OwnershipTransfer
- SecureRequired
- AuditRequired

Proximity:

- naechste Ablage
- Quelle
- Confidence
- Override

No File Ingress:

- Cache Scope
- Original bytes
- Guest file check

## CLI-Vorstufe

`run-rkwp-diagnostics.ps1` liefert die erste Vorstufe:

```powershell
.\tools\run-rkwp-diagnostics.ps1 -AuditList
.\tools\run-rkwp-diagnostics.ps1 -Session
.\tools\run-rkwp-diagnostics.ps1 -Lease
.\tools\run-rkwp-diagnostics.ps1 -Violations
.\tools\run-rkwp-diagnostics.ps1 -ExportMarkdown diagnostics.md
```

## Nicht-Ziele

- keine Owner-Endanwender-GUI
- keine Cloud-Diagnostics
- keine automatischen Policy-Aenderungen
- keine Anzeige freier Originaldateien auf Guest-Ablagen
