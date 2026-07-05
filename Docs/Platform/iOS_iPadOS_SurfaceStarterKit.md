# iOS and iPadOS Surface Starter Kit

Dokument-ID: RKWS-PLATFORM-IOS-IPADOS-001
Status: Draft
Datum: 2026-07-05

## Ziel

iPhone und iPad werden als mobile Ablagen vorbereitet.

Die erste native Surface App zeigt RKWP Frames, bereitet Haptik vor und simuliert eine Glass Edge am Rand. Sie speichert keine freie Originaldatei.

## Produktrolle

iOS/iPadOS ist eine mobile Ablage im Arbeitsraum.

Erste Rolle:

- FrameGuestSurface
- Touch-Gesten
- Haptik
- Glass Edge am Rand
- Frame anzeigen
- No File Ingress pruefen

## No File Ingress

Pflicht:

- keine PDF-Datei in App-Container, Downloads oder temporaeren freien Dateien speichern
- keine Originalbytes als Datei materialisieren
- Windows/macOS Owner bleibt Owner
- FrameOnly respektieren
- Return/Close invalidiert Frame

## Native Zielrichtung

Final ist native App ueber Xcode.

PWA ist nur Uebergang. Browser-Chrome, Adressleiste und Browser-Grenzen stoeren das Gefuehl eines Arbeitsraums.

## Erste Quellen

Sicher vorbereiten:

- RK Workspace App als eigene Ablage
- Document Picker
- Share Extension
- Pasteboard bewusst und sparsam

Nicht als erste Annahme:

- globale Erfassung beliebiger App-Inhalte
- Umgehen der iOS-Sandbox
- stille Dateiuebernahme

## Gesten

Zu pruefen:

- TouchHold
- Drei-Finger-Langdruck
- LongPress
- Drag nach Edge
- Cancel
- haptisches Feedback bei Ankunft und Ablegen

## Erster iPad-Test

Windows besitzt PDF.
iPad zeigt PDF-Frame.
iPad bekommt keine PDF-Datei.
Windows bleibt Owner.
iPad gibt zurueck.

## Xcode-Hinweise

macOS-Codex baut das Xcode-Projekt.

Pruefen:

- Signing/Development Team
- USB-Geraet
- Local Network Permission fuer DevTransport
- Haptics API
- File/document permissions
- Logging fuer No File Ingress

## Offene Blocker

- kein Xcode in diesem Windows-Thread
- DevTransport zu iOS/iPadOS folgt in MA008
- native Renderer-/FramePresenter-Entscheidung offen
- finale Geste offen
