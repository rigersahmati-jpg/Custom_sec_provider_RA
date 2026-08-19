namespace CustomSecProvider.RA.Models;

/// <summary>
/// Represents tenant context resolved from the SaaS backend.
/// </summary>
public class TenantContext
{
    /// <summary>Unique tenant identifier.</summary>
    public required string TenantId { get; set; }

    /// <summary>Current tenant status (Active, Suspended, PastDue).</summary>
    public required TenantStatus Status { get; set; }

    /// <summary>Subscription plan tier (Free, Pro, Enterprise).</summary>
    public required SubscriptionPlan Plan { get; set; }

    /// <summary>Geographic region for data residency (US, EU, APAC).</summary>
    public required string DataRegion { get; set; }

    /// <summary>Organization unit or department context.</summary>
    public string? OrgUnit { get; set; }

    /// <summary>Maximum concurrent viewer seats allowed.</summary>
    public int MaxViewerSeats { get; set; }

    /// <summary>Maximum concurrent designer seats allowed.</summary>
    public int MaxDesignerSeats { get; set; }

    /// <summary>Enabled features for this tenant (export, schedule, premium_datasets).</summary>
    public string[] EnabledFeatures { get; set; } = Array.Empty<string>();
}

/// <summary>Tenant status enumeration.</summary>
public enum TenantStatus
{
    Active = 0,
    Suspended = 1,
    PastDue = 2,
    Provisioning = 3,
    Deleted = 4
}

/// <summary>Subscription plan enumeration.</summary>
public enum SubscriptionPlan
{
    Free = 0,
    Pro = 1,
    Enterprise = 2,
    Custom = 3
}
