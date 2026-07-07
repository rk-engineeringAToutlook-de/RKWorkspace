# MA017 Cross-Device Scripts Checkpoint

Status: AP661-670 ready.
Datum: 2026-07-07

## Umgesetzt

- `run-ma017-windows-to-mac-closed-pdf.ps1`
- `run-ma017-windows-to-mac-open-pdf.ps1`
- `run-ma017-windows-to-ipad-closed-pdf.ps1`
- `run-ma017-windows-to-ipad-open-pdf.ps1`
- `run-ma017-windows-all-pdf-lifecycle.ps1`
- `run-ma017-cross-device-session-monitor.ps1`
- `export-ma017-cross-device-diagnostics.ps1`
- `Docs/Testing/MA017_CrossDeviceOwnerInstructions.md`
- `Docs/Testing/MA017_CrossDeviceFailurePlaybook.md`

## Ziel

MA017 besitzt jetzt eigene Cross-Device-Pilot-Skripte. MA016 bleibt eingefroren und wird nicht als aktiver Pilotpfad weitergeschrieben.

## Smoke Commands

```powershell
.\tools\run-ma017-windows-to-mac-closed-pdf.ps1 -SmokeTest
.\tools\run-ma017-windows-to-mac-open-pdf.ps1 -SmokeTest
.\tools\run-ma017-windows-to-ipad-closed-pdf.ps1 -SmokeTest
.\tools\run-ma017-windows-to-ipad-open-pdf.ps1 -SmokeTest
.\tools\run-ma017-windows-all-pdf-lifecycle.ps1 -SmokeTest
.\tools\run-ma017-cross-device-session-monitor.ps1 -SmokeTest
.\tools\export-ma017-cross-device-diagnostics.ps1 -SmokeTest
```

## Akzeptanz

- Windows Owner bleibt Originalbesitzer.
- macOS/iPad bleiben als Gastablaegen vorbereitet.
- Closed PDF und Open PDF laufen im Smoke.
- No File Ingress bleibt verpflichtend.
- Diagnostics exportiert in `release/ma017/reports/cross-device-diagnostics.md`.

## Ergebnis

MA017 Cross-Device Scripts: READY
