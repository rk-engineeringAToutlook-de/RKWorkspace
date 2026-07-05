# Glass Edge PDF Frame Demo

Status: Draft  
Datum: 2026-07-05

## Ziel

Die Demo verbindet erstmals Glass Edge, naechste Ablage, echte PDF, Original-Owned CarryLease, FrameSession und No File Ingress.

Der Ablauf ist bewusst lokal simuliert. Wichtig ist nicht finale Grafik, sondern die Semantik:

- Die PDF liegt auf der Originalablage Windows.
- Die naechste Ablage wird ueber `NearestAblageSelector` bestimmt.
- Eine Glass Edge wird an der passenden Kante aktiv.
- Die PDF wird als `OriginalThing` registriert.
- Eine `CarryLease` wird aktiv.
- Die Originalablage sperrt das Ding logisch.
- Die Zielablage sieht nur einen kontrollierten PDF-Frame.
- Die Zielablage erhaelt keine freie PDF-Datei, keinen Originalpfad und keine kopierten PDF-Bytes.
- Rueckgabe und Recovery geben den Owner wieder frei.

## Start

```powershell
.\tools\run-glass-edge-pdf-frame-demo.ps1
.\tools\run-glass-edge-pdf-frame-demo.ps1 -SmokeTest
.\tools\run-glass-edge-pdf-frame-demo.ps1 -PdfPath "samples/Objects/Rechnung.pdf"
```

## Sichtbare Sprache

Erlaubt:

- Ablage
- PDF liegt hier im Frame
- Zurueckgegeben
- Hier ablegen

Nicht verwenden:

- Senden
- Empfangen
- Upload
- Download
- Datei empfangen
- PDF heruntergeladen

## State Flow

```text
RestingOnOwnerAblage
Picked
Carried
GlassEdgeActive
ObjectEnteringEdge
FrameSessionOpen
PresentedOnGuest
ViewOnly
ReturningToOwner
ReturnedToOwner
```

Recovery simuliert Heartbeat-Verlust und fuehrt ueber GracePeriod zu `RecoveredByOwner`.

## Aktuelle Einschraenkungen

- Die naechste Ablage ist simuliert.
- Die Glass Edge ist CLI-/Smoke-simuliert.
- Die PDF wird noch nicht als echte Seite gerendert.
- Die Demo prueft Frame- und Ownership-Mechanik, nicht finale PDF-Grafik.
- Echte macOS/iOS/iPadOS/Android-Surfaces brauchen spaeter Plattformadapter, Gesten, Rendering und produktive Security.

## Warum wichtig

Diese Demo zeigt den ersten technischen Produktpfad fuer kritische digitale Dinge: nicht Datei uebertragen, sondern Ursprung behalten und nur einen kontrollierten Frame auf einer anderen Ablage darstellen.
