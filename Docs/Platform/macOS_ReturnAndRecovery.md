# macOS Return und Recovery

Status: MA013.06 specification  
Datum: 2026-07-06

## Ziel

macOS muss Frames sauber zurueckgeben und bei Verbindungsverlust korrekt reagieren.

## Return

1. Benutzer waehlt `zurueckgeben`.
2. macOS sendet `CarryLeaseReturn`.
3. UI sperrt weitere Eingaben fuer diesen Frame.
4. MemoryOnly Frame-Cache wird geloescht.
5. Windows Owner entsperrt das Original.
6. macOS zeigt `wiederhergestellt` oder schliesst die Testoberflaeche.

Pflichtpayload:

```text
leaseId
frameSessionId
frameClose=true
guestKeptOriginalFile=false
```

## Recovery

Bei ConnectionLost:

- macOS zeigt `Verbindung verloren`.
- Heartbeat stoppt nicht still, sondern wird im Audit markiert.
- Frame bleibt nur in MemoryOnly bis Recovery oder Revocation.
- Nach Recovery zeigt macOS `wiederhergestellt`.
- Nach Revocation wird der Frame invalid.

## Revocation

Wenn Windows `CarryLeaseRevoked` sendet:

- Frame wird geschlossen oder deutlich deaktiviert.
- Cache wird geloescht.
- keine lokale Kopie bleibt uebrig.
- Audit schreibt `RevocationHandled`.

## Contract-Auswirkung

`contracts/macOS-guest/rkwp-macos-guest-contract-v0.1.json` enthaelt Return und Revocation als Pflichtflows. Der Contract-Test muss weiterhin melden:

```text
Return: OK
Revocation: OK
NoFileIngress: SUCCESS
```
