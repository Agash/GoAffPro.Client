using System.Net;
using System.Security.Cryptography;
using Polly;
using Polly.Extensions.Http;

namespace GoAffPro.Client.Policies;

public static class RetryPolicies
{
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

    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(static response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1));
    }

    public static IAsyncPolicy<HttpResponseMessage> CreateCompositePolicy()
    {
        return Policy.WrapAsync(CreateTransientRetryPolicy(), CreateCircuitBreakerPolicy());
    }
}
