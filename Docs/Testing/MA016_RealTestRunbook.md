# MA016 Real Test Runbook

## Purpose

This runbook is the final MA016 owner test path for the first controlled real-platform pilot.

## Preparation

1. Confirm the branch is `feature/ma016-ma017-real-platform-pilot`.
2. Run `git status`.
3. Run `.\tools\run-ma016-smoke.ps1`.
4. Run `.\tools\run-ma016-pilot-lab.ps1 -SmokeTest`.
5. Run `.\tools\package-ma016-owner-test.ps1 -SmokeTest`.

## Windows local proof

1. Run `.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf -Policy CriticalInfrastructure`.
2. Confirm `NoFileIngress: SUCCESS`.
3. Confirm `OwnerPdfSafetyGuard: OK`.
4. Confirm `UnauthorizedCapsuleOpen: DENIED`.
5. Confirm `ExpiredCapsule: RECOVERED_BY_OWNER`.

## Windows to macOS preparation

1. Run `.\tools\run-pilot-windows-to-mac-closed-pdf.ps1 -SmokeTest -Policy CriticalInfrastructure`.
2. Open `release/ma016/handoff/macOS_FINAL_HANDOFF.md`.
3. Execute native macOS work in Xcode when available.

## Windows to iPad preparation

1. Run `.\tools\run-pilot-windows-to-ipad-closed-pdf.ps1 -SmokeTest -Policy TrustedPersonalDevices`.
2. Open `release/ma016/handoff/iOS_iPadOS_FINAL_HANDOFF.md`.
3. Install the native iPad app through Xcode when available.

## Owner feedback

After each run:

```powershell
.\tools\record-ma016-feedback.ps1 -Scenario WindowsLocal -Rating Yellow -Comment "Owner note"
.\tools\export-ma016-feedback-report.ps1
```

## Stop immediately

- A PDF file appears on a guest.
- An original Windows path appears on a guest.
- Recovery cannot return the thing to the owner.
- The Owner feels uncertain about what is happening.
