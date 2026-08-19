using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Tracks and enforces concurrent seat usage per tenant and seat type.
/// Implement this with Redis, in-memory cache, or your distributed store.
/// </summary>
public interface ISeatCounterStore
{
    /// <summary>
    /// Get the current count of active sessions for a given tenant and seat type.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="seatType">Seat type (Viewer, Designer, Admin).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current active seat count.</returns>
    Task<int> GetCurrentCountAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment seat count (called when a session is created).
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="seatType">Seat type to increment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IncrementAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrement seat count (called when a session expires or is disposed).
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="seatType">Seat type to decrement.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DecrementAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);
}
