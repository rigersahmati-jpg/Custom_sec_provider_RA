namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Service to check and manage the global incident mode (break-glass security posture).
/// </summary>
public interface IIncidentModeService
{
    /// <summary>
    /// Check if global incident mode is currently enabled.
    /// </summary>
    Task<bool> IsIncidentModeEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable incident mode (force read-only for all new sessions).
    /// </summary>
    Task EnableIncidentModeAsync(string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable incident mode (return to normal operations).
    /// </summary>
    Task DisableIncidentModeAsync(CancellationToken cancellationToken = default);
}
