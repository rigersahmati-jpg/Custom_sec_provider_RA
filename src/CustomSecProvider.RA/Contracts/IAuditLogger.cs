using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Service for compliance-ready audit logging of policy decisions.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Log a policy decision with all relevant details for audit trail.
    /// </summary>
    Task LogDecisionAsync(
        string userId,
        string tenantId,
        PolicyDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log an authentication attempt.
    /// </summary>
    Task LogAuthenticationAsync(
        string userId,
        string tenantId,
        bool success,
        string? reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log a seat allocation event.
    /// </summary>
    Task LogSeatAllocationAsync(
        string userId,
        string tenantId,
        string action, // "allocate" or "release"
        string seatType,
        bool success,
        CancellationToken cancellationToken = default);
}
