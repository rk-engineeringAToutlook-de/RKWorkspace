# RKWP Input Channel

Dokument-ID: RKWS-RKWP-INPUT-001
Status: Draft
Datum: 2026-07-05

## Ziel

Der RKWP Input Channel beschreibt kontrollierte Eingaben in einen Frame. Er macht einen Frame bedienbar, ohne das Original auf die Gastablage zu uebertragen und ohne Besitz zu wechseln.

Input ist damit kein Transfer, kein CopyOut und keine Materialisierung. Input ist eine policygepruefte Interaktion innerhalb einer aktiven `FrameSession`.

## Eingabearten

`FrameInputEvent` unterstuetzt ab MA007.06:

- PointerMove
- PointerDown
- PointerUp
- Tap
- DoubleTap
- Scroll
- Zoom
- KeyboardText
- KeyboardCommand
- AnnotationStart
- AnnotationUpdate
- AnnotationEnd

Jedes Event enthaelt:

- InputEventId
- FrameSessionId
- LeaseId
- SourceAblageId
- TargetAblageId
- SequenceNumber
- InputType
- Coordinates optional
- Delta optional
- Text optional
- Modifiers
- Pressure optional
- PointerKind
- Timestamp

## Bindung

Ein InputEvent ist nur gueltig, wenn es an die aktive CarryLease und die aktive FrameSession gebunden ist.

Mindestregeln:

- `LeaseId` muss zur aktiven Lease passen.
- `FrameSessionId` muss zur aktiven FrameSession passen.
- `SequenceNumber` muss positiv sein.
- Eingaben ohne passende Session werden abgelehnt.
- abgelehnte Eingaben erzeugen `PolicyDenied` im Audit.

## Policy

`FramePolicy` trennt die Eingabearten:

- AllowPointer
- AllowScroll
- AllowZoom
- AllowKeyboard
- AllowTextInput
- AllowAnnotation
- AllowClipboard
- AllowExtract
- AllowSystemShortcuts

Kritische Default-Umgebung:

- Frame anzeigen erlaubt.
- Scroll/Zoom nur, wenn Policy es ausdruecklich erlaubt.
- Pointer/Input verboten.
- Keyboard/Text verboten.
- Annotation verboten.
- Clipboard, Extract und System Shortcuts verboten.

## Bedienregeln

ViewOnly bedeutet: Keine veraendernde Interaktion.

Interactive bedeutet: Scroll, Zoom und Pointer koennen erlaubt werden, ohne dass die Gastablage das Original besitzt.

Annotate bedeutet: Annotation-Events koennen erlaubt werden. Die eigentliche Aenderung gehoert spaeter in ein ChangeSet und wird nicht stillschweigend in das Original geschrieben.

TextInput ist konservativ: Es braucht Policy-Freigabe und einen editierbaren Frame. Ein normales Interactive-Frame darf keinen Text direkt in das Original schreiben.

## Sicherheit

Der Input Channel ist eine Angriffsflaeche. Deshalb darf er nicht als lokales UI-Ereignis ohne RKWP-Kontext behandelt werden.

Produktive Plattformen muessen spaeter zusaetzlich pruefen:

- sichere Session
- Replay-Schutz ueber Message Sequence/Nonce
- Policy Binding
- Trust der Ablage
- Renderer-Sandboxing
- Audit fuer abgelehnte und sicherheitsrelevante Eingaben

## Plattformhinweise

Windows, macOS, iOS/iPadOS, Android und Linux koennen unterschiedliche Eingabemodelle besitzen. RKWP normalisiert nur die Protokollbedeutung. Die jeweilige Surface muss lokale Events in `FrameInputEvent` uebersetzen und dabei die Policy beachten.

Touch, Pen, Trackpad-Gesten und Haptik bleiben plattformspezifisch. Der Protokollkanal ist vorbereitet, aber noch keine finale produktive Eingabepipeline.

## Teststatus

MA007.06 testet:

- ViewOnly lehnt PointerInput ab.
- Interactive erlaubt Scroll und Zoom.
- Annotate erlaubt Annotation.
- fehlende Annotation-Policy lehnt Annotation ab.
- falsche LeaseId wird abgelehnt.
- falsche FrameSessionId wird abgelehnt.
- SequenceNumber muss positiv sein.
- KeyboardText ohne Berechtigung wird abgelehnt.
- Zoom ohne Policy wird abgelehnt.
- Denials erzeugen `PolicyDenied`.

## Offene Punkte

- echte Surface-Eingabeadapter
- echte Scroll-/Zoom-Ausfuehrung im PDF-Renderer
- Annotation als ChangeSet
- produktive Trust- und Security-Integration
- plattformspezifische Haptik
