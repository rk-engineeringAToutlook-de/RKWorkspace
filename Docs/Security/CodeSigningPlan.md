# Code Signing Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP188 plant Signierung fuer alle Plattformen.

## Windows

- Authenticode fuer Installer/Binaries
- Zertifikat nicht im Repo
- CI nur mit sicherem Secret Store
- Dev Builds unsigniert oder testsigniert

## macOS

- Developer ID
- Hardened Runtime
- Notarization
- stapled ticket

## iOS

- Apple Development fuer USB
- TestFlight spaeter mit App Store Connect
- Entitlements dokumentieren

## Android

- debug keystore fuer Dev
- release keystore extern sichern
- Play/App Distribution spaeter

## Key Storage

- keine privaten Keys im Repo
- keine Keys im Codex-Chat
- Rotation dokumentieren
- Zugriff begrenzen

## CI

CI darf nur signieren, wenn:

- Branch/Tag erlaubt
- Secret Store verfuegbar
- Audit erzeugt
- Artifact Hash erzeugt

## Nicht-Ziele

- keine produktive Signatur in AP188
- kein Zertifikat erzeugen
- kein Secret in Context-Pack exportieren
