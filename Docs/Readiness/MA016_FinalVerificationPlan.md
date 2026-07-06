# MA016 Final Verification Plan

Status: Accepted
Datum: 2026-07-07

## Ziel

Dieser Plan definiert die finale MA016-Verifikation vor dem Uebergang zu MA017.

## Scope

Geprueft werden:

- RKWP Protocol Tests.
- PDF Frame Smoke.
- Windows PDF Frame Pilot.
- Windows PDF Lifecycle Closed/Open.
- No File Ingress.
- Cross-Device Session Monitor.
- Cross-Device Diagnostics und Audit.
- Cross-Device Security und Policy Regression.
- MA016 Pilot Lab.
- Owner Test Package.
- Context Pack Secret Scan.
- Local Config Redaction.
- Build- und Pilot-Artefakt-Cleanup.
- Manual Map.
- RKWP Chaos.
- RKWP Performance.
- RKWP Load.

## Pflichtbefehle

~~~powershell
.\tools\run-ma016-smoke.ps1
.\tools\clean-build-artifacts.ps1
.\tools\clean-pilot-artifacts.ps1
.\tools\export-codex-context.ps1
.\tools\test-context-pack-no-secrets.ps1
.\tools\check-ma016-final-status.ps1
~~~

## Erfolgskriterien

- Alle Builds: 0 Warnungen, 0 Fehler.
- `MA016Smoke: SUCCESS`.
- `RKWP Performance`: SUCCESS.
- `RKWP Load`: SUCCESS.
- Context Pack: SUCCESS.
- Secret Scan: 0 Findings.
- Final Status Guard: sauber nach Commit.
- Keine `bin/` oder `obj/` Artefakte.

## No-Go Kriterien

- Gastablage erhaelt Originaldatei, Originalpfad oder kopierte PDF-Bytes.
- Secret Scan findet ein echtes Geheimnis.
- Build- oder Load-Test scheitert.
- Final Status Guard meldet lokale Secrets oder Build-Artefakte.

