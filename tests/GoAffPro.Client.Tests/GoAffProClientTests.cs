using System.Net;
using GoAffPro.Client.Exceptions;
using FluentAssertions;

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

        token.Should().Be("abc123");
        client.BearerToken.Should().Be("abc123");
        observedRequest.Should().NotBeNull();
        observedRequest!.Method.Should().Be(HttpMethod.Post);
        observedRequest.RequestUri!.ToString().Should().Contain("user/login");
        observedBody.Should().Contain("email=demo%40example.test");
        observedBody.Should().Contain("password=secret");
    }

    [Fact]
    public async Task LoginAsync_WhenApiReturnsError_ThrowsGoAffProApiException()
    {
        using var handler = new TestHttpMessageHandler((_, _) =>
            TestHttpMessageHandler.JsonResponse("""{"error":"bad credentials"}""", HttpStatusCode.Unauthorized));

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        Func<Task> action = async () => await client.LoginAsync("demo", "wrong");

        await action.Should().ThrowAsync<GoAffProApiException>();
    }

    [Fact]
    public async Task GetOrdersAsync_WhenFeedContainsOrders_ReturnsMappedOrders()
    {
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                return TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":"o-1","total":101.25,"commission":"10.5"},{"order_id":"o-2"}],"limit":2,"offset":0,"count":2}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<global::GoAffPro.Client.Models.GoAffProOrder> orders = await client.GetOrdersAsync(limit: 10, offset: 0);

        orders.Select(static order => order.Id).Should().Equal("o-1", "o-2");
        orders[0].Total.Should().Be(101.25m);
        orders[0].Commission.Should().Be(10.5m);
    }
}
