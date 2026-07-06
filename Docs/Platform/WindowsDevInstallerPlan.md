# Windows Dev Installer Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP182 bereitet ein Windows Dev Package vor. Es ist kein produktiver Installer und installiert keinen Dienst.

## Inhalt

- Dev-Skripte
- Config Samples
- Policy Samples
- Lab Admin Guide
- Windows Agent Dev Plan
- keine Secrets
- keine lokalen Identitaeten
- keine privaten Keys

## Binaries

Binaries werden spaeter durch Build/CI erzeugt. Das aktuelle Package sammelt nur sichere Entwicklungsartefakte.

## Scripts

```powershell
.\tools\run-windows-agent-dev.ps1 -SmokeTest
.\tools\install-windows-agent-dev.ps1 -SmokeTest
.\tools\uninstall-windows-agent-dev.ps1 -SmokeTest
.\tools\run-config-tool.ps1 -SmokeTest
```

## Uninstall

Der Dev-Uninstall bleibt Stub, bis ein echter Autostart oder Service existiert. Er muss trotzdem dokumentieren:

- welche Dateien entfernt wuerden
- welche Config lokal bleibt
- welche Identitaeten nicht automatisch geloescht werden

## Smoke

```powershell
.\tools\package-windows-dev.ps1 -SmokeTest
```

Erwartung:

- Scripts vorhanden
- Config Samples vorhanden
- Critical Infrastructure Policy Pack vorhanden
- keine Secrets im Package
- `RESULT: SUCCESS`
