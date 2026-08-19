using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomSecProvider.RA.Contracts;

namespace CustomSecProvider.RA.Services;

/// <summary>
/// Simple audit sink that logs to Debug output for quick testing.
/// </summary>
public sealed class SimpleAuditSink : IAuditDecisionSink
{
    public Task WriteAsync(
        string tenantId, 
        string userId, 
        bool isAllowed, 
        string reasonCode, 
        IReadOnlyCollection<string> roles, 
        CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var decision = isAllowed ? "ALLOW" : "DENY";
        var rolesList = string.Join(",", roles ?? Array.Empty<string>());

        var logEntry = $"[{timestamp}] CSP_AUDIT | Decision={decision} | TenantId={tenantId} | UserId={userId} | ReasonCode={reasonCode} | Roles={rolesList}";
        
        System.Diagnostics.Debug.WriteLine(logEntry);
        Console.WriteLine(logEntry);

        return Task.CompletedTask;
    }
}
