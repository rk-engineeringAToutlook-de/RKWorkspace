# RKWP Secure Session

Dokument-ID: RKWS-RKWP-SECURE-SESSION-001  
Status: Draft  
Datum: 2026-07-06

## Ziel

MA009.01 fuehrt den ersten Secure-Session-Spike fuer RKWP ein. Der Spike ist noch keine finale produktive Kryptografie. Er legt aber die verbindliche Struktur fuer gegenseitige Ablage-Authentisierung, Session-Key-Vorbereitung, Replay-Schutz, Policy-Bindung und Audit fest.

## Grundsatz

Eine technische Verbindung ist kein Trust. Eine aktive RKWP Session wird nur akzeptiert, wenn:

- Owner-Ablage und Guest-Ablage identifiziert sind.
- beide Ablagen nicht untrusted oder revoked sind.
- Pairing nicht pending, denied oder revoked ist.
- beide Seiten im Handshake authentisiert wurden.
- die Session an PolicyId und PolicyVersion gebunden ist.
- Replay-Schutz aktiv ist.
- Audit-Ereignisse geschrieben werden.

## Modelle

Der Code liegt in:

```text
src/Protocol/RKWorkspace.Protocol/Security/RkwpSecureSession.cs
```

Wichtige Modelle:

- `RkwpSecureSession`
- `RkwpSecureSessionState`
- `RkwpKeyMaterial`
- `RkwpAblageCertificate`
- `RkwpDevCertificate`
- `RkwpSessionKey`
- `RkwpHandshake`
- `RkwpHandshakeState`
- `RkwpSignature`
- `RkwpSecurityPolicy`
- `RkwpSecurityValidationResult`

## State Machine

Der Secure-Session-Zustand kennt:

- `Uninitialized`
- `HandshakeStarted`
- `IdentityExchanged`
- `Authenticated`
- `SessionKeyEstablished`
- `Active`
- `Rejected`
- `Expired`
- `Revoked`
- `Failed`

Nur `Active` darf Frame-, Lease- oder Input-Nachrichten akzeptieren.

## Development Secure Session

`RkwpSecureSession.EstablishDevelopment(...)` erzeugt eine strukturell authentisierte Development-Session.

Dabei gilt:

- Dev-Zertifikate sind nur fuer Development/Test.
- Signaturen sind strukturelle SHA256-Nachweise, keine produktive PKI.
- Session Keys sind vorbereitete Fingerprints, keine finale Key-Exchange-Implementierung.
- `RkwpSecurityMode.Authenticated` wird fuer diesen Spike genutzt.
- Audit schreibt `HandshakeStarted`, `IdentityExchanged` und `SecureSessionAuthenticated`.

## Message Validation

Eine Nachricht wird gegen die Secure Session geprueft:

- `SessionId` muss zur aktiven Session passen.
- Header `policy-id` muss zur Session Policy passen.
- Header `policy-version` muss zur Session Policy passen.
- optionaler `CarryLease` muss dieselbe SessionId haben.
- `CarryLease` und `FrameSession` muessen dieselbe PolicyId und PolicyVersion besitzen.

Bei Abweichung ist die Nachricht nicht gueltig.

## Replay-Schutz

Der Spike nutzt weiter `RkwpSequenceValidator`:

- Nonce muss vorhanden sein.
- SequenceNumber muss positiv sein.
- SequenceNumber muss pro Session monoton steigen.
- Nonce darf pro Session nur einmal verwendet werden.

Replay erzeugt `ReplayDetected`.

## Policy Binding

`RkwpSecurityPolicy` bindet Secure Session, CarryLease und FrameSession an:

- `PolicyId`
- `PolicyVersion`
- SecureSessionRequired
- MutualAuthenticationRequired
- ReplayProtectionRequired
- AuditRequired

Wenn sich eine Policy waehrend einer aktiven Session aendert, darf die Session nicht still weiterlaufen. Der naechste Schritt ist entweder neue Bestätigung, Revocation oder PolicyVersion-Erhoehung.

## Was Noch Fehlt

Noch nicht produktiv implementiert:

- echte TLS- oder Noise-Session.
- echte Mutual Authentication mit Trust Store.
- echte Certificate Chain.
- echte Key Agreement Implementierung.
- persistenter produktiver Key Store.
- Pairing UI.
- Hardware-/Dongle-Root-of-Trust.

## Tests

`tools/run-rkwp-tests.ps1` prueft ab MA009.01:

- DevIdentity kann erzeugt werden.
- `.rkworkspace-dev/` ist in Git ignoriert.
- Mutual Dev Authentication ist erfolgreich.
- Untrusted Ablage wird abgelehnt.
- Revoked Ablage wird abgelehnt.
- Replay wird erkannt.
- fremde SessionId wird abgelehnt.
- LeaseId/SessionId-Mismatch wird abgelehnt.
- PolicyMismatch wird abgelehnt.
- Handshake erzeugt Audit.
