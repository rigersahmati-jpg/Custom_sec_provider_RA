namespace CustomSecProvider.RA.Models;

public class EntitlementContext
{
    public required PlanTier PlanTier { get; set; }
    public bool AllowExport { get; set; }
    public bool AllowScheduling { get; set; }
    public bool AllowPremiumDatasets { get; set; }
    public int MaxConcurrentViewers { get; set; }
    public int MaxConcurrentDesigners { get; set; }
    public int MaxConcurrentAdmins { get; set; }
}

public enum PlanTier
{
    Free,
    Pro,
    Enterprise
}
