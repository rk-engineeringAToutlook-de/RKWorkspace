# MA017 PDF Lifecycle Threat Model

Status: Accepted
Datum: 2026-07-07

## Scope

Dieses Threat Model betrachtet Closed PDF Capsule und Open PDF Frame im MA017 Real-Pilot.

## Assets

- Original-PDF auf der Owner-Ablage.
- Frame-Kapsel ohne Datei-Ingress.
- OpenFrame-Sitzung ohne Originalpfad auf der Gastablage.
- Audit-Trail fuer Return, Recovery und Policy-Denials.
- Policy-Profile fuer CriticalInfrastructure und TrustedPersonalDevices.

## Risiken

### Datei-Ingress auf Gastablage

Risiko: Die Gastablage erhaelt eine freie PDF-Datei, Originalbytes oder einen Originalpfad.

Kontrolle:

- `CapsuleNoFileIngress: SUCCESS`.
- `OpenFrameNoFileIngress: SUCCESS`.
- `GuestHasPdfFile: NO`.
- `GuestHasOriginalPath: NO`.
- `GuestHasCopiedPdfBytes: NO`.

### Unauthorized Capsule Open

Risiko: Eine Kapsel wird ohne gueltige Policy oder gueltigen Owner-Kontext geoeffnet.

Kontrolle:

- `UnauthorizedCapsuleOpen: DENIED`.
- Policy-Denial wird auditiert.

### Unauthorized OpenFrame Input

Risiko: Eine Gastablage schreibt, tippt oder annotiert in einen OpenFrame, obwohl die Policy dies nicht erlaubt.

Kontrolle:

- `UnauthorizedOpenFrameInput: DENIED`.
- `RkwpAuditEventType.PolicyDenied`.
- `FramePolicy.CriticalViewOnly` blockiert nicht erlaubte Eingaben.

### KeepCapsule ohne Policy

Risiko: Eine Gastablage behaelt die Kapsel nach dem Schliessen.

Kontrolle:

- `KeepCapsulePolicy: DENIED` fuer CriticalInfrastructure.
- `AllowKeepCapsule` nur in erlaubten Profilen.

### Stale Lease oder verlorene Verbindung

Risiko: Eine ausgeliehene Kapsel bleibt haengen.

Kontrolle:

- `ExpiredCapsule: RECOVERED_BY_OWNER`.
- `Recovery: SUCCESS`.
- Owner bleibt Originalbesitzer.

### Cross-Device Replay oder fremde Session

Risiko: Ein Cross-Device Pilotpfad akzeptiert Nachrichten aus einer fremden Session oder eine alte Sequenz.

Kontrolle:

- `run-ma017-security-regression.ps1` bindet die bestehende RKWP Security Regression ein.
- Secure Session, Lease Binding und Policy Binding bleiben Pflicht.
- Cross-Device Audit muss `CrossDeviceAuditEventsPresent: SUCCESS` zeigen.

### Proximity/UWB Privacy

Risiko: UWB, BLE oder Dongle-Simulation erzeugen Bewegungsprofile oder transportieren Nutzdaten.

Kontrolle:

- `PrivacyMode: EphemeralLab`.
- Beacon-IDs sind temporaer fuer Labortests.
- Proximity liefert nur Richtung, Distanzklasse, optionale Meterdistanz und Confidence.
- Keine PDF-Bytes, keine Originalpfade und keine personenbezogene Bewegungshistorie.

### Pilot-Warnungen werden uebersehen

Risiko: DevelopmentLab oder TrustedPersonalDevices werden als produktionsreif missverstanden.

Kontrolle:

- `SecurityModeWarning: NON_PRODUCTION_SECURITY`.
- `SecureDevWarning: DEV_ONLY_NOT_PRODUCTION`.
- `OwnerPdfSafetyGuard: WARNINGS_PRESENT`.
- CriticalInfrastructure-Pfade erwarten `SecurityModeWarning: NONE`.

## Security Gate

~~~powershell
.\tools\run-ma017-security-policy-pilot.ps1 -SmokeTest
.\tools\run-ma017-security-checkpoint.ps1 -SmokeTest
~~~

## Entscheidung

Der PDF Lifecycle bleibt FrameOnly und Original-Owned.
Besitzuebergang ist nicht Bestandteil dieses Pilotpfads.

## MA017 Reports

- `release/ma017/reports/security-regression-report.md`
- `release/ma017/reports/policy-regression-report.md`
- `release/ma017/reports/security-audit-report.md`
- `release/ma017/reports/no-file-ingress-report.md`
- `release/ma017/reports/uwb-privacy-report.md`
- `release/ma017/reports/emergency-return-report.md`
- `release/ma017/reports/stale-lease-recovery-report.md`
- `release/ma017/reports/security-checkpoint-report.md`
