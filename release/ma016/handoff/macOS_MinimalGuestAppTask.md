# macOS Minimal Guest App Task - MA016

## Goal

Create a minimal native macOS app in Xcode that acts as a RK Workspace Guest Ablage.

The app receives RKWP frame messages, shows controlled frame content, supports Return and Recovery, and proves No File Ingress.

## Minimum Implementation

- Swift or SwiftUI shell.
- Ablage identity loaded from local dev config.
- RKWP DevTransport client stub or local message replay.
- FrameCapsule view.
- OpenFrame view.
- Return button using visible text `zurueckgeben`.
- Recovery display using visible text `wiederhergestellt`.
- MemoryOnly frame cache.
- No File Ingress verification command output.

## Explicit Non-Goals

- No production networking.
- No ownership transfer.
- No original PDF materialization.
- No platform-wide overlay.
- No final UX polish.

## Required RKWP Flows

- `AblageHello`
- `AblageCapabilities`
- `FrameSessionOpen`
- `FrameUpdate`
- `FrameInput` if allowed
- `CarryLeaseHeartbeat`
- `CarryLeaseReturn`
- `CarryLeaseRevoked`

## Required Smoke Output

```text
macOSGuestAblage: STARTED
FrameCapsule: OK
OpenFrame: OK
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
GuestHasCopiedPdfBytes: NO
FrameCache: MemoryOnly
Return: SUCCESS
Recovery: SUCCESS
RESULT: SUCCESS
```

## Handoff Result

The app is ready for the first Windows-to-macOS local pilot when the smoke output above is reproducible on macOS.
