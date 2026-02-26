using GoAffPro.Client.Policies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoAffPro.Client;

/// <summary>
/// Dependency injection extensions for registering <see cref="GoAffProClient"/>.
/// </summary>
/// <example>
/// Bind from <c>appsettings.json</c>:
/// <code>
/// // appsettings.json
/// { "GoAffPro": { "BearerToken": "your-token" } }
///
/// builder.Services.AddGoAffProClient(builder.Configuration.GetSection("GoAffPro"));
/// </code>
///
/// Or configure inline:
/// <code>
/// builder.Services.AddGoAffProClient(opt =>
///     opt.BearerToken = Environment.GetEnvironmentVariable("GOAFFPRO_TOKEN"));
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="GoAffProClient"/> with inline option configuration.
    /// </summary>
    /// <param name="services">Service collection to register into.</param>
    /// <param name="configureOptions">Optional callback to configure options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGoAffProClient(
        this IServiceCollection services,
        Action<GoAffProClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        GoAffProClientOptions options = new();
        configureOptions?.Invoke(options);
        return RegisterCore(services, options);
    }

    /// <summary>
    /// Registers <see cref="GoAffProClient"/> and binds <see cref="GoAffProClientOptions"/>
    /// from an <see cref="IConfiguration"/> section.
    /// </summary>
    /// <param name="services">Service collection to register into.</param>
    /// <param name="configuration">
    /// Configuration section to bind (e.g. <c>builder.Configuration.GetSection("GoAffPro")</c>).
    /// Supported keys: <c>BaseUrl</c>, <c>BearerToken</c>, <c>Timeout</c>,
    /// <c>MaxRetries</c>, <c>CircuitBreakerThreshold</c>, <c>CircuitBreakerDuration</c>,
    /// <c>DefaultPollingInterval</c>, <c>DefaultPageSize</c>.
    /// </param>
    /// <param name="configureOptions">
    /// Optional additional callback applied after config binding (e.g. to read secrets).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGoAffProClient(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<GoAffProClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        GoAffProClientOptions options = new();
        configuration.Bind(options);
        configureOptions?.Invoke(options);
        return RegisterCore(services, options);
    }

    private static IServiceCollection RegisterCore(IServiceCollection services, GoAffProClientOptions options)
    {
        _ = services.AddSingleton(options);

        // GoAffProClient must be singleton: it owns bearer token state and observer loops.
        // Transient or scoped registration would silently lose auth state between resolutions.
        _ = services
            .AddHttpClient<GoAffProClient>(
                (sp, http) =>
                {
                    GoAffProClientOptions opts = sp.GetRequiredService<GoAffProClientOptions>();
                    http.BaseAddress = GoAffProClient.BuildBaseUri(opts.BaseUrl);
                    http.Timeout = opts.Timeout;
                })
            .AddPolicyHandler((sp, _) =>
                RetryPolicies.CreateTransientRetryPolicy(sp.GetRequiredService<GoAffProClientOptions>().MaxRetries))
            .AddPolicyHandler((sp, _) =>
            {
                GoAffProClientOptions opts = sp.GetRequiredService<GoAffProClientOptions>();
                return RetryPolicies.CreateCircuitBreakerPolicy(opts.CircuitBreakerThreshold, opts.CircuitBreakerDuration);
            });

        _ = services.AddSingleton<IGoAffProClient>(static sp => sp.GetRequiredService<GoAffProClient>());
        return services;
    }
}
