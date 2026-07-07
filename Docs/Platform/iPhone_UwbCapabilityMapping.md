# iPhone UWB Capability Mapping

Status: MA017 planning.

## Ziel

iPhone UWB wird als Praezisionssignal fuer die naechste Ablage vorbereitet. Es ist kein Transportkanal und keine Ownership-Quelle.

## Plattformrealitaet

iPhone UWB ist an Apple-Hardware, Berechtigungen und Nearby-Interaction-/Accessory-Modelle gebunden. Der erste RK Workspace Pilot behandelt iPhone deshalb als vorbereitete Surface, nicht als garantierte UWB-Messquelle.

## Mapping

| Plattformsignal | RK Workspace Mapping |
| --- | --- |
| Device or accessory identity | AblageId or DongleId |
| Distance estimate | AblageDistance.DistanceMeters |
| Proximity class | AblageDistanceKind |
| Direction hint, if available | AblagePose.Direction |
| Signal confidence | AblageDistance.Confidence |
| Permission missing | UwbProviderStatus.ConsentRequired |
| Hardware unavailable | UwbProviderStatus.HardwareUnavailable |

## Regeln

- UWB ist opt-in.
- Keine dauerhafte Bewegungs-Historie.
- Keine Datei- oder PDF-Daten im UWB-Pfad.
- Manual Map bleibt Fallback.
- Secure Session und CarryLease bleiben RKWP-Aufgaben.

## Pilotstatus

MA017 dokumentiert das Mapping und simuliert den Pfad ueber `SimulatedUwbProximityProvider` und `SimulatedDongleAnchorProvider`.
