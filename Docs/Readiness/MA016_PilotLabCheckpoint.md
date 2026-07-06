# MA016 Pilot Lab Checkpoint

## Status

AP531-540 add a repeatable MA016 Pilot Lab layer. The lab does not replace the product path; it orchestrates the Windows-led pilot checks, reports, feedback capture and repeatability checks before larger real-device runs.

## Implemented tools

- `tools/run-ma016-pilot-lab.ps1`
- `tools/configure-ma016-pilot.ps1`
- `tools/export-ma016-pilot-report.ps1`
- `tools/record-ma016-feedback.ps1`
- `tools/export-ma016-feedback-report.ps1`
- `tools/cleanup-ma016-pilot.ps1`
- `tools/run-ma016-repeatability.ps1`
- `tools/export-ma016-performance-baseline.ps1`

## Sample configs

- Windows local
- Windows-to-Mac
- Windows-to-iPad
- UWB simulation
- Dongle preparation

## Verification

Required smoke lines:

```text
MA016PilotLab: SUCCESS
PilotConfigWizard: SUCCESS
MA016PilotReport: SUCCESS
OwnerFeedbackCapture: SUCCESS
MA016FeedbackReport: SUCCESS
MA016Repeatability: SUCCESS
MA016PerformanceBaseline: SUCCESS
PilotCleanup: SUCCESS
RESULT: SUCCESS
```

## Limits

- Owner feedback is local-only and stored under ignored logs by default.
- Native macOS/iPad execution still requires Xcode-side runs.
- Performance values are smoke baselines, not final product targets.

## Decision

MA016 can proceed to readiness review once this checkpoint and the MA016 smoke suite remain green.
