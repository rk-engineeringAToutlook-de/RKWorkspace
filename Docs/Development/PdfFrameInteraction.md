# PDF Frame Interaction

Status: Draft  
Datum: 2026-07-05

## Ziel

MA008.05 fuehrt erste kontrollierte Interaktion im PDF-Frame ein:

- Scroll
- Zoom
- einfache Annotation als ChangeSet

Die Guest-Ablage besitzt die PDF nicht. Sie interagiert nur mit der aktiven `FrameSession`.

## Umsetzung

Code:

```text
src/Frame/RKWorkspace.Frame.Pdf/PdfFrameInteractionService.cs
```

Der Service erzeugt und validiert:

- `Scroll` als `FrameInputType.Scroll`
- `Zoom` als `FrameInputType.Zoom`
- `AnnotationStart`
- `AnnotationUpdate`
- `AnnotationEnd`
- `ChangeSetOperationKind.AnnotationAdded`

## Annotation

Eine Annotation ist in V1 eine einfache Rechteck-/Textnotiz-Simulation. Sie enthaelt:

- Position
- Seite
- Text optional
- Farbe optional
- CreatedBy GuestAblage
- LeaseId
- FrameSessionId

Das Ergebnis ist ein `ChangeSet`. Es wird nicht direkt in die PDF geschrieben.

## Owner-Entscheidung

Der Owner entscheidet spaeter:

- Accept / Apply
- Reject
- RequireReview
- CreateNewVersion
- ForkVersion

Fuer V1 prueft der Smoke, dass Apply zu `Applied` fuehren kann und Reject das Original unveraendert laesst.

## Policy

- `FramePolicy.InteractiveView` erlaubt Scroll und Zoom.
- `FramePolicy.Annotate` erlaubt Annotation.
- `FramePolicy.CriticalViewOnly` lehnt Annotation ab.
- `ChangeSetPolicy.ViewOnly` erzeugt kein ChangeSet.
- `ChangeSetPolicy.Annotate` erlaubt `AnnotationAdded`.

## No File Ingress

Auch nach Scroll, Zoom und Annotation gilt:

- keine PDF-Datei auf Guest
- kein Originalpfad auf Guest
- keine Originalbytes auf Guest

## Teststatus

`tools/run-rkwp-tests.ps1` prueft:

- Scroll erlaubt
- Zoom erlaubt
- Annotation erzeugt ChangeSet
- Annotation bei ViewOnly abgelehnt
- ChangeSet ohne gueltige Lease abgelehnt
- Accept/Apply erzeugt `Applied`
- Reject laesst Original unveraendert
- No File Ingress bleibt erfuellt
