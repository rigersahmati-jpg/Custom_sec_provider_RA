namespace CustomSecProvider.RA.Models;

/// <summary>
/// Plan tier and feature entitlements for a tenant.
/// </summary>
public sealed class EntitlementContext
{
    /// <summary>Current plan tier (Free/Pro/Enterprise).</summary>
    public required PlanTier PlanTier { get; set; }

    /// <summary>Whether data export is allowed for this plan.</summary>
    public required bool AllowExport { get; set; }

    /// <summary>Whether scheduled report delivery is allowed.</summary>
    public required bool AllowScheduling { get; set; }

    /// <summary>Whether premium datasets are accessible.</summary>
    public required bool AllowPremiumDatasets { get; set; }

    /// <summary>Maximum concurrent Viewer seat licenses.</summary>
    public required int MaxConcurrentViewers { get; set; }

    /// <summary>Maximum concurrent Designer seat licenses.</summary>
    public required int MaxConcurrentDesigners { get; set; }

    /// <summary>Maximum concurrent Admin seat licenses.</summary>
    public int MaxConcurrentAdmins { get; set; } = 5;
}
