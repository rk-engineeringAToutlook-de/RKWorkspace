# iOS TestFlight And Dev Install Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP184 beschreibt den Installationspfad fuer iPhone und iPad.

## USB Dev

Erster Pfad:

- Xcode
- USB-Geraet
- Development Team
- lokaler Build
- Local Network Permission
- Haptics testen
- No File Ingress Sandbox testen

## TestFlight spaeter

TestFlight kommt erst, wenn:

- RKWP Client stabil ist
- Frame Presenter stabil ist
- keine Originaldateien in die App sandboxen
- Logs redacted exportierbar sind
- Entitlements geklaert sind

## Provisioning

Benötigt:

- Bundle Identifier
- Team Provisioning Profile
- Device Registration fuer Dev
- spaeter App Store Connect/TestFlight

## Entitlements

Moegliche Entitlements:

- Local Network
- Bonjour, falls Dev-Discovery genutzt wird
- Keychain Access
- Haptics sind frameworkseitig, kein spezielles Entitlement

## Logs

Logs bleiben lokal, redacted, ohne PDF-Inhalte und ohne private Schluessel.

## Nicht-Ziele

- keine App Store Distribution
- keine Cloud-Sync
- keine Dateiablage als Original
