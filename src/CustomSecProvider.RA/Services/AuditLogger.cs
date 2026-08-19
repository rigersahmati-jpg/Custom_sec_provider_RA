using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;
using Microsoft.Extensions.Logging;

namespace CustomSecProvider.RA.Services;

/// <summary>
/// Structured audit logging service for compliance and security audits.
/// Logs all policy decisions with reason codes, roles, and claims (without sensitive data).
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    public Task LogDecisionAsync(
        string userId,
        string tenantId,
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var roles = string.Join(",", decision.Roles);
        var claims = string.Join(",", decision.Claims.Keys);

        _logger.LogInformation(
            "PolicyDecision | TenantId={TenantId} | UserId={UserId} | Allowed={Allowed} | ReasonCode={ReasonCode} | Roles={Roles} | Claims={Claims} | Timestamp={Timestamp}",
            tenantId,
            userId,
            decision.IsAllowed,
            decision.ReasonCode,
            roles,
            claims,
            decision.DecisionTime);

        return Task.CompletedTask;
    }

    public Task LogAuthenticationAsync(
        string userId,
        string tenantId,
        bool success,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Authentication | TenantId={TenantId} | UserId={UserId} | Success={Success} | Reason={Reason} | Timestamp={Timestamp}",
            tenantId,
            userId,
            success,
            reason ?? "N/A",
            DateTime.UtcNow);

        return Task.CompletedTask;
    }

    public Task LogSeatAllocationAsync(
        string userId,
        string tenantId,
        string action,
        string seatType,
        bool success,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "SeatAllocation | TenantId={TenantId} | UserId={UserId} | Action={Action} | SeatType={SeatType} | Success={Success} | Timestamp={Timestamp}",
            tenantId,
            userId,
            action,
            seatType,
            success,
            DateTime.UtcNow);

        return Task.CompletedTask;
    }
}
