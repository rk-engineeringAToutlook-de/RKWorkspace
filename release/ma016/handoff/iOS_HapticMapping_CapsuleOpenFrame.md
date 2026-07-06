# iOS Haptic Mapping for Capsule and OpenFrame - MA016

## Principle

Haptics confirm human state. They are not decoration.

## Mapping

| Moment | Pattern | Intent |
| --- | --- | --- |
| CapsuleArrived | softArrival | The Ding has arrived here. |
| CapsuleOpened | softConfirm | The Kapsel answers the touch. |
| OpenFrameArrived | softArrival | The Frame is now here. |
| Return | softComplete | The user has given it back. |
| Recovery | softNotice | The owner recovered the thing. |
| Denied | firmButShort | Action is not allowed. |

## Rules

- No continuous vibration.
- No aggressive error pulse.
- No haptic pattern for technical transport events.
- Respect Reduce Motion and accessibility settings.
