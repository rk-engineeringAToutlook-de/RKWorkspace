# Android Surface Starter Kit

Dokument-ID: RKWS-PLATFORM-ANDROID-001
Status: Draft
Datum: 2026-07-05

## Ziel

Android wird als mobile Ablage vorbereitet.

Die erste Android Surface App zeigt RKWP Frames, nutzt Touch/Haptik, simuliert Glass Edge und speichert keine freie Originaldatei.

## Produktrolle

Android Surface ist eine mobile Ablage im Arbeitsraum.

Erste Rolle:

- FrameGuestSurface
- Touch-Geste
- Haptik
- Glass Edge
- RKWP Session
- No File Ingress

## Quellen

Sicher vorbereiten:

- Sharesheet
- Intent
- Content URI
- Clipboard bewusst und begrenzt

Accessibility ist nur ein spaeterer Spezialpfad und muss vorsichtig bewertet werden.

## Native Zielrichtung

Final ist native Android App ueber Android Studio/Gradle.

PWA ist nur Uebergang.

## No File Ingress

Pflicht:

- keine Originaldatei als freie Datei speichern
- Content URI nicht in unkontrollierte lokale Datei kopieren
- OriginalOwned respektieren
- FrameOnly als sicherer Default

## Permissions

Zu pruefen:

- Local Network fuer DevTransport
- Content URI Grants
- Clipboard
- Haptics
- Files/Media nur nach User-Wahl
- Accessibility spaeter und nur nach Policy

## MA016 UWB Capability Plan

Android bleibt in MA016 vorbereitet. Ein spaeterer Android-Provider muss dieselben Ablage-Proximity-Modelle liefern:

- AblageId
- Richtung
- Distanzklasse
- optional Meterwert
- Confidence
- ProviderStatus

Android UWB darf keine Nutzdaten, keine Originaldatei und keinen Bewegungsverlauf speichern. Wenn Hardware oder Permission fehlt, meldet der Provider `HardwareUnavailable` oder `ConsentRequired` und Manual Map bleibt Fallback.

## Offene Blocker

- keine Android Studio/Gradle-Struktur
- DevTransport zu Android folgt spaeter
- finale Touch-Geste offen
- Content-URI-Policy offen
