# PDF Annotation ChangeSet

Status: MA013.34  
Datum: 2026-07-06

## Ziel

Annotationen auf einer Gastablage duerfen das Original nicht direkt veraendern. Sie werden als ChangeSet an den Owner zurueckgegeben.

## Unterstuetzte Operationen

| Operation | Status | ChangeSet-Abbildung |
| --- | --- | --- |
| Highlight | Aktiv | `AnnotationAdded` |
| Note | Aktiv | `AnnotationAdded` |
| Rectangle | Aktiv | `AnnotationAdded` |
| Free text | Geplant | `TextInserted`, policy-gesteuert |

## Ablauf

```text
Guest annotiert Frame
  -> AnnotationStart / AnnotationUpdate / AnnotationEnd
  -> FramePolicy prueft Annotate
  -> ChangeSetPolicy prueft Annotation
  -> ChangeSet Draft
  -> Owner Accept / Reject / Apply / Fork
```

## Regeln

- ViewOnly lehnt Annotationen ab.
- Ohne aktive CarryLease gibt es kein ChangeSet.
- Expired Lease wird abgelehnt.
- Owner-Entscheidung veraendert das Original nur bei explizitem `ApplyToOriginal`.
- Reject laesst das Original unveraendert.
- Free Text bleibt geplant, bis Text-Policy und UI-Freigabe vollstaendig sind.

## Tests

- `PdfFrameAnnotationCreatesChangeSet`
- `PdfFrameAnnotationViewOnlyRejected`
- `PdfAnnotationKindsSupported`
- `PdfFrameInteractionAcceptApplied`
- `PdfFrameInteractionRejectKeepsOriginal`
- `PdfFrameInteractionNoFileIngress`

## No File Ingress

Annotationen referenzieren FrameSession, Lease, Page und geometrische Werte. Sie enthalten keine PDF-Datei, keinen Originalpfad und keine Originalbytes.
