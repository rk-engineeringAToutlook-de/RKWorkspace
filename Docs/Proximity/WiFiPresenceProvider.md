# WiFi Presence Provider

Status: Draft  
Datum: 2026-07-06

## Ziel

AP153 fuehrt WiFi als optionale Presence-Quelle ein. WiFi erkennt nicht praezise, wo eine Ablage liegt. Es kann aber bestaetigen, dass eine bereits bekannte Ablage im selben lokalen Arbeitsraum erreichbar ist.

## Implementierter Modell-Slice

Der erste Code-Slice liegt in:

```text
src/Shell/RKWorkspace.Shell/Ablage/WiFiPresenceProvider.cs
```

Er enthaelt:

- `NetworkPeerPresence`
- `WiFiPresenceProvider`

Dieser Provider ist simuliert und scannt kein echtes Netzwerk. Er uebersetzt vorbereitete Peer-Signale in `AblageProximitySnapshot`.

## Signal

Ein WiFi-Peer kann spaeter liefern:

- AblageId
- Anzeigename
- Plattform
- Netzprofil
- letzter Sichtzeitpunkt
- optionale Latenz
- Confidence
- Verfuegbarkeit

## Distanz

WiFi liefert keine echte Distanz. Die erste Heuristik nutzt Latenz und Confidence nur als grobe Naeheklasse:

- sehr niedrige Latenz: `VeryNear`
- niedrige Latenz: `Near`
- mittlere Latenz: `Medium`
- hohe Latenz: `Far`
- sehr hohe Latenz: `VeryFar`

Meterdistanz bleibt leer.

## Rolle im Sensor-Mix

WiFi ist ein Zusatzsignal fuer:

- Peer erreichbar
- gleicher Raum wahrscheinlich
- Dev-Transport-Pfad moeglich
- letzte bekannte Ablage nicht stale

WiFi darf die Richtung nicht allein bestimmen. Richtung kommt aus Manual Map, UWB, Dongle-Orientierung oder spaeter Sensorfusion.

## Grenzen

- WLAN sagt nicht, ob ein Geraet links oder rechts steht.
- VLAN, Gastnetz, VPN und Firewall koennen Presence faelschen oder verhindern.
- iOS/Android erlauben nicht beliebige Hintergrund-Discovery.
- WiFi darf keine Nutzdatenentscheidung treffen.

## Erwarteter Testpfad

```powershell
.\tools\run-rkwp-tests.ps1
.\tools\run-tests.ps1
```

Der Code-Slice wird ueber den Shell-Build und die bestehenden Foundation-Tests mitgebaut. Ein eigener echter WiFi-Scan-Test wird erst eingefuehrt, wenn ein Dev-LAN-Transport und Plattformrechte vorhanden sind.
