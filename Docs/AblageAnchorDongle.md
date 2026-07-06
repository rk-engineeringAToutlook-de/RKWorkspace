# Ablage Anchor Dongle

Status: Draft  
Datum: 2026-07-05

## Grundsatz

Der Dongle ist kein Transferstick.

Der Dongle ist ein Ablage-Anker.

## Spaetere Aufgaben

- Identitaet einer Ablage sichtbar machen
- Naehe und Richtung liefern
- BLE-Beacon bereitstellen
- optional UWB-Ranging bereitstellen
- sichere Kopplung vorbereiten
- abgeschottete Systeme ohne Softwareinstallation repraesentieren
- Monitor-, KVM- oder Industrie-Ablagen repraesentieren

## Proximity-Roadmap

Der Dongle wird erst eingefuehrt, wenn die manuelle Raumkarte und die simulierte Auswahl stabil sind.

1. `Simulated`: feste Test-Ablagen fuer Smoke Tests.
2. `ManualMap`: Owner ordnet Ablagen manuell im Raum an.
3. `BLE`: grobe Naehe ueber RSSI, keine Richtungsgarantie.
4. `WiFi`: optionale Netzwerknaehe, nur als Zusatzsignal.
5. `Dongle`: stabile Ablage-Identitaet und lokaler Trust-Hinweis.
6. `UWB`: praezisere Distanz und spaeter Richtung.
7. `SensorFusion`: mehrere Quellen werden zusammengefuehrt.

Der Dongle darf nur `AblageProximitySnapshot`-Daten verbessern. Er entscheidet nicht ueber Ownership, Transfer oder Dateiinhalt.

## Manual Map Vorstufe

Bis echte Hardware existiert, ersetzt die Manual Map den Dongle:

- Ablage-ID
- Anzeigename
- relative Richtung
- Distanzklasse
- optionale Meterdistanz
- Confidence
- Quelle

Damit kann der Owner schon testen, ob sich die eine gläserne Kante richtig anfuehlt, bevor Hardware gebaut wird.

## Abgrenzung

Der Dongle repraesentiert nicht zwingend einen Rechner. Er repraesentiert einen Ort im Arbeitsraum, an dem ein digitales Ding abgelegt werden kann.

## Verbindung zu RKWP

Ein spaeterer Dongle darf keine Nutzdaten speichern und kein Transferstick werden. Er kann RKWP spaeter nur unterstuetzen durch:

- Ablage-Identitaet
- Naehe
- Richtung
- Trust-Hinweis
- sichere Kopplung

Ownership, CarryLease und FrameSession bleiben Protocol-Aufgaben.

## Verbindung zu Identity und Pairing

MA008.02 fuehrt die Software-Grundlage fuer Ablage-Identitaet und Trust ein. Ein spaeterer Dongle kann diese Identitaet staerker machen, ersetzt aber nicht das Pairing und nicht die `AblageTrustPolicy`.

Der Dongle darf spaeter helfen bei:

- stabiler `AblageId`
- lokaler Vertrauensanzeige
- Key-Material oder Secure-Element-Hinweis
- Proximity-Signal
- Revocation-Hinweis

Er darf weiterhin keine Originaldaten aufnehmen und keine Ownership-Entscheidung treffen.

## MA013 Hardware Readiness

Die naechsten Dongle-Dokumente liegen unter:

```text
Docs/Hardware/AblageAnchorDongle_MVP.md
Docs/Hardware/DongleFirmwareArchitecture.md
Docs/Hardware/UsbDongleControlProtocol.md
```

Sie fixieren den Dongle weiterhin als Ablage-Anker, nicht als Transferstick.
