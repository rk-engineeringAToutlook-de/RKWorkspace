# RK Workspace

# MA016 Test Matrix

Status: Accepted  
Datum: 2026-07-07

| Bereich | Test | Gate |
| --- | --- | --- |
| Windows lokal | Closed PDF Capsule | Capsule sichtbar, oeffnen, Return, Recovery |
| Windows lokal | Open PDF Frame | OpenPdfContext, OpenFrame, Return, Recovery |
| macOS | Guest Handoff | START HERE, Config, Contracts, No File Ingress |
| iPad/iPhone | Xcode Handoff | USB-Install, Sandbox, Haptics, Capsule/OpenFrame |
| Manual Map | Zielwahl | naechste Ablage, Richtung, Distanz |
| UWB Simulator | Naeheprofil | static, closer, away, passing, noisy |
| Dongle | Hardwarepfad | UWB/BLE/USB/AblageIdentity geplant |
| Security | No File Ingress | keine PDF-Datei, keine Originalbytes, kein Originalpfad |
| Policy | Critical/Personal | Capsule/OpenFrame erlauben oder blockieren |
| Owner Feedback | Pilot-Feedback | rot/gelb/gruen und Kommentar |

## Minimaler MA016-Smoke

```powershell
.\tools\run-ma016-smoke.ps1
```

Der Smoke buendelt nur verfuegbare lokale Checks. Echte macOS-/iOS-Geraetetests bleiben Runbook- und Handoff-Gates.
