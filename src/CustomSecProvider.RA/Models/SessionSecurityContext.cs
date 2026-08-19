namespace CustomSecProvider.RA.Models;

public class SessionSecurityContext
{
    public required string UserId { get; set; }
    public required string TenantId { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
    public required PolicyDecision Decision { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public int TTLSeconds { get; set; } = 3600;
}
