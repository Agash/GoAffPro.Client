using System.Net;
using System.Security.Cryptography;
using Polly;
using Polly.Extensions.Http;

namespace GoAffPro.Client.Policies;

/// <summary>
/// Factory methods for Polly resilience policies used by GoAffPro HTTP calls.
/// </summary>
/// <remarks>
/// All policies handle the following transient conditions:
/// <list type="bullet">
///   <item><description>HTTP 5xx server errors</description></item>
///   <item><description>Network-level failures (<see cref="HttpRequestException"/>)</description></item>
///   <item><description>HTTP 429 Too Many Requests</description></item>
/// </list>
/// </remarks>
public static class RetryPolicies
{
    /// <summary>
    /// Creates an exponential back-off retry policy with per-attempt jitter.
    /// </summary>
    /// <param name="retryCount">
    /// Maximum retry attempts. Pass <c>0</c> to return a no-op policy.
    /// Defaults to <c>3</c>.
    /// </param>
    /// <returns>An async Polly retry policy.</returns>
    public static IAsyncPolicy<HttpResponseMessage> CreateTransientRetryPolicy(int retryCount = 3)
    {
        return retryCount <= 0
            ? Policy.NoOpAsync<HttpResponseMessage>()
            : HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(static r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: retryCount,
                sleepDurationProvider: static attempt =>
                {
                    // Exponential back-off: 2s, 4s, 8s … plus 0–300 ms of jitter.
                    int jitter = RandomNumberGenerator.GetInt32(0, 300);
                    return TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(jitter);
                });
    }

    /// <summary>
    /// Creates a circuit-breaker policy that opens after consecutive transient failures.
    /// </summary>
    /// <param name="handledEventsAllowedBeforeBreaking">
    /// Consecutive failures required to trip the breaker. Pass <see langword="null"/>
    /// or <c>0</c> to disable. Defaults to <c>5</c>.
    /// </param>
    /// <param name="durationOfBreak">
    /// How long the circuit stays open after tripping. Defaults to 1 minute.
    /// </param>
    /// <returns>An async Polly circuit-breaker policy, or a no-op when disabled.</returns>
    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(
        int? handledEventsAllowedBeforeBreaking = 5,
        TimeSpan? durationOfBreak = null)
    {
        return handledEventsAllowedBeforeBreaking is not > 0
            ? Policy.NoOpAsync<HttpResponseMessage>()
            : HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(static r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking.Value,
                durationOfBreak: durationOfBreak ?? TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Creates the composed resilience policy from <paramref name="options"/>,
    /// wrapping retry around circuit-breaker.
    /// Used by default <see cref="System.Net.Http.HttpClient"/> construction
    /// inside <see cref="GoAffProClient"/>.
    /// </summary>
    /// <param name="options">
    /// Options that drive retry count, circuit threshold, and break duration.
    /// When <see langword="null"/>, production defaults are used.
    /// </param>
    public static IAsyncPolicy<HttpResponseMessage> CreateCompositePolicy(GoAffProClientOptions? options = null)
    {
        return Policy.WrapAsync(
            CreateTransientRetryPolicy(options?.MaxRetries ?? 3),
            CreateCircuitBreakerPolicy(options?.CircuitBreakerThreshold, options?.CircuitBreakerDuration));
    }
}
