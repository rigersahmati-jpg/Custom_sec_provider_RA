using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

/// <summary>
/// Core policy evaluation engine implementing Zero-Sync Governance.
/// Evaluates user/tenant state, entitlements, seat limits, and incident mode at session time.
/// </summary>
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

    /// <summary>
    /// Evaluate policy for a user session following the Zero-Sync Governance pattern.
    /// </summary>
    public async Task<PolicyDecision> EvaluateAsync(string userToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Resolve user and tenant context
            var userContext = await _identityService.GetUserContextAsync(userToken, cancellationToken);
            if (userContext == null)
            {
                return DenyDecision(
                    userId: userToken,
                    tenantId: "UNKNOWN",
                    reasonCode: ReasonCode.USER_NOT_FOUND);
            }

            // Step 2: Check if user and tenant are active
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

            // Step 3: Resolve entitlements (real-time plan check)
            var entitlements = await _entitlementService.GetEntitlementsAsync(userContext.TenantId, cancellationToken);
            if (entitlements == null)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.TENANT_NOT_FOUND);
            }

            // Step 4: Check security posture (incident mode)
            var posture = await _postureService.GetPostureAsync(cancellationToken);
            if (posture == SecurityPosture.IncidentDenied)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.INCIDENT_MODE_DENIED);
            }

            // Step 5: Check seat limits
            var currentSeats = await _seatStore.GetCurrentCountAsync(userContext.TenantId, userContext.SeatType, cancellationToken);
            var maxSeats = GetMaxSeatsForType(entitlements, userContext.SeatType);

            if (currentSeats >= maxSeats)
            {
                return DenyDecision(
                    userId: userContext.UserId,
                    tenantId: userContext.TenantId,
                    reasonCode: ReasonCode.SEAT_LIMIT_EXCEEDED);
            }

            // Step 6: Map roles based on plan tier and security posture
            var roles = MapRolesByPlanTier(entitlements.PlanTier, posture);
            var claims = BuildClaims(userContext, entitlements, posture);

            // Step 7: Increment seat counter
            await _seatStore.IncrementAsync(userContext.TenantId, userContext.SeatType, cancellationToken);

            // Step 8: Build allow decision
            var decision = new PolicyDecision
            {
                IsAllowed = true,
                UserId = userContext.UserId,
                TenantId = userContext.TenantId,
                Roles = roles,
                Organizations = userContext.OrgUnits,
                Claims = claims,
                ReasonCode = ReasonCode.ACCESS_GRANTED,
                SessionTTLSeconds = 1800 // 30 minutes
            };

            // Step 9: Audit
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
            // Log and fail closed
            return DenyDecision(
                userId: userToken,
                tenantId: "UNKNOWN",
                reasonCode: ReasonCode.EVALUATION_ERROR);
        }
    }

    /// <summary>
    /// Build a deny decision with audit.
    /// </summary>
    private PolicyDecision DenyDecision(string userId, string tenantId, string reasonCode)
    {
        // Audit deny decision (fire and forget to avoid blocking on audit errors)
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

    /// <summary>
    /// Map roles based on plan tier.
    /// </summary>
    private static IEnumerable<string> MapRolesByPlanTier(PlanTier planTier, SecurityPosture posture)
    {
        var roles = new List<string>();

        // If incident mode is read-only, only grant Viewer
        if (posture == SecurityPosture.IncidentReadOnly)
        {
            roles.Add("Viewer");
            return roles;
        }

        // Map roles by plan tier
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

    /// <summary>
    /// Build immutable claims for data scoping and feature control.
    /// </summary>
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

    /// <summary>
    /// Get max concurrent seats for a seat type based on entitlements.
    /// </summary>
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
