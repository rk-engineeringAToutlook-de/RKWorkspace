# RKWP Secrets Storage by Platform

Status: MA013.27 plan  
Datum: 2026-07-06

## Ziel

Private Keys und Secrets muessen sicher gespeichert werden.

## Plattformen

| Plattform | geplanter Store |
| --- | --- |
| Windows | DPAPI, Windows Credential Manager |
| macOS | Keychain |
| iOS/iPadOS | Keychain / Secure Enclave soweit sinnvoll |
| Android | Android Keystore |
| Linux | Secret Service / keyring |
| Dongle | Secure Element optional |

## Regeln

- Private Keys nie in Git.
- Private Keys nie in Context Packs.
- Dev Keys klar als Development markieren.
- Rotation und Revocation unterstuetzen.
- Export nur verschluesselt und policygesteuert.

## Recovery

Verlust einer Ablage bedeutet nicht, dass Originale verloren sind. Owner bleibt Besitzer. Trust fuer verlorene Ablage wird revoked.
