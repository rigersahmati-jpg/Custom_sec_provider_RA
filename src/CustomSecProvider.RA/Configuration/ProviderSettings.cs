namespace CustomSecProvider.RA.Configuration;

public sealed class ProviderSettings
{
    public required bool FailClosed { get; init; } = true;
    public required int EntitlementCacheTtlSeconds { get; init; } = 60;
    public required SeatPolicySettings SeatPolicy { get; init; } = new();
    public required IncidentPolicySettings IncidentPolicy { get; init; } = new();
}

public sealed class SeatPolicySettings
{
    public required string OverageAction { get; init; } = "Deny";
}

public sealed class IncidentPolicySettings
{
    public required string Action { get; init; } = "ReadOnly";
}
