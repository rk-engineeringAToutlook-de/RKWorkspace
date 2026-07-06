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
- RequiresPolicyApproval
- RequiresTransformation
- RequiresAdapter
- NotSupported

CopyOut, ForkVersion und MoveOwnership duerfen nicht still passieren. Sie brauchen Policy und spaeter eine ausdrueckliche Benutzerentscheidung.

Ab MA007.08 gilt konservativ:

- Default denied, solange Policy OwnershipTransfer nicht ausdruecklich erlaubt.
- CopyOut/ForkVersion koennen bei PDF erlaubt werden.
- MoveOwnership braucht immer starke Bestaetigung.
- SettingsWindow ist `NotSupported`.
- RemoteSession braucht passende TargetCapabilities fuer SessionHandoff.
- Denied oder NotSupported materialisiert nichts.

## Request

`OwnershipTransferRequest` beschreibt:

- RequestId
- ThingId
- CurrentOwnerAblageId
- RequestedNewOwnerAblageId
- RequestedMode
- RequestedDisposition
- RequestedBy
- Reason
- TargetCapabilities
- RequestedAt
- PolicyId

Kompatibilitaetsaliasse `OwnerAblageId` und `GuestAblageId` bleiben fuer bestehende Tests erhalten.

## Decision

`OwnershipTransferDecision` enthaelt:

- DecisionId
- RequestId
- Approved/Denied
- RequiresUserConfirmation
- RequiresPolicyApproval
- RequiresTransformation
- RequiresAdapter
- Reason
- ApprovedMode
- OriginalDisposition
- CreatedAt

Eine Decision ist noch keine Materialisierung. Erst eine Approved-Decision darf eine kontrollierte Materialisierung erzeugen.

## Materialization

`MaterializationResult` beschreibt:

- MaterializedThingId
- TargetAblageId
- TargetLocation
- Mode
- NewOwnerAblageId
- OriginalDisposition
- VersionReference
- CreatedAt

Bei `CopyOut`, `ForkVersion` und `SnapshotExport` entsteht ein kontrolliertes neues Ding auf der Zielablage. Bei `MoveOwnership` wechselt der Owner nur nach Approved-Decision. Bei `SessionHandoff` entsteht keine normale Datei.

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

MA007.08 prueft zusaetzlich `MarkAsMoved` und `CreateVersionLink`. `CreateVersionLink` erzeugt eine `VersionReference`, damit Original und neue Version nachvollziehbar verbunden bleiben.

## Policy Profiles

MA009.07 macht OwnershipTransfer profilabhaengig:

- `CriticalInfrastructure`: blockiert CopyOut, ForkVersion und MoveOwnership.
- `PresentationOnly`: blockiert OwnershipTransfer.
- `OfficeDefault`: CopyOut ist nur mit ausdruecklicher Bestaetigung vorbereitet.
- `TrustedPersonalDevices`: CopyOut ist optional, aber ebenfalls bestaetigungsgebunden.
- `DevelopmentLab`: kein produktiver Transferpfad; DevMode dient nur Tests.

Das Tool `tools/run-policy-profile.ps1 -SmokeTest` prueft diese Regeln.

## UX-Regel

Wenn spaeter wirklich Besitz uebernommen wird, darf der Frame visuell nicht hart verschwinden. Der Schutzrahmen loest sich auf und das Ding bleibt an derselben Stelle sichtbar. In MA007.08 ist das nur Modell und Dokumentation, keine finale Visualisierung.
