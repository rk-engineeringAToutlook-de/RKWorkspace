# macOS Frame Guest UI

Status: MA013.04 specification  
Datum: 2026-07-06

## Ziel

Die erste macOS-App zeigt nur einen RKWP Frame. Sie uebernimmt keine Datei und wird nicht als finale Produktoberflaeche verstanden.

Der Mensch soll sehen:

- Das Ding liegt hier im Frame.
- Die Verbindung ist intakt oder verloren.
- Das Ding kann zurueckgegeben werden.
- Fuer Development ist sichtbar, dass keine PDF-Datei auf macOS entstanden ist.

## Oberflaechenform

V1 darf ein schlichtes AppKit- oder SwiftUI-Fenster verwenden. Eine native Surface ohne klassisches Fenster bleibt Zielpfad, ist aber fuer den ersten Test nicht Pflicht.

Pflichtbereiche:

- FramePresenter: zeigt Owner-gerenderte Bitmap-Frames.
- Statuszeile: `liegt hier im Frame`, `Verbindung verloren`, `wiederhergestellt`.
- Aktion: `zurueckgeben`.
- Dev-Nachweis: `keine PDF-Datei vorhanden`.
- Optionaler Debugbereich: SessionId, LeaseId, Sequenz, Auditstatus.

## Sichtbare Sprache

Erlaubt:

- `liegt hier im Frame`
- `zurueckgeben`
- `Verbindung verloren`
- `wiederhergestellt`
- `nicht verfuegbar`

Nicht sichtbar als Primaersprache:

- Transfer
- Upload
- Download
- Sync
- Server
- Client
- Device
- Agent

## Statusmodell

| RKWP Zustand | sichtbarer Text |
| --- | --- |
| FrameSessionOpen | liegt gleich hier |
| FrameSessionReady | liegt hier im Frame |
| FrameUpdate | liegt hier im Frame |
| CarryLeaseReturn pending | zurueckgeben |
| CarryLeaseRevoked | nicht verfuegbar |
| ConnectionLost | Verbindung verloren |
| Recovery complete | wiederhergestellt |

## Debug und Audit

Debug ist optional und standardmaessig ausgeblendet. Wenn aktiv, darf er technische Begriffe zeigen, weil er nicht Teil der Owner-Oberflaeche ist.

Audit-Pflicht:

- FrameSessionOpen gesehen.
- FrameUpdate verarbeitet.
- Heartbeat gesendet.
- Return gesendet.
- Revocation verarbeitet.
- No File Ingress geprueft.

## No File Ingress

Die UI darf keine Speicher- oder Downloadaktion anbieten. macOS zeigt nur Frame-Daten im Speicher. Originalpfad, PDF-Datei und Originalbytes bleiben auf der Owner-Ablage.

Erfolg:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```
