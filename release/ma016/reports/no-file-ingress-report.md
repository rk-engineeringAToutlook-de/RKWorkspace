# MA016 No File Ingress Report

Generated: 2026-07-06T23:02:46Z

## Scope

This report covers the Windows PDF lifecycle pilot for Closed PDF Capsule and Open PDF Frame.

## Evidence

- Closed PDF Capsule: PASS
- Open PDF Frame: PASS
- Guest has no PDF file: PASS
- Guest has no original path: PASS
- Guest has no copied PDF bytes: PASS
- Capsule cache is MemoryOnly: PASS
- OpenFrame cache is MemoryOnly: PASS
- Unauthorized capsule open is denied: PASS
- Expired capsule is recovered by owner: PASS

## Commands

`powershell
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -ClosedPdf
.\tools\run-windows-pdf-lifecycle-pilot.ps1 -SmokeTest -OpenPdf -Page 1 -Zoom 1.25 -ViewerName 'Windows PDF Viewer'
`

## Result

NoFileIngressReport: SUCCESS
RESULT: SUCCESS
