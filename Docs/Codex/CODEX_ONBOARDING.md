# Codex Onboarding

Status: Draft
Datum: 2026-07-05

## Erste Regel

RK Workspace wird Human-Experience-getrieben entwickelt. Code ist Mittel, nicht Ziel.

## Aktueller Produktpfad

1. Workspace Shell ist das Produkt, nicht Developer Studio.
2. Single Glass Edge zeigt die naechste Ablage.
3. MA007.00 fuehrt RKWP Original-Owned Frame ein.
4. Das Original bleibt beim Owner.
5. Gastoberflaechen bekommen Frames, keine Datei-Ingress-Kopie.

## Synchronisation

GitHub bleibt die zentrale Synchronisationsstelle fuer Branches, Commits und spaetere Plattform-Handoffs. Codex arbeitet lokal, pusht aber nur nach ausdruecklicher Owner-Freigabe.

## Wichtigste Dokumente

- `Spec/HumanExperienceSpecification_HX000.md`
- `Docs/Nordstern.md`
- `Docs/WorkspaceShell.md`
- `Docs/GlassEdgeNearestAblage.md`
- `Docs/Protocol/RKWP_ProtocolFoundation.md`
- `Docs/Protocol/RKWP_OwnershipAndLease.md`
- `Docs/Protocol/RKWP_FrameSession.md`
- `Docs/Roadmap/RKWorkspace_Roadmap.md`

## Tests vor Commit

```powershell
.\tools\run-tests.ps1
.\tools\run-studio.ps1 -SmokeTest
.\tools\run-rkwp-tests.ps1
.\tools\run-pdf-frame-smoke.ps1
```
