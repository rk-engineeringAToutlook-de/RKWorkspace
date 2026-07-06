# macOS Codex Auftrag: MA013 RKWP Client

Status: Prepared  
Datum: 2026-07-06

## Ziel

Baue den minimalen macOS RKWP Client fuer die erste echte macOS Frame Guest Surface. Windows bleibt Owner. macOS zeigt nur Frames und speichert keine PDF-Datei.

## Lies Zuerst

1. `Docs/Codex/CURRENT_CONTEXT.md`
2. `Docs/Platform/macOS_RepositoryBootstrap.md`
3. `release/handoff/macOS_Codex_MA013_Bootstrap.md`
4. `release/schema/rkwp-envelope-schema-v0.1.json`
5. `src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-client-architecture.md`
6. `src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-message-flow.md`
7. `src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-devtransport-client.md`
8. `src/Surfaces/RKWorkspace.Surface.macOS/macos-rkwp-securedev-client.md`
9. `Docs/Platform/macOS_FrameGuestUI.md`
10. `Docs/Platform/macOS_NoFileIngressChecklist.md`
11. `Docs/Platform/macOS_ReturnAndRecovery.md`
12. `Docs/Platform/macOS_FrameRenderingStrategy.md`
13. `Docs/Platform/macOS_DevAgentPackaging.md`
14. `Docs/Platform/macOS_GestureAndGlassEdge.md`
15. `contracts/macOS-guest/rkwp-macos-guest-contract-v0.1.json`
16. `Docs/Protocol/RKWP_ProtocolFoundation.md`
17. `Docs/Protocol/RKWP_SecureSession.md`
18. `src/Surfaces/RKWorkspace.Surface.Abstractions`

## Aufgaben

1. macOS AblageIdentity laden oder erzeugen.
2. DevTransport Client fuer `rkwp+tcp-dev://<windows-ip>:57100` vorbereiten.
3. SecureDev Session als DevelopmentAuthenticated markieren.
4. `AblageHello` senden.
5. DevPairing mit Windows vorbereiten.
6. `FrameSessionOpen` empfangen.
7. `FrameUpdate` decodieren.
8. Frame anzeigen.
9. Heartbeat senden.
10. Return senden.
11. Revocation empfangen.
12. Audit-Log schreiben.
13. No File Ingress pruefen.
14. sichtbare Sprache gemaess macOS Frame Guest UI einhalten.
15. Return, Recovery und Revocation gemaess Contract behandeln.

## Erfolg

```text
Platform: MacOS
AblageHello: OK
SecureDevSession: Active
FrameSession: Active
FrameUpdate: OK
Heartbeat: OK
Return: SUCCESS
Revocation: HANDLED
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

## Blocker Ehrlich Melden

- DevTransport noch nicht echt verbunden.
- SecureDev nur DevelopmentAuthenticated.
- PDF-/Frame-Renderer nur Mock.
- macOS Permissions fehlen.
- Windows Owner nicht erreichbar.
