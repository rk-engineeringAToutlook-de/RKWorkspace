# RK Workspace Documentation Index

Dokument-ID: RKWS-DOCS-INDEX
Version: 1.0.0
Status: Accepted
Datum: 2026-07-07

## Zweck

Dieser Index sammelt die wichtigsten Dokumentationspfade fuer RK Workspace. Er dient als Einstieg fuer Codex, Owner-Tests und spaetere Plattform-Handovers.

## Fuehrende Dokumente

- `Docs/Nordstern.md`
- `Spec/HumanExperienceSpecification_HX000.md`
- `Spec/HumanExperienceSpecification_HX001.md`
- `Spec/HumanExperienceSpecification_HX001A.md`
- `Spec/ProductPhilosophy.md`
- `Docs/Glossary.md`
- `Docs/Architecture/README.md`
- `Docs/ADR/README.md`

## MA016 Pilotstand

MA016 buendelt den ersten realen Cross-Device-Pilotpfad:

- Windows Owner PDF Lifecycle.
- Closed PDF Capsule.
- Open PDF Frame.
- No File Ingress.
- SecureDev und Policy-Gates.
- macOS und iOS/iPadOS Handoff-Pakete.
- UWB-Simulation und Proximity Fusion.
- Pilot Lab, Feedback, Hygiene und Context Pack.

## MA016 Kernpfade

- `Docs/Readiness/MA016_CompletionReport.md`
- `Docs/Readiness/MA016_ReadinessReview.md`
- `Docs/Readiness/MA016_GoNoGoDecision.md`
- `Docs/Testing/MA016_RealTestRunbook.md`
- `Docs/HumanExperience/MA016_HumanExperienceSummary.md`
- `release/MA016_READINESS_SUMMARY.md`
- `release/ma016/reports/REPOSITORY_HYGIENE.md`

## Plattform-Handovers

- `release/ma016/handoff/macOS_FINAL_HANDOFF.md`
- `release/ma016/handoff/iOS_iPadOS_FINAL_HANDOFF.md`
- `release/ma016/handoff/macOS_NEXT_REAL_IMPLEMENTATION_TASK.md`
- `release/ma016/handoff/iOS_NEXT_REAL_IMPLEMENTATION_TASK.md`
- `release/ma016/handoff/WINDOWS_OWNER_HARDENING_TASK.md`

## Verifikation

Wichtige Befehle:

~~~powershell
.\tools\run-ma016-smoke.ps1 -SkipHeavy
.\tools\test-context-pack-no-secrets.ps1
.\tools\clean-build-artifacts.ps1
.\tools\clean-pilot-artifacts.ps1
.\tools\check-ma016-final-status.ps1
~~~

