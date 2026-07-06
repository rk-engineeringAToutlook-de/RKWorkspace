# Real PDF Frame Viewer

Status: Draft  
Datum: 2026-07-05

## Ziel

Der PDF-Frame-Pfad soll eine echte PDF als ursprungsgebundenen Frame auf einer Gastablage zeigen, ohne dass die Gastablage eine freie PDF-Datei, einen Originalpfad oder kopierte PDF-Bytes erhaelt.

## Aktueller Stand

MA007.05 verbessert den bisherigen Smoke-Pfad:

- Sample-PDF wird als echtes PDF validiert.
- Owner bleibt Eigentuemer.
- CarryLease ist aktiv.
- FrameSession ist aktiv.
- Owner ist waehrend der Lease logisch gesperrt.
- Guest erhaelt eine sichere Frame-Repraesentation.
- Scroll und Zoom sind als Frame-Faehigkeiten vorbereitet.
- Rueckgabe und Recovery funktionieren.
- No File Ingress wird im Smoke geprueft.

## Renderer-Status

Der PDF-Inhalt wird noch nicht als echte Seite gerendert. Der aktuelle Stand nutzt `PdfFrameRepresentationKind.MetadataPreview`. Damit ist die Frame- und Ownership-Mechanik testbar, aber der visuelle PDF-Viewer ist noch nicht final.

Renderer-Blocker:

- Es ist noch keine PDF-Rendering-Abhaengigkeit entschieden.
- Der Renderer muss Owner-seitig arbeiten oder strikt FrameOnly bleiben.
- Der Guest darf keine Originaldatei und keine freie PDF-Kopie erhalten.
- Lizenz, Packaging und Plattformpfad muessen dokumentiert sein.

## Renderer-Kandidaten

- PDFium: technisch stark, Lizenz und Packaging pruefen.
- MuPDF: leistungsfaehig, Lizenz besonders sorgfaeltig pruefen.
- Windows PDF Preview Handler: Windows-nah, aber nicht plattformneutral.
- WebView2: nur Owner-seitig als Renderer denkbar, nicht als Dateiuebergabe an Guest.
- Skia/PDF: pruefen, ob Darstellung ohne Gast-Dateimaterialisierung stabil moeglich ist.

## Sicherheitsregel

Auch bei echtem Rendering bleibt:

- Originalablage besitzt die PDF.
- Zielablage sieht nur FrameUpdates.
- Zielablage bekommt keine PDF-Datei.
- Zielablage bekommt keinen Originalpfad.
- Zielablage bekommt keine kopierten PDF-Bytes als Datei.

## Smoke-Test

```powershell
.\tools\run-pdf-frame-smoke.ps1
```

Der Smoke prueft Sample-PDF, CarryLease, FrameSession, FrameRepresentation, GuestHasPdfFile `NO`, OriginalFileBytes `NO`, Rueckgabe, Recovery und `NoFileIngress: SUCCESS`.

Ab MA009.08 prueft der Smoke zusaetzlich die testbare Owner/Guest-State-UX:

- Owner sichtbar: `wartet auf Rueckgabe`
- Guest sichtbar: `liegt hier im Frame`
- Rueckgabe sichtbar: `zurueckgegeben`
- Recovery sichtbar: `wieder verfuegbar`
- Revocation sichtbar: `nicht verfuegbar`
- Ablauf sichtbar: `Verbindung verloren`
- sichtbare Zustandstexte enthalten keine verbotenen Produktwoerter

## Naechster Schritt

Der naechste echte Viewer-Schritt ist ein Owner-seitiger Renderer-Prototyp, der die erste Seite als Bild-/FrameUpdate erzeugt. Erst danach soll eine Gastoberflaeche das Bild anzeigen.
