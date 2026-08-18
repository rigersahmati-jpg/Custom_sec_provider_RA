# Configuration Schema

Use `config/custom-security-provider.sample.json` as your runtime config template.

Key sections:

- `provider`: fail behavior, cache TTL, posture defaults
- `endpoints`: identity, entitlements, billing APIs
- `redis`: seat counters + incident mode flag store
- `seatEnforcement`: overage mode + key templates
- `claimMapping`: normalized claim names expected by Wyn models and security rules

Recommended defaults:

- `failClosed = true`
- `entitlementCacheTtlSeconds = 60`
- `overageAction = Deny`
- `incidentModeAction = ReadOnly`

