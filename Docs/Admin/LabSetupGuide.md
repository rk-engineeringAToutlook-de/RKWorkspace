# Lab Setup Guide

Status: Draft  
Datum: 2026-07-06

## Ziel

AP179 gibt dem Owner eine Laboranleitung fuer den naechsten Pilot.

## Windows Owner

```powershell
.\tools\run-config-tool.ps1 -SmokeTest
.\tools\run-manual-map.ps1 -Validate
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
```

## macOS Guest

Vorbereitung:

- Context-Pack exportieren
- macOS Handoff lesen
- Accessibility/Screen Recording pruefen
- RKWP Client Flow vorbereiten

## iPad Guest

Vorbereitung:

- Xcode-Projektpfad lesen
- USB-Testplan lesen
- Local Network Permission beachten
- No File Ingress Sandbox pruefen

## Config

```powershell
.\tools\run-config-tool.ps1 -List
.\tools\run-config-tool.ps1 -CreateLocal
.\tools\run-config-tool.ps1 -UseProfile DevelopmentLab
.\tools\run-config-tool.ps1 -Redact
```

## Manual Map

```powershell
.\tools\run-manual-map.ps1 -Set -Ablage macOS -Direction Right -Distance Near
.\tools\run-manual-map.ps1 -Set -Ablage iPad -Direction Up -Distance Medium
.\tools\run-manual-map.ps1 -Show
```

## Policy

```powershell
.\tools\run-policy-profile.ps1 -List
.\tools\run-policy-profile.ps1 -Show CriticalInfrastructure
.\tools\run-policy-profile.ps1 -SmokeTest
```

## Logs

```powershell
.\tools\run-rkwp-diagnostics.ps1 -SmokeTest
.\tools\run-rkwp-diagnostics.ps1 -AuditList
.\tools\run-rkwp-diagnostics.ps1 -ExportMarkdown diagnostics.md
```

## Tests

Vor Pilot:

```powershell
.\tools\run-tests.ps1
.\tools\export-codex-context.ps1
```

Kein Push aus dem Labor ohne Owner-Entscheidung.
