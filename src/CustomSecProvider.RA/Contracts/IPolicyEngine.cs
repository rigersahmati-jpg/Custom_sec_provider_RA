using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

public interface IPolicyEngine
{
    Task<PolicyDecision> EvaluateAsync(string userToken, CancellationToken cancellationToken = default);
}
