# RKWP Ablage Identity And Trust

Dokument-ID: RKWS-RKWP-IDENTITY-001
Status: Draft
Datum: 2026-07-05

## Ziel

MA008.02 fuehrt eine erste Ablage-Identitaet und ein Trust-Gate fuer RKWP ein. Eine Ablage darf nicht deshalb eine Lease bekommen, weil sie technisch erreichbar ist. Sie muss identifizierbar sein und von einer Policy erlaubt werden.

## Ablage Identity

Der Code liegt in:

```text
src/Protocol/RKWorkspace.Protocol/Identity
```

`AblageIdentity` beschreibt eine Ablage als Ort im Arbeitsraum:

- `AblageId`
- `DisplayName`
- `SurfaceType`
- `Platform`
- optionaler `PublicKey`
- `TrustLevel`
- `PairingState`
- `Capabilities`
- `CreatedAt`
- `LastSeen`
- `Metadata`

Diese Identitaet beschreibt keine Datei und kein Geraet als Mittelpunkt. Sie beschreibt die Ablage, auf der ein digitales Ding erscheinen darf.

## Trust Levels

`AblageTrustLevel` kennt:

- `Unknown`
- `Untrusted`
- `DevTrusted`
- `UserTrusted`
- `PolicyTrusted`
- `EnterpriseTrusted`
- `Revoked`

Default-Regel:

- `Unknown` darf nichts.
- `Untrusted` darf nichts.
- `Revoked` darf nichts.
- `DevTrusted` darf nur lokale Dev-Tests im unsicheren Development-Modus.
- `PolicyTrusted` und `EnterpriseTrusted` duerfen nur gemaess `AblageTrustPolicy`.

## Pairing State

`AblagePairingState` kennt:

- `Unpaired`
- `PairingRequested`
- `PairingPending`
- `Paired`
- `Denied`
- `Revoked`
- `Expired`

`PairingRequested` und `PairingPending` sind noch nicht erlaubt. Sie blockieren Leases, bis eine Policy oder ein Owner sie freigibt.

## Trust Policy

`AblageTrustPolicy` entscheidet, welche Handlung erlaubt ist:

- `AllowFrameOnly`
- `AllowInteractiveFrame`
- `AllowInput`
- `AllowExtract`
- `AllowOwnershipTransfer`
- `RequireSecureSession`
- `RequireUserConfirmation`
- `RequireAudit`

Das Trust-Gate ist konservativ. Wenn eine Eigenschaft nicht ausdruecklich erlaubt ist, wird sie abgelehnt.

## RKWP Integration

`AblageHello` und `AblageCapabilities` tragen ab MA008.02 Identitaetsdaten im Payload. Die Quelle muss zur RKWP-Session passen.

`CarryLease` wird nur vergeben, wenn:

- die Ablage nicht `Unknown`, `Untrusted` oder `Revoked` ist
- Pairing nicht pending, denied, revoked oder expired ist
- die angeforderte Ownership- oder Frame-Art zur Policy passt
- eine erforderliche sichere Session nicht durch `DevelopmentInsecure` ersetzt wird

## Dev Pairing

`DevAblagePairingService` ist ausdruecklich nur fuer lokale Entwicklung. Er erzeugt keine produktive Kopplung und keinen Trust Store.

Produktive Pairing-Pfade muessen spaeter:

- eine echte Ablage-Authentisierung nutzen.
- Trust-Entscheidungen persistent speichern.
- Revocation unterstuetzen.
- Secure Session erzwingen.
- echte Public Keys verwenden.
- eine gegenseitige Authentisierung erzwingen.
- Owner-Bestaetigung speichern.
- Revocation und Audit einschliessen.
- sichere Session Keys ableiten.

## Secure Session Bezug

MA009.01 bindet AblageIdentity an den Secure-Session-Handshake. Eine Dev-Ablage kann fuer lokale Tests ein Dev-Zertifikat erhalten. Dieses Zertifikat gehoert zur AblageId und darf nur in Development/Test verwendet werden.

Der Handshake wird abgelehnt, wenn:

- TrustLevel `Unknown`, `Untrusted` oder `Revoked` ist.
- Pairing `PairingRequested`, `PairingPending`, `Denied` oder `Revoked` ist.
- Zertifikat nicht zur AblageId passt.
- Dev-Zertifikate von der aktiven SecurityPolicy nicht erlaubt sind.

Details stehen in `Docs/Protocol/RKWP_SecureSession.md` und `Docs/Security/RKWP_DevCertificates.md`.

## Tests

Die RKWP-Protokolltests pruefen:

- Hello/Capabilities referenzieren die Identitaet.
- Unknown bekommt keine Lease.
- DevTrusted darf FrameOnly nur im DevMode.
- Untrusted wird abgelehnt.
- Revoked wird abgelehnt.
- PairingRequested wird Pending.
- PairingDenied verhindert Lease.
- Paired erlaubt gemaess Policy.
- SecureSessionRequired blockiert `DevelopmentInsecure`.

## MA011 Secure Session Path

AblageIdentity bleibt die Grundlage fuer gegenseitige Authentisierung. MA011.01 ergaenzt den Secure Session Path:

- Untrusted, Revoked, Denied und Pending bleiben vor Lease und Frame blockiert.
- DevelopmentAuthenticated darf nur strukturierte Dev-Identitaeten verwenden.
- ProductionSecure benoetigt spaeter produktive Zertifikate und Trust Store.
- Heartbeat und Revocation duerfen nur aus aktiven authentisierten Sessions kommen.

Die technische Detailregel steht in `Docs/Security/RKWP_SecureSessionPath.md`.
