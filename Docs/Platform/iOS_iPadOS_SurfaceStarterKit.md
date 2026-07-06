# iOS and iPadOS Surface Starter Kit

Dokument-ID: RKWS-PLATFORM-IOS-IPADOS-001  
Status: Draft  
Datum: 2026-07-06

## Ziel

iPhone und iPad werden als mobile Ablagen vorbereitet. Die erste native Surface App zeigt RKWP Frames, fuehlt sich wie eine Ablage an und speichert keine freie Originaldatei.

## Produktrolle

iOS/iPadOS Surface ist:

- mobile Ablage.
- Frame Guest Surface.
- Touch- und Haptikflaeche.
- Glass-Edge-Gegenkante.
- No-File-Ingress-Beweisflaeche.

iOS/iPadOS Surface ist nicht:

- Dateiempfaenger.
- Sync-Ziel.
- globaler App-Scraper.
- Standard-Ort fuer Ownership Transfer.

## Minimaler Aufbau

1. Xcode-Projekt fuer iOS/iPadOS anlegen.
2. iPhone/iPad per USB starten.
3. AblageIdentity erzeugen.
4. RKWP DevTransport Client vorbereiten.
5. DevPairing mit Windows Owner vorbereiten.
6. FrameSession empfangen.
7. Frame anzeigen.
8. No File Ingress pruefen.
9. Haptik bei Frame-Ankunft vorbereiten.
10. Glass Edge am Rand simulieren.
11. Return senden.
12. Logs schreiben.

## No File Ingress

Pflicht:

- keine PDF-Datei in App-Container, Downloads, Files-App oder temporaeren freien Dateien speichern.
- keine Originalbytes als Datei materialisieren.
- keinen Windows-Originalpfad als lokale Datei behandeln.
- FrameOnly respektieren.
- Return/Close invalidiert Frame.

## Erste Quellen

Sicher vorbereiten:

- RK Workspace App als eigene Ablage.
- Share Extension.
- Document Picker.
- Pasteboard bewusst und sparsam.

Nicht als erste Annahme:

- globale Erfassung beliebiger App-Inhalte.
- Umgehen der iOS-Sandbox.
- stille Dateiuebernahme.

## Native Zielrichtung

Final ist native App ueber Xcode.

PWA ist nur Uebergang. Browser-Chrome, Adressleiste und Browser-Grenzen stoeren das Gefuehl eines Arbeitsraums.

## Gesten und Haptik

Zu pruefen:

- TouchHold.
- Drei-Finger-Langdruck.
- LongPress.
- Drag nach Edge.
- Cancel.
- haptisches Feedback bei Ankunft, Greifen, Edge-Aktivierung, Ablegen und Denied.

## Handoff

Der vollstaendige iOS/iPadOS-Codex-Auftrag liegt hier:

```text
release/handoff/iOS_iPadOS_Codex_MA009_SurfaceApp.md
```

MA008-Referenz:

```text
release/handoff/iOS_iPadOS_MA008_Handoff.md
```

## Testpfad

Erster echter Test:

1. Windows besitzt PDF.
2. iPad meldet sich als mobile Ablage.
3. Windows gibt FrameOnly frei.
4. iPad zeigt PDF-Frame.
5. iPad bekommt keine PDF-Datei.
6. iPad meldet HapticsPrepared.
7. iPad sendet Return.
8. Windows bestaetigt Rueckgabe oder Recovery.

## Xcode-Hinweise

macOS-Codex baut das Xcode-Projekt.

Pruefen:

- Signing/Development Team.
- USB-Geraet.
- Developer Mode auf dem Geraet.
- Local Network Permission fuer DevTransport.
- Haptics API.
- File/document permissions.
- Logging fuer No File Ingress.

## Offene Blocker

- kein Xcode in diesem Windows-Thread.
- netzwerkfaehiger DevTransport zu iOS/iPadOS fehlt noch.
- native Renderer-/FramePresenter-Entscheidung offen.
- finale Geste offen.
