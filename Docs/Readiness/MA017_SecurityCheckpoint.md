# MA017 Security Checkpoint

Status: Draft
Datum: 2026-07-07

## Zweck

Dieser Checkpoint schliesst AP691-700 ab.

MA017 besitzt damit eigene Security-/Policy-Regressionen, Reports und Pilot-Warnregeln.

## Abgedeckte Arbeitspakete

- AP691: Security Regression MA017 erweitern.
- AP692: Policy Regression MA017 erweitern.
- AP693: Audit Report MA017.
- AP694: No File Ingress Report MA017.
- AP695: UWB Privacy Report MA017.
- AP696: Security Warnings in Pilot.
- AP697: Emergency Return Test MA017.
- AP698: Stale Lease Startup Recovery MA017.
- AP699: Threat Model MA017 aktualisieren.
- AP700: Security Checkpoint MA017.

## Gate

```powershell
.\tools\run-ma017-security-checkpoint.ps1 -SmokeTest
```

Schneller Smoke innerhalb des Gesamt-Smokes:

```powershell
.\tools\run-ma017-security-checkpoint.ps1 -SmokeTest -SkipRegression
```

## Erwartete Marker

```text
MA017SecurityRegression: SUCCESS
MA017PolicyRegression: SUCCESS
MA017AuditReport: SUCCESS
MA017NoFileIngressReport: SUCCESS
MA017UwbPrivacyReport: SUCCESS
MA017EmergencyReturn: SUCCESS
MA017StaleLeaseRecovery: SUCCESS
MA017SecurityCheckpoint: SUCCESS
RESULT: SUCCESS
```

## Grenzen

Dieser Checkpoint fuehrt keine neue Produktivverschluesselung ein und ersetzt keine spaetere externe Security-Pruefung.

Er verifiziert den MA017-Pilotpfad:

- Original-Owned PDF Lifecycle
- No File Ingress
- Cross-Device Policy
- UWB/Dongle Privacy
- Stale Lease Recovery
- Emergency Return
- sichtbare Pilot-Warnungen
