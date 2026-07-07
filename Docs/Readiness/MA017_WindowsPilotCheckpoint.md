# MA017 Windows Pilot Checkpoint

Status: Verified
Datum: 2026-07-07

## Abgedeckte Arbeitspakete

- AP611: Closed PDF Pilot als Standardtest.
- AP612: Open PDF Pilot als Standardtest.
- AP613: Capsule oeffnen sichtbar.
- AP614: OpenFrame sichtbar.
- AP615: Owner Lock eindeutig.
- AP616: CloseFrame Return gegen KeepCapsule testbar.
- AP617: Recovery fuer Closed PDF Capsule.
- AP618: Recovery fuer OpenFrame.
- AP619: verbotene sichtbare Sprache geprueft.
- AP620: Windows Pilot Checkpoint.

## Gate

~~~powershell
.\tools\run-ma017-windows-pdf-pilot.ps1 -SmokeTest
~~~

## Ergebnis

- ClosedPdfStandard: SUCCESS.
- OpenPdfStandard: SUCCESS.
- WindowsPdfStandard: SUCCESS.
- Recovery: SUCCESS.
- VisibleForbiddenTerms: SUCCESS.
- NoFileIngress: SUCCESS.

## Bedeutung

Der Windows-Pilot ist damit nicht mehr nur ein Einzeltest aus MA016.
Er ist der erste MA017-Pilotstandard fuer Original-Owned PDF Lifecycle.
