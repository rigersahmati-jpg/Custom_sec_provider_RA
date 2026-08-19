namespace CustomSecProvider.RA.Configuration;

public class CSPConfiguration
{
    public string? IdentityServiceUrl { get; set; }
    public string? EntitlementsServiceUrl { get; set; }
    public string? TenantServiceUrl { get; set; }
    public string? RedisConnectionString { get; set; }
    public int CacheTTLSeconds { get; set; } = 120;
    public bool FailClosedOnBackendError { get; set; } = true;
    public string? SeatOverageAction { get; set; } = "Deny";
}
