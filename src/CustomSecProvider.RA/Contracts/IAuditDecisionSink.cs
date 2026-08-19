namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Persists security decision audit logs for compliance and troubleshooting.
/// Implement this with your audit log system (e.g., structured logging, event bus, or database).
/// </summary>
public interface IAuditDecisionSink
{
    /// <summary>
    /// Write a security decision audit log entry.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="userId">User identifier.</param>
    /// <param name="isAllowed">Whether access was allowed.</param>
    /// <param name="reasonCode">Reason code for the decision.</param>
    /// <param name="roles">Roles assigned (if allowed).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteAsync(
        string tenantId,
        string userId,
        bool isAllowed,
        string reasonCode,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
