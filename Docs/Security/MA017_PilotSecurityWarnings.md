# MA017 Pilot Security Warnings

Status: Draft
Datum: 2026-07-07

## Zweck

Dieses Dokument beschreibt die sichtbaren Sicherheitswarnungen im MA017-Pilot.

MA017 darf im Lab mit Development- und Non-Production-Security laufen. Der Owner darf dabei aber niemals glauben, dass dieser Zustand produktionsreif ist.

## Warnungsarten

### Development Mode

Marker:

```text
SecurityModeWarning: NON_PRODUCTION_SECURITY
SecureDevWarning: DEV_ONLY_NOT_PRODUCTION
```

Bedeutung:

- Pilot laeuft in einer Entwicklungsumgebung.
- Transport und Identitaeten sind fuer Labortests geeignet.
- Dieser Modus ist kein Produktivbetrieb.

### Production Gate

Marker:

```text
SecurityModeWarning: NONE
OwnerPdfSafetyGuard: OK
```

Bedeutung:

- Der getestete Policy-Pfad erfuellt den Security-Gate-Anspruch fuer den jeweiligen Smoke-Test.
- Keine Development-Warnung darf in CriticalInfrastructure erwartet werden.

### Owner Safety Guard

Marker:

```text
OwnerPdfSafetyGuard: WARNINGS_PRESENT
```

Bedeutung:

- Der Owner-Pilot darf weiterlaufen.
- Der Report muss die Warnungen aber sichtbar auffuehren.

## UX-Regel

Warnungen duerfen nicht wie Fehler wirken, wenn der Pilot absichtlich im Labormodus laeuft.

Sie muessen aber klar verhindern, dass ein Lab-Pfad mit Produktiv-Sicherheit verwechselt wird.

## Mindestnachweis

```text
PilotSecurityWarnings: READY
DevelopmentWarnings: Visible
CriticalPolicyWarnings: None
RESULT: SUCCESS
```
