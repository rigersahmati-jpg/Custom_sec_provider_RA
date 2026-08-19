namespace CustomSecProvider.RA.Models;

/// <summary>
/// SaaS plan tier for feature entitlements and seat limits.
/// </summary>
public enum PlanTier
{
    /// <summary>Free tier: limited viewers only.</summary>
    Free = 0,

    /// <summary>Professional tier: viewers and designers.</summary>
    Pro = 1,

    /// <summary>Enterprise tier: full access with admin capabilities.</summary>
    Enterprise = 2
}
