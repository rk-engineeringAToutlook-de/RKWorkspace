# MA016 Cross-Device Failure Playbook

## Build Fails

Run:

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf
```

If a file lock occurs, close running test processes, wait briefly, clean `bin/obj`, and retry.

## macOS Not Ready

Use the handoff:

```text
release/ma016/handoff/macOS_START_HERE.md
```

Record blocker as `PENDING_EXTERNAL_MACOS_XCODE`.

## iPad Not Ready

Use:

```text
Docs/Testing/MA016_iOS_USBInstallRunbook.md
```

Record blocker as `PENDING_XCODE_USB_INSTALL`.

## No File Ingress Fails

Stop the pilot. Do not continue cross-device testing.

Run:

```powershell
.\tools\run-no-file-ingress-report.ps1
```

Investigate any created PDF file, original path, or original bytes.

## UWB/Fusion Selects Wrong Ablage

Fallback to Manual Map and record the provider:

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest -UseGlassEdge -UseManualMap -PlaySequence
```

## Recovery

If heartbeat or guest app is lost, owner recovery must end with:

```text
ExpiredCapsule: RECOVERED_BY_OWNER
Recovery: SUCCESS
```
