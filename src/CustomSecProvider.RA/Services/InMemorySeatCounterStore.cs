using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

public sealed class InMemorySeatCounterStore : ISeatCounterStore
{
    private static readonly Dictionary<string, int> Counters = new();
    private static readonly object _lock = new object();

    public Task<int> GetCurrentCountAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default)
    {
        var key = $"{tenantId}|{seatType}";
        lock (_lock)
        {
            Counters.TryGetValue(key, out var count);
            return Task.FromResult(count);
        }
    }

    public Task IncrementAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default)
    {
        var key = $"{tenantId}|{seatType}";
        lock (_lock)
        {
            if (Counters.TryGetValue(key, out var count))
                Counters[key] = count + 1;
            else
                Counters[key] = 1;
        }
        return Task.CompletedTask;
    }

    public Task DecrementAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default)
    {
        var key = $"{tenantId}|{seatType}";
        lock (_lock)
        {
            if (Counters.TryGetValue(key, out var count) && count > 0)
                Counters[key] = count - 1;
        }
        return Task.CompletedTask;
    }

    public Task ResetAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var keysToRemove = Counters.Keys.Where(k => k.StartsWith($"{tenantId}|")).ToList();
            foreach (var key in keysToRemove)
                Counters.Remove(key);
        }
        return Task.CompletedTask;
    }
}
