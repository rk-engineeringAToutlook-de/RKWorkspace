# macOS No File Ingress Checklist

Status: MA013.05 checklist  
Datum: 2026-07-06

## Ziel

macOS muss beweisen, dass es einen PDF-Frame anzeigen kann, ohne eine PDF-Datei, einen Originalpfad oder Originalbytes zu behalten.

## Harte Regeln

- keine PDF-Datei im App-Sandbox-Container.
- keine PDF im Downloads-Ordner.
- keine PDF in temporaeren Dateien.
- keine PDF-Bytes als Datei.
- kein Originalpfad in UI, Log oder Cache.
- nur MemoryOnly Frame-Cache.
- Frame-Cache wird bei Return, Revocation und Recovery-Abbruch geloescht.
- Audit-Log bestaetigt No File Ingress.

## Manuelle Pruefschritte auf macOS

1. App starten.
2. Windows Owner verbinden.
3. Frame anzeigen lassen.
4. App-Sandbox-Container pruefen.
5. Downloads-Ordner pruefen.
6. temporaere Ordner pruefen.
7. Audit-Log pruefen.
8. Return ausloesen.
9. Cache erneut pruefen.

## Automatisierbare Pruefschritte

```bash
find "$HOME/Library/Containers" -iname "*.pdf"
find "$TMPDIR" -iname "*.pdf"
find "$HOME/Downloads" -iname "*.pdf"
```

Diese Befehle muessen fuer die macOS RK Workspace App keinen neuen PDF-Treffer liefern.

## Payload-Verbote

RKWP-Payloads duerfen nicht enthalten:

- `pdfBytes`
- `originalBytes`
- `originalFileBytes`
- `originalPath`
- `localPdfPath`
- `downloadPath`
- `filePath`

## Pflichtnachweise

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
GuestHasCopiedPdfBytes: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

## Audit

Mindestens ein Audit-Ereignis:

```text
NoFileIngressChecked
status=success
platform=macOS
frameOnly=true
```
