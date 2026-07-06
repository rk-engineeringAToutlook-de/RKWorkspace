# macOS RKWP DevTransport Client

Status: MA013.02 architecture stub  
Datum: 2026-07-06

## Ziel

Der DevTransport Client verbindet macOS im Labor mit Windows Owner. Er ist noch kein Produkttransport.

## Verbindungsziel

Windows Owner:

```text
rkwp+tcp-dev://<windows-ip>:57100
```

Smoke auf Windows:

```powershell
.\tools\run-windows-owner-for-mac.ps1
```

## Client-Aufgaben

- Host und Port aus Dev-Konfiguration lesen.
- TCP-Verbindung aufbauen.
- RKWP Envelope serialisieren/deserialisieren.
- Timeouts sauber behandeln.
- Disconnect erkennen.
- Reconnect spaeter vorbereiten.
- keine Originaldatei materialisieren.

## Konfiguration

Lokale Dev-Konfiguration:

```text
~/Library/Application Support/RKWorkspace/devtransport.json
```

Beispiel:

```json
{
  "ownerUrl": "rkwp+tcp-dev://192.168.1.10:57100",
  "securityMode": "DevelopmentAuthenticated",
  "allowDevPairing": true,
  "heartbeatIntervalMs": 1000
}
```

## Fehlerfaelle

- Owner nicht erreichbar.
- Firewall blockiert.
- Host falsches Netzwerk.
- Protokollversion unbekannt.
- Session abgelehnt.
- Pairing abgelehnt.

Alle Fehler muessen sichtbar ins Dev-Log, aber nicht als Produkt-UI mit technischen Begriffen.
