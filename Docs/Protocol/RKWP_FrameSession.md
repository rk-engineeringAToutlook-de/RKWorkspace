# RKWP Frame Session

Dokument-ID: RKWS-RKWP-FRAME-001
Status: Draft
Datum: 2026-07-05

## Ziel

Eine FrameSession macht ein Ding auf einer Gastablage sichtbar, ohne das Original dorthin zu uebertragen.

Ab MA007.03 ist eine FrameSession an `SessionId`, `LeaseId`, `PolicyId` und `PolicyVersion` gebunden. Dadurch kann ein Guest Frame nicht losgeloest von der autoritativen CarryLease weiterleben.

## Zustaende

- Opening
- Ready
- Active
- Paused
- Returning
- Closed
- Revoked
- Expired
- Failed

## FrameOnly PDF Slice

MA007.00 fuehrt einen PDF-Frame-Smoke ein:

```text
src/Frame/RKWorkspace.Frame.Pdf
src/Tools/RKWorkspace.PdfFrameOwner
src/Tools/RKWorkspace.FrameGuestSurface
samples/Objects/Rechnung.pdf
```

Der Owner liest die PDF und erzeugt eine Frame-Repräsentation. Der Gast erhaelt:

- FrameSessionId
- ThingId
- DisplayName
- PageCount
- SourceHash
- DisplayText
- RepresentationKind
- RendererStatus
- vorbereitete Scroll-/Zoom-Faehigkeiten

Der Gast erhaelt nicht:

- Originaldateipfad
- Originaldatei-Bytes
- lokale PDF-Kopie

## Rendering-Status

MA007.00 liest eine echte Sample-PDF, validiert Header, Metadaten, Hash und Page-Anzahl und erzeugt daraus eine sichere Frame-Repräsentation. Ein echter Bitmap-/Page-Renderer ist noch nicht aktiv.

MA007.05 schaerft diesen Status: Der Guest bekommt eine explizite `MetadataPreview` als sichere Frame-Repräsentation. `FrameRepresentation: OK`, Scroll und Zoom werden im Smoke geprueft. Echte PDF-Seiten werden weiterhin nicht gerendert; das ist ein dokumentierter Renderer-Blocker.

MA007.01 nutzt diese PDF als erstes echtes digitales Testobjekt. Die erlaubte Sprache im Owner-/Guest-Smoke lautet `PDF ist als Frame ausgeliehen`, `Frame geoeffnet`, `Liegt hier im Frame` und `Zurueckgegeben`. Verbotene Begriffe wie Senden, Empfangen, Download oder Transfer werden im FrameOnly-Pfad nicht verwendet.

Blocker fuer echtes Rendering:

- Es ist noch keine produktive PDF-Renderer-Abhaengigkeit im Projekt entschieden.
- Der Renderer muss plattformneutral oder sauber adapterisiert sein.
- Der Renderer darf FrameOnly nicht umgehen und keine Originaldatei auf dem Gast materialisieren.

Vorgeschlagene naechste Optionen:

- Windows-Prototyp mit PDFium oder WebView2 nur im Owner-Kontext.
- Plattformneutraler Renderer als separater Adapter.
- Mobile FramePresenter rendert nur erhaltene FrameUpdates, nicht die Originaldatei.

## Bedienmodell

FrameOnly erlaubt Anzeige, Scroll und Zoom. Editieren, Extrahieren und Ownership-Wechsel sind nicht automatisch erlaubt.

## Input Channel

MA007.06 bereitet den `FrameInputEvent`-Kanal vor. Eingabe bedeutet keinen Besitzwechsel und keine Dateiuebergabe. Jede Eingabe ist an `LeaseId`, `FrameSessionId`, `SequenceNumber` und eine `FramePolicy` gebunden.

Unterstuetzt sind Pointer, Touch/Tap, Scroll, Zoom, Keyboard und Annotation-Events. `FrameInputValidator` lehnt nicht passende Lease- oder FrameSession-Bindungen, nicht positive Sequenzen und policywidrige Eingaben ab. Abgelehnte Eingaben erzeugen `PolicyDenied` im Audit.

