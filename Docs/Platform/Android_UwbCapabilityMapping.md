# Android UWB Capability Mapping

Status: MA017 planning.

## Ziel

Android UWB wird als spaeteres Naehe- und Richtungssignal vorbereitet. Es ersetzt weder Pairing noch Trust noch Transport.

## Plattformrealitaet

Android UWB haengt von Hardware, OS-Version, Herstellerfreigaben und App-Berechtigungen ab. RK Workspace behandelt Android deshalb im ersten Schritt als Surface mit optionalem UWB-Signal.

## Mapping

| Plattformsignal | RK Workspace Mapping |
| --- | --- |
| Android device or accessory identity | AblageId or DongleId |
| Distance/ranging result | AblageDistance.DistanceMeters |
| Ranging quality | AblageDistance.Confidence |
| Angle of arrival, if available | AblagePose.Direction |
| Nearby permission missing | UwbProviderStatus.ConsentRequired |
| UWB not supported | UwbProviderStatus.HardwareUnavailable |
| unstable result | UwbProviderStatus.Degraded |

## Pilotregeln

- Android UWB darf nur `AblageProximitySnapshot` verbessern.
- Payload bleibt im RKWP-/Transportpfad, nicht im UWB-Pfad.
- Keine Personenortung.
- Keine versteckte Hintergrundmessung.
- Manual Map und Dongle-Anker bleiben Fallbacks.

## Pilotstatus

MA017 liefert noch keine native Android-App. Das Mapping ist bereit fuer eine spaetere Surface-Implementierung.
