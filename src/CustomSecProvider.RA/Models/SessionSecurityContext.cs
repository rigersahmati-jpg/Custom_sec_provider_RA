namespace CustomSecProvider.RA.Models;

/// <summary>
/// Complete security context for a user session in Wyn.
/// This is what the CSP returns after policy evaluation.
/// </summary>
public class SessionSecurityContext
{
    /// <summary>User's unique identifier.</summary>
    public required string UserId { get; set; }

    /// <summary>Tenant identifier for multi-tenant scoping.</summary>
    public required string TenantId { get; set; }

    /// <summary>Wyn roles assigned based on entitlements and policy.</summary>
    public string[] Roles { get; set; } = Array.Empty<string>();

    /// <summary>Immutable claims for data scoping, routing, and feature control.</summary>
    public IReadOnlyDictionary<string, object> Claims { get; set; } = 
        new Dictionary<string, object>();

    /// <summary>Policy evaluation result.</summary>
    public required PolicyDecision Decision { get; set; }

    /// <summary>When this session context was issued.</summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Suggested time-to-live for this session context (in seconds).</summary>
    public int TTLSeconds { get; set; } = 3600; // 1 hour default
}
