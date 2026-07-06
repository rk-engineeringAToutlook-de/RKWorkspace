# Security Production Path Task

## Goal

Replace SecureDev lab security with a product-grade secure session path.

## Inputs

- `Docs\Security\RKWP_SecurityGate.md`
- `Docs\Security\RKWP_CryptoDecision.md`
- `Docs\Security\RKWP_MutualAuthenticationModel.md`
- `Docs\Security\RKWP_CertificateProvisioning.md`
- `Docs\Security\RKWP_SecretsStorageByPlatform.md`

## Required work

- Decide TLS/mTLS or approved alternative.
- Define platform trust stores.
- Define revocation behavior.
- Define certificate provisioning.
- Define audit retention and redaction.
- Keep No File Ingress as a hard gate.

## Done when

```text
SecurityMode: ProductionRequired
MutualAuthentication: OK
ReplayProtection: OK
NoFileIngress: SUCCESS
RESULT: SUCCESS
```
