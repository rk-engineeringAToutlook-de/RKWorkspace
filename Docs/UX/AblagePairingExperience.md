# Ablage Pairing Experience

Status: MA013.24 UX concept  
Datum: 2026-07-06

## Ziel

Pairing darf nicht technisch wirken. Der Mensch soll nicht ein Geraet konfigurieren, sondern einer neuen Ablage vertrauen.

## Sichtbares Modell

Erlaubt:

- neue Ablage entdeckt.
- Vertrauen herstellen.
- vertraut.
- unbekannt.
- kritisch.
- ablehnen.
- widerrufen.

Nicht als Primaersprache:

- Device.
- Endpoint.
- Certificate.
- Server.
- Client.

## Ablauf

1. Neue Ablage wird erkannt.
2. RK Workspace fragt: Soll diese Ablage Teil deines Arbeitsraums sein?
3. Nutzer bestaetigt oder lehnt ab.
4. Technisch wird Mutual Auth/Trust darunter gesetzt.
5. Revocation bleibt jederzeit moeglich.

## Sicherheitsanker

Optional:

- QR-Code.
- Fingerprint.
- Dongle.
- physische Naehe.
- Enterprise Trust.

## UX-Regel

Der Nutzer soll denken:

```text
Diese Ablage gehoert zu meinem Arbeitsraum.
```

Nicht:

```text
Ich habe ein Device gekoppelt.
```
