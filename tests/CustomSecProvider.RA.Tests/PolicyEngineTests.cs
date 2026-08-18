using Xunit;
using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;
using CustomSecProvider.RA.Services;

namespace CustomSecProvider.RA.Tests;

public sealed class PolicyEngineTests
{
    [Fact]
    public async Task Disabled_User_Should_Be_Denied()
    {
        var sut = CreateEngine(new UserContext
        {
            UserId = "U-1",
            TenantId = "T-1",
            IsTenantActive = true,
            IsUserActive = false,
            SeatType = SeatType.Viewer,
            DataRegion = "US",
            OrgUnits = []
        });

        var decision = await sut.EvaluateAsync("token");

        Assert.False(decision.IsAllowed);
        Assert.Equal("USER_DISABLED", decision.ReasonCode);
    }

    [Fact]
    public async Task Enterprise_Designer_Should_Get_Designer_Role()
    {
        var sut = CreateEngine(new UserContext
        {
            UserId = "U-2",
            TenantId = "T-1",
            IsTenantActive = true,
            IsUserActive = true,
            SeatType = SeatType.Designer,
            DataRegion = "EU",
            OrgUnits = ["Finance"]
        }, new EntitlementContext
        {
            PlanTier = PlanTier.Enterprise,
            AllowExport = true,
            AllowScheduling = true,
            AllowPremiumDatasets = true,
            MaxConcurrentViewers = 10,
            MaxConcurrentDesigners = 5
        }, seatCount: 0);

        var decision = await sut.EvaluateAsync("token");

        Assert.True(decision.IsAllowed);
        Assert.Contains("WYN_DESIGNER", decision.Roles);
    }

    [Fact]
    public async Task Seat_Overage_Should_Be_Denied()
    {
        var sut = CreateEngine(new UserContext
        {
            UserId = "U-3",
            TenantId = "T-2",
            IsTenantActive = true,
            IsUserActive = true,
            SeatType = SeatType.Viewer,
            DataRegion = "US",
            OrgUnits = []
        }, new EntitlementContext
        {
            PlanTier = PlanTier.Pro,
            AllowExport = true,
            AllowScheduling = true,
            AllowPremiumDatasets = false,
            MaxConcurrentViewers = 2,
            MaxConcurrentDesigners = 1
        }, seatCount: 2);

        var decision = await sut.EvaluateAsync("token");

        Assert.False(decision.IsAllowed);
        Assert.Equal("SEAT_LIMIT_EXCEEDED", decision.ReasonCode);
    }

    [Fact]
    public async Task Incident_Mode_Should_Force_ReadOnly()
    {
        var sut = CreateEngine(new UserContext
        {
            UserId = "U-4",
            TenantId = "T-3",
            IsTenantActive = true,
            IsUserActive = true,
            SeatType = SeatType.Viewer,
            DataRegion = "US",
            OrgUnits = []
        }, posture: SecurityPosture.IncidentMode);

        var decision = await sut.EvaluateAsync("token");

        Assert.True(decision.IsAllowed);
        Assert.Contains("WYN_READ_ONLY", decision.Roles);
        Assert.Equal("INCIDENT_MODE", decision.ReasonCode);
    }

    private static PolicyEngine CreateEngine(
        UserContext user,
        EntitlementContext? entitlements = null,
        int seatCount = 0,
        SecurityPosture posture = SecurityPosture.Normal)
    {
        var identity = new FakeIdentityTenantService(user);
        var entitlement = new FakeEntitlementService(entitlements ?? new EntitlementContext
        {
            PlanTier = PlanTier.Free,
            AllowExport = false,
            AllowScheduling = false,
            AllowPremiumDatasets = false,
            MaxConcurrentViewers = 5,
            MaxConcurrentDesigners = 1
        });
        var seatStore = new FakeSeatCounterStore(seatCount);
        var postureSvc = new FakePostureService(posture);
        var audit = new NoopAuditSink();

        return new PolicyEngine(identity, entitlement, seatStore, postureSvc, audit);
    }

    private sealed class FakeIdentityTenantService(UserContext user) : IIdentityTenantService
    {
        public Task<UserContext> GetUserContextAsync(string userToken, CancellationToken cancellationToken = default) => Task.FromResult(user);
    }

    private sealed class FakeEntitlementService(EntitlementContext entitlements) : IEntitlementService
    {
        public Task<EntitlementContext> GetEntitlementsAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult(entitlements);
    }

    private sealed class FakeSeatCounterStore(int seatCount) : ISeatCounterStore
    {
        public Task<int> GetCurrentCountAsync(string tenantId, SeatType seatType, CancellationToken cancellationToken = default) => Task.FromResult(seatCount);
    }

    private sealed class FakePostureService(SecurityPosture posture) : ISecurityPostureService
    {
        public Task<SecurityPosture> GetPostureAsync(CancellationToken cancellationToken = default) => Task.FromResult(posture);
    }

    private sealed class NoopAuditSink : IAuditDecisionSink
    {
        public Task WriteAsync(string tenantId, string userId, bool isAllowed, string reasonCode, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
