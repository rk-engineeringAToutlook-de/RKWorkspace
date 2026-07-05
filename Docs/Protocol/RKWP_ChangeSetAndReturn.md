# RKWP ChangeSet And Return

Dokument-ID: RKWS-RKWP-CHANGESET-001
Status: Draft
Datum: 2026-07-05

## Ziel

Ein Frame kann spaeter Eingaben erlauben, aber die Gastablage darf dadurch nicht automatisch Besitzerin oder Quelle einer neuen Datei werden.

`ChangeSet` beschreibt kontrollierte Aenderungen, die auf einer Gastablage entstehen und zur Originalablage zurueckgegeben werden. Die Originalablage entscheidet, was damit passiert.

## Grundsatz

Keine Aenderung wird stillschweigend uebernommen.

Standard:

- Original bleibt Owner.
- Gastablage erzeugt keine freie Datei.
- Bearbeitung erzeugt ein ChangeSet.
- Owner entscheidet ueber Accept, Reject, Review, Apply, NewVersion oder Fork.

## Modelle

`ChangeSet` enthaelt:

- ChangeSetId
- ThingId
- LeaseId
- FrameSessionId
- OwnerAblageId
- GuestAblageId
- BaseVersionId
- Operations
- CreatedAt
- SubmittedAt
- State
- PolicyId
- PolicyVersion
- PolicyHash
- OriginReference
- Conflicts

`ChangeSetOperationKind` enthaelt:

- AnnotationAdded
- AnnotationRemoved
- TextExtracted
- TextInserted
- FieldChanged
- SnapshotTaken
- MetadataChanged
- Unknown

## Zustaende

- Draft
- Submitted
- Accepted
- Rejected
- Conflict
- Applied
- Discarded

## Entscheidungen

- Accept
- Reject
- RequireReview
- CreateNewVersion
- ApplyToOriginal
- ForkVersion

`Reject` veraendert das Original nicht. `ForkVersion` und `CreateNewVersion` erzeugen eine `VersionReference`, lassen das Original aber unveraendert. `ApplyToOriginal` ist eine bewusste Owner-Entscheidung und darf nicht automatisch durch Gast-Interaktion entstehen.

## Konflikte

Vorbereitet sind:

- BaseVersionMismatch
- OwnerChangedDuringLease
- GuestSubmittedExpiredLease
- PolicyChanged
- UnsupportedOperation
- Unknown

Policy-Aenderungen nach Erstellung eines ChangeSets fuehren in V1 zu `RequireReview` und `Conflict`.

## Policy

`ChangeSetPolicy` entscheidet, ob eine Gastablage ueberhaupt Aenderungen erzeugen darf.

Kritisch-konservativ:

- ViewOnly erzeugt kein ChangeSet.
- Annotationen nur mit ausdruecklicher Policy.
- TextInserted und FieldChanged sind ohne Policy verboten.
- SnapshotTaken ist ohne Policy verboten.
- OwnerDecision bleibt Pflicht.

## PDF

PDF ist der erste vorbereitete Testpfad.

In V1 ist echte PDF-Modifikation noch nicht final. Annotationen werden als `ChangeSetOperationKind.AnnotationAdded` vorbereitet. Ein Owner kann spaeter entscheiden, ob diese Annotation verworfen, uebernommen, als neue Version oder als Fork behandelt wird.

## E-Mail

E-Mail-Entwuerfe koennen spaeter ueber Adapter ChangeSets erzeugen. Die Mail-Ablage bleibt Owner, bis ein Adapter und eine Policy eine andere Entscheidung erlauben.

## SettingsWindow

SettingsWindow ist kein normales uebernehmbares Ding. Aenderungen sind Eingaben im Originalsystem. ChangeSet ist dort hoechstens Audit/Protokoll, nicht Ownership Transfer.

## Tests

MA007.07 testet:

- ChangeSet kann erstellt werden.
- ChangeSet ohne Lease ist ungueltig.
- ViewOnly lehnt ChangeSet ab.
- Annotate erlaubt ChangeSet.
- Owner kann Accept entscheiden.
- Owner kann Reject entscheiden.
- Reject veraendert Original nicht.
- ForkVersion erzeugt VersionReference.
- Expired Lease lehnt ChangeSet ab.
- Unsupported Operation wird abgelehnt.
- PolicyChanged fuehrt zu RequireReview/Conflict.

## Offene Punkte

- echte PDF-Annotation schreiben
- echte Konfliktauflösung
- UI fuer Owner-Review
- Adapter fuer E-Mail und andere Objektarten
- Audit-Persistenz fuer ChangeSet-Entscheidungen