Kritische Defaults bleiben konservativ: ViewOnly lehnt Pointer/Input ab, Annotation und Keyboard sind ohne ausdrueckliche Policy verboten, und TextInput braucht einen editierbaren Frame.

## ChangeSet Rueckgabe

MA007.07 bereitet `ChangeSet` fuer veraendernde Frame-Interaktion vor. Annotationen oder spaetere Bearbeitungen erzeugen nicht automatisch eine Datei auf der Gastablage und werden nicht still in das Original geschrieben. Sie werden als ChangeSet eingereicht; der Owner entscheidet ueber Accept, Reject, Review, ApplyToOriginal, CreateNewVersion oder ForkVersion.

## Revocation

Eine Revocation invalidiert die FrameSession. Gruende sind unter anderem OwnerRequested, PolicyChanged, HeartbeatLost, SecurityViolation, Timeout, UserCancelled und GuestDisconnected. Das Ergebnis muss den Guest Frame ungueltig machen und das Ding beim Owner logisch entsperren.

## Glass Edge PDF Frame Demo

MA007.04 verbindet FrameSession erstmals mit Glass Edge und einer echten Sample-PDF. Die Zielablage sieht `PDF liegt hier im Frame`, erhaelt aber keine freie PDF-Datei, keinen Originalpfad und keine kopierten PDF-Bytes. Der Renderer bleibt noch ein Metadaten-Placeholder; getestet wird die Frame- und Ownership-Mechanik.

## Glass Edge PDF Frame E2E

MA008.04 erweitert diesen Pfad zu einem E2E-Smoke. `ObjectEnteringEdge` fuehrt zu CarryLease und FrameSession. `FrameSessionOpen` und `FrameSessionReady` sind Teil des RKWP-Eventflows. Der Guest sieht weiterhin nur eine Frame-Repräsentation und keinen Originaldateizugriff.

## Owner/Guest State UX fuer Tests

MA009.08 fuehrt eine gemeinsame Zustandsuebersetzung fuer Owner- und Guest-Smokes ein. Sie ist noch keine finale Produktoberflaeche, aber sie verhindert, dass Tests wieder technische oder missverstaendliche Woerter anzeigen.

Owner:

- `LockedOnOwner` wird sichtbar zu `wartet auf Rueckgabe`.
- `Returned` wird sichtbar zu `zurueckgegeben`.
- `RecoveredByOwner` wird sichtbar zu `wieder verfuegbar`.

Guest:

- `FrameActive` wird sichtbar zu `liegt hier im Frame`.
- `Revoked` wird sichtbar zu `nicht verfuegbar`.
- `Expired` wird sichtbar zu `Verbindung verloren`.

Die Validierung prueft die sichtbaren Zustandstexte gegen verbotene Produktwoerter wie `uebertragen`, `empfangen`, `download`, `geraet`, `agent` und `workspace`.

## Windows PDF Frame Pilot

MA010.01 macht den FrameOnly-Pfad erstmals als lokalen Windows-Pilot sichtbar. `tools/run-windows-pdf-frame-pilot.ps1` startet zwei lokale Pilotflaechen:

- Ablage Windows Owner
- Ablage Windows Guest

Die FrameSession bleibt Original-Owned. Der Owner zeigt `ausgeliehen`, `wartet auf Rueckgabe`, `zurueckgegeben` und `wieder verfuegbar`. Die Guest-Ablage zeigt `liegt gleich hier`, `PDF liegt hier im Frame`, `zurueckgeben`, `nicht verfuegbar` und `Verbindung verloren`.

Der Pilot verwendet weiterhin `MetadataPreview`, solange der echte PDF-Seitenrenderer blockiert ist. No File Ingress bleibt hart: kein Guest-PDF, kein Originalpfad und keine kopierten PDF-Bytes.
