# iOS/iPadOS Haptics Plan

Status: MA011.09 planning baseline  
Datum: 2026-07-06

## Ziel

Haptik bestaetigt menschliche Wahrnehmung, nicht Technik. Sie meldet, dass ein Ding angekommen ist, gehalten wird oder zurueckgegeben wurde.

## APIs

Start mit einfachen UIKit-Generatoren:

- `UIImpactFeedbackGenerator`.
- `UINotificationFeedbackGenerator`.
- `UISelectionFeedbackGenerator`.

Spaeter optional:

- `CoreHaptics` fuer feinere Muster.

## Ereignisse

### FrameReady

Wenn ein Frame sichtbar wird:

```text
leichtes, kurzes Ankommen
```

Keine Alarmhaptik.

### Return

Wenn der Owner Return bestaetigt:

```text
kurzer, sauberer Abschluss
```

### ConnectionLost

Bei Verbindungsverlust:

```text
keine harte Warnung im ersten Test
sichtbarer Status reicht
```

## Testfragen

- Fuehlt sich der Frame angekommen an?
- Ist die Haptik ruhig genug?
- Stoert sie beim Lesen?
- Verstaerkt sie `liegt hier im Frame`?

## Grenzen

Haptik ist vorbereitet. Der erste Windows-Test validiert vor allem No File Ingress und Frame-Anzeige. Haptik darf den Test nicht blockieren, wenn ein Geraet sie nicht unterstuetzt.
