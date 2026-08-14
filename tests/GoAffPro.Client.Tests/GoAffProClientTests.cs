using System.Net;
using GoAffPro.Client.Exceptions;

namespace GoAffPro.Client.Tests;

[TestClass]
public sealed class GoAffProClientTests
{
    [TestMethod]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsAndStoresAccessToken()
    {
        HttpRequestMessage? observedRequest = null;
        string? observedBody = null;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            observedRequest = request;
            observedBody = request.Content?.ReadAsStringAsync(CancellationToken.None).GetAwaiter().GetResult();
            return TestHttpMessageHandler.JsonResponse("""{"access_token":"abc123"}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        string token = await client.LoginAsync("demo@example.test", "secret");

        Assert.AreEqual("abc123", token);
        Assert.AreEqual("abc123", client.BearerToken);
        Assert.IsNotNull(observedRequest);
        Assert.AreEqual(HttpMethod.Post, observedRequest!.Method);
        Assert.Contains("user/login", observedRequest.RequestUri!.ToString());
        Assert.IsNotNull(observedBody);
        Assert.Contains("email=demo%40example.test", observedBody);
        Assert.Contains("password=secret", observedBody);
    }

    [TestMethod]
    public async Task LoginAsync_WhenApiReturnsError_ThrowsGoAffProApiException()
    {
        using var handler = new TestHttpMessageHandler((_, _) =>
            TestHttpMessageHandler.JsonResponse("""{"error":"bad credentials"}""", HttpStatusCode.Unauthorized));

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        _ = await Assert.ThrowsAsync<GoAffProApiException>(
            () => client.LoginAsync("demo", "wrong"));
    }

    [TestMethod]
    public async Task SetBearerToken_WhenCalled_SendsAuthorizationHeaderOnGeneratedRequests()
    {
        HttpRequestMessage? observedRequest = null;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            observedRequest = request;
            return TestHttpMessageHandler.JsonResponse("""{"stores":[],"count":0,"limit":10}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        client.SetBearerToken("token-123");

        _ = await client.Api.Public.Sites.GetAsync();

        Assert.IsNotNull(observedRequest);
        Assert.IsNotNull(observedRequest.Headers.Authorization);
        Assert.AreEqual("Bearer", observedRequest.Headers.Authorization.Scheme);
        Assert.AreEqual("token-123", observedRequest.Headers.Authorization.Parameter);
    }

    [TestMethod]
    public async Task OrdersFeed_WhenDateFiltersAreProvided_UsesPreferredUtcWireFormat()
    {
        HttpRequestMessage? observedRequest = null;
        var createdAtMin = new DateTimeOffset(2026, 1, 14, 23, 54, 3, TimeSpan.FromHours(1));
        var createdAtMax = new DateTimeOffset(2026, 1, 15, 0, 4, 3, TimeSpan.FromHours(1));

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            observedRequest = request;
            return TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":5,"offset":0}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        _ = await client.Api.User.Feed.Orders.GetAsync(config =>
        {
            config.QueryParameters.CreatedAtMin = createdAtMin;
            config.QueryParameters.CreatedAtMax = createdAtMax;
            config.QueryParameters.FieldsAsGetFieldsQueryParameterType = [GoAffPro.Client.Generated.User.Feed.Orders.GetFieldsQueryParameterType.Id];
            config.QueryParameters.Limit = 5;
            config.QueryParameters.Offset = 0;
        });

        Assert.IsNotNull(observedRequest);
        string requestUri = observedRequest!.RequestUri!.ToString();
        Assert.Contains("created_at_min=2026-01-14T22%3A54%3A03.000Z", requestUri);
        Assert.Contains("created_at_max=2026-01-14T23%3A04%3A03.000Z", requestUri);
        Assert.DoesNotContain("%2B00%3A00", requestUri);
        Assert.DoesNotContain(".0000000", requestUri);
    }
}
