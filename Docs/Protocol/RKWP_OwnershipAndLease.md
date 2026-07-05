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

Default fuer kritische Objekte:

- `OwnershipMode.FrameOnly`
- View, Scroll, Zoom, Return, Revoke
- keine Originaldatei auf dem Gast
- Heartbeat
- Grace Period
- Owner Recovery

## Recovery

Wenn Heartbeats fehlen oder eine Lease auslaeuft, muss der Owner das Ding wieder als autoritativ behandeln und den Gast-Frame invalidieren. Das ist kein visueller Fehlerfall, sondern eine Schutzregel.

## MA007.01 PDF FrameOnly Smoke

Die erste echte Datei im Test ist `samples/Objects/Rechnung.pdf`. Sie bleibt auf der Owner-Ablage, wird waehrend der Lease logisch gesperrt und erscheint auf der Gastablage nur als Frame-Repräsentation.

Der Smoke prueft:

- Owner bleibt Eigentuemer.
- Guest erhaelt keinen Originalpfad.
- Guest erhaelt keine PDF-Bytes als Datei.
- Rueckgabe setzt `Returned`.
- Heartbeat-Verlust fuehrt zu Recovery.
- Es gibt keinen unendlichen Lock.
