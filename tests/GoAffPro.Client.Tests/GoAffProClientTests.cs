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
                return TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":"o-1","total":101.25,"commission":"10.5"},{"order_id":"o-2","subtotal":90,"affiliate_id":"5","status":"approved"}],"limit":2,"offset":0,"count":2}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<global::GoAffPro.Client.Models.GoAffProOrder> orders = await client.GetOrdersAsync(limit: 10, offset: 0);

        orders.Select(static order => order.Id).Should().Equal("o-1", "o-2");
        orders[0].Total.Should().Be(101.25m);
        orders[0].Commission.Should().Be(10.5m);
        orders[1].Subtotal.Should().Be(90m);
        orders[1].AffiliateId.Should().Be("5");
        orders[1].Status.Should().Be("approved");
    }

    [Fact]
    public async Task GetRewardsAsync_WhenCalled_DoesNotHitEndpointAndReturnsEmpty()
    {
        bool rewardsEndpointCalled = false;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/rewards", StringComparison.OrdinalIgnoreCase))
            {
                rewardsEndpointCalled = true;
                return TestHttpMessageHandler.JsonResponse("""{"error":"not found"}""", HttpStatusCode.NotFound);
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        // Intentionally calling obsolete API to verify temporary disable behavior.
#pragma warning disable CS0618
        IReadOnlyList<global::GoAffPro.Client.Models.GoAffProReward> rewards = await client.GetRewardsAsync(limit: 10, offset: 0);
#pragma warning restore CS0618

        rewards.Should().BeEmpty();
        rewardsEndpointCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrdersAsync_WithTimeFilters_PassesParametersToRequest()
    {
        Uri? observedUri = null;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            observedUri = request.RequestUri;
            return TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 31, 23, 59, 59, TimeSpan.Zero);
        await client.GetOrdersAsync(from: from, toDate: to, limit: 50);

        observedUri.Should().NotBeNull();
        observedUri!.Query.Should().Contain("created_at_min=");
        observedUri.Query.Should().Contain("created_at_max=");
    }

    [Fact]
    public async Task GetAffiliatesAsync_WithTimeFilters_PassesParametersToRequest()
    {
        Uri? observedUri = null;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            observedUri = request.RequestUri;
            return TestHttpMessageHandler.JsonResponse("""{"traffic":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 31, 23, 59, 59, TimeSpan.Zero);
        await client.GetAffiliatesAsync(from: from, toDate: to, limit: 50);

        observedUri.Should().NotBeNull();
        observedUri!.Query.Should().Contain("start_time=");
        observedUri.Query.Should().Contain("end_time=");
    }

    [Fact]
    public async Task GetPayoutsAsync_WhenCalled_ReturnsMappedPayouts()
    {
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/payouts", StringComparison.OrdinalIgnoreCase))
            {
                return TestHttpMessageHandler.JsonResponse("""{"payouts":[{"id":"p-1","amount":100,"status":"paid"},{"id":"p-2","affiliate_id":"5","amount":50.50,"status":"pending"}]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<global::GoAffPro.Client.Models.GoAffProPayout> payouts = await client.GetPayoutsAsync(limit: 10, offset: 0);

        payouts.Count.Should().Be(2);
        payouts[0].Id.Should().Be("p-1");
        payouts[0].Amount.Should().Be(100m);
        payouts[0].Status.Should().Be("paid");
        payouts[1].AffiliateId.Should().Be("5");
        payouts[1].Amount.Should().Be(50.50m);
    }

    [Fact]
    public async Task GetProductsAsync_WhenCalled_ReturnsMappedProducts()
    {
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/products", StringComparison.OrdinalIgnoreCase))
            {
                return TestHttpMessageHandler.JsonResponse("""{"products":[{"id":"prod-1","name":"Widget","price":29.99},{"id":"prod-2","name":"Gadget","price":49.99,"sale_price":39.99}]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<global::GoAffPro.Client.Models.GoAffProProduct> products = await client.GetProductsAsync(limit: 10, offset: 0);

        products.Count.Should().Be(2);
        products[0].Id.Should().Be("prod-1");
        products[0].Name.Should().Be("Widget");
        products[0].Price.Should().Be(29.99m);
        products[1].SalePrice.Should().Be(39.99m);
    }
}
