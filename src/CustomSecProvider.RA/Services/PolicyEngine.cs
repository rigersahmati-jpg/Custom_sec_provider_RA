using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Models;
using Microsoft.Extensions.Logging;

namespace CustomSecProvider.RA.Services;

/// <summary>
/// Core policy evaluation engine implementing the Zero-Sync Governance model.
/// Evaluates tenant/user state, entitlements, seats, and incident mode in real time.
/// </summary>
public class PolicyEngine : IPolicyEngine
{
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly IEntitlementsService _entitlementsService;
    private readonly ISeatManagementService _seatService;
    private readonly IIncidentModeService _incidentModeService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<PolicyEngine> _logger;

    public PolicyEngine(
        ITenantService tenantService,
        IIdentityService identityService,
        IEntitlementsService entitlementsService,
        ISeatManagementService seatService,
        IIncidentModeService incidentModeService,
        IAuditLogger auditLogger,
        ILogger<PolicyEngine> logger)
    {
        _tenantService = tenantService;
        _identityService = identityService;
        _entitlementsService = entitlementsService;
        _seatService = seatService;
        _incidentModeService = incidentModeService;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<PolicyDecision> EvaluateAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var decision = new PolicyDecision();

        try
        {
            // 1. Resolve tenant context
            var tenant = await _tenantService.ResolveTenantAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                decision.IsAllowed = false;
                decision.ReasonCode = ReasonCodes.InvalidToken;
                decision.Message = "Tenant not found.";
                _logger.LogWarning("Policy evaluation failed: tenant not found. TenantId={TenantId}", tenantId);
                return decision;
            }

            // 2. Check tenant status
            if (tenant.Status == TenantStatus.Suspended)
            {
                decision.IsAllowed = false;
                decision.ReasonCode = ReasonCodes.TenantSuspended;
                decision.Message = "Your organization's analytics access is temporarily suspended.";
                _logger.LogWarning(
                    "Policy evaluation blocked: tenant suspended. TenantId={TenantId}, UserId={UserId}",
                    tenantId, userId);
                await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
                return decision;
            }

            if (tenant.Status == TenantStatus.PastDue)
            {
                decision.IsAllowed = false;
                decision.ReasonCode = ReasonCodes.TenantPastDue;
                decision.Message = "Your organization's account is past due. Please contact billing.";
                _logger.LogWarning(
                    "Policy evaluation blocked: tenant past due. TenantId={TenantId}, UserId={UserId}",
                    tenantId, userId);
                await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
                return decision;
            }

            // 3. Resolve user context
            var user = await _identityService.ResolveUserAsync(userId, cancellationToken);
            if (user == null)
            {
                decision.IsAllowed = false;
                decision.ReasonCode = ReasonCodes.InvalidToken;
                decision.Message = "User not found.";
                _logger.LogWarning(
                    "Policy evaluation failed: user not found. UserId={UserId}, TenantId={TenantId}",
                    userId, tenantId);
                return decision;
            }

            // 4. Check user status
            if (user.Status == UserStatus.Disabled)
            {
                decision.IsAllowed = false;
                decision.ReasonCode = ReasonCodes.UserDisabled;
                decision.Message = "Your account is disabled. Please contact your administrator.";
                _logger.LogWarning(
                    "Policy evaluation blocked: user disabled. UserId={UserId}, TenantId={TenantId}",
                    userId, tenantId);
                await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
                return decision;
            }

            if (user.Status == UserStatus.Suspended)
            {
                decision.IsAllowed = false;
                decision.ReasonCode = ReasonCodes.UserSuspended;
                decision.Message = "Your account is suspended. Please contact your administrator.";
                _logger.LogWarning(
                    "Policy evaluation blocked: user suspended. UserId={UserId}, TenantId={TenantId}",
                    userId, tenantId);
                await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
                return decision;
            }

            // 5. Check incident mode (break-glass security)
            var incidentModeEnabled = await _incidentModeService.IsIncidentModeEnabledAsync(cancellationToken);
            if (incidentModeEnabled)
            {
                decision.Roles = new[] { "WYN_READ_ONLY" };
                decision.Claims["security_posture"] = "incident";
                decision.Claims["allow_export"] = false;
                decision.Claims["allow_schedule"] = false;
                decision.ReasonCode = ReasonCodes.IncidentMode;
                decision.Message = "System is in incident mode. Access is read-only.";
                decision.IsAllowed = true;
                _logger.LogInformation(
                    "Incident mode active. User downgraded to read-only. UserId={UserId}, TenantId={TenantId}",
                    userId, tenantId);
                await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
                return decision;
            }

            // 6. Get plan and entitlements
            var plan = await _entitlementsService.GetPlanAsync(tenantId, cancellationToken);
            var features = await _entitlementsService.GetEnabledFeaturesAsync(tenantId, cancellationToken);

            // 7. Check seat limits based on seat type
            if (user.SeatType == SeatType.Designer || user.SeatType == SeatType.Admin)
            {
                var canAllocate = await _seatService.CanAllocateSeatAsync(tenantId, user.SeatType, cancellationToken);
                if (!canAllocate)
                {
                    decision.IsAllowed = false;
                    decision.ReasonCode = ReasonCodes.SeatLimitExceeded;
                    decision.Message = "Your organization has reached the maximum Designer seats. Please upgrade or contact support.";
                    _logger.LogWarning(
                        "Policy evaluation blocked: designer seat limit exceeded. UserId={UserId}, TenantId={TenantId}",
                        userId, tenantId);
                    await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
                    return decision;
                }
            }
            else if (user.SeatType == SeatType.Viewer)
            {
                var canAllocate = await _seatService.CanAllocateSeatAsync(tenantId, SeatType.Viewer, cancellationToken);
                if (!canAllocate)
                {
                    decision.IsAllowed = false;
                    decision.ReasonCode = ReasonCodes.SeatLimitExceeded;
                    decision.Message = "Your organization has reached the maximum Viewer seats. Please upgrade or contact support.";
                    _logger.LogWarning(
                        "Policy evaluation blocked: viewer seat limit exceeded. UserId={UserId}, TenantId={TenantId}",
                        userId, tenantId);
                    await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
                    return decision;
                }
            }

            // 8. Map plan to Wyn roles and inject claims
            ApplyRoleMapping(decision, plan, features, user, tenant);

            // 9. Mark success
            decision.IsAllowed = true;
            decision.ReasonCode = ReasonCodes.Success;
            decision.Message = "Access granted.";

            _logger.LogInformation(
                "Policy evaluation succeeded. UserId={UserId}, TenantId={TenantId}, Plan={Plan}, Roles={Roles}",
                userId, tenantId, plan, string.Join(",", decision.Roles));

            await _auditLogger.LogDecisionAsync(userId, tenantId, decision, cancellationToken);
            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Policy evaluation error. UserId={UserId}, TenantId={TenantId}",
                userId, tenantId);

