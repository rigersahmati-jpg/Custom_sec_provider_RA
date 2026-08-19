using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

public interface IIdentityTenantService
{
    Task<UserContext?> GetUserContextAsync(string userToken, CancellationToken cancellationToken = default);
}
