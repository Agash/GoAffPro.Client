using System.Net;
using System.Security.Cryptography;
using Polly;
using Polly.Extensions.Http;

namespace GoAffPro.Client.Policies;

/// <summary>
/// Factory methods for Polly resilience policies used by GoAffPro HTTP calls.
/// </summary>
public static class RetryPolicies
{
    /// <summary>
    /// Creates an exponential backoff retry policy with jitter for transient HTTP failures.
    /// </summary>
    /// <returns>An asynchronous Polly policy for retries.</returns>
    public static IAsyncPolicy<HttpResponseMessage> CreateTransientRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(static response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: static attempt =>
                {
                    int jitterMilliseconds = RandomNumberGenerator.GetInt32(0, 300);
                    return TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(jitterMilliseconds);
                });
    }

    /// <summary>
    /// Creates a circuit-breaker policy for repeated transient failures.
    /// </summary>
    /// <returns>An asynchronous Polly circuit-breaker policy.</returns>
    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(static response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Creates the composed policy used by default HTTP client construction.
    /// </summary>
    /// <returns>A wrapped policy combining retry and circuit breaker behavior.</returns>
    public static IAsyncPolicy<HttpResponseMessage> CreateCompositePolicy()
    {
        return Policy.WrapAsync(CreateTransientRetryPolicy(), CreateCircuitBreakerPolicy());
    }
}
