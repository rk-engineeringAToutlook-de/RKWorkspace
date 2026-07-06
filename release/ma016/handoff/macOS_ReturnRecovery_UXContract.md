# macOS Return and Recovery UX Contract - MA016

## Goal

macOS must express Return and Recovery as human workspace states, not technical protocol states.

## Visible States

- `liegt hier im Frame`
- `zurueckgeben`
- `Verbindung verloren`
- `nicht verfuegbar`
- `wiederhergestellt`

## Return

When the user chooses Return, the app clears MemoryOnly frame state and reports:

```text
Return: SUCCESS
GuestHasPdfFile: NO
FrameCache: MemoryOnly
```

## Recovery

When the owner recovers an expired or lost capsule, the macOS app must invalidate the guest frame and show:

```text
wiederhergestellt
```

The user must not be told that a transfer failed. The object is no longer available here because the owner has recovered it.

## Forbidden Visible Words

- Transfer
- Download
- Upload
- Sync
- Device
- Agent
- Endpoint
- Server
- Client
