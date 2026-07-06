# MA016 Readiness Review

## Scope

MA016 brings the Windows-led real platform pilot to a repeatable decision point. The goal is not final production networking. The goal is proving the owner-side PDF lifecycle, cross-device contracts, No File Ingress, policy gates, proximity simulation and pilot-lab orchestration.

## Ready

- Windows Closed PDF Capsule pilot.
- Windows Open PDF Frame pilot.
- FrameCapsule registry, return and stale recovery.
- No File Ingress evidence for closed and open PDF paths.
- macOS handoff package.
- iOS/iPadOS handoff package.
- UWB simulation and proximity fusion.
- Cross-device security and policy regressions.
- MA016 Pilot Lab orchestration.
- Owner feedback capture and report path.

## Not yet native

- Native macOS guest execution still requires Xcode-side implementation and run.
- Native iPad/iPhone guest execution still requires Xcode-side implementation and device install.
- UWB hardware and dongle firmware remain prepared, not physically validated.
- SecureDev is a development security path, not final production security.

## Required verification

```text
.\tools\run-ma016-smoke.ps1
.\tools\run-ma016-pilot-lab.ps1 -SmokeTest
.\tools\package-ma016-owner-test.ps1 -SmokeTest
.\tools\export-codex-context.ps1
```

## Review decision

MA016 is ready for controlled owner testing when all listed commands are green and the Owner confirms that the runbook is understandable without reading source code.
