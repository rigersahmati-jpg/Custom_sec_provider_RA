namespace CustomSecProvider.RA.Models;

/// <summary>
/// Policy decision reason codes for audit and support.
/// </summary>
public static class ReasonCode
{
    /// <summary>User successfully authorized.</summary>
    public const string ACCESS_GRANTED = "ACCESS_GRANTED";

    /// <summary>User account is disabled in the SaaS platform.</summary>
    public const string USER_DISABLED = "USER_DISABLED";

    /// <summary>User not found in identity provider.</summary>
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";

    /// <summary>Tenant account is inactive or suspended.</summary>
    public const string TENANT_INACTIVE = "TENANT_INACTIVE";

    /// <summary>Tenant not found in entitlements backend.</summary>
    public const string TENANT_NOT_FOUND = "TENANT_NOT_FOUND";

    /// <summary>Concurrent seat limit exceeded for the seat type.</summary>
    public const string SEAT_LIMIT_EXCEEDED = "SEAT_LIMIT_EXCEEDED";

    /// <summary>Security incident mode active: read-only enforcement.</summary>
    public const string INCIDENT_MODE_READONLY = "INCIDENT_MODE_READONLY";

    /// <summary>Security incident mode active: all access denied.</summary>
    public const string INCIDENT_MODE_DENIED = "INCIDENT_MODE_DENIED";

    /// <summary>Policy evaluation error or misconfiguration.</summary>
    public const string EVALUATION_ERROR = "EVALUATION_ERROR";
}
