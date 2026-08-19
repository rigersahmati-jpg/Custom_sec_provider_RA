namespace CustomSecProvider.RA.Models;

public class UserContext
{
    public required string UserId { get; set; }
    public required string TenantId { get; set; }
    public bool IsUserActive { get; set; }
    public bool IsTenantActive { get; set; }
    public SeatType SeatType { get; set; }
    public string DataRegion { get; set; } = "US";
    public string[] OrgUnits { get; set; } = Array.Empty<string>();
}

public enum SeatType
{
    Viewer,
    Designer,
    Admin
}
