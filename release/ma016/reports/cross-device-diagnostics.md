# MA016 Cross-Device Diagnostics

Generated: 2026-07-06T23:26:34Z

## Monitor

~~~text
RK Workspace Cross-Device Session Monitor
-----------------------------------------
WindowsOwner: READY
macOSGuest: HANDOFF_READY
iPadGuest: HANDOFF_READY
ClosedPdfCapsule: READY
OpenPdfFrame: READY
NoFileIngress: REQUIRED
UwbSimulation: READY
NativeDeviceExecution: PENDING_EXTERNAL_MACOS_XCODE
  Wiederherzustellende Projekte werden ermittelt...
  Alle Projekte sind für die Wiederherstellung auf dem neuesten Stand.
  RKWorkspace.Protocol -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Protocol\RKWorkspace.Protocol\bin\Debug\net8.0\RKWorkspace.Protocol.dll
  RKWorkspace.Frame.Pdf -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Frame\RKWorkspace.Frame.Pdf\bin\Debug\net8.0\RKWorkspace.Frame.Pdf.dll
  RKWorkspace.Shell -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Shell\RKWorkspace.Shell\bin\Debug\net8.0\RKWorkspace.Shell.dll
  RKWorkspace.WindowsPdfFramePilot -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Tools\RKWorkspace.WindowsPdfFramePilot\bin\Debug\net8.0\RKWorkspace.WindowsPdfFramePilot.dll

Der Buildvorgang wurde erfolgreich ausgeführt.
    0 Warnung(en)
    0 Fehler

Verstrichene Zeit 00:00:00.89
RK Workspace Windows PDF Frame Pilot
------------------------------------
Modus: Smoke-Test
Hinweis: Diese Anzeige ist ein Testwerkzeug, keine finale UX.

Originalablage
---------------
Ablage: Ablage Windows Owner
PDF-Name: Rechnung.pdf
Zustand: wartet auf Rueckgabe
Seite: 1/1
Seitennavigation: bereit
Bearbeitung: gesperrt waehrend ausgeliehen
Nach Rueckgabe: wieder verfuegbar
Recovery: wiederhergestellt

Gastablage
----------
Ablage: Ablage Windows Guest
Frame: liegt hier im Frame
FrameStatus: liegt hier im Frame
Kapsel: Kapsel bereit
OpenFrame: nicht geoeffnet
Zoom: bereit
Scroll: bereit
Rueckgabe: zurueckgeben
Verbindung: Verbindung verloren
PDF-Datei: keine PDF-Datei vorhanden

PDF Lifecycle
-------------
LifecycleMode: ClosedPdfCapsule
PolicyProfile: DevelopmentLab
FrameCapsule: OK
CapsuleState: Created
CapsuleOpen: OK
FinalCapsuleState: Returned
CloseFrameBehavior: CloseReturns
KeepCapsulePolicy: DENIED
CapsuleNoFileIngress: SUCCESS
OpenFrameNoFileIngress: SUCCESS
CapsuleCache: MemoryOnly
OpenFrameCache: MemoryOnly
UnauthorizedCapsuleOpen: DENIED
ExpiredCapsule: RECOVERED_BY_OWNER
AuditEvents: ClosedPdfPicked -> CapsuleCreated -> CapsuleOpened -> CloseReturnsEvaluated -> PdfReturned -> PdfRecovered

Cross-Device Audit
------------------
CrossDeviceAuditEvents: CrossDeviceSessionStarted -> CapsuleArrived -> GuestReturned -> GuestRecovered
CrossDeviceAuditEventsPresent: SUCCESS
CrossDeviceSessionStarted: OK
CapsuleArrived: OK
OpenFrameArrived: NOT USED
GuestReturned: OK
GuestRecovered: OK
UwbSelectedTarget: NOT USED

Sicherheitsstatus
-----------------
No File Ingress: SUCCESS
No File Ingress Status: sichtbar im Log
Sichtbare Sprache: SUCCESS
SecurityModeWarning: NON_PRODUCTION_SECURITY
SecureDevWarning: DEV_ONLY_NOT_PRODUCTION
OwnerPdfSafetyGuard: WARNINGS_PRESENT
OwnerPdfSafetyWarning: DevModeNotProduction
OwnerPdfSafetyWarning: NonProductionSecurity


Smoke Checks
------------
PDF: Rechnung.pdf
SamplePdf: OK
OwnerSurface: STARTED
GuestSurface: STARTED
PdfOriginalRegistered: OK
CarryLease: Active
OwnerLocked: OK
FrameSession: Active
GuestFrame: OK
CurrentPage: 1
PageCount: 1
PageNavigation: OK
ZoomPrepared: OK
ScrollPrepared: OK
OwnerInitialStatus: verfuegbar
OwnerLeasedStatus: ausgeliehen
OwnerLockedStatus: wartet auf Rueckgabe
OwnerReturnedStatus: wieder verfuegbar
OwnerRecoveryStatus: wiederhergestellt
GuestOpeningStatus: liegt gleich hier
GuestFrameStatus: liegt hier im Frame
GuestReturnStatus: zurueckgeben
GuestRevokedStatus: nicht verfuegbar
GuestExpiredStatus: Verbindung verloren
PreviewKind: RenderedFirstPage
RendererStatus: Rendered
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
GuestHasCopiedPdfBytes: NO
NoFileIngress: SUCCESS
Rueckgabe: SUCCESS
Recovery: SUCCESS
ReturnVisibleState: SUCCESS
RecoveryVisibleState: SUCCESS
VisibleForbiddenTerms: SUCCESS
VisibleStateLanguage: SUCCESS
LifecycleMode: ClosedPdfCapsule
FrameCapsule: OK
CapsuleOpen: OK
OpenPdfContext: NOT USED
OpenFrame: NOT USED
CloseFrameBehavior: CloseReturns
KeepCapsulePolicy: DENIED
CapsuleNoFileIngress: SUCCESS
OpenFrameNoFileIngress: SUCCESS
CapsuleCache: MemoryOnly
OpenFrameCache: MemoryOnly
AuditEventsPresent: SUCCESS
UnauthorizedCapsuleOpen: DENIED
ExpiredCapsule: RECOVERED_BY_OWNER
CrossDeviceAuditEventsPresent: SUCCESS
SecurityModeWarning: NON_PRODUCTION_SECURITY
SecureDevWarning: DEV_ONLY_NOT_PRODUCTION
OwnerPdfSafetyGuard: WARNINGS_PRESENT

WindowsPdfFramePilot: SUCCESS
RESULT: SUCCESS
CrossDeviceSessionMonitor: SUCCESS
RESULT: SUCCESS
~~~

## Result

CrossDeviceDiagnostics: SUCCESS
RESULT: SUCCESS
