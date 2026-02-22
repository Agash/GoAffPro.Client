using GoAffPro.Client.Policies;
using Microsoft.Extensions.DependencyInjection;

namespace GoAffPro.Client;

/// <summary>
/// Dependency injection extensions for registering <see cref="GoAffProClient"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="GoAffProClient"/> and related dependencies in the service collection.
    /// </summary>
    /// <param name="services">Service collection to register into.</param>
    /// <param name="configureOptions">Optional callback for configuring <see cref="GoAffProClientOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGoAffProClient(
        this IServiceCollection services,
        Action<GoAffProClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        GoAffProClientOptions options = new();
        configureOptions?.Invoke(options);
        _ = services.AddSingleton(options);

        _ = services
            .AddHttpClient<GoAffProClient>(
                static (serviceProvider, httpClient) =>
                {
                    GoAffProClientOptions configuredOptions = serviceProvider.GetRequiredService<GoAffProClientOptions>();
                    httpClient.BaseAddress = GoAffProClient.BuildBaseUri(configuredOptions.BaseUrl);
                    httpClient.Timeout = configuredOptions.Timeout;
                })
            .AddPolicyHandler(RetryPolicies.CreateTransientRetryPolicy())
            .AddPolicyHandler(RetryPolicies.CreateCircuitBreakerPolicy());

        _ = services.AddTransient<IGoAffProClient>(static serviceProvider => serviceProvider.GetRequiredService<GoAffProClient>());
        return services;
    }
}
