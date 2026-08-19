using System;
using System.Threading;
using System.Threading.Tasks;
using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

public sealed class RealEntitlementService : IEntitlementService
{
    private readonly HttpClient _httpClient;
    private readonly string _entitlementsServiceUrl;

    public RealEntitlementService(string entitlementsServiceUrl = null)
    {
        _entitlementsServiceUrl = entitlementsServiceUrl ?? Environment.GetEnvironmentVariable("ENTITLEMENTS_SERVICE_URL") ?? "http://localhost:5000";
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<EntitlementContext?> GetEntitlementsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                return null;

            PlanTier planTier;
            if (tenantId.StartsWith("enterprise-", StringComparison.OrdinalIgnoreCase))
                planTier = PlanTier.Enterprise;
            else if (tenantId.StartsWith("pro-", StringComparison.OrdinalIgnoreCase))
                planTier = PlanTier.Pro;
            else
                planTier = PlanTier.Free;

            return planTier switch
            {
                PlanTier.Free => new EntitlementContext
                {
                    PlanTier = PlanTier.Free,
                    AllowExport = false,
                    AllowScheduling = false,
                    AllowPremiumDatasets = false,
                    MaxConcurrentViewers = 10,
                    MaxConcurrentDesigners = 0,
                    MaxConcurrentAdmins = 0
                },

                PlanTier.Pro => new EntitlementContext
                {
                    PlanTier = PlanTier.Pro,
                    AllowExport = true,
                    AllowScheduling = false,
                    AllowPremiumDatasets = false,
                    MaxConcurrentViewers = 25,
                    MaxConcurrentDesigners = 10,
                    MaxConcurrentAdmins = 0
                },

                PlanTier.Enterprise => new EntitlementContext
                {
                    PlanTier = PlanTier.Enterprise,
                    AllowExport = true,
                    AllowScheduling = true,
                    AllowPremiumDatasets = true,
                    MaxConcurrentViewers = 500,
                    MaxConcurrentDesigners = 100,
                    MaxConcurrentAdmins = 50
                },

                _ => null
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Entitlements service error: {ex.Message}");
            return null;
        }
    }
}
