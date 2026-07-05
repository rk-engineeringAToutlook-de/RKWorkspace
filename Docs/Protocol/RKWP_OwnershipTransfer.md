# RKWP Ownership Transfer

Dokument-ID: RKWS-RKWP-TRANSFER-001
Status: Draft
Datum: 2026-07-05

## Zweck

Ownership Transfer ist nicht dasselbe wie Tragen. Tragen bedeutet in MA007.00: Der Owner bleibt Owner und der Gast erhaelt einen kontrollierten Frame.

Ein echter Ownership-Wechsel ist spaeter ein eigener, bestaetigter Vorgang.

## Moegliche Modi

- FrameOnly
- InteractiveFrame
- ExtractOnly
- CopyOut
- ForkVersion
- MoveOwnership
- SnapshotExport
- SessionHandoff
- NotTransferable

## Entscheidungen

`OwnershipTransferService.Decide` gibt zurueck:

- Approved
- Denied
- RequiresUserConfirmation
- NotSupported

CopyOut, ForkVersion und MoveOwnership duerfen nicht still passieren. Sie brauchen Policy und spaeter eine ausdrueckliche Benutzerentscheidung.

## Original Disposition

Wenn Ownership spaeter wirklich wechselt, muss festgelegt werden, was mit dem Original geschieht:

- RetainOriginal
- DeleteOriginal
- MarkAsMoved
- CreateTombstone
- CreateVersionLink
- KeepReadOnlyArchive
- RequireManualCleanup

MA007.00 nutzt fuer kritische Defaults `RetainOriginal`.
