using CustomSecProvider.RA.Models;

namespace CustomSecProvider.RA.Contracts;

/// <summary>
/// Service to resolve entitlements and feature flags from the billing/subscription system.
/// </summary>
public interface IEntitlementsService
{
    /// <summary>
    /// Get the subscription plan for a tenant.
    /// </summary>
    Task<SubscriptionPlan> GetPlanAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get enabled features for a tenant based on their plan and add-ons.
    /// </summary>
    Task<string[]> GetEnabledFeaturesAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a specific feature is enabled for a tenant.
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(string tenantId, string featureName, CancellationToken cancellationToken = default);
}
