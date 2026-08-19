using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Service to manage and enforce seat limits (concurrent viewers, designers).
/// </summary>
public interface ISeatManagementService
{
    /// <summary>
    /// Get current concurrent seat usage for a tenant.
    /// </summary>
    Task<SeatUsage> GetSeatUsageAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if adding a new session of a given seat type would exceed quota.
    /// </summary>
    Task<bool> CanAllocateSeatAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new active session (allocate a seat).
    /// </summary>
    Task RegisterSessionAsync(string tenantId, string userId, SeatType seatType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregister an ended session (release a seat).
    /// </summary>
    Task UnregisterSessionAsync(string tenantId, string userId, SeatType seatType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Current seat usage snapshot for a tenant.
/// </summary>
public class SeatUsage
{
    public int ActiveViewers { get; set; }
    public int ActiveDesigners { get; set; }
    public int MaxViewers { get; set; }
    public int MaxDesigners { get; set; }
}
