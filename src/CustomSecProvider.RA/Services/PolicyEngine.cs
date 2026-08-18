using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Services;

public sealed class PolicyEngine(
    IIdentityTenantService identityTenantService,
    IEntitlementService entitlementService,
    ISeatCounterStore seatCounterStore,
    ISecurityPostureService postureService,
    IAuditDecisionSink auditSink)
{
    private const string ReasonAllowed = "ALLOWED";
    private const string ReasonTenantSuspended = "TENANT_SUSPENDED";
    private const string ReasonUserDisabled = "USER_DISABLED";
    private const string ReasonSeatExceeded = "SEAT_LIMIT_EXCEEDED";
    private const string ReasonIncidentMode = "INCIDENT_MODE";

    public async Task<PolicyDecision> EvaluateAsync(string userToken, CancellationToken cancellationToken = default)
    {
        var user = await identityTenantService.GetUserContextAsync(userToken, cancellationToken);

        if (!user.IsTenantActive)
            return await Deny(user, ReasonTenantSuspended, cancellationToken);

        if (!user.IsUserActive)
            return await Deny(user, ReasonUserDisabled, cancellationToken);

        var posture = await postureService.GetPostureAsync(cancellationToken);
        if (posture == SecurityPosture.IncidentMode)
        {
            var incidentDecision = BuildReadOnlyDecision(user, ReasonIncidentMode);
            await auditSink.WriteAsync(user.TenantId, user.UserId, incidentDecision.IsAllowed, incidentDecision.ReasonCode, incidentDecision.Roles, cancellationToken);
            return incidentDecision;
        }

        var entitlements = await entitlementService.GetEntitlementsAsync(user.TenantId, cancellationToken);

        var isSeatAllowed = await IsSeatAllowed(user, entitlements, cancellationToken);
        if (!isSeatAllowed)
            return await Deny(user, ReasonSeatExceeded, cancellationToken);

        var allowDecision = BuildAllowedDecision(user, entitlements);
        await auditSink.WriteAsync(user.TenantId, user.UserId, allowDecision.IsAllowed, allowDecision.ReasonCode, allowDecision.Roles, cancellationToken);
        return allowDecision;
    }

    private async Task<bool> IsSeatAllowed(UserContext user, EntitlementContext entitlements, CancellationToken cancellationToken)
    {
        var current = await seatCounterStore.GetCurrentCountAsync(user.TenantId, user.SeatType, cancellationToken);

        return user.SeatType switch
        {
            SeatType.Viewer => current < entitlements.MaxConcurrentViewers,
            SeatType.Designer => current < entitlements.MaxConcurrentDesigners,
            _ => false
        };
    }

    private static PolicyDecision BuildAllowedDecision(UserContext user, EntitlementContext entitlements)
    {
        var roles = ResolveRoles(user, entitlements);

        var claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tenant_id"] = user.TenantId,
            ["user_id"] = user.UserId,
            ["seat_type"] = user.SeatType.ToString(),
            ["plan_tier"] = entitlements.PlanTier.ToString(),
            ["data_region"] = user.DataRegion,
            ["allow_export"] = entitlements.AllowExport.ToString(),
            ["allow_scheduling"] = entitlements.AllowScheduling.ToString(),
            ["allow_premium_datasets"] = entitlements.AllowPremiumDatasets.ToString(),
            ["security_posture"] = SecurityPosture.Normal.ToString()
        };

        return new PolicyDecision
        {
            IsAllowed = true,
            ReasonCode = ReasonAllowed,
            Roles = roles,
            Claims = claims
        };
    }

    private static string[] ResolveRoles(UserContext user, EntitlementContext entitlements)
    {
        if (entitlements.PlanTier == PlanTier.Enterprise && user.SeatType == SeatType.Designer)
            return ["WYN_DESIGNER"];

        if (entitlements.PlanTier == PlanTier.Pro)
            return ["WYN_VIEWER", "WYN_PRO_FEATURES"];

        return ["WYN_VIEWER"];
    }

    private static PolicyDecision BuildReadOnlyDecision(UserContext user, string reason)
    {
        var claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tenant_id"] = user.TenantId,
            ["user_id"] = user.UserId,
            ["security_posture"] = SecurityPosture.IncidentMode.ToString(),
            ["allow_export"] = bool.FalseString,
            ["allow_scheduling"] = bool.FalseString,
            ["allow_download"] = bool.FalseString
        };

        return new PolicyDecision
        {
            IsAllowed = true,
            ReasonCode = reason,
            Roles = ["WYN_READ_ONLY"],
            Claims = claims
        };
    }

    private async Task<PolicyDecision> Deny(UserContext user, string reason, CancellationToken cancellationToken)
    {
        var decision = new PolicyDecision
        {
            IsAllowed = false,
            ReasonCode = reason,
            Roles = [],
            Claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["tenant_id"] = user.TenantId,
                ["user_id"] = user.UserId,
                ["security_posture"] = SecurityPosture.Normal.ToString()
            }
        };

        await auditSink.WriteAsync(user.TenantId, user.UserId, decision.IsAllowed, decision.ReasonCode, decision.Roles, cancellationToken);
        return decision;
    }
}
