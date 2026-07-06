# UWB Privacy and Consent

Status: MA016 foundation.

## Principle

UWB improves proximity. It does not decide ownership, copy data, or identify people.

## Allowed Data

- AblageId
- distance class
- optional distance in meters
- direction hint
- confidence
- provider status
- last seen timestamp

## Forbidden Data

- original file content
- PDF bytes
- personal tracking history
- precise location history outside the active lab session
- hidden background scanning without visible consent

## Consent Rules

- UWB is opt-in per lab surface.
- The user must be able to disable UWB.
- If consent is missing, provider status is `ConsentRequired`.
- Manual Map remains available as fallback.

## Retention

MA016 stores no long-term UWB movement history. Logs may contain short-lived diagnostics for test proof only.

## Result

UWB is permitted only as an Ablage proximity signal.
