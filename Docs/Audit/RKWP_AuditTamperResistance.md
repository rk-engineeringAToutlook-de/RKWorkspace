# RKWP Audit Tamper Resistance

Status: MA013.28 roadmap  
Datum: 2026-07-06

## Ziel

Audit Logs duerfen nicht leicht manipulierbar sein.

## Roadmap

- Hash Chain pro Log.
- Signatur pro Segment.
- Append-only Speicher.
- Rotation.
- Retention.
- Redaction fuer Datenschutz.
- Export fuer Diagnose.
- Remote Audit optional fuer Enterprise.

## Hash Chain

Jeder Record enthaelt Hash des vorherigen Records. Manipulation bricht die Kette.

## Signing

Segmente koennen mit AblageIdentity signiert werden. Private Keys liegen im Plattform-Secret-Store.

## Redaction

Audit darf keine Originaldatei, keine Originalbytes und keine unnoetigen Pfade enthalten.

## Produktregel

Audit ist Sicherheitsnachweis, nicht Telemetrie-Sammelstelle.
