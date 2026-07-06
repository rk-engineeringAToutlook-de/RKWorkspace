# macOS RKWP Message Flow

Status: MA013.02 architecture stub  
Datum: 2026-07-06

## Ziel

Dieses Dokument beschreibt die minimale Nachrichtensequenz fuer macOS als Frame Guest Surface.

## Sequenz

```text
macOS App Start
  -> AblageIdentity load/create
  -> connect Windows Owner
  -> AblageHello
  -> AblageCapabilities
  <- CarryLeaseGranted
  <- FrameSessionOpen
  -> FrameSessionReady
  <- FrameUpdate
  -> CarryLeaseHeartbeat
  -> CarryLeaseReturn
  <- FrameClose
```

## AblageHello

Pflichtdaten:

- AblageId.
- Platform `MacOS`.
- SurfaceRole `FrameGuestSurface`.
- SecurityMode `DevelopmentAuthenticated` fuer SecureDev.
- Capabilities `FrameView`, `Heartbeat`, `Return`, `NoFileIngress`.

## FrameSession Empfangen

macOS akzeptiert eine `FrameSessionOpen` nur, wenn:

- SessionId bekannt ist.
- LeaseId bekannt ist.
- Policy `FrameOnly` oder kompatibel ist.
- OwnerAblageId zum Windows Owner passt.
- Payload keine Original-PDF-Datei ist.

## FrameUpdate Decodieren

V1 erwartet:

- FrameId.
- DisplayName.
- PreviewKind.
- FrameFormat.
- optional Base64-Framebild.
- RendererStatus.

Nicht erlaubt:

- OriginalFileBytes.
- OriginalPath.
- freier Dateiname als Speicherziel.

## Heartbeat

macOS sendet Heartbeats, solange der Frame aktiv ist. Ein fehlender Heartbeat loest auf Windows Recovery aus.

## Return

Return sendet:

- SessionId.
- LeaseId.
- FrameSessionId.
- sichtbaren Zustand `zurueckgeben`.
- keine Datei.

## Revocation

Bei `CarryLeaseRevoked` oder `FrameClose`:

- Frame lokal entfernen.
- Status `nicht verfuegbar` anzeigen.
- Speicher leeren.
- Auditlog schreiben.
