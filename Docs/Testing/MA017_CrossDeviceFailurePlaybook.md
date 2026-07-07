# MA017 Cross-Device Failure Playbook

## Build oder Smoke scheitert

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-ma017-windows-all-pdf-lifecycle.ps1 -SmokeTest
```

Wenn Datei-Locks entstehen:

1. laufende Testprozesse schliessen
2. kurz warten
3. `bin/obj` bereinigen
4. Smoke erneut starten

## macOS ist nicht bereit

Weiter mit:

```text
release/ma017/handoff/macOS_START_HERE.md
```

Blocker:

```text
PENDING_EXTERNAL_MACOS_XCODE
```

## iPad oder iPhone ist nicht bereit

Weiter mit:

```text
release/ma017/runbooks/iOS_USBInstallRunbook.md
```

Blocker:

```text
PENDING_XCODE_USB_INSTALL
```

## No File Ingress scheitert

Pilot stoppen. Nicht weiter testen.

```powershell
.\tools\run-ma017-security-policy-pilot.ps1 -SmokeTest
.\tools\run-no-file-ingress-report.ps1 -SmokeTest
```

Pruefen:

- wurde eine PDF-Datei auf der Gastablage erzeugt?
- ist ein Originalpfad sichtbar?
- wurden Originalbytes materialisiert?

## Glass Edge oder Proximity waehlt falsch

```powershell
.\tools\run-ma017-uwb-dongle-pilot.ps1 -SmokeTest
.\tools\run-manual-map.ps1 -SmokeTest
```

Bei unruhiger Auswahl:

- Manual Map als Fallback verwenden
- UWB/Dongle als Diagnosequelle markieren
- keine Produktentscheidung aus instabiler Simulation ableiten

## Recovery

Recovery ist bestanden, wenn der Owner wieder Kontrolle bekommt:

```text
ExpiredCapsule: RECOVERED_BY_OWNER
Recovery: SUCCESS
```
