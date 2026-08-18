using CustomSecProvider.RA.Models;
using CustomSecProvider.RA.Services;

namespace CustomSecProvider.RA.Wyn;

public sealed class WynSecurityContextAdapter
{
    public WynSecurityContext BuildContext(PolicyDecision decision)
    {
        return new WynSecurityContext
        {
            IsAuthenticated = decision.IsAllowed,
            Roles = decision.Roles,
            Claims = decision.Claims
        };
    }
}

public sealed class WynSecurityContext
{
    public required bool IsAuthenticated { get; init; }
    public required string[] Roles { get; init; } = [];
    public required Dictionary<string, string> Claims { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
