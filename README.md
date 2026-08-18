# Custom Security Provider RA (Wyn Enterprise)

Stateless, real-time Custom Security Provider (CSP) reference architecture for **Wyn Enterprise Embedded Analytics**.

This repository implements a production-oriented starter for multi-tenant SaaS scenarios where analytics access must be enforced in real time using your core platform as the source of truth.

## Why this project exists

Traditional analytics integrations often rely on scheduled sync jobs that copy users/roles into Wyn. That approach causes:

- permission drift
- delayed offboarding
- weak seat enforcement
- operational overhead

This project demonstrates a **Zero-Sync Governance** pattern:

- Evaluate tenant/user status, plan entitlements, and seat limits at session time.
- Inject trusted tenant claims into Wyn session context.
- Apply deterministic role mapping for Free/Pro/Enterprise packaging.
- Produce auditable allow/deny decisions with reason codes.

## Business scenarios covered

### Scenario A: Real-time plan upgrade
A tenant upgrades from Pro to Enterprise in your billing portal.

**Expected result:** On the next analytics session, CSP returns elevated roles/features without any manual Wyn administration.

### Scenario B: Immediate offboarding
A user is disabled in your SaaS app.

**Expected result:** Access is denied on next session attempt with reason code `USER_DISABLED`.

### Scenario C: Seat overage protection
Tenant with 5 Viewer seats attempts a 6th concurrent viewer session.

**Expected result:** CSP denies or downgrades based on policy (`Deny` by default) with reason code `SEAT_LIMIT_EXCEEDED`.

### Scenario D: Incident mode (break-glass)
Security team toggles global incident mode in config/cache.

**Expected result:** New sessions are forced read-only and export/scheduling is disabled.

## Project structure

- `src/CustomSecProvider.RA/` — .NET 8 class library (CSP core)
- `src/CustomSecProvider.RA/Configuration/` — strongly typed config and schema notes
- `src/CustomSecProvider.RA/Contracts/` — abstractions for identity/entitlements/seat store
- `src/CustomSecProvider.RA/Models/` — policy and decision models
- `src/CustomSecProvider.RA/Services/` — policy evaluation engine
- `src/CustomSecProvider.RA/Wyn/` — adapter placeholders for Wyn CSP integration
- `tests/CustomSecProvider.RA.Tests/` — unit tests for policy scenarios
- `config/custom-security-provider.sample.json` — ready-to-adapt config for Wyn environment
- `docs/` — architecture and integration guidance

## Requirements

- .NET SDK 8.0+
- Access to Wyn Enterprise CSP extension points/SDK assemblies
- Connectivity to your SaaS identity + entitlement backend
- Optional Redis for seat counters and posture flags

## Quick start

1. Clone repository.
2. Copy `config/custom-security-provider.sample.json` and adjust values.
3. Implement backend connectors in `Contracts` + `Services` (or wire your existing APIs).
4. Build:

```bash
dotnet build
```

5. Run tests:

```bash
dotnet test
```

6. Publish class library and deploy DLL to Wyn `SecurityProviders` folder (plus required dependency DLLs).

## Wyn deployment notes

Based on Wyn guidance:

- Stop Wyn service before copying DLLs.
- Copy CSP assembly + dependency assemblies into `SecurityProviders`.
- Add provider in Wyn Admin Portal (`Configuration > Security Providers`).
- Restart Wyn service and test login.

> On SQL client package compatibility scenarios, keep connection-string parameters aligned with your Wyn version requirements.

## Default policy posture

- Fail-closed by default when policy cannot be evaluated.
- Seat overage action defaults to `Deny`.
- Incident mode action defaults to `ReadOnly`.
- Short TTL caching for entitlement lookups.

## Notes

This repository is a **reference architecture**. You should adapt:

- your exact Wyn CSP interfaces
- your tenant/user/plan schema
- your security logging standard
- your compliance constraints (SOC2/GDPR/local residency)

