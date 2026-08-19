namespace CustomSecProvider.RA.Models;

/// <summary>
/// Represents user context resolved from the SaaS backend.
/// </summary>
public class UserContext
{
    /// <summary>Unique user identifier.</summary>
    public required string UserId { get; set; }

    /// <summary>Tenant identifier this user belongs to.</summary>
    public required string TenantId { get; set; }

    /// <summary>User's account status (Active, Disabled, Suspended, PendingActivation).</summary>
    public required UserStatus Status { get; set; }

    /// <summary>Assigned seat type (Viewer, Designer, Admin).</summary>
    public required SeatType SeatType { get; set; }

    /// <summary>User's display name or email.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Internal application role (AccountOwner, Analyst, Staff).</summary>
    public string? InternalAppRole { get; set; }
}

/// <summary>User account status enumeration.</summary>
public enum UserStatus
{
    Active = 0,
    Disabled = 1,
    Suspended = 2,
    PendingActivation = 3,
    Deleted = 4
}

/// <summary>Analytics seat type enumeration.</summary>
public enum SeatType
{
    Viewer = 0,
    Designer = 1,
    Admin = 2
}
