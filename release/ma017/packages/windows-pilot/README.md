# MA017 Windows Pilot Package

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Paket ist der Windows Owner Einstieg fuer MA017.

Windows bleibt Owner der Original-PDF. Andere Ablagen erhalten nur Kapsel oder Frame.

## Enthaltene Gates

- `tools/run-ma017-windows-pdf-pilot.ps1 -SmokeTest`
- `tools/run-ma017-windows-all-pdf-lifecycle.ps1 -SmokeTest`
- `tools/run-ma017-security-policy-pilot.ps1 -SmokeTest`
- `tools/run-ma017-security-checkpoint.ps1 -SmokeTest -SkipRegression`

## Nachweise

- `release/ma017/reports/windows-pdf-standard-report.md`
- `release/ma017/reports/no-file-ingress-report.md`
- `release/ma017/reports/security-checkpoint-report.md`
- `release/ma017/reports/human-experience-report.md`

## Ergebnis

```text
WindowsPilotPackage: READY
NoFileIngress: REQUIRED
OriginalOwnership: Owner
RESULT: SUCCESS
```
