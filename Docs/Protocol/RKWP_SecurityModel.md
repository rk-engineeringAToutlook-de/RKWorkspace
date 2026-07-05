# RKWP Security Model

Dokument-ID: RKWS-RKWP-SECURITY-001
Status: Draft
Datum: 2026-07-05

## Ziel

RKWP muss spaeter produktiv gegen Replay, fremde Ablagen, Lease-Missbrauch und unerlaubte Materialisierung geschuetzt werden. MA007.00 implementiert noch keine produktive Kryptografie, legt aber die Pflichtfelder und die Schutzstelle fest.

## Sicherheitsmodi

`RkwpSecurityMode` definiert die erlaubten Session-Modi:

- `DevelopmentInsecure`
- `Authenticated`
- `Encrypted`
- `EncryptedAndAuthenticated`
- `ProductionRequired`

`DevelopmentInsecure` ist nur fuer Tests und lokale Pipeline-Smokes erlaubt. Kritische oder produktive Sessions muessen mindestens `EncryptedAndAuthenticated` oder `ProductionRequired` verlangen.

## Entwicklungsmodus

`DevelopmentRkwpSessionProtector` erzeugt ein nicht-produktives `DEV-` AuthTag. Es dient nur dazu, die Protocol-Pipeline lokal testbar zu machen.

Wichtig:

- Development AuthTags sind keine Security.
- Eine Session mit `SecureSessionRequired` darf nicht vom Development Protector erfuellt werden.
- Der Development Protector markiert sich selbst als Development-only und meldet, dass keine echte Verschluesselung und keine echte produktive Authentisierung stattfindet.
- Produktive Profile muessen spaeter Session-Schluessel, Authenticated Encryption und Replay-Schutz enthalten.

## Replay-Schutz

Jede RKWP-Nachricht besitzt `Nonce` und `SequenceNumber`. `RkwpSequenceValidator` erzwingt fuer eine Session:

- `SequenceNumber` steigt monoton.
- `Nonce` darf innerhalb der Session nicht wiederverwendet werden.
- fehlende Nonce ist ein Security-Fehler.
- fehlende oder nicht positive SequenceNumber ist ein Security-Fehler.
- Replay wird als `ReplayDetected` auditiert.

## Lease Binding und Policy Binding

Lease-bezogene Nachrichten muessen an `SessionId` und `LeaseId` gebunden sein. `RkwpLeaseBindingValidator` lehnt Heartbeat, Return oder Revocation ab, wenn Nachricht, Lease und FrameSession nicht zusammenpassen.

`PolicyId`, `PolicyVersion` und optional `PolicyHash` werden an `CarryLease` und `FrameSession` gebunden. Aendert sich die Policy waehrend einer aktiven Session, ist V1 konservativ: Die Abweichung wird als `PolicyDenied` auditiert und die Session muss kontrolliert revokiert oder mit neuer Zustimmung neu aufgebaut werden.

## Audit, Revocation und Recovery

`RkwpAuditEvent`, `RkwpAuditTrail` und `IRkwpAuditSink` dokumentieren sicherheitsrelevante Entscheidungen. Mindestereignisse sind SessionStart, LeaseGrant, FrameOpen, FrameReturn, Replay, PolicyDenied, Revocation und Recovery.

Revocation erfolgt ueber `RkwpRevocationRequest`, `RkwpRevocationReason` und `RkwpRevocationResult`. Eine Revocation setzt Lease und FrameSession auf `Revoked`, invalidiert den Guest Frame, entsperrt den Owner logisch und schreibt Audit.

Recovery unterscheidet jetzt `LeaseExpired`, `RecoveredByOwner`, `Revoked`, `ConnectionLost` und `Returned`.

## Mindestanforderungen fuer Produktion

- verschluesselte Session
- gegenseitige Ablage-Authentifizierung
- Nonce/SequenceNumber-Replay-Schutz
- Lease-spezifische Berechtigungen
- Revocation fuer Frame und Carry Lease
- Audit fuer Ownership-Entscheidungen
- Policy Binding fuer aktive Leases und Frames

## Original-Owned Schutz

Das wichtigste Security-Ziel ist nicht Verschluesselung allein. Das wichtigste Ziel ist: Der Gast darf nicht unbemerkt zur neuen Quelle der Wahrheit werden.

Deshalb gilt:

- FrameOnly ist Default.
- Guest Frame enthaelt keine Originaldatei.
- CopyOut, ForkVersion und MoveOwnership brauchen spaeter explizite Bestaetigung und Policy.
- Lease-Verlust fuehrt zur Owner-Recovery.
