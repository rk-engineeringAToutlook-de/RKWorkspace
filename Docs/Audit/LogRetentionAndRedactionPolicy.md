# Log Retention And Redaction Policy

Status: Draft  
Datum: 2026-07-06

## Ziel

AP176 definiert erste Regeln fuer Log-Aufbewahrung und Redaction.

## Retention

Development Lab:

- kurze lokale Aufbewahrung
- manuelle Bereinigung erlaubt
- Export nur bewusst

Office:

- definierte Aufbewahrung je Organisation
- Audit exportierbar
- personenbezogene Daten minimieren

Critical Infrastructure:

- AuditRequired
- manipulationsarme Speicherung planen
- Export signieren oder hashbar machen
- Redaction vor Weitergabe

## Redaction

Redact:

- lokale Pfade
- AblageId bei externer Weitergabe
- Host/IP
- Zertifikatsdetails
- Personenbezug

Nicht redacten:

- EventType
- PolicyDecision
- NoFileIngress Ergebnis
- Recovery Status
- Zeitstempel grob

## Export

Markdown-Export darf nur Diagnose und Status enthalten. Keine Originaldaten, keine PDF-Inhalte, keine Screenshots, keine privaten Schluessel.

## Privacy

Logs beweisen Verhalten, nicht Benutzerueberwachung. Keine Bewegungsprofile, keine permanenten Raumkarten, keine Cloud-Sammlung.
