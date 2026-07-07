# macOS Handoff Checkpoint - MA017

Status: Verified
Datum: 2026-07-07

## Ergebnis

AP631-640 sind vorbereitet:

- macOS START HERE final.
- Apple Swift/RKWP Bundle final.
- macOS Minimal Guest App Task final.
- macOS Config Sample final.
- Capsule Contract final.
- OpenFrame Contract final.
- No File Ingress Contract final.
- Pilot Runbook final.
- Build-/Permissions-Checkliste final.
- Windows-Smoke fuer macOS Handoff gruen.

## Gate

~~~powershell
.\tools\run-ma017-macos-handoff.ps1 -SmokeTest
~~~

## Naechster nativer Schritt

Auf macOS wird mit Xcode aus dem Swift Bundle eine minimale Guest Ablage gebaut.
Windows bleibt Owner; macOS zeigt nur Frame/Kapsel.
