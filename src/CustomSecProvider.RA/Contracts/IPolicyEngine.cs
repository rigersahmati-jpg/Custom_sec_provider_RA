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
    /// <param name="userToken">User token or identifier from the login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Policy decision with allow/deny and assigned roles.</returns>
    Task<PolicyDecision> EvaluateAsync(
        string userToken,
        CancellationToken cancellationToken = default);
}
