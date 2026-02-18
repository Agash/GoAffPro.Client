using GoAffPro.Client.Policies;
using Microsoft.Extensions.DependencyInjection;

namespace GoAffPro.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoAffProClient(
        this IServiceCollection services,
        Action<GoAffProClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        GoAffProClientOptions options = new();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        services
            .AddHttpClient<GoAffProClient>(
                static (serviceProvider, httpClient) =>
                {
                    GoAffProClientOptions configuredOptions = serviceProvider.GetRequiredService<GoAffProClientOptions>();
                    httpClient.BaseAddress = GoAffProClient.BuildBaseUri(configuredOptions.BaseUrl);
                    httpClient.Timeout = configuredOptions.Timeout;
                })
            .AddPolicyHandler(RetryPolicies.CreateTransientRetryPolicy())
            .AddPolicyHandler(RetryPolicies.CreateCircuitBreakerPolicy());

        services.AddTransient<IGoAffProClient>(static serviceProvider => serviceProvider.GetRequiredService<GoAffProClient>());
        return services;
    }
}
