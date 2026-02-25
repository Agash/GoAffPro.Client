using System.Net;
using FluentAssertions;
using GoAffPro.Client.Exceptions;

namespace GoAffPro.Client.Tests;

public sealed class GoAffProClientTests
{
    [Fact]
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

        _ = token.Should().Be("abc123");
        _ = client.BearerToken.Should().Be("abc123");
        _ = observedRequest.Should().NotBeNull();
        _ = observedRequest!.Method.Should().Be(HttpMethod.Post);
        _ = observedRequest.RequestUri!.ToString().Should().Contain("user/login");
        _ = observedBody.Should().Contain("email=demo%40example.test");
        _ = observedBody.Should().Contain("password=secret");
    }

    [Fact]
    public async Task LoginAsync_WhenApiReturnsError_ThrowsGoAffProApiException()
    {
        using var handler = new TestHttpMessageHandler((_, _) =>
            TestHttpMessageHandler.JsonResponse("""{"error":"bad credentials"}""", HttpStatusCode.Unauthorized));

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        Func<Task> action = async () => await client.LoginAsync("demo", "wrong");

        _ = await action.Should().ThrowAsync<GoAffProApiException>();
    }

    [Fact]
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

        _ = observedRequest.Should().NotBeNull();
        _ = observedRequest!.Headers.Authorization.Should().NotBeNull();
        _ = observedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        _ = observedRequest.Headers.Authorization.Parameter.Should().Be("token-123");
    }
}
