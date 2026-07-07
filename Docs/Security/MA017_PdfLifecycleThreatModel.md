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

## Security Gate

~~~powershell
.\tools\run-ma017-security-policy-pilot.ps1 -SmokeTest
~~~

## Entscheidung

Der PDF Lifecycle bleibt FrameOnly und Original-Owned.
Besitzuebergang ist nicht Bestandteil dieses Pilotpfads.
