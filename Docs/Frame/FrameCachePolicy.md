# Frame Cache Policy

Status: MA010.07

## Ziel

Frame-Daten duerfen effizient gehalten werden, ohne dass die Gastablage eine Originaldatei bekommt.

Cache ist kein OwnershipTransfer. Cache ist temporaer, kontrolliert und loeschbar.

## Scopes

- `MemoryOnly`
- `TemporaryEncrypted`
- `Disabled`
- `DevInspectable`

## Defaults

| Umgebung | Default |
| --- | --- |
| Development | MemoryOnly |
| CriticalInfrastructure | MemoryOnly oder Disabled |
| Production | MemoryOnly, spaeter ggf. TemporaryEncrypted |

`TemporaryEncrypted` ist nur vorbereitet. Es wird erst aktiviert, wenn Schluessel, Lebensdauer und Loeschverhalten produktionsnah implementiert sind.

`DevInspectable` ist nur fuer Development erlaubt. `CriticalInfrastructure` blockiert `DevInspectable`.

## Erlaubt

FrameCache darf:

- gerenderte Bildframes speichern.
- temporaer halten.
- bei FrameClose loeschen.
- bei Revocation loeschen.
- bei Recovery loeschen.

## Verboten

FrameCache darf nicht:

- Original-PDF speichern.
- PDF-Bytes speichern.
- Originaldatei rekonstruierbar speichern.
- Dateipfad zum Original geben.
- OwnershipTransfer ersetzen.

## Aktueller Stand

Implementiert in:

```text
src/Frame/RKWorkspace.Frame.Pdf/
```

Modelle:

- `FrameCachePolicy`
- `FrameCacheEntry`
- `FrameCacheScope`
- `FrameCacheEvictionReason`
- `FrameCacheDiagnostics`
- `FrameCache`

Der PDF-Smoke prueft:

- `FrameCacheScope: MemoryOnly`
- `FrameCacheOriginalBytes: NO`
- `FrameCacheFileWrites: 0`
- `FrameCacheClose: CLEARED`
- `DevInspectableDevelopment: OK`
- `CriticalBlocksDevInspectable: OK`
