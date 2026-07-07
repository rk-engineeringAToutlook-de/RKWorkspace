# RKWP Policy Profiles

Status: Accepted  
Datum: 2026-07-06

## Ziel

RK Workspace verwendet Policy-Profile, damit Umgebung und Risiko nicht jedes Mal einzeln zusammengesetzt werden muessen.

Ein Profil buendelt:

- OwnershipPolicy
- FramePolicy
- ExtractionPolicy
- OwnershipTransferPolicy
- SecurityConfiguration
- LeaseTimeout
- Development-/Simulationserlaubnis
- PDF-Lifecycle-Freigaben fuer Capsule, OpenFrame und KeepCapsule

## Profile

### CriticalInfrastructure

- FrameOnly
- NoFileIngress
- NoOwnershipTransfer
- NoExtract by default
- AuditRequired
- SecureSessionRequired
- Revocable
- kurzer LeaseTimeout
- Capsule erlaubt
- OpenFrame erlaubt
- KeepCapsule verboten

### OfficeDefault

- FrameOnly default
- ExtractText optional
- CopyOut nur mit Bestaetigung
- Audit optional
- SecureSession preferred/required im aktuellen Modell
- Capsule erlaubt
- OpenFrame erlaubt
- KeepCapsule verboten

### DevelopmentLab

- DevMode erlaubt
- Simulated Proximity erlaubt
- DevelopmentInsecure erlaubt mit Warnung
- NoFileIngress bleibt testbar
- Capsule erlaubt
- OpenFrame erlaubt
- KeepCapsule erlaubt, aber nur fuer Labortests

### PresentationOnly

- ViewOnly
- NoInput
- NoExtract
- NoOwnershipTransfer
- Capsule erlaubt
- OpenFrame verboten
- KeepCapsule verboten

### TrustedPersonalDevices

- FrameOnly default
- InteractiveFrame erlaubt
- CopyOut optional mit Bestaetigung
- Haptics erlaubt
- OwnershipTransfer spaeter nur nach Policy/Bestaetigung
- Capsule erlaubt
- OpenFrame erlaubt
- KeepCapsule erlaubt

## PDF Lifecycle Policy Fields

Im technischen Modell heisst das Kapsel-Feld `AllowFrameCapsule`.
Ab MA017 ist `AllowCapsule` als Alias vorhanden, damit die Owner-nahe Sprache eindeutig bleibt.

| Feld | Bedeutung |
| --- | --- |
| `AllowCapsule` / `AllowFrameCapsule` | Kapsel darf erzeugt und geoeffnet werden. |
| `AllowOpenFrame` | Geoeffnete PDF darf als OpenFrame sichtbar werden. |
| `AllowKeepCapsule` | Kapsel darf nach CloseFrame auf der Gastablage bleiben. |

Security-Regel:

- CriticalInfrastructure: Capsule und OpenFrame ja, KeepCapsule nein.
- TrustedPersonalDevices: Capsule, OpenFrame und KeepCapsule ja, aber NoFileIngress bleibt Pflicht.
- PresentationOnly: Capsule ja, OpenFrame und KeepCapsule nein.

## Tool

```powershell
.\tools\run-policy-profile.ps1 -List
.\tools\run-policy-profile.ps1 -Show CriticalInfrastructure
.\tools\run-policy-profile.ps1 -Validate
.\tools\run-policy-profile.ps1 -SmokeTest
```

Ab MA010.09 validiert zusaetzlich das zentrale Configuration Tool Policy-Samples:

```powershell
.\tools\run-config-tool.ps1 -SmokeTest
```

Das Sample `config/samples/policy-critical.sample.json` prueft `CriticalInfrastructure` mit `NoFileIngress: true` und `OwnershipTransferAllowed: false`.

## Smoke-Test

Der Smoke-Test prueft:

- CriticalInfrastructure blockiert OwnershipTransfer.
- CriticalInfrastructure erfordert SecureSession.
- PresentationOnly blockiert Input.
- DevelopmentLab erlaubt DevMode mit Warnung.
- OfficeDefault erlaubt CopyOut nur mit Bestaetigung.
- alle Profile validieren erfolgreich.

## Default-Empfehlung

Fuer unbekannte oder industrielle Umgebungen ist `CriticalInfrastructure` der sichere Default.

Fuer normale Bueroarbeit ist `OfficeDefault` brauchbar, solange CopyOut ausdruecklich bestaetigt wird.

Fuer lokale Entwicklung ist `DevelopmentLab` erlaubt, aber nie produktiv.
