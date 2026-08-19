using CustomSecProvider.RA.Models;

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
    public required IEnumerable<string> Roles { get; init; } = new List<string>();
    public required IReadOnlyDictionary<string, object> Claims { get; init; } = new Dictionary<string, object>();
}
