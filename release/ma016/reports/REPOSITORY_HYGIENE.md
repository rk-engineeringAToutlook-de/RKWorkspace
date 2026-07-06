# MA016 Repository Hygiene

Status: Draft
Datum: 2026-07-07

## Scope

This report documents the MA016 repository hygiene safeguards for the pilot branch.

## Reviewed Areas

- Build artifacts: `bin/` and `obj/`.
- Temporary pilot state: `release/ma016/logs`, `release/ma016/context`, `release/ma016/temp`, `release/ma016/frame-cache`.
- Local developer identities and certificates.
- Local configuration files and redacted config output.
- Context pack secret scanning.
- Sample PDF data.

## Sample Data Review

`samples/Objects/Rechnung.pdf` is a minimal repository sample PDF.

Observed properties:

- File size: 666 bytes.
- Header: `%PDF-1.4`.
- One-page test object intended for lifecycle and frame smoke tests.
- No production customer content is intentionally stored in this sample.

## Safeguard Tools

- `tools/test-context-pack-no-secrets.ps1`
- `tools/redact-local-config.ps1`
- `tools/clean-build-artifacts.ps1`
- `tools/clean-pilot-artifacts.ps1`
- `tools/check-ma016-final-status.ps1`

## Gitignore Coverage

The root `.gitignore` excludes local security material, local pilot state, logs, local configs, frame cache and build artifacts. Tracked sample and handoff files remain intentionally versioned.

## Operating Rule

Before a handoff or owner package:

~~~powershell
.\tools\clean-build-artifacts.ps1
.\tools\clean-pilot-artifacts.ps1
.\tools\test-context-pack-no-secrets.ps1
.\tools\check-ma016-final-status.ps1
~~~

## Result

RepositoryHygiene: READY