            decision.IsAllowed = false;
            decision.ReasonCode = ReasonCodes.BackendUnavailable;
            decision.Message = "An error occurred during policy evaluation. Please retry.";
            return decision;
        }
    }

    private void ApplyRoleMapping(
        PolicyDecision decision,
        SubscriptionPlan plan,
        string[] features,
        UserContext user,
        TenantContext tenant)
    {
        // Initialize claims
        decision.Claims["tenant_id"] = tenant.TenantId;
        decision.Claims["user_id"] = user.UserId;
        decision.Claims["data_region"] = tenant.DataRegion;
        decision.Claims["plan_tier"] = plan.ToString();
        decision.Claims["security_posture"] = "normal";
        decision.Claims["seat_type"] = user.SeatType.ToString();

        if (!string.IsNullOrEmpty(tenant.OrgUnit))
        {
            decision.Claims["org_unit"] = tenant.OrgUnit;
        }

        if (features.Length > 0)
        {
            decision.Claims["enabled_features"] = features;
        }

        // Map plan to Wyn roles
        var roles = new List<string>();

        switch (plan)
        {
            case SubscriptionPlan.Enterprise:
                if (user.SeatType == SeatType.Admin)
                {
                    roles.Add("WYN_TENANT_ADMIN");
                }
                else if (user.SeatType == SeatType.Designer)
                {
                    roles.Add("WYN_DESIGNER");
                }
                else
                {
                    roles.Add("WYN_VIEWER");
                }

                // Enterprise features
                decision.Claims["allow_export"] = features.Contains("export");
                decision.Claims["allow_schedule"] = features.Contains("schedule");
                decision.Claims["allow_authoring"] = true;
                decision.Claims["allow_premium_datasets"] = features.Contains("premium_datasets");
                break;

            case SubscriptionPlan.Pro:
                roles.Add("WYN_VIEWER");
                decision.Claims["allow_export"] = features.Contains("export");
                decision.Claims["allow_schedule"] = features.Contains("schedule");
                decision.Claims["allow_authoring"] = false;
                decision.Claims["allow_premium_datasets"] = false;
                break;

            case SubscriptionPlan.Free:
                roles.Add("WYN_VIEWER");
                decision.Claims["allow_export"] = false;
                decision.Claims["allow_schedule"] = false;
                decision.Claims["allow_authoring"] = false;
                decision.Claims["allow_premium_datasets"] = false;
                break;

            case SubscriptionPlan.Custom:
            default:
                roles.Add("WYN_VIEWER");
                decision.Claims["allow_export"] = features.Contains("export");
                decision.Claims["allow_schedule"] = features.Contains("schedule");
                decision.Claims["allow_authoring"] = features.Contains("authoring");
                decision.Claims["allow_premium_datasets"] = features.Contains("premium_datasets");
                break;
        }

        decision.Roles = roles.ToArray();
    }
}
