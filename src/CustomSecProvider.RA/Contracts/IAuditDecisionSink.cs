namespace CustomSecProvider.RA.Contracts;

public interface IAuditDecisionSink
{
    Task WriteAsync(string tenantId, string userId, bool isAllowed, string reasonCode, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default);
}
