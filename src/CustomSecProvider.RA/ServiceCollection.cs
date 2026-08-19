using CustomSecProvider.RA.Contracts;
using CustomSecProvider.RA.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomSecProvider.RA;

/// <summary>
/// Extension method to register CSP services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Custom Security Provider services to the DI container.
    /// Call this in your Startup or Program.cs:
    ///   services.AddCustomSecurityProvider();
    /// </summary>
    public static IServiceCollection AddCustomSecurityProvider(this IServiceCollection services)
    {
        services.AddScoped<IPolicyEngine, PolicyEngine>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        
        // NOTE: Register your backend service implementations (IIdentityService, ITenantService, etc.)
        // Example:
        //   services.AddScoped<IIdentityService, MyIdentityService>();
        //   services.AddScoped<ITenantService, MyTenantService>();
        //   etc.
        
        return services;
    }
}
