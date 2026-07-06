# Install Readiness

Status: Draft  
Datum: 2026-07-06

## Ziel

AP190 fasst den Installationsstatus zusammen.

## Windows

- Dev Agent Modi vorhanden.
- Dev Package Script vorhanden.
- produktiver Installer fehlt.
- Service bleibt spaeterer Pfad.

## macOS

- Installer Plan vorhanden.
- App Bundle, Launch Agent, Permissions, Signing und Notarization sind geplant.
- echte App noch offen.

## iOS

- USB Dev und TestFlight Plan vorhanden.
- Xcode-Projekt bleibt Handoff.
- Entitlements/Provisioning offen.

## Android

- Kotlin/RKWP/Haptics/Frame/NoFileIngress Plan vorhanden.
- echte App offen.

## Linux

- .NET Agent und UI-Optionen geplant.
- Wayland/X11 Risiken dokumentiert.

## Signing

- Code Signing Plan vorhanden.
- keine produktiven Keys im Repo.
- CI-Signierung nur spaeter mit Secret Store.

## Updates

- Update Mechanism Plan vorhanden.
- signed updates, rollback und policy gates beschrieben.
- kein Auto-Updater implementiert.

## Readiness

Bereit fuer Dev-Package- und Plattform-Handoff-Arbeit. Noch nicht bereit fuer produktive Installation.
