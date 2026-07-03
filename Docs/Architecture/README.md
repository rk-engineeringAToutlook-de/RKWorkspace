# RKWS Architecture Notes

Dokument-ID: RKWS-ARCH-INDEX  
Version: 0.5.0
Status: Accepted  
Datum: 2026-07-02

## Zweck

Dieses Verzeichnis sammelt vertiefende Architekturunterlagen, die die nummerierten Hauptdokumente ergaenzen.

Alle Architekturunterlagen folgen HX-000. Wenn eine Architekturentscheidung den Benutzer wieder an Geraete, Fenster oder Betriebssysteme erinnert, muss sie ueberarbeitet werden.

## Diagramm

```mermaid
flowchart TB
    HX000["HX-000"] --> Freeze["ArchitectureFreeze"]
    Freeze["ArchitectureFreeze"] --> Baseline["ArchitectureBaseline"]
    Baseline --> Review["ArchitectureBaselineReview"]
    Review --> Readiness["EngineeringReadinessCheck"]
    Freeze --> Context["SystemContext"]
    Review --> Git["Git-Freigabe"]
```

## Startpunkte

- `Spec/HumanExperienceSpecification_HX000.md`
- `Docs/Architecture/ArchitectureFreeze.md`
- `Docs/Architecture/ArchitectureBaseline.md`
- `Docs/Architecture/ArchitectureBaseline_v1.0.md`
- `Docs/Architecture/ArchitectureBaselineReview.md`
- `Docs/Architecture/EngineeringReadinessCheck.md`
- `Docs/Architecture/OpenIssuesBeforeMA003.md`
- `Docs/Architecture/GitReleaseReadiness.md`
- `Docs/Architecture/GitInitialCommitReadiness.md`
- `Docs/Architecture/ArchitectureReview.md`
- `Docs/Architecture/SystemContext.md`
- `Docs/Glossary.md`
- `Docs/00_ProductVision.md`
- `Docs/01_SystemArchitecture.md`
- `Docs/02_SoftwareArchitecture.md`
- `Docs/04_CommunicationProtocol.md`
- `Docs/06_SecurityModel.md`

## Querverweise

- `Spec/HumanExperienceSpecification_HX000.md`
- `Docs/ADR/README.md`
- `Spec/DocumentationQuality.md`

## Aenderungsverlauf

| Version | Datum | Aenderung |
| --- | --- | --- |
| 0.5.0 | 2026-07-03 | HX-000 als uebergeordnete Regel fuer Architekturentscheidungen ergaenzt. |
| 0.4.0 | 2026-07-02 | Engineering Readiness Check und Baseline v1.0 aufgenommen. |
| 0.3.0 | 2026-07-02 | Architecture Baseline Completion aufgenommen. |
| 0.2.0 | 2026-07-02 | Architecture-Freeze und Review aufgenommen. |
| 0.1.0 | 2026-07-02 | Architektur-Index angelegt. |
