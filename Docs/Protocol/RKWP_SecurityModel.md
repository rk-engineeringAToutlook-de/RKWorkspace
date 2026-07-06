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

MA010.02 fuehrt `LocalNetworkDev`/DevLan als LAN-faehiges Laborprofil ein. DevLan darf DevPairing, SessionId, Heartbeat, FrameUpdate und No-File-Ingress-Pruefungen tragen. Es bleibt `DevelopmentInsecure`: kein finales TLS, kein produktiver Trust Store, keine kritische Umgebung. Produktive Nutzung bleibt durch `RkwpSecurityGate` blockiert, bis Secure Session, Authentisierung, Integritaet, Verschluesselung und Audit produktionsnah implementiert sind.

## Security Gate

MA008.09 fuehrt `RkwpSecurityConfiguration` und `RkwpSecurityGate` ein. Dieses Gate trennt Umgebung und Session-Modus:

- Umgebung: `Development`, `Test`, `Staging`, `Production`
- Session-Modus: `DevelopmentInsecure`, `Authenticated`, `Encrypted`, `EncryptedAndAuthenticated`, `ProductionRequired`

Production erzwingt:

- kein `DevelopmentInsecure`
- SecureSessionRequired
- AuditRequired
- ReplayProtectionRequired
- PolicyBindingRequired
- MutualAuthenticationRequired
- EncryptionRequired

Development darf `DevelopmentInsecure` nur mit sichtbarer Warnung verwenden. Test darf ihn fuer nicht-produktive automatisierte Checks erlauben. Staging soll Production spiegeln und Lockerungen nur explizit dokumentieren.

Details stehen in `Docs/Security/RKWP_SecurityGate.md`.

MA010.09 fuehrt `RKWorkspace.Configuration` ein. Das Konfigurationssystem validiert `SecurityMode`, `SecureSessionRequired`, `AuditRequired` und die Policy-Kopplung. `DevelopmentInsecure` bleibt erlaubt fuer Lab-Smokes, wird aber als Warnung ausgegeben.

## Policy Profiles

MA009.07 fuehrt `RkwpPolicyProfile` ein. Profile buendeln Ownership, Frame, Extraction, OwnershipTransfer und Security-Konfiguration.

Profile:

- `CriticalInfrastructure`
- `OfficeDefault`
- `DevelopmentLab`
- `PresentationOnly`
- `TrustedPersonalDevices`

CriticalInfrastructure erzwingt SecureSession, Audit, FrameOnly, NoFileIngress und blockiert OwnershipTransfer. DevelopmentLab erlaubt DevelopmentInsecure nur mit Warnung und bleibt nicht-produktiv. Details stehen in `Docs/Policy/RKWP_PolicyProfiles.md`.

## Secure Session Spike

MA009.01 fuehrt `RkwpSecureSession` ein. Der Spike bildet die produktiv notwendige Struktur ab:

- gegenseitige Ablage-Authentisierung
- Handshake-State
- Dev-Zertifikate fuer lokale Tests
- Session-Key-Vorbereitung
- Policy-Bindung
- Replay-Schutz ueber Nonce und SequenceNumber
- Audit fuer Handshake und Authentisierung

Details stehen in `Docs/Protocol/RKWP_SecureSession.md`.

Wichtig: MA009.01 ist noch keine finale Produktkryptografie. Dev-Zertifikate und Dev-Signaturen sind strukturelle Entwicklungsnachweise. Production bleibt ueber `RkwpSecurityGate` gegen `DevelopmentInsecure` gesperrt.

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

Ab MA009.01 darf ein Transportprofil auch Development-Secure-Session-Felder tragen. Das bedeutet nicht, dass der Transport produktiv ist. Es bedeutet nur, dass Handshake, Identitaet, Policy-Bindung und Replay-Schutz schon gegen die RKWP-Modelle getestet werden koennen.

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

## Frame Cache Schutz

MA010.07 fuehrt `FrameCachePolicy` ein. Cache ist kein OwnershipTransfer und keine freie Dateiablage.

Default ist `MemoryOnly`. `TemporaryEncrypted` ist nur vorbereitet. `DevInspectable` ist nur in Development erlaubt und wird fuer `CriticalInfrastructure` blockiert.

Der Cache darf gerenderte Frames temporaer halten, muss aber bei FrameClose, Revocation oder Recovery loeschen. Er darf keine Original-PDF, keine Originalbytes, keinen Originalpfad und keine rekonstruierbare Originaldatei speichern.

## Ownership Transfer Gate

MA007.08 schaerft Ownership Transfer als separates Security Gate. Ohne ausdrueckliche Policy wird OwnershipTransfer abgelehnt. Nur `Approved` darf materialisieren. `RequiresUserConfirmation`, `RequiresAdapter`, `Denied` und `NotSupported` erzeugen keine neue Quelle der Wahrheit.

Objektarten bleiben relevant: `SettingsWindow` ist nicht uebernehmbar, `RemoteSession` braucht passende Handoff-Capabilities, und `MoveOwnership` braucht immer starke Bestaetigung.
