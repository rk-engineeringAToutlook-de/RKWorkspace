# macOS Frame Guest Surface Design

Status: Prepared handoff  
Datum: 2026-07-06

## Ziel

Die macOS Frame Guest Surface zeigt einen RKWP Frame, ohne die Owner-PDF zu besitzen.

## Rollen

| Rolle | Aufgabe |
| --- | --- |
| Surface Host | startet App/Host und haelt AblageIdentity |
| DevTransport Client | verbindet sich zum Windows Owner |
| Frame Presenter | zeigt FrameRepresentation |
| Input Channel | sendet nur policygebundene Eingaben |
| Return Controller | gibt Frame zurueck |
| Audit Logger | beweist No File Ingress |

## Minimaler Ablauf

1. App startet.
2. AblageIdentity wird erzeugt.
3. `AblageHello` an Windows Owner.
4. `AblageCapabilities` mit FrameOnly, NoFileIngress, Heartbeat, Return.
5. DevPairing wird angefordert.
6. CarryLease und FrameSession werden empfangen.
7. Frame wird angezeigt.
8. Heartbeat wird gesendet.
9. Return wird gesendet.
10. Frame wird geschlossen.

## Frame Darstellung

Erster Stand:

- Metadaten-Frame oder Owner-gerenderte FrameRepresentation anzeigen.
- PDFKit erst nutzen, wenn sichergestellt ist, dass keine freie PDF-Datei auf macOS entsteht.
- sichtbare Sprache: `liegt hier im Frame`, `zurueckgeben`, `nicht verfuegbar`.

## No File Ingress Proof

macOS muss loggen:

```text
GuestHasPdfFile: NO
GuestHasOriginalPath: NO
OriginalFileBytes: NO
NoFileIngress: SUCCESS
```

## Spaeter

- Glass Edge am passenden Bildschirmrand.
- Trackpad-Geste.
- Haptik/Force Touch.
- ChangeSet UI fuer Annotation.
- produktive Secure Session.
