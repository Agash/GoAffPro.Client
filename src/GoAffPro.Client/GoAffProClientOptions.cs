namespace GoAffPro.Client;

/// <summary>
/// Configuration options for <see cref="GoAffProClient"/>.
/// </summary>
/// <remarks>
/// When using dependency injection, bind this from your configuration section via
/// <see cref="ServiceCollectionExtensions.AddGoAffProClient(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action{GoAffProClientOptions}?)"/>.
/// </remarks>
public sealed class GoAffProClientOptions
{
    /// <summary>
    /// Base API URL for GoAffPro. Defaults to <c>https://api.goaffpro.com/v1/</c>.
    /// </summary>
    /// <remarks>Override only when targeting a local mock or non-standard deployment.</remarks>
    public Uri BaseUrl { get; set; } = new("https://api.goaffpro.com/v1/", UriKind.Absolute);

    /// <summary>
    /// Optional bearer token to apply at client startup, skipping an explicit
    /// <see cref="GoAffProClient.LoginAsync"/> call.
    /// </summary>
    public string? BearerToken { get; set; }

    /// <summary>
    /// HTTP timeout applied to individual API requests. Defaults to 30 seconds.
    /// </summary>
    /// <remarks>
    /// The <c>/user/feed/products</c> endpoint has been observed to hang.
    /// Consider a shorter per-request timeout via a second client instance for that endpoint.
    /// </remarks>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Number of retry attempts for transient failures (429, 5xx, network errors).
    /// Defaults to <c>3</c>. Set to <c>0</c> to disable retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Consecutive transient failures required to open the circuit breaker.
    /// Defaults to <c>5</c>. Set to <see langword="null"/> to disable.
    /// </summary>
    public int? CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// How long the circuit breaker stays open (rejecting calls) after tripping.
    /// Defaults to 1 minute.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Default polling interval used by observer and streaming methods when the
    /// caller does not specify one. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan DefaultPollingInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default page size for paginated feed polling. Defaults to <c>100</c>.
    /// </summary>
    public int DefaultPageSize { get; set; } = 100;
}
