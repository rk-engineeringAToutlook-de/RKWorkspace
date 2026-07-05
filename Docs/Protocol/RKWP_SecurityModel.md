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

## Dev Transport Security

MA008.01 fuehrt `NamedPipeDev` als lokalen RKWP Dev-Transport ein. Dieser Transport ist ausschliesslich fuer Entwicklung und Smoke Tests gedacht.

Er darf nicht als produktiv gelten, weil ihm noch fehlen:

- produktive Verschluesselung
- gegenseitige Ablage-Authentisierung
- Session Keys
- produktiver Trust Store
- Pairing
- produktive Transport-Auditierung

Der Dev-Transport darf RKWP-Security-Felder durchreichen und testen, aber er ersetzt nicht den `RkwpSessionProtector`.

## Ablage Identity und Trust

MA008.02 fuehrt `AblageIdentity`, `AblageTrustLevel`, `AblagePairingState` und `AblageTrustPolicy` ein. `AblageHello` und `AblageCapabilities` muessen die Identitaet referenzieren. Eine technische Verbindung erzeugt keinen Trust.

Default-Regel:

- Unknown, Untrusted, Revoked und Denied bekommen keine Lease.
- PairingRequested und PairingPending blockieren bis zur Entscheidung.
- DevTrusted ist nur fuer DevelopmentInsecure Dev-Tests erlaubt.
- PolicyTrusted und EnterpriseTrusted duerfen nur gemaess aktiver Policy handeln.
- `RequireSecureSession` blockiert `DevelopmentInsecure`.

Details stehen in `Docs/Protocol/RKWP_AblageIdentityAndTrust.md` und `Docs/Protocol/RKWP_Pairing.md`.

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

## Input Channel Schutz

MA007.06 fuehrt eine erste policygebundene Input-Pruefung fuer Frames ein. `FrameInputEvent` muss an die aktive `CarryLease` und `FrameSession` gebunden sein. Falsche `LeaseId`, falsche `FrameSessionId`, nicht positive `SequenceNumber` oder nicht erlaubte Eingabearten werden abgelehnt und als `PolicyDenied` auditiert.

Die `FramePolicy` trennt Pointer, Scroll, Zoom, Keyboard, TextInput, Annotation, Clipboard, Extract und System Shortcuts. Dadurch kann eine kritische Umgebung einen Frame sichtbar machen, aber fast alle Eingaben unterbinden.

Der Input Channel bleibt ein Sicherheitsrisiko fuer spaetere Produktpfade. Produktive Umsetzungen muessen ihn mit Trust, Secure Session, Renderer-Sandboxing, Replay-Schutz und Audit koppeln.

## Audit, Revocation und Recovery

`RkwpAuditEvent`, `RkwpAuditTrail` und `IRkwpAuditSink` dokumentieren sicherheitsrelevante Entscheidungen. Mindestereignisse sind SessionStart, LeaseGrant, FrameOpen, FrameReturn, Replay, PolicyDenied, Revocation und Recovery.

MA008.08 ergaenzt Development-Persistenz mit JSONL unter `logs/rkwp-audit/`. `RkwpAuditLogRecord` enthaelt EventId, Timestamp, EventType, SessionId, LeaseId, FrameSessionId, ThingId, SourceAblageId, TargetAblageId, Severity, Message und Metadata. `RkwpSessionDiagnostics` fasst aktive Sessions, Leases, FrameSessions, Heartbeats, PolicyDenied, SecurityViolations, OwnershipTransferRequests, Revocations und No File Ingress zusammen.

Revocation erfolgt ueber `RkwpRevocationRequest`, `RkwpRevocationReason` und `RkwpRevocationResult`. Eine Revocation setzt Lease und FrameSession auf `Revoked`, invalidiert den Guest Frame, entsperrt den Owner logisch und schreibt Audit.

Recovery unterscheidet jetzt `LeaseExpired`, `RecoveredByOwner`, `Revoked`, `ConnectionLost` und `Returned`.

## Mindestanforderungen fuer Produktion

- verschluesselte Session
- gegenseitige Ablage-Authentifizierung
- produktives Pairing und Trust Store
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

## Ownership Transfer Gate

MA007.08 schaerft Ownership Transfer als separates Security Gate. Ohne ausdrueckliche Policy wird OwnershipTransfer abgelehnt. Nur `Approved` darf materialisieren. `RequiresUserConfirmation`, `RequiresAdapter`, `Denied` und `NotSupported` erzeugen keine neue Quelle der Wahrheit.

Objektarten bleiben relevant: `SettingsWindow` ist nicht uebernehmbar, `RemoteSession` braucht passende Handoff-Capabilities, und `MoveOwnership` braucht immer starke Bestaetigung.
