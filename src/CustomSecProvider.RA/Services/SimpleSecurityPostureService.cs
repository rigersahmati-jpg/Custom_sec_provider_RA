using System;
using System.Threading;
using System.Threading.Tasks;
using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

public sealed class SimpleSecurityPostureService : ISecurityPostureService
{
    public Task<SecurityPosture> GetPostureAsync(CancellationToken cancellationToken = default)
    {
        var incidentModeEnv = Environment.GetEnvironmentVariable("CSP_INCIDENT_MODE");
        
        SecurityPosture posture = SecurityPosture.Normal;

        if (bool.TryParse(incidentModeEnv, out var isIncident) && isIncident)
        {
            var modeEnv = Environment.GetEnvironmentVariable("CSP_INCIDENT_MODE_TYPE") ?? "readonly";
            posture = modeEnv.Equals("denied", StringComparison.OrdinalIgnoreCase)
                ? SecurityPosture.IncidentDenied
                : SecurityPosture.IncidentReadOnly;

            System.Diagnostics.Debug.WriteLine($"[SecurityPostureService] Incident mode active: {posture}");
        }

        return Task.FromResult(posture);
    }
}
