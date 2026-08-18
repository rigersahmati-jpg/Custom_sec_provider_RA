namespace CustomSecProvider.RA.Models;

public sealed class EntitlementContext
{
    public required PlanTier PlanTier { get; init; }
    public required bool AllowExport { get; init; }
    public required bool AllowScheduling { get; init; }
    public required bool AllowPremiumDatasets { get; init; }
    public required int MaxConcurrentViewers { get; init; }
    public required int MaxConcurrentDesigners { get; init; }
}
