# MA017 Final Verification Plan

Status: Accepted
Datum: 2026-07-07

## Ziel

Dieser Plan definiert die finale MA017-Verifikation vor dem ersten echten Cross-Device-Pilotlauf.

## Scope

Geprueft werden:

- MA016 Verified Baseline.
- MA017 Windows PDF Standard Pilot.
- Closed PDF Capsule und Open PDF Frame.
- No File Ingress fuer Capsule und OpenFrame.
- Security und Policy Regression.
- Proximity, Manual Map, UWB Simulator und Dongle Simulation.
- Glass Edge als Trigger fuer die naechste Ablage.
- macOS Handoff Package.
- iOS/iPadOS Handoff Package.
- Owner Feedback, Human Experience Checkpoint und Pilot Lab.
- Performance/Stability Checkpoint.
- Documentation, Packaging und Repository Hygiene.

## Pflichtbefehle

~~~powershell
.\tools\run-ma017-final-verification.ps1 -SkipHeavy
.\tools\run-ma017-final-cleanup-checkpoint.ps1 -SmokeTest -AllowDirty
.\tools\export-codex-context.ps1
.\tools\test-context-pack-no-secrets.ps1 -SmokeTest
~~~

Vor einem echten Pilotlauf ohne Abkuerzung:

~~~powershell
.\tools\run-ma017-final-verification.ps1
.\tools\clean-build-artifacts.ps1
.\tools\run-ma017-final-cleanup-checkpoint.ps1
~~~

## Erfolgskriterien

- Alle Builds: 0 Warnungen, 0 Fehler.
- `MA017Smoke: SUCCESS`.
- `MA017FinalVerification: SUCCESS`.
- `NoFileIngress: SUCCESS`.
- `MA017SecurityCheckpoint: SUCCESS`.
- `MA017PerformanceStability: SUCCESS`.
- `MA017DocumentationCheckpoint: SUCCESS`.
- `MA017FinalCleanupCheckpoint: SUCCESS`.
- Context Pack Secret Scan: 0 Findings.
- Keine `bin/` oder `obj/` Artefakte nach finalem Cleanup.

## No-Go Kriterien

- Gastablage erhaelt Originaldatei, Originalpfad oder kopierte PDF-Bytes.
- Original Ownership kann nicht eindeutig beim Owner gehalten werden.
- Security- oder Policy-Regression scheitert.
- UWB/Manual-Map-Fusion waehlt keine eindeutige naechste Ablage.
- macOS- oder iOS-Handoff ist nicht nachvollziehbar vorbereitet.
- Secret Scan findet ein echtes Geheimnis.
- Repository Hygiene meldet Build-Artefakte nach Cleanup.

## Aktueller Stand

Der finale MA017-Pilot ist bereit fuer Owner-gefuhrte Tests, aber noch nicht fuer Produktion. macOS und iOS/iPadOS sind als Handoff-Pakete vorbereitet; native Ausfuehrung auf echter Apple-Hardware bleibt externer Testschritt.
