# macOS Agent Installer Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP183 beschreibt die spaetere macOS Installation.

## App Bundle

Der macOS Agent soll als App Bundle oder Helper-App vorbereitet werden:

- RK Workspace Surface
- Frame Guest Presenter
- RKWP Client
- Diagnostics
- Config Loader

## Launch Agent

Ein Launch Agent kann spaeter Autostart und Hintergrundbetrieb uebernehmen. Der erste Realtest bleibt manuell startbar.

## Permissions

Zu pruefen:

- Accessibility
- Screen Recording
- Local Network
- Files and Folders, falls Objektadapter es brauchen

## Signing und Notarization

Produktpfad:

- Developer ID
- Hardened Runtime
- Notarization
- stapled ticket

Dev-Pfad:

- lokaler Build
- dokumentierte Gatekeeper-Schritte
- keine produktive Signaturannahme

## Config

macOS nutzt dieselben Profile:

- DevelopmentLab fuer Labortest
- OfficeDefault fuer spaeteren Pilot
- CriticalInfrastructure erst nach Security Review

## Nicht-Ziele

- kein produktiver Installer in AP183
- keine automatische Rechteerteilung
- kein Speichern freier Originaldateien auf Guest
