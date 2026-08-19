namespace CustomSecProvider.RA.Models;

public class PolicyDecision
{
    public required bool IsAllowed { get; set; }
    public required string UserId { get; set; }
    public required string TenantId { get; set; }
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    public IEnumerable<string>? Organizations { get; set; }
    public IReadOnlyDictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
    public required string ReasonCode { get; set; }
    public int SessionTTLSeconds { get; set; } = 1800;
}

public static class ReasonCode
{
    public const string ACCESS_GRANTED = "ACCESS_GRANTED";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string USER_DISABLED = "USER_DISABLED";
    public const string TENANT_INACTIVE = "TENANT_INACTIVE";
    public const string TENANT_NOT_FOUND = "TENANT_NOT_FOUND";
    public const string SEAT_LIMIT_EXCEEDED = "SEAT_LIMIT_EXCEEDED";
    public const string INCIDENT_MODE_DENIED = "INCIDENT_MODE_DENIED";
    public const string EVALUATION_ERROR = "EVALUATION_ERROR";
}
