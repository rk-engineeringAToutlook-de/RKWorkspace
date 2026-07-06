# MA016 Pilot Start Recommendation

Status: Recommended
Datum: 2026-07-07

## Recommendation

Start the next controlled pilot with Windows Owner plus macOS Guest Surface.

After the macOS path is stable, repeat with iPad/iPhone Guest Surface.

## Order

1. Windows local owner pilot.
2. Windows-to-macOS native guest smoke.
3. Windows-to-iPad/iPhone native guest smoke.
4. UWB simulation review with manual map fallback.
5. Owner feedback interview using MA016 HX guides.

## Required Before Pilot

~~~powershell
.\tools\run-ma016-smoke.ps1
.\tools\test-context-pack-no-secrets.ps1
.\tools\check-ma016-final-status.ps1
~~~

## Owner-Facing Rule

The owner should not be asked to evaluate transport, protocol or security internals.

The owner evaluates:

- Did the PDF feel like it stayed mine?
- Did the other Ablage feel usable?
- Did I think about devices?
- Did I trust recovery?

