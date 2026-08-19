using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Resolves user identity and tenant context from the SaaS platform.
/// Implement this to connect to your core identity provider.
/// </summary>
public interface IIdentityTenantService
{
    /// <summary>
    /// Resolve user and tenant context from a user token/claim.
    /// </summary>
    /// <param name="userToken">User identifier or bearer token from Wyn login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User context with identity and tenant info, or null if user not found.</returns>
    Task<UserContext?> GetUserContextAsync(string userToken, CancellationToken cancellationToken = default);
}
