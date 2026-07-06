# iOS Share Extension Object Source

Status: MA013.19 roadmap  
Datum: 2026-07-06

## Ziel

iOS kann nicht global beliebige App-Inhalte greifen. Eine Share Extension ist deshalb eine spaetere, plattformkonforme Quelle.

## Quellen

- Share Extension.
- Document Picker.
- bewusstes Pasteboard.
- RK Workspace App.

## FrameOnly

Wenn ein Objekt aus iOS kommt, muss Ownership eindeutig sein. Fuer kritische Dinge bleibt der sichere Pfad FrameOnly.

## Ownership

- iOS darf nicht still Eigentum an fremden Dateien behaupten.
- CopyOut ist spaeter nur mit Policy erlaubt.
- Original-Owned bleibt Standard fuer kritische Dokumente.

## Sandbox

Die Share Extension arbeitet in ihrem Container und gibt nur erlaubte Referenzen oder sichere Frame-Anfragen weiter.

## No File Ingress

Fuer fremde Owner-Frames gilt weiter:

- kein Originalpfad.
- keine Originalbytes.
- keine freie PDF in Files App.

## Roadmap

1. Share Extension Target planen.
2. erlaubte Content Types definieren.
3. Policy fuer CopyOut entwerfen.
4. No File Ingress fuer eingehende Frames erhalten.
