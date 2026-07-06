# macOS Capsule and OpenFrame Contract

macOS must support FrameCapsule and OpenFrame as two views of the same Original-Owned RKWP lifecycle.

## Capsule

The capsule is the visible guest-side handle for a closed PDF thing.

## OpenFrame

The OpenFrame is the visible guest-side state for an already-open PDF context.

## Shared Constraints

- Windows remains owner.
- No File Ingress is mandatory.
- Return clears MemoryOnly cache.
- Recovery invalidates guest frame.
- Visible text remains human and avoids technical transport words.
