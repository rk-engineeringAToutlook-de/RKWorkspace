# iOS Xcode Project Bootstrap

Status: MA013.11 handoff  
Datum: 2026-07-06

## Ziel

iOS/iPadOS bekommt einen konkreten Xcode-Startauftrag fuer die erste native RK Workspace Surface App.

## Projekt

Vorschlag:

```text
RKWorkspaceSurface
Bundle Identifier: de.rkengineering.rkworkspace.surface
Deployment Target: iOS 17 / iPadOS 17 oder hoeher
Language: Swift
UI: SwiftUI mit UIKit-Bruecken bei Bedarf
```

## App-Zweck

Die App ist eine mobile Ablage fuer RKWP Frames. Sie speichert keine Owner-PDF und erzeugt keine freie Datei in der Files App.

## Erste Targets

- iPad Frame Guest Surface.
- iPhone Compact Frame Guest Surface.
- Shared RKWP Codable Models.
- Development Logging.

## Pflichtquellen

- `release/schema/rkwp-envelope-schema-v0.1.json`
- `Docs/Platform/iOS_RKWPClientFlow.md`
- `Docs/Platform/iOS_FramePresenter.md`
- `Docs/Platform/iOS_NoFileIngressSandboxChecklist.md`
- `Docs/Platform/iOS_USBDeviceTestRunbook.md`

## Netzwerk

Die App braucht Local Network Permission und verbindet sich im Lab mit dem Windows Owner.

```text
rkwp+tcp-dev://<windows-ip>:57100
```

## Codable Models

Aus dem RKWP JSON Schema werden Swift Codable Models abgeleitet. Pflichtfelder bleiben strikt:

- messageId
- messageType
- sessionId
- sourceAblageId
- targetAblageId
- timestamp
- sequenceNumber
- nonce

## No File Ingress

- kein PDF in Documents.
- kein PDF in Caches.
- kein PDF in Temp.
- kein Export in Files App ohne spaetere Policy.
- nur MemoryOnly FrameCache.

## USB-Test

Der erste echte Lauf erfolgt ueber macOS/Xcode und ein per USB angeschlossenes iPhone oder iPad.
