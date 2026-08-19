using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Core policy evaluation engine that determines access, roles, and claims for a session.
/// </summary>
public interface IPolicyEngine
{
    /// <summary>
    /// Evaluate policy for a user session: resolve tenant/user state, apply entitlements,
    /// check seats, apply incident mode, and return security context with roles and claims.
    /// </summary>
    Task<PolicyDecision> EvaluateAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default);
}
