# Swift Model Plan

## Models

- `RkwpEnvelope`
- `RkwpMessageType`
- `AblageIdentity`
- `FrameCapsule`
- `OpenPdfContext`
- `OpenFrame`
- `FrameCachePolicy`
- `NoFileIngressReport`
- `ReturnRecoveryState`

## Mapping

`FrameCapsule` maps to RKWP messages:

- `CapsuleCreated`
- `CapsuleOpened`
- `CapsuleReturned`
- `CapsuleRevoked`
- `CapsuleRecovered`

`OpenFrame` maps to RKWP messages:

- `OpenFrameStarted`
- `OpenFrameReady`
- `OpenFrameReturned`
- `OpenFrameRevoked`
- `OpenFrameRecovered`

## Storage Rule

Frame data may live in memory. Original PDF bytes, paths, or files must not be stored.
