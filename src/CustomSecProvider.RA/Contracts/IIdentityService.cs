using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Service to resolve user identity and status from the SaaS backend.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Authenticate and resolve user context by token or credential.
    /// </summary>
    Task<UserContext?> ResolveUserAsync(string userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user is still active in the SaaS platform.
    /// </summary>
    Task<bool> IsUserActiveAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
}
