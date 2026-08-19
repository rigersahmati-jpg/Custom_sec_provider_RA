using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Checks current security posture (incident mode, emergency read-only, etc.).
/// Implement this with Redis, config server, or your posture service.
/// </summary>
public interface ISecurityPostureService
{
    /// <summary>
    /// Get current security posture (Normal, IncidentReadOnly, IncidentDenied).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current security posture.</returns>
    Task<SecurityPosture> GetPostureAsync(CancellationToken cancellationToken = default);
}
