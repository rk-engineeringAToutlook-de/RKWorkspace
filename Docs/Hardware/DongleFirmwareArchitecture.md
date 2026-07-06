# Dongle Firmware Architecture

Status: Draft  
Datum: 2026-07-06

## Ziel

AP156 beschreibt die spaetere Firmware-Architektur fuer den Ablage Anchor Dongle. Sie bleibt absichtlich schlank, weil der Dongle keine Nutzdaten transportiert.

## Module

Geplante Module:

- Boot und Version
- Identity
- Beacon
- USB Control
- Proximity Sensor
- Trust Hint
- Firmware Update
- Diagnostics
- Recovery

## Boot

Beim Start meldet die Firmware:

- Firmware-Version
- Hardware-Revision
- DongleId
- Pairing-/Provisioning-Status
- Health-Status

## Identity

Identity ist lokal eindeutig und spaeter mit einer AblageId gekoppelt. Die Firmware darf kein Original-Objekt besitzen und keine Ownership-Operation ausfuehren.

## Beacon

Beacon sendet nur minimale Presence-Daten:

- Dongle alias
- Beacon version
- Proximity hint
- ephemeral nonce oder rotating id fuer spaetere Privacy

## USB Control

USB Control ist fuer lokale Diagnose und Provisioning vorgesehen. Kein Massenspeicher, kein Dateiinhalt.

## Update

Produkt-Firmware benoetigt spaeter:

- signierte Updates
- Rollback-Schutz
- Recovery-Mode
- Version Reporting

Dieser Block implementiert noch keine Firmware. Er friert nur die Architekturgrenzen ein.

## Security

Der Dongle kann Trust unterstuetzen, aber nicht ersetzen:

- Pairing bleibt RKWP/Surface-Aufgabe.
- Revocation bleibt Policy-Aufgabe.
- CarryLease bleibt Protocol-Aufgabe.
- FrameSession bleibt Protocol-Aufgabe.
