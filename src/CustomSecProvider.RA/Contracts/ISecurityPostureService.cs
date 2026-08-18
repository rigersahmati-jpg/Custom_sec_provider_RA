using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

public interface ISecurityPostureService
{
    Task<SecurityPosture> GetPostureAsync(CancellationToken cancellationToken = default);
}
