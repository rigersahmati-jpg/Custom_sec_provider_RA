using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Service to resolve tenant context and status from the SaaS backend.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Resolve tenant context by tenant ID.
    /// </summary>
    Task<TenantContext?> ResolveTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a tenant is in good standing (active billing, not suspended).
    /// </summary>
    Task<bool> IsTenantActiveAsync(string tenantId, CancellationToken cancellationToken = default);
}
