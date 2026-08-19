namespace CustomSecProvider.RA.Models;

/// <summary>
/// Result of policy evaluation: the decision to allow/deny/downgrade and assigned roles/claims.
/// </summary>
public class PolicyDecision
{
    /// <summary>Whether access is allowed.</summary>
    public bool IsAllowed { get; set; }

    /// <summary>Reason for denial or restriction (e.g., TENANT_SUSPENDED, USER_DISABLED, SEAT_LIMIT_EXCEEDED).</summary>
    public string? ReasonCode { get; set; }

    /// <summary>Human-readable explanation of the decision.</summary>
    public string? Message { get; set; }

    /// <summary>Wyn roles to assign to the session.</summary>
    public string[] Roles { get; set; } = Array.Empty<string>();

    /// <summary>Claims to inject into the session context.</summary>
    public Dictionary<string, object> Claims { get; set; } = new();

    /// <summary>Timestamp when decision was made.</summary>
    public DateTime DecisionTime { get; set; } = DateTime.UtcNow;

    /// <summary>Whether this decision came from cache (vs live evaluation).</summary>
    public bool FromCache { get; set; }
}

/// <summary>Policy decision reason codes for audit compliance.</summary>
public static class ReasonCodes
{
    public const string Success = "SUCCESS";
    public const string TenantSuspended = "TENANT_SUSPENDED";
    public const string TenantPastDue = "TENANT_PAST_DUE";
    public const string UserDisabled = "USER_DISABLED";
    public const string UserSuspended = "USER_SUSPENDED";
    public const string SeatLimitExceeded = "SEAT_LIMIT_EXCEEDED";
    public const string IncidentMode = "INCIDENT_MODE";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string BackendUnavailable = "BACKEND_UNAVAILABLE";
    public const string ConfigurationError = "CONFIGURATION_ERROR";
}
