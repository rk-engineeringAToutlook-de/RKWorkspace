# Windows PDF Frame Pilot

Status: MA011.05

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

## Sichtbarer Owner-Test-Modus

Der Standardstart zeigt keine finale UX, sondern eine bewusst einfache Testanzeige:

- `Originalablage`
- `Gastablage`
- `Sicherheitsstatus`

Technische Werte wie OwnerAblageId, GuestAblageId, LeaseId, FrameSessionId, Renderername und Pfade erscheinen nur mit:

```powershell
.\tools\run-windows-pdf-frame-pilot.ps1 -Debug
```

Der Smoke-Test gibt zusaetzlich maschinenlesbare Pruefzeilen aus.

## Sichtbarer Ablauf

Owner:

- PDF liegt auf der Owner-Ablage.
- Zustand beginnt mit `verfuegbar`.
- Nach FrameOnly-Lease wird der Owner `ausgeliehen` und `wartet auf Rueckgabe`.
- Nach Rueckgabe steht `wieder verfuegbar`.
- Recovery fuehrt zu `wiederhergestellt`.

Guest:

- Frame beginnt mit `liegt gleich hier`.
- Aktiver Frame zeigt `PDF liegt hier im Frame`.
- Rueckgabe ist vorbereitet.
- Revocation fuehrt zu `nicht verfuegbar`.
- Heartbeat-Loss fuehrt zu `Verbindung verloren`.

Sichtbare Texte vermeiden weiterhin technische Begriffe wie Transfer, Download, Upload, empfangen oder gesendet. Diese Begriffe duerfen nur in technischer Dokumentation oder Diagnose vorkommen, nicht in der Owner-Test-Anzeige.

## Rendering-Status

Der Pilot nutzt weiterhin denselben sicheren FrameOnly-Pfad wie die PDF-Frame-Smokes. Ab MA011.04 kann der Development-Pfad mit `PopplerPdfFrameRenderer` die erste Seite als PNG-Frame rendern. Falls Poppler fehlt, bleibt `RendererStatus: RendererBlocked` als expliziter Blocker sichtbar.

## No File Ingress

Der Smoke-Test prueft:

- `GuestHasPdfFile: NO`
- `GuestHasOriginalPath: NO`
- `GuestHasCopiedPdfBytes: NO`
- `NoFileIngress: SUCCESS`

Damit bleibt AP031 ein sichtbarer Pilot, aber kein Datei-Ingress auf die Guest-Ablage.

## MA011.05 Stabilisierung

MA011.05 stabilisiert den Pilot fuer Owner-Tests:

- Originalablage zeigt PDF-Name, Status `ausgeliehen`, Bearbeitungssperre, Rueckgabe und Recovery.
- Gastablage zeigt Frame, FrameStatus, Rueckgabe, Verbindung verloren und `keine PDF-Datei vorhanden`.
- No File Ingress ist im Sicherheitsstatus und im Smoke-Log sichtbar.
- Debug-Texte sind optional und stoeren den normalen Owner-Test-Modus nicht.
