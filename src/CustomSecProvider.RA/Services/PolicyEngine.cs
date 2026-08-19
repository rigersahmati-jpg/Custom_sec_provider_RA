using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

public sealed class PolicyEngine : IPolicyEngine
{
    private readonly IIdentityTenantService _identityService;
    private readonly IEntitlementService _entitlementService;
    private readonly ISeatCounterStore _seatStore;
    private readonly ISecurityPostureService _postureService;
    private readonly IAuditDecisionSink _auditSink;

    public PolicyEngine(
        IIdentityTenantService identityService,
        IEntitlementService entitlementService,
        ISeatCounterStore seatStore,
        ISecurityPostureService postureService,
        IAuditDecisionSink auditSink)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _entitlementService = entitlementService ?? throw new ArgumentNullException(nameof(entitlementService));
        _seatStore = seatStore ?? throw new ArgumentNullException(nameof(seatStore));
        _postureService = postureService ?? throw new ArgumentNullException(nameof(postureService));
        _auditSink = auditSink ?? throw new ArgumentNullException(nameof(auditSink));
    }

    public async Task<PolicyDecision> EvaluateAsync(string userToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var userContext = await _identityService.GetUserContextAsync(userToken, cancellationToken);
            if (userContext == null)
            {
                return DenyDecision(
                    userId: userToken,
                    tenantId: "UNKNOWN",
                    reasonCode: ReasonCode.USER_NOT_FOUND);
            }

            if (!userContext.IsUserActive)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.USER_DISABLED);
            }

            if (!userContext.IsTenantActive)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.TENANT_INACTIVE);
            }

            var entitlements = await _entitlementService.GetEntitlementsAsync(userContext.TenantId, cancellationToken);
            if (entitlements == null)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.TENANT_NOT_FOUND);
            }

            var posture = await _postureService.GetPostureAsync(cancellationToken);
            if (posture == SecurityPosture.IncidentDenied)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.INCIDENT_MODE_DENIED);
            }

            var currentSeats = await _seatStore.GetCurrentCountAsync(userContext.TenantId, userContext.SeatType, cancellationToken);
            var maxSeats = GetMaxSeatsForType(entitlements, userContext.SeatType);

            if (currentSeats >= maxSeats)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.SEAT_LIMIT_EXCEEDED);
            }

            var roles = MapRolesByPlanTier(entitlements.PlanTier, posture);
            var claims = BuildClaims(userContext, entitlements, posture);

            await _seatStore.IncrementAsync(userContext.TenantId, userContext.SeatType, cancellationToken);

            var decision = new PolicyDecision
            {
                IsAllowed = true,
                UserId = userContext.UserId,
                TenantId = userContext.TenantId,
                Roles = roles,
                Organizations = userContext.OrgUnits,
                Claims = claims,
                ReasonCode = ReasonCode.ACCESS_GRANTED,
                SessionTTLSeconds = 1800
            };

            await _auditSink.WriteAsync(
                userContext.TenantId,
                userContext.UserId,
                true,
                ReasonCode.ACCESS_GRANTED,
                roles.ToList(),
                cancellationToken);

            return decision;
        }
        catch (Exception ex)
        {
            return DenyDecision(
                userId: userToken,
                tenantId: "UNKNOWN",
                reasonCode: ReasonCode.EVALUATION_ERROR);
        }
    }

    private PolicyDecision DenyDecision(string userId, string tenantId, string reasonCode)
    {
        _ = _auditSink.WriteAsync(tenantId, userId, false, reasonCode, Array.Empty<string>());
        return new PolicyDecision
        {
            IsAllowed = false,
            UserId = userId,
            TenantId = tenantId,
            Roles = Array.Empty<string>(),
            ReasonCode = reasonCode,
            SessionTTLSeconds = 0
        };
    }

    private static IEnumerable<string> MapRolesByPlanTier(PlanTier planTier, SecurityPosture posture)
    {
        var roles = new List<string>();

        if (posture == SecurityPosture.IncidentReadOnly)
        {
            roles.Add("Viewer");
            return roles;
        }

        switch (planTier)
        {
            case PlanTier.Free:
                roles.Add("Viewer");
                break;

            case PlanTier.Pro:
                roles.Add("Viewer");
                roles.Add("Designer");
                break;

            case PlanTier.Enterprise:
                roles.Add("Viewer");
                roles.Add("Designer");
                roles.Add("Admin");
                break;
        }

        return roles;
    }

    private static IReadOnlyDictionary<string, object> BuildClaims(
        UserContext userContext,
        EntitlementContext entitlements,
        SecurityPosture posture)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = userContext.UserId,
            ["tenant_id"] = userContext.TenantId,
            ["plan_tier"] = entitlements.PlanTier.ToString(),
            ["data_region"] = userContext.DataRegion,
            ["allow_export"] = entitlements.AllowExport && posture != SecurityPosture.IncidentReadOnly,
            ["allow_scheduling"] = entitlements.AllowScheduling && posture != SecurityPosture.IncidentReadOnly,
            ["allow_premium_datasets"] = entitlements.AllowPremiumDatasets,
            ["incident_mode"] = posture != SecurityPosture.Normal
        };

        if (userContext.OrgUnits.Length > 0)
        {
            claims["org_units"] = userContext.OrgUnits;
        }

        return claims;
    }

    private static int GetMaxSeatsForType(EntitlementContext entitlements, SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Viewer => entitlements.MaxConcurrentViewers,
            SeatType.Designer => entitlements.MaxConcurrentDesigners,
            SeatType.Admin => entitlements.MaxConcurrentAdmins,
            _ => 0
        };
    }
}
