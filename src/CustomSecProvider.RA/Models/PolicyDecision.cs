namespace CustomSecProvider.RA.Models;

/// <summary>
/// Policy evaluation result: whether access is granted and what roles/claims apply.
/// </summary>
public sealed class PolicyDecision
{
    /// <summary>Whether the user is allowed to access the system.</summary>
    public required bool IsAllowed { get; set; }

    /// <summary>User identifier (may differ from request if mapped).</summary>
    public required string UserId { get; set; }

    /// <summary>Tenant identifier for scoping.</summary>
    public required string TenantId { get; set; }

    /// <summary>Wyn roles to assign to this session.</summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>Organizations (departments/groups) for data scoping.</summary>
    public IEnumerable<string> Organizations { get; set; } = Array.Empty<string>();

    /// <summary>Immutable claims for data filtering, routing, feature flags.</summary>
    public IReadOnlyDictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();

    /// <summary>Reason code for allow/deny decision (audit trail).</summary>
    public required string ReasonCode { get; set; }

    /// <summary>Suggested TTL in seconds for this session.</summary>
    public int SessionTTLSeconds { get; set; } = 1800; // 30 minutes default
}
