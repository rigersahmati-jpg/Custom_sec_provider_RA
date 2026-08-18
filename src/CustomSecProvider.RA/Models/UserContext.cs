namespace CustomSecProvider.RA.Models;

public sealed class UserContext
{
    public required string UserId { get; init; }
    public required string TenantId { get; init; }
    public required bool IsUserActive { get; init; }
    public required bool IsTenantActive { get; init; }
    public required SeatType SeatType { get; init; }
    public required string DataRegion { get; init; }
    public required string[] OrgUnits { get; init; } = [];
}
