# MA017 Security/Policy Pilot Checkpoint

Status: Verified
Datum: 2026-07-07

## Abgedeckte Arbeitspakete

- AP621: No File Ingress Report fuer Closed PDF Capsule.
- AP622: No File Ingress Report fuer Open PDF Frame.
- AP623: Regression fuer Closed, Open, KeepCapsule, Return und Recovery.
- AP624: Policy-Felder fuer Capsule/OpenFrame.
- AP625: CriticalInfrastructure PDF Lifecycle Policy.
- AP626: TrustedPersonalDevices PDF Lifecycle Policy.
- AP627: Unauthorized Capsule Open Audit.
- AP628: Unauthorized OpenFrame Input Audit.
- AP629: PDF Lifecycle Threat Model Update.
- AP630: Security/Policy Pilot Checkpoint.

## Gate

~~~powershell
.\tools\run-ma017-security-policy-pilot.ps1 -SmokeTest
~~~

## Erwartete Marker

~~~text
NoFileIngressReport: SUCCESS
ClosedPdfSecurity: SUCCESS
OpenPdfSecurity: SUCCESS
UnauthorizedKeepCapsule: DENIED
UnauthorizedCapsuleOpen: DENIED
UnauthorizedOpenFrameInput: DENIED
CrossDeviceSecurityRegression: SUCCESS
CrossDevicePolicyRegression: SUCCESS
MA017SecurityPolicyPilot: SUCCESS
RESULT: SUCCESS
~~~

## Policy-Felder

Im Code ist `AllowFrameCapsule` das explizite technische Feld fuer die Frame-Kapsel.
Ab MA017 wird der Owner-nahe Begriff `AllowCapsule` als Alias dokumentiert und im Modell bereitgestellt.

Die relevanten Entscheidungen:

- `AllowCapsule`: Kapsel darf ueberhaupt erzeugt und geoeffnet werden.
- `AllowOpenFrame`: geoeffnete PDF darf als OpenFrame sichtbar sein.
- `AllowKeepCapsule`: Kapsel darf nach CloseFrame auf der Gastablage verbleiben.

## Ergebnis

Der MA017 Security/Policy Pilot ist verifiziert, wenn das Gate erfolgreich laeuft und der Report unter `release/ma017/reports/security-policy-pilot-report.md` erzeugt wurde.
