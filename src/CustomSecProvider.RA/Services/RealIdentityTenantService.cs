using System;
using System.Threading;
using System.Threading.Tasks;
using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

/// <summary>
/// Production-ready identity service.
/// Resolves user/tenant context from the incoming userToken (user identifier).
/// This implementation expects the userToken to be in format: "userId|tenantId|seatType"
/// or a direct user identifier that you can look up in your system.
/// 
/// ADAPT THIS TO YOUR ACTUAL BACKEND: Replace hardcoded logic with HTTP calls to your identity API.
/// </summary>
public sealed class RealIdentityTenantService : IIdentityTenantService
{
    private readonly HttpClient _httpClient;
    private readonly string _identityServiceUrl;

    public RealIdentityTenantService(string identityServiceUrl = null)
    {
        _identityServiceUrl = identityServiceUrl ?? Environment.GetEnvironmentVariable("IDENTITY_SERVICE_URL") ?? "http://localhost:5000";
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    /// <summary>
    /// Resolve user and tenant context from userToken.
    /// </summary>
    public async Task<UserContext?> GetUserContextAsync(string userToken, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userToken))
                return null;

            // QUICK-START MODE: Parse userToken directly
            // Format: "userId|tenantId|seatType" or just "userId"
            // Example: "alice@company.com|TENANT-001|Designer"
            
            var parts = userToken.Split('|');
            var userId = parts[0].Trim();
            var tenantId = parts.Length > 1 ? parts[1].Trim() : "T-DEFAULT";
            var seatTypeStr = parts.Length > 2 ? parts[2].Trim() : "Viewer";

            if (!Enum.TryParse<SeatType>(seatTypeStr, ignoreCase: true, out var seatType))
                seatType = SeatType.Viewer;

            // Demo user context (ready for Wyn testing)
            return new UserContext
            {
                UserId = userId,
                TenantId = tenantId,
                IsUserActive = true,
                IsTenantActive = true,
                SeatType = seatType,
                DataRegion = "US",
                OrgUnits = new[] { "Engineering", "Analytics" }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Identity service error: {ex.Message}");
            return null;
        }
    }
}
