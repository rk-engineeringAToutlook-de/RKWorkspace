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

Der Gast erhaelt nicht:

- Originaldateipfad
- Originaldatei-Bytes
- lokale PDF-Kopie

## Rendering-Status

MA007.00 liest eine echte Sample-PDF, validiert Header, Metadaten, Hash und Page-Anzahl und erzeugt daraus eine sichere Frame-Repräsentation. Ein echter Bitmap-/Page-Renderer ist noch nicht aktiv.

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

## Revocation

Eine Revocation invalidiert die FrameSession. Gruende sind unter anderem OwnerRequested, PolicyChanged, HeartbeatLost, SecurityViolation, Timeout, UserCancelled und GuestDisconnected. Das Ergebnis muss den Guest Frame ungueltig machen und das Ding beim Owner logisch entsperren.

## Glass Edge PDF Frame Demo

MA007.04 verbindet FrameSession erstmals mit Glass Edge und einer echten Sample-PDF. Die Zielablage sieht `PDF liegt hier im Frame`, erhaelt aber keine freie PDF-Datei, keinen Originalpfad und keine kopierten PDF-Bytes. Der Renderer bleibt noch ein Metadaten-Placeholder; getestet wird die Frame- und Ownership-Mechanik.
