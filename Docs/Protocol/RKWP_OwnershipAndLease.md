# RKWP Ownership And Lease

Dokument-ID: RKWS-RKWP-OWNERSHIP-001
Status: Draft
Datum: 2026-07-05

## Grundsatz

Ein digitales Ding besitzt einen Owner. In MA007.00 bleibt dieser Owner die Quelle der Wahrheit. Eine andere Ablage darf das Ding nur ueber einen `CarryLease` und eine `FrameSession` erleben.

## Ownership-Zustaende

Der Code liegt in:

```text
src/Protocol/RKWorkspace.Protocol/Ownership
```

Wichtige Zustaende:

- `OriginalOwned`
- `LeasedToGuest`
- `LockedOnOwner`
- `PresentedOnGuest`
- `InteractiveOnGuest`
- `ReturningToOwner`
- `ReturnedToOwner`
- `LeaseExpired`
- `LeaseRevoked`
- `RecoveredByOwner`

## Carry Lease

`CarryLease` beschreibt die zeitlich begrenzte Berechtigung, ein Ding als Frame auf einer Gastablage zu erleben.

Ab MA008.02 wird eine Lease nicht mehr nur aus Session und Policy erzeugt. Vorher muss das `AblageTrustGate` pruefen, ob die Guest-Ablage nach `AblageIdentity`, `AblageTrustLevel`, `AblagePairingState` und `AblageTrustPolicy` ueberhaupt berechtigt ist.

Default fuer kritische Objekte:

- `OwnershipMode.FrameOnly`
- View, Scroll, Zoom, Return, Revoke
- keine Originaldatei auf dem Gast
- Heartbeat
- Grace Period
- Owner Recovery
- SessionId-Bindung
- PolicyId und PolicyVersion
- optional geplanter PolicyHash

Unknown, Untrusted, Revoked, Denied und Pending blockieren die Lease. `DevTrusted` ist nur fuer lokale Dev-Tests erlaubt.

## Recovery

Wenn Heartbeats fehlen oder eine Lease auslaeuft, muss der Owner das Ding wieder als autoritativ behandeln und den Gast-Frame invalidieren. Das ist kein visueller Fehlerfall, sondern eine Schutzregel.

Ab MA007.03 unterscheidet Recovery:

- `LeaseExpired`
- `RecoveredByOwner`
- `Revoked`
- `ConnectionLost`
- `Returned`

Heartbeat-Verlust fuehrt nach Grace Period zu `RecoveredByOwner`. Security-Verletzungen und Policy-Aenderungen sollen ueber Revocation laufen.

## Binding-Regeln

Lease-bezogene Nachrichten sind nur gueltig, wenn `SessionId` und `LeaseId` zur aktiven Lease passen. Eine Return-Nachricht aus einer fremden Session oder ein Heartbeat mit falscher LeaseId ist ein Security-Fehler.

Policy-Binding bindet `PolicyId`, `PolicyVersion` und optional `PolicyHash` an CarryLease und FrameSession. Aendert sich die Policy waehrend einer aktiven Session, darf V1 nicht stillschweigend weiterlaufen: Audit `PolicyDenied` und kontrollierte Revocation oder neue Zustimmung sind erforderlich.

Ab MA009.01 prueft `RkwpSecureSession` diese Bindung ebenfalls. Eine Message aus einer fremden Session, eine Lease mit falscher SessionId oder eine PolicyId-/PolicyVersion-Abweichung wird als ungueltig bewertet. Damit haengt FrameOnly nicht nur an der UI, sondern an Session, Lease und Policy.

## Glass Edge Integration

MA007.04 nutzt die Glass Edge als Ausloeser fuer den FrameOnly-Pfad. Die Kante zeigt die naechste Ablage, die CarryLease bleibt an den Owner gebunden, und die Zielablage bekommt nur eine Frame-Darstellung. Rueckgabe setzt die Lease auf `Returned`; Recovery nach Heartbeat-Verlust fuehrt zu `RecoveredByOwner`.

MA007.05 verbessert den PDF-Frame-Smoke: Die Gastablage meldet eine sichere Frame-Repräsentation, aber keine PDF-Datei, keinen Originalpfad und keine kopierten PDF-Bytes. Recovery verhindert weiterhin einen unendlichen Lock auf der Owner-Ablage.

MA007.06 ergaenzt den Input Channel. Eingaben im Frame sind keine Besitzuebernahme. Sie werden an CarryLease und FrameSession gebunden, policygeprueft und bei Ablehnung auditiert. Annotationen und andere veraendernde Eingaben werden spaeter als ChangeSet zur Owner-Entscheidung vorbereitet, nicht als freie Datei auf der Gastablage.

MA007.07 fuehrt `ChangeSet` als Rueckgabemodell fuer kontrollierte Aenderungen ein. Der Guest kann damit nur einen Aenderungsvorschlag einreichen. `Reject` veraendert das Original nicht; `ForkVersion` erzeugt eine `VersionReference`; `ApplyToOriginal` bleibt eine bewusste Owner-Entscheidung.

MA007.09 bereitet Windows Object Adapter vor. Eine echte PDF-Datei wird als `PdfDocument` und `OriginReference` auf der Owner-Ablage registriert. Der Adapter erzeugt keine Guest-Datei und aendert Ownership nicht.

## MA007.01 PDF FrameOnly Smoke

Die erste echte Datei im Test ist `samples/Objects/Rechnung.pdf`. Sie bleibt auf der Owner-Ablage, wird waehrend der Lease logisch gesperrt und erscheint auf der Gastablage nur als Frame-Repräsentation.

Der Smoke prueft:

- Owner bleibt Eigentuemer.
- Guest erhaelt keinen Originalpfad.
- Guest erhaelt keine PDF-Bytes als Datei.
- Rueckgabe setzt `Returned`.
- Heartbeat-Verlust fuehrt zu Recovery.
- Es gibt keinen unendlichen Lock.
