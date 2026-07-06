# Windows PDF Frame Pilot

Status: MA010.01

Der Windows PDF Frame Pilot macht den bestehenden Original-Owned PDF-Frame-Pfad lokal sichtbar. Er startet noch keine finale Produktoberflaeche, zeigt aber zwei lokale Ablagen als Pilotflaechen:

- Ablage Windows Owner
- Ablage Windows Guest

Die PDF bleibt beim Owner. Die Guest-Ablage bekommt nur eine Frame-Repraesentation mit Metadaten und darf keinen Originalpfad, keine Originaldatei und keine kopierten PDF-Bytes erhalten.

## Start

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1
.\tools\run-windows-pdf-frame-pilot.ps1 -SmokeTest
.\tools\run-windows-pdf-frame-pilot.ps1 -PdfPath "samples/Objects/Rechnung.pdf"
.\tools\run-windows-pdf-frame-pilot.ps1 -OwnerVisible
.\tools\run-windows-pdf-frame-pilot.ps1 -GuestVisible
```

## Sichtbarer Ablauf

Owner:

- PDF liegt auf der Owner-Ablage.
- Zustand beginnt mit `wieder verfuegbar`.
- Nach FrameOnly-Lease wird der Owner `ausgeliehen` und `wartet auf Rueckgabe`.
- Nach Rueckgabe steht `zurueckgegeben`.
- Recovery fuehrt wieder zu `wieder verfuegbar`.

Guest:

- Frame beginnt mit `liegt gleich hier`.
- Aktiver Frame zeigt `PDF liegt hier im Frame`.
- Rueckgabe ist vorbereitet.
- Revocation fuehrt zu `nicht verfuegbar`.
- Heartbeat-Loss fuehrt zu `Verbindung verloren`.

## Rendering-Status

Der Pilot nutzt aktuell dieselbe sichere `MetadataPreview` wie die bisherigen PDF-Frame-Smokes. Ein echter Seitenrenderer ist noch nicht aktiv. `RendererStatus: RendererBlocked` bleibt deshalb sichtbar und wird in MA010.06 gezielt bearbeitet.

## No File Ingress

Der Smoke-Test prueft:

- `GuestHasPdfFile: NO`
- `GuestHasOriginalPath: NO`
- `GuestHasCopiedPdfBytes: NO`
- `NoFileIngress: SUCCESS`

Damit bleibt AP031 ein sichtbarer Pilot, aber kein Datei-Ingress auf die Guest-Ablage.
