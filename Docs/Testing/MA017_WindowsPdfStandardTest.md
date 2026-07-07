# MA017 Windows PDF Standard Test

Status: Verified
Datum: 2026-07-07

## Ziel

Der Windows PDF Pilot ist ab MA017 ein Standardtest fuer den realen Pilotpfad.

Er prueft nicht nur technische Ausfuehrung, sondern sichtbare Vertrauenssignale:

- Closed PDF Capsule.
- Open PDF Frame.
- sichtbares Oeffnen der Capsule.
- sichtbarer OpenFrame.
- eindeutiger Owner Lock.
- CloseFrame Return als Standard.
- KeepCapsule als verbotene Policy-Variante.
- Recovery fuer Closed PDF Capsule und Open PDF Frame.
- Unauthorized OpenFrame Input als Denial/Audit-Signal.
- keine sichtbare verbotene Sprache.

## Standardbefehl

~~~powershell
.\tools\run-ma017-windows-pdf-pilot.ps1 -SmokeTest
~~~

## Erwartete Marker

~~~text
ClosedPdfStandard: SUCCESS
OpenPdfStandard: SUCCESS
WindowsPdfStandardSmoke: SUCCESS
WindowsPdfStandard: SUCCESS
RESULT: SUCCESS
~~~

## Owner-Sicht

Der Test gilt nur als tragfaehig, wenn das PDF als Original im Besitz des Owners wirkt.
Die Gegenseite darf kein Dateizugriffsziel sein, sondern nur eine kontrollierte Frame- oder Capsule-Sicht.

## Nicht-Ziele

- keine echte Dateiuebergabe.
- kein Datei-Ingress.
- keine Kopie auf der Gastseite.
- keine produktive Netzwerkfreigabe.
