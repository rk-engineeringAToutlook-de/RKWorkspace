# RKWP Security Gate

Status: Prepared
Datum: 2026-07-05

## Ziel

Das RKWP Security Gate verhindert, dass ein unsicherer Development-Modus versehentlich als produktiv gilt.

Development ist erlaubt. Production ist strikt.

## Komponenten

Neue Protokolltypen:

- `RkwpSecurityConfiguration`
- `RkwpSecurityEnvironmentMode`
- `RkwpSecurityGate`
- `RkwpSecurityGateDecision`

## Konfiguration

`RkwpSecurityConfiguration` enthaelt:

- SecurityMode
- AllowDevelopmentInsecure
- RequireMutualAuthentication
- RequireEncryption
- RequireReplayProtection
- RequireAudit
- RequirePolicyBinding
- EnvironmentName

SecurityMode meint hier die Umgebung:

- Development
- Test
- Staging
- Production

Das ist bewusst getrennt von `RkwpSecurityMode`, der die konkrete Session-Sicherheit beschreibt, zum Beispiel `DevelopmentInsecure` oder `EncryptedAndAuthenticated`.

## Production Rules

Production erzwingt:

- kein `DevelopmentInsecure`
- SecureSessionRequired
- AuditRequired
- ReplayProtectionRequired
- PolicyBindingRequired
- MutualAuthenticationRequired
- EncryptionRequired

Wenn eine dieser Regeln verletzt wird, ist die Gate-Entscheidung `Allowed=false`.

## Development Rules

Development darf `DevelopmentInsecure` verwenden, aber nur mit sichtbarer Warnung.

Erlaubt:

- lokale Smokes
- Handoff-Tests
- Pipeline-Tests
- nicht-produktive Labors

Nicht erlaubt:

- Produktivbetrieb
- kritische Umgebung
- echte vertrauliche Daten
- stille Behandlung als sichere Session

## Test und Staging

Test darf `DevelopmentInsecure` fuer automatisierte nicht-produktive Checks erlauben.

Staging soll Production spiegeln. Jede Lockerung muss explizit, sichtbar und temporaer sein.

## Tests

Die RKWP-Tests pruefen:

- Production + DevelopmentInsecure schlaegt fehl.
- Development + DevelopmentInsecure ist erlaubt und erzeugt Warnung.
- Production ohne Audit schlaegt fehl.
- Production ohne ReplayProtection schlaegt fehl.
- Production ohne PolicyBinding schlaegt fehl.
- Test und Staging dokumentieren ihr Verhalten ueber Warnungen.

## Offene Punkte

- echte produktive Kryptografie
- produktive Key-Aushandlung
- produktiver Trust Store
- signierter/manipulationssicherer Audit-Speicher
- Policy-Migration mit Owner-Bestaetigung
- sichere Clock-/Timestamp-Strategie
