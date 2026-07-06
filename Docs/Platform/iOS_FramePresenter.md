# iOS Frame Presenter

Status: MA013.13 specification  
Datum: 2026-07-06

## Ziel

iPad und iPhone zeigen RKWP FrameUpdates an, ohne PDF-Dateien zu speichern.

## UI Optionen

- SwiftUI `Image` fuer einfache Bitmap-Frames.
- UIKit `UIImageView` fuer feinere Performance-Kontrolle.
- Metal spaeter fuer sehr grosse Frames oder flüssige Tile-Updates.

## Rendering

V1 zeigt komplette PNG/JPEG/Bitmap Frames. Tile Updates kommen spaeter.

## Cache

- MemoryOnly.
- letzter Frame im RAM.
- kein Disk Cache.
- Cache leeren bei Return, Revocation, App Kill und Recovery-Abbruch.

## iPad

- grossflaechige FrameView.
- Split View beruecksichtigen.
- Orientation wechseln ohne Frameverlust.

## iPhone

- Compact UI.
- Status reduziert.
- Return weiterhin erreichbar.
- Debug nur ausklappbar.

## Haptics

Haptics bestaetigen nicht Technik, sondern Zustand:

- Frame kommt an.
- Glass Edge aktiv.
- Return bestaetigt.
- Verbindung verloren.

## No File Ingress

Der Frame Presenter darf keinen PDF-Renderer auf Originalbytes nutzen. Er zeigt nur Owner-gerenderte Frames.
