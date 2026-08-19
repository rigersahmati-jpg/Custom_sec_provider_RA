using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Resolves plan tier and feature entitlements for a tenant.
/// Implement this to connect to your billing/entitlements system.
/// </summary>
public interface IEntitlementService
{
    /// <summary>
    /// Get plan tier and feature entitlements for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entitlement context with plan tier and feature flags.</returns>
    Task<EntitlementContext?> GetEntitlementsAsync(string tenantId, CancellationToken cancellationToken = default);
}
