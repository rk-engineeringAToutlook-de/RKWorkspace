# Policy Editor Plan

Status: Draft  
Datum: 2026-07-06

## Ziel

AP172 beschreibt, wie RK Workspace Policies spaeter editierbar werden, ohne dass Owner aus Versehen Sicherheitsregeln brechen.

## Profile

Critical:

- FrameOnly
- NoExtract
- NoOwnershipTransfer
- SecureRequired
- AuditRequired
- ShortLease

Office:

- FrameOnly default
- Scroll/Zoom erlaubt
- CopyOut nur mit Bestaetigung
- sichere Session erforderlich

Personal:

- vertraute eigene Ablagen
- Haptik erlaubt
- CopyOut nur bestaetigt
- No File Ingress bleibt Standard

Development:

- DevTransport erlaubt
- simulierte Proximity erlaubt
- sichtbare Warnungen
- nicht fuer Produktion

## Editor-Regeln

- Vorlagen statt freie JSON-Bearbeitung.
- Riskante Optionen brauchen Erklaertext.
- No File Ingress darf nicht still deaktiviert werden.
- Ownership Transfer bleibt default denied.
- Jede Aenderung erzeugt Audit.
- Validation muss vor Speichern laufen.

## Spaetere UI

Der Editor zeigt:

- Profil
- erlaubte Aktionen
- Frame-Modus
- Extraction
- Haptik
- Lease-Zeit
- Security-Anforderung
- Audit-Anforderung

## CLI-Vorstufe

Bis zur UI bleibt verbindlich:

```powershell
.\tools\run-policy-profile.ps1 -List
.\tools\run-policy-profile.ps1 -Show CriticalInfrastructure
.\tools\run-policy-profile.ps1 -Validate
.\tools\run-policy-profile.ps1 -SmokeTest
```
