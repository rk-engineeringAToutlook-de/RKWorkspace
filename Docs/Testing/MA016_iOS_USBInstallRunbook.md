# MA016 iOS USB Install Runbook

## Goal

Install the RK Workspace mobile Guest Ablage on iPhone or iPad through Xcode and run the first local frame smoke.

## Prerequisites

- macOS host with Xcode.
- iPhone or iPad connected by USB.
- Developer Mode enabled on the device.
- Apple Development Team configured.
- Windows owner available on the same lab network.

## Steps

1. Open the Xcode project.
2. Select the physical iPhone or iPad.
3. Apply `release/ma016/config/ios-guest.sample.json`.
4. Build and run.
5. Confirm the app shows `Ablage`.
6. Run Capsule smoke.
7. Run OpenFrame smoke.
8. Run Return.
9. Inspect app container.
10. Export smoke log.

## Expected Smoke Output

```text
iOSGuestAblage: STARTED
FrameCapsule: OK
OpenFrame: OK
HapticsPrepared: OK
DocumentsOriginalPdf: NO
CachesOriginalPdf: NO
TempOriginalPdf: NO
FilesAppOriginalPdf: NO
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
Return: SUCCESS
Recovery: SUCCESS
RESULT: SUCCESS
```

## Container Checks

Use Xcode Devices and Simulators to download the app container.

Check:

- Documents
- Library/Caches
- tmp
- App Group container
- Files export locations

No original PDF may be present.
