# MA016 Cross-Device Audit Events

Generated: 2026-07-07T00:25:35Z

## Required events

- CrossDeviceSessionStarted
- CapsuleArrived
- OpenFrameArrived
- GuestReturned
- GuestRecovered
- UwbSelectedTarget

## Current Windows pilot proof

~~~text
  Wiederherzustellende Projekte werden ermittelt...
  Alle Projekte sind für die Wiederherstellung auf dem neuesten Stand.
  RKWorkspace.Protocol -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Protocol\RKWorkspace.Protocol\bin\Debug\net8.0\RKWorkspace.Protocol.dll
  RKWorkspace.Frame.Pdf -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Frame\RKWorkspace.Frame.Pdf\bin\Debug\net8.0\RKWorkspace.Frame.Pdf.dll
  RKWorkspace.Shell -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Shell\RKWorkspace.Shell\bin\Debug\net8.0\RKWorkspace.Shell.dll
  RKWorkspace.WindowsPdfFramePilot -> E:\HiDrive\users\RK Workspace\RKWorkspace\src\Tools\RKWorkspace.WindowsPdfFramePilot\bin\Debug\net8.0\RKWorkspace.WindowsPdfFramePilot.dll

Der Buildvorgang wurde erfolgreich ausgeführt.
    0 Warnung(en)
    0 Fehler

Verstrichene Zeit 00:00:00.99
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
PolicyProfile: TrustedPersonalDevices
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
UnauthorizedOpenFrameInput: DENIED
ExpiredCapsule: RECOVERED_BY_OWNER
AuditEvents: ClosedPdfPicked -> CapsuleCreated -> CapsuleOpened -> CloseReturnsEvaluated -> PdfReturned -> PdfRecovered

Glass Edge
----------
UseGlassEdge: YES
UseManualMap: NO
UseUwbSim: YES
UseProximityFusion: NO
ProximityMode: UwbSim
UwbProviderStatus: Simulated
UwbProfile: MovingCloser
NearestAblage: Ablage iPad
EdgeDirection: Right
EventFlow: GlassEdgeAppearing -> GlassEdgeActive -> ObjectEnteringEdge -> CarryLeaseRequested -> CarryLeaseGranted -> FrameSessionOpen -> FrameSessionReady -> ObjectInTransit -> ObjectEmerging -> ObjectPlaced
GlassEdgeAppearing: OK
GlassEdgeActive: OK
ObjectEnteringEdge: OK
ObjectInTransit: OK
ObjectEmerging: OK
ObjectPlaced: OK
FrameSessionOpen: OK
FrameSessionReady: OK
PlaySequence: SUCCESS

Cross-Device Audit
------------------
CrossDeviceAuditEvents: CrossDeviceSessionStarted -> CapsuleArrived -> GuestReturned -> GuestRecovered -> UwbSelectedTarget
CrossDeviceAuditEventsPresent: SUCCESS
CrossDeviceSessionStarted: OK
CapsuleArrived: OK
OpenFrameArrived: NOT USED
GuestReturned: OK
GuestRecovered: OK
UwbSelectedTarget: OK

Sicherheitsstatus
-----------------
No File Ingress: SUCCESS
No File Ingress Status: sichtbar im Log
Sichtbare Sprache: SUCCESS
SecurityModeWarning: NON_PRODUCTION_SECURITY
SecureDevWarning: NON_PRODUCTION_SECURITY
OwnerPdfSafetyGuard: WARNINGS_PRESENT
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
UnauthorizedOpenFrameInput: DENIED
ExpiredCapsule: RECOVERED_BY_OWNER
CrossDeviceAuditEventsPresent: SUCCESS
SecurityModeWarning: NON_PRODUCTION_SECURITY
SecureDevWarning: NON_PRODUCTION_SECURITY
OwnerPdfSafetyGuard: WARNINGS_PRESENT
NearestAblageSelected: OK
GlassEdgeIntegration: SUCCESS
GlassEdgePlaySequence: SUCCESS

WindowsPdfFramePilot: SUCCESS
RESULT: SUCCESS
~~~

## Result

CrossDeviceAudit: SUCCESS
RESULT: SUCCESS
