# RKWP Emergency Return

Status: MA013.25 protocol deepening  
Datum: 2026-07-06

## Ziel

Der Owner muss ein Ding jederzeit zurueckholen koennen.

## Begriffe

- EmergencyReturn: Owner fordert sofortige Rueckgabe an.
- OwnerRevocation: Owner beendet Lease aktiv.
- GuestInvalidation: Guest darf Frame nicht mehr anzeigen.
- FrameKillSignal: technische Invalidierung der FrameSession.

## Ablauf

1. Owner entscheidet Revocation.
2. Owner sendet `CarryLeaseRevoked`.
3. Guest invalidiert Frame.
4. Guest leert MemoryOnly Cache.
5. Owner entsperrt Original.
6. Audit schreibt Revocation und No File Ingress.

## Regeln

- Guest darf keine lokale PDF behalten.
- Guest darf keine weiteren Eingaben senden.
- Owner darf Original wieder verfuegbar machen.
- Recovery darf Revocation nicht rueckgaengig machen.

## Tests

Bestehende RKWP-Tests decken Revocation, revoked Ablage, expired Lease, No File Ingress und Audit ab. AP129 buendelt diese in einer Security Regression Suite.
