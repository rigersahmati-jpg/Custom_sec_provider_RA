namespace CustomSecProvider.RA.Configuration;

public sealed class ProviderSettings
{
    public bool FailClosed { get; init; } = true;
    public int EntitlementCacheTtlSeconds { get; init; } = 60;
    public SeatPolicySettings SeatPolicy { get; init; } = new();
    public IncidentPolicySettings IncidentPolicy { get; init; } = new();
}

public sealed class SeatPolicySettings
{
    public string OverageAction { get; init; } = "Deny";
}

public sealed class IncidentPolicySettings
{
    public string Action { get; init; } = "ReadOnly";
}
