using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

public interface IEntitlementService
{
    Task<EntitlementContext> GetEntitlementsAsync(string tenantId, CancellationToken cancellationToken = default);
}
