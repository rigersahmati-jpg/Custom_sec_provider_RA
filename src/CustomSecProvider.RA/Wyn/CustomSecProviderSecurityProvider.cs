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
    private static readonly Dictionary<string, SessionRecord> Sessions = new();
    private static readonly object _sessionsLock = new object();

    public string ProviderName => "CustomSecProviderSecurityProvider";

    public CustomSecProviderSecurityProvider(IEnumerable<ConfigurationItem> configs)
    {
        try
        {
            var identity = new RealIdentityTenantService();
            var entitlements = new RealEntitlementService();
            var seats = new InMemorySeatCounterStore();
            var posture = new SimpleSecurityPostureService();
            var audit = new SimpleAuditSink();

            _policyEngine = new PolicyEngine(identity, entitlements, seats, posture, audit);
            System.Diagnostics.Debug.WriteLine("[CustomSecProviderSecurityProvider] Initialized successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomSecProviderSecurityProvider] Init error: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GenerateTokenAsync(string username, string password, object customizedParam = null)
    {
        try
        {
            var userToken = !string.IsNullOrWhiteSpace(username) ? username : customizedParam?.ToString();
            if (string.IsNullOrWhiteSpace(userToken))
                return null;

            var decision = await _policyEngine.EvaluateAsync(userToken);
            if (!decision.IsAllowed)
                return null;

            var token = Guid.NewGuid().ToString("N");
            lock (_sessionsLock)
            {
                Sessions[token] = new SessionRecord
                {
                    Token = token,
                    UserId = decision.UserId,
                    TenantId = decision.TenantId,
                    Roles = decision.Roles.ToArray(),
                    Organizations = decision.Organizations?.ToArray() ?? Array.Empty<string>(),
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
                    Claims = decision.Claims
                };
            }
            return token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GenerateTokenAsync] Error: {ex.Message}");
            return null;
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        lock (_sessionsLock)
        {
            if (!Sessions.TryGetValue(token, out var s))
                return Task.FromResult(false);
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
        lock (_sessionsLock) { Sessions.Remove(token); }
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
        lock (_sessionsLock)
        {
            if (!Sessions.TryGetValue(token, out var s)) return null;
            if (DateTime.UtcNow > s.ExpiresAtUtc)
            {
                Sessions.Remove(token);
                return null;
            }
            return new WynExternalUser(s.UserId, s.Roles, s.Organizations, s.TenantId, s.Claims);
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
        public IReadOnlyDictionary<string, object> Claims { get; init; } = new Dictionary<string, object>();
    }

    private sealed class WynExternalUser : IExternalUserContext, IExternalUserDescriptor
    {
        public WynExternalUser(string userId, IEnumerable<string> roles, IEnumerable<string> orgs, string tenantId = null, IReadOnlyDictionary<string, object> claims = null)
        {
            Id = userId;
            Name = userId;
            DisplayName = userId;
            Roles = roles?.ToList() ?? new List<string>();
            Organizations = orgs?.ToList() ?? new List<string>();
            TenantId = tenantId;
            Claims = claims ?? new Dictionary<string, object>();
        }

        public string Id { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public IEnumerable<string> Roles { get; }
        public IEnumerable<string> Organizations { get; }
        public string TenantId { get; }
        public IReadOnlyDictionary<string, object> Claims { get; }
    }
}
