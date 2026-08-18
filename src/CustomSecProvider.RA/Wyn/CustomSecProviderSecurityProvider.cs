using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrapeCity.Enterprise.Identity.ExternalIdentityProvider;
using GrapeCity.Enterprise.Identity.ExternalIdentityProvider.Configuration;
using GrapeCity.Enterprise.Identity.SecurityProvider;
using CustomSecProvider.RA.Services;
using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Wyn;

public sealed class CustomSecProviderSecurityProvider : ISecurityProvider
{
    private readonly PolicyEngine _policyEngine;

    // TODO: replace with real persistent store (Redis/DB) for production
    private static readonly Dictionary<string, SessionRecord> Sessions = new();

    public string ProviderName => "CustomSecProviderSecurityProvider";

    public CustomSecProviderSecurityProvider(IEnumerable<ConfigurationItem> configs)
    {
        // TODO: bind configs from Wyn config UI
        var identity = new StubIdentityTenantService();
        var entitlements = new StubEntitlementService();
        var seats = new StubSeatCounterStore();
        var posture = new StubSecurityPostureService();
        var audit = new StubAuditSink();

        _policyEngine = new PolicyEngine(identity, entitlements, seats, posture, audit);
    }

    public async Task<string> GenerateTokenAsync(string username, string password, object customizedParam = null)
    {
        try
        {
            // In your final version, exchange username/password/customizedParam for your real user token
            var userToken = !string.IsNullOrWhiteSpace(username) ? username : customizedParam?.ToString();

            if (string.IsNullOrWhiteSpace(userToken))
                return null;

            var decision = await _policyEngine.EvaluateAsync(userToken);

            if (!decision.IsAllowed)
                return null;

            var token = Guid.NewGuid().ToString("N");

            lock (Sessions)
            {
                Sessions[token] = new SessionRecord
                {
                    Token = token,
                    UserId = decision.UserId,
                    TenantId = decision.TenantId,
                    Roles = decision.Roles.ToArray(),
                    Organizations = decision.Organizations?.ToArray() ?? Array.Empty<string>(),
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
                };
            }

            return token;
        }
        catch
        {
            return null;
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        lock (Sessions)
        {
            if (!Sessions.TryGetValue(token, out var s)) return Task.FromResult(false);
            if (DateTime.UtcNow > s.ExpiresAtUtc)
            {
                Sessions.Remove(token);
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }

    public Task DisposeTokenAsync(string token)
    {
        lock (Sessions) { Sessions.Remove(token); }
        return Task.CompletedTask;
    }

    public Task<IExternalUserContext> GetUserContextAsync(string token)
    {
        var user = Resolve(token);
        return Task.FromResult<IExternalUserContext>(user);
    }

    public Task<IExternalUserDescriptor> GetUserDescriptorAsync(string token)
    {
        var user = Resolve(token);
        return Task.FromResult<IExternalUserDescriptor>(user);
    }

    public Task<string[]> GetUserRolesAsync(string token)
    {
        var user = Resolve(token);
        return Task.FromResult(user?.Roles?.ToArray() ?? Array.Empty<string>());
    }

    public Task<string[]> GetUserOrganizationsAsync(string token)
    {
        var user = Resolve(token);
        return Task.FromResult(user?.Organizations?.ToArray() ?? Array.Empty<string>());
    }

    private WynExternalUser Resolve(string token)
    {
        lock (Sessions)
        {
            if (!Sessions.TryGetValue(token, out var s)) return null;
            if (DateTime.UtcNow > s.ExpiresAtUtc)
            {
                Sessions.Remove(token);
                return null;
            }

            return new WynExternalUser(
                s.UserId,
                s.Roles,
                s.Organizations
            );
        }
    }

    private sealed class SessionRecord
    {
        public string Token { get; init; }
        public string UserId { get; init; }
        public string TenantId { get; init; }
        public string[] Roles { get; init; } = Array.Empty<string>();
        public string[] Organizations { get; init; } = Array.Empty<string>();
        public DateTime ExpiresAtUtc { get; init; }
    }

    // Minimal IExternalUserContext + IExternalUserDescriptor adapter
    private sealed class WynExternalUser : IExternalUserContext, IExternalUserDescriptor
    {
        public WynExternalUser(string userId, IEnumerable<string> roles, IEnumerable<string> orgs)
        {
            Id = userId;
            Name = userId;
            DisplayName = userId;
            Roles = roles?.ToList() ?? new List<string>();
            Organizations = orgs?.ToList() ?? new List<string>();
        }

        public string Id { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public IEnumerable<string> Roles { get; }
        public IEnumerable<string> Organizations { get; }
    }

    // ---- Temporary stub services for initial plugin load ----
    private sealed class StubIdentityTenantService : IIdentityTenantService
    {
        public Task<UserContext> GetUserContextAsync(string userToken, CancellationToken cancellationToken = default)
            => Task.FromResult(new UserContext
            {
                UserId = userToken,
                TenantId = "T-DEFAULT",
                IsTenantActive = true,
                IsUserActive = true,
                SeatType = SeatType.Viewer,
                DataRegion = "US",
                OrgUnits = Array.Empty<string>()
            });
    }

    private sealed class StubEntitlementService : IEntitlementService
    {
        public Task<EntitlementContext> GetEntitlementsAsync(string tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(new EntitlementContext
            {
                PlanTier = PlanTier.Enterprise,
                AllowExport = true,
                AllowScheduling = true,
                AllowPremiumDatasets = true,
                MaxConcurrentViewers = 100,
                MaxConcurrentDesigners = 50
            });
    }

    private sealed class StubSeatCounterStore : ISeatCounterStore
    {
        public Task<int> GetCurrentCountAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }

    private sealed class StubSecurityPostureService : ISecurityPostureService
    {
        public Task<SecurityPosture> GetPostureAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(SecurityPosture.Normal);
    }

    private sealed class StubAuditSink : IAuditDecisionSink
    {
        public Task WriteAsync(string tenantId, string userId, bool isAllowed, string reasonCode, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
