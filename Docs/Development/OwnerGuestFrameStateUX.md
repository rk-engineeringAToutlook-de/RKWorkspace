# Owner Guest Frame State UX

Status: Draft  
Datum: 2026-07-06

## Ziel

MA009.08 macht den Original-Owned Frame-Zustand fuer Tests sichtbar.

Es geht nicht um finales Produktdesign. Es geht darum, dass Owner- und Gastablage in lokalen Smokes nachvollziehbar zeigen:

- das Original bleibt beim Owner
- die Gastablage sieht nur einen Frame
- der Owner ist waehrend der Lease logisch gesperrt
- Rueckgabe und Recovery sind sichtbar
- die Gastablage besitzt keine freie Datei

## Sichtbare Owner-Texte

| Zustand | Sichtbarer Text |
| --- | --- |
| OriginalOwned | wieder verfuegbar |
| LeasedToGuest | ausgeliehen |
| LockedOnOwner | wartet auf Rueckgabe |
| Returned | zurueckgegeben |
| RecoveredByOwner | wieder verfuegbar |

## Sichtbare Guest-Texte

| Zustand | Sichtbarer Text |
| --- | --- |
| FrameOpening | liegt gleich hier |
| FrameReady | liegt hier im Frame |
| FrameActive | liegt hier im Frame |
| Returning | zurueckgeben |
| Revoked | nicht verfuegbar |
| Expired | Verbindung verloren |

## Sprachregel

Die testbare Zustandsanzeige nutzt bewusst keine Woerter wie:

- uebertragen
- empfangen
- heruntergeladen
- gesendet
- download
- upload
- sync
- server
- client
- endpoint
- device
- geraet
- agent
- workspace
- ipc
- request
- response

Technische Begriffe duerfen in Code, Protokoll und Diagnoseausgaben bleiben. Die neue Validierung bezieht sich auf die sichtbaren Zustands-Texte.

## Implementierung

Die gemeinsame Logik liegt in:

```text
src/Frame/RKWorkspace.Frame.Pdf/OwnerGuestFrameStateUx.cs
```

Genutzt wird sie durch:

```text
src/Tools/RKWorkspace.PdfFrameOwner
src/Tools/RKWorkspace.FrameGuestSurface
src/Tools/RKWorkspace.WindowsLocalFrameE2E
```

## Verifikation

```powershell
.\tools\run-pdf-frame-smoke.ps1
.\tools\run-windows-local-frame-e2e.ps1 -SmokeTest
.\tools\run-tests.ps1
```

Die Smokes pruefen:

- Owner zeigt `wartet auf Rueckgabe`
- Owner zeigt nach Rueckgabe `zurueckgegeben`
- Owner zeigt nach Recovery `wieder verfuegbar`
- Guest zeigt `liegt hier im Frame`
- Guest zeigt nach Revocation `nicht verfuegbar`
- Guest zeigt nach Ablauf `Verbindung verloren`
- sichtbare Zustands-Texte enthalten keine verbotenen Woerter

## Grenzen

Der aktuelle Stand ist eine CLI-/Smoke-Visualisierung fuer Tests. Eine finale Shell- oder Surface-Darstellung folgt erst nach stabiler Frame- und Cross-Device-Basis.
