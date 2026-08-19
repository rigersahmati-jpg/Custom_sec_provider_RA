using System;
using System.Threading;
using System.Threading.Tasks;
using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

/// <summary>
/// Simple security posture service for quick testing.
/// 
/// QUICK-START:
/// - Always returns SecurityPosture.Normal
/// - Can be toggled via environment variable for testing incident mode
/// 
/// FOR PRODUCTION:
/// - Check a centralized incident/breach flag (Redis cache, config service, etc.)
/// - Implement break-glass procedures
/// - Log all incident mode activations
/// </summary>
public sealed class SimpleSecurityPostureService : ISecurityPostureService
{
    public Task<SecurityPosture> GetPostureAsync(CancellationToken cancellationToken = default)
    {
        // Check environment variable for incident mode override (for testing)
        var incidentModeEnv = Environment.GetEnvironmentVariable("CSP_INCIDENT_MODE");
        
        SecurityPosture posture = SecurityPosture.Normal;

        if (bool.TryParse(incidentModeEnv, out var isIncident) && isIncident)
        {
            // Check if it's "read-only" or "denied" mode
            var modeEnv = Environment.GetEnvironmentVariable("CSP_INCIDENT_MODE_TYPE") ?? "readonly";
            posture = modeEnv.Equals("denied", StringComparison.OrdinalIgnoreCase)
                ? SecurityPosture.IncidentDenied
                : SecurityPosture.IncidentReadOnly;

            System.Diagnostics.Debug.WriteLine($"[SecurityPostureService] Incident mode active: {posture}");
        }

        return Task.FromResult(posture);
    }
}
