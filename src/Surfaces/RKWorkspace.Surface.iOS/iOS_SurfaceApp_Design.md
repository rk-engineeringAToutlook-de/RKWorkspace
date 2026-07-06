# iOS/iPadOS Surface App Design

Status: Prepared handoff  
Datum: 2026-07-06

## Ziel

Die iOS/iPadOS Surface App zeigt RKWP Frames als mobile Ablage. Sie besitzt keine Owner-Datei.

## Rollen

| Rolle | Aufgabe |
| --- | --- |
| Surface Host | startet App und haelt AblageIdentity |
| DevTransport Client | verbindet sich zum Windows Owner |
| Frame Presenter | zeigt FrameRepresentation |
| Gesture Provider | erkennt TouchHold, LongPress, Drag und Cancel |
| Haptics Provider | gibt subtile Rueckmeldung |
| Return Controller | gibt Frame zurueck |
| Audit Logger | beweist No File Ingress |

## Minimaler Ablauf

1. App startet.
2. AblageIdentity wird erzeugt.
3. `AblageHello` an Windows Owner.
4. `AblageCapabilities` mit FrameOnly, NoFileIngress, Haptics, TouchInput, Return.
5. DevPairing wird vorbereitet.
6. CarryLease und FrameSession werden empfangen.
7. Frame wird angezeigt.
8. Haptik signalisiert Frame-Ankunft.
9. Touch-Geste kann Frame fokussieren.
10. Return wird gesendet.
11. Frame wird geschlossen.

## Frame Darstellung

Erster Stand:

- Metadaten-Frame oder Owner-gerenderte FrameRepresentation anzeigen.
- keine freie PDF-Datei materialisieren.
- sichtbare Sprache: `liegt hier im Frame`, `zurueckgeben`, `nicht verfuegbar`.

## Glass Edge

Die App simuliert am passenden Rand eine Glass Edge. Sie zeigt nicht technische Verbindung, sondern eine naechste Ablage im Arbeitsraum.

## No File Ingress Proof

Die App muss loggen:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

## Spaeter

- native PDF-/Frame-Darstellung.
- echte Haptik-Abstimmung auf iPhone und iPad.
- Share Extension fuer bewusst gewaehlte Quellen.
- produktive Secure Session.
