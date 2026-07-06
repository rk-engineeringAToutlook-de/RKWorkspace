# MA016 iOS/iPadOS Final Handoff

## Goal

Build and install a native iPad/iPhone Guest Surface through Xcode that displays a controlled RKWP frame without file ingress.

## Required inputs

- `release/ma016/handoff/iOS_START_HERE.md`
- `release/ma016/handoff/iOS_HapticMapping_CapsuleOpenFrame.md`
- `release/ma016/handoff/iPad_FrameCapsuleLayoutSpec.md`
- `release/ma016/handoff/iPhone_CompactCapsuleLayoutSpec.md`
- `release/ma016/schemas/ios-capsule-contract.v0.1.json`
- `release/ma016/schemas/ios-openframe-contract.v0.1.json`
- `release/ma016/schemas/ios-no-file-ingress-sandbox-contract.v0.1.json`

## Required proof

```text
iOSGuestIdentity: OK
PrimaryPlatform: IPadOS
PhonePlatform: IOS
FrameView: OK
GlobalAppCapture: FALSE
NoFileIngress: SUCCESS
RESULT: SUCCESS
```

## Non-negotiable

The app sandbox must not contain the original PDF. Files App, Share Sheet and pasteboard paths must not bypass FrameOnly semantics.
