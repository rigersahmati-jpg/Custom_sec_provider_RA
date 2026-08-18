namespace CustomSecProvider.RA.Models;

public sealed class PolicyDecision
{
    public bool IsAllowed { get; init; }
    public required string ReasonCode { get; init; }
    public required string[] Roles { get; init; } = [];
    public required Dictionary<string, string> Claims { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
