# RK Workspace

# MA016 Go/No-Go Criteria

Status: Accepted  
Datum: 2026-07-07

## Go

- Windows Closed PDF Capsule Smoke ist gruen.
- Windows Open PDF Frame Smoke ist gruen oder klar als vorbereiteter Fallback dokumentiert.
- No File Ingress ist fuer Capsule und OpenFrame nachweisbar.
- Return und Recovery sind sichtbar, auditierbar und wiederholbar.
- macOS START HERE, Config und Contracts sind vollstaendig.
- iOS/iPad START HERE, Xcode/USB-Runbook, Config und Contracts sind vollstaendig.
- Manual Map und UWB-Simulator koennen als Proximity-Quelle genutzt oder klar simuliert werden.
- Context Pack enthaelt alle MA016-Handoffs.
- Keine `bin/obj`-Artefakte bleiben im Arbeitsbaum.

## No-Go

- Guest erhaelt eine freie PDF-Datei, Originalbytes oder Originalpfad.
- Owner verliert Kontrolle ueber Rueckgabe oder Recovery.
- Security-Dev-Modus wird als produktiv dargestellt.
- macOS/iOS-Handoff ist unvollstaendig.
- Pilot startet ohne dokumentierte Failure- und Recovery-Anweisungen.

## Entscheidung

Go/No-Go wird nicht nach Technik-Schoenheit entschieden, sondern danach, ob der Owner den naechsten echten Test sicher, nachvollziehbar und wiederholbar starten kann.
