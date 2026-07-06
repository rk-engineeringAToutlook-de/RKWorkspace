# iOS No File Ingress Sandbox Checklist

Status: MA013.15 checklist  
Datum: 2026-07-06

## Ziel

iOS/iPadOS darf keine freie Owner-Datei speichern.

## Zu pruefen

- App Container.
- Documents.
- Library/Caches.
- tmp.
- Files App Export.
- Share Extension Container.
- Pasteboard-Nutzung.

## Regeln

- keine PDF-Datei speichern.
- kein Originalpfad speichern.
- keine Originalbytes als Datei.
- nur MemoryOnly FrameCache.
- Cache bei Return loeschen.
- Cache bei Revocation loeschen.
- Files-App-Export nur spaeter mit expliziter Policy.

## Debug-Ausgabe

```text
AppContainerOriginalPdf: NO
DocumentsOriginalPdf: NO
CachesOriginalPdf: NO
TempOriginalPdf: NO
FilesAppOriginalPdf: NO
GuestHasPdfFile: NO
NoFileIngress: SUCCESS
```

## Manuelle Pruefung

1. App per Xcode installieren.
2. Frame anzeigen lassen.
3. Xcode Devices and Simulators Container laden.
4. Documents/Caches/tmp pruefen.
5. Return ausloesen.
6. Container erneut pruefen.
