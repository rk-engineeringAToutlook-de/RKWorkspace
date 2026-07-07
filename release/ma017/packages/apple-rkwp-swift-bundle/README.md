# Apple RKWP Swift Bundle - MA017

Status: final handoff bundle
Datum: 2026-07-07

## Zweck

Dieses Bundle ist die Startbasis fuer macOS, iPadOS und iOS.

Es definiert nur RKWP Envelope- und Payload-Modelle.
Es implementiert keine produktive Netzwerkverbindung und schreibt keine Originaldateien.

## Dateien

- `RKWPModels.swift`: Swift Codable Modelle.
- `samples/capsule-created.json`: Capsule Sample Message.
- `samples/openframe-started.json`: OpenFrame Sample Message.
- `samples/no-file-ingress-result.json`: No File Ingress Sample Result.

## Regeln

- `containsOriginalFileBytes` muss immer `false` sein.
- `hasOriginalPath` muss immer `false` sein.
- `guestKeptOriginalFile` muss immer `false` sein.
- Frame Cache bleibt MemoryOnly.
- Besitz bleibt beim Owner.

## Erste Integration

1. Neues macOS SwiftUI Projekt in Xcode erstellen.
2. `RKWPModels.swift` in das Projekt kopieren.
3. Samples in Unit Tests laden.
4. No File Ingress Assertions vor jeder UI-Anzeige pruefen.
5. Sichtbare Texte mit `macOS_START_HERE.md` abgleichen.
