# Renderer Sandbox Model

Status: MA013.36  
Datum: 2026-07-06

## Ziel

PDF- und Frame-Renderer sind Angriffsfläche. Ein kaputtes oder boesartiges PDF darf weder RK Workspace Core noch Owner-Originale, Guest-Ablagen oder Transport-Sessions gefaehrden.

## Produktmodell

```text
Owner Shell
  -> Renderer Broker
  -> isolierter Renderer-Prozess
  -> PageFrame / TileFrame
  -> FrameCache MemoryOnly
  -> RKWP FrameUpdate
```

## Prozessisolation

- Renderer laeuft ausserhalb des Shell-/Agent-Hauptprozesses.
- Crash beendet nur den Renderer-Prozess.
- Broker setzt Timeout und Abbruch.
- Nach Crash wird Owner freigegeben oder in Recovery gesetzt.

## Least Privilege

- Nur Lesezugriff auf das konkrete Owner-PDF.
- Kein Schreibzugriff ausser kurzlebige interne Sandbox-Artefakte.
- Kein Zugriff auf Guest-Dateisystem.
- Keine Shell-, Clipboard- oder UI-Automation aus dem Renderer.

## Netzwerk

Renderer-Prozess hat im Produktpfad kein Netzwerk. Updates, Telemetrie und Download sind nicht Teil des Renderer-Prozesses.

## Temp Handling

- Standard: MemoryOnly.
- Temp-Dateien nur verschluesselt und nur fuer Renderer-internen Kurzlebenszyklus.
- Temp wird bei FrameClose, Crash, Timeout und Recovery geloescht.
- Keine Originalbytes im Guest-Cache.

## Malicious PDF

Kaputte PDFs muessen kontrolliert scheitern:

- Empty file
- Invalid PDF
- Huge metadata
- Corrupt header
- Encrypted PDF planned

Erwartung:

- kein Crash im Hauptprozess
- klare Fehlermeldung
- Owner entsperrt oder Recovery
- Audit-Event

## Renderer Updates

- Version und Hash jedes Renderer-Binaries werden dokumentiert.
- Security Advisory Prozess ist Pflicht vor Produktfreigabe.
- Update-Rollback darf keine bestehenden Original-Owned-Sessions beschaedigen.

## Audit

Audit muss mindestens erfassen:

- RendererStart
- RendererFailure
- MalformedPdfRejected
- FrameRendered
- TileRendered
- NoFileIngressChecked
- CacheEvicted

## Aktueller MA013-Stand

Die Sandbox ist dokumentiert und die Regression fuer kaputte PDFs ist modelliert. Eine echte Prozesssandbox folgt nach der Renderer-Produktentscheidung.
