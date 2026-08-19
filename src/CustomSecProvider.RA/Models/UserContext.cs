namespace CustomSecProvider.RA.Models;

/// <summary>
/// User identity and tenant context resolved from the identity provider.
/// </summary>
public sealed class UserContext
{
    /// <summary>Unique user identifier in the SaaS platform.</summary>
    public required string UserId { get; set; }

    /// <summary>Tenant identifier for multi-tenant scoping.</summary>
    public required string TenantId { get; set; }

    /// <summary>Whether the user account is active (not disabled/suspended).</summary>
    public required bool IsUserActive { get; set; }

    /// <summary>Whether the tenant account is active (not suspended).</summary>
    public required bool IsTenantActive { get; set; }

    /// <summary>Primary seat type this user holds.</summary>
    public required SeatType SeatType { get; set; }

    /// <summary>Data residency region (e.g., US, EU, APAC).</summary>
    public required string DataRegion { get; set; }

    /// <summary>Organizational units or departments this user belongs to.</summary>
    public string[] OrgUnits { get; set; } = Array.Empty<string>();
}
