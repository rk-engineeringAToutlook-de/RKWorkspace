# Windows Agent Installation Plan

Status: MA010.08

## Ziel

RK Workspace braucht spaeter einen lokalen Windows Agent. MA010.08 bereitet die Struktur vor, installiert aber keinen produktiven Dienst.

## Aktueller Dev-Pfad

Start:

```powershell
.\tools\run-windows-agent-dev.ps1
.\tools\run-windows-agent-dev.ps1 -SmokeTest
```

Vorbereitete Install-Scripts:

```powershell
.\tools\install-windows-agent-dev.ps1 -SmokeTest
.\tools\uninstall-windows-agent-dev.ps1 -SmokeTest
```

Diese Scripts fuehren keine produktive Installation aus. Sie dokumentieren und pruefen nur den Dev-Pfad.

## Spaetere Agent-Aufgaben

- AblageIdentity laden.
- RKWP Dev/Secure Transport hosten.
- Frame Owner Host bereitstellen.
- Frame Guest Surface bereitstellen.
- Proximity Provider anbinden.
- Glass Edge Surface anbinden.
- Audit Log schreiben.
- Policy Profile laden.
- Recovery ausfuehren.
- Context Reporting fuer Codex/Handoff erzeugen.

## Installationsphasen

1. Dev-Host ohne Installation.
2. Benutzerkontext mit Autostart.
3. Tray/Status spaeter.
4. Service-Prototyp optional.
5. produktive Installation erst nach Security- und Policy-Review.

## Smoke-Kriterien

`run-windows-agent-dev.ps1 -SmokeTest` prueft:

- Agent startet.
- AblageIdentity ist vorhanden.
- RKWP-Komponenten sind erreichbar/vorbereitet.
- keine Installation erforderlich.
- Shutdown ist sauber.
- `RESULT: SUCCESS`.
