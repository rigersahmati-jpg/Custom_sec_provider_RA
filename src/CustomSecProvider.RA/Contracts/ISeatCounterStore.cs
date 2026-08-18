using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

public interface ISeatCounterStore
{
    Task<int> GetCurrentCountAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default);
}
