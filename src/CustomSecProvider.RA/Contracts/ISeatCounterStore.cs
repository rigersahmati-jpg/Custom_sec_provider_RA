using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

public interface ISeatCounterStore
{
    Task<int> GetCurrentCountAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);
    Task IncrementAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);
    Task DecrementAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);
    Task ResetAsync(string tenantId, CancellationToken cancellationToken = default);
}
