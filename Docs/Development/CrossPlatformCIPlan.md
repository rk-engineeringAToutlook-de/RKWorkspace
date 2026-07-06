# Cross Platform CI Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP187 plant CI fuer die Plattformen.

## Windows

- .NET build
- Unit/Integration
- RKWP tests
- Windows object adapter smoke
- Windows PDF Frame Pilot smoke
- Windows Dev Package smoke

## macOS

- macOS Surface Contracts
- Swift/Xcode Build spaeter
- Permissions checklist als Artifact
- macOS Guest Compatibility Harness

## Linux

- .NET build
- Protocol tests
- Linux plan validation
- keine GUI-Pflicht im ersten CI

## iOS spaeter

- Xcode build
- simulator build
- device test manuell
- TestFlight erst nach Signing Plan

## Android spaeter

- Gradle build
- unit tests
- emulator smoke spaeter

## Artifacts

- Codex Context ZIP
- Windows Dev Package
- test logs
- readiness summaries

## Nicht-Ziele

- keine Secrets in CI
- keine produktive Signatur im normalen PR-Build
- keine echten mobilen Geraete als Pflicht im ersten Gate
