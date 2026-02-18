using GoAffPro.Client.Events;
using FluentAssertions;

namespace GoAffPro.Client.Tests;

public sealed class GoAffProEventDetectorTests
{
    [Fact]
    public async Task NewOrdersAsync_WhenFeedContainsDuplicates_EmitsOnlyUniqueOrderIds()
    {
        int ordersCallCount = 0;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                ordersCallCount++;
                return ordersCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":1},{"id":1},{"id":2}]}""")
                    : TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);

        IReadOnlyList<OrderEvent> events = await TakeAsync(detector.NewOrdersAsync(), expectedCount: 2);

        events.Select(static item => item.Id).Should().Equal("1", "2");
    }

    [Fact]
    public async Task NewRewardsAsync_WhenFeedContainsRewardIds_EmitsRewards()
    {
        int rewardsCallCount = 0;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/rewards", StringComparison.OrdinalIgnoreCase))
            {
                rewardsCallCount++;
                return rewardsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"rewards":[{"reward_id":"r-1"},{"reward_id":"r-2"}]}""")
                    : TestHttpMessageHandler.JsonResponse("""{"rewards":[]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"rewards":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);

        IReadOnlyList<RewardEvent> events = await TakeAsync(detector.NewRewardsAsync(), expectedCount: 2);

        events.Select(static item => item.Id).Should().Equal("r-1", "r-2");
    }

    [Fact]
    public async Task NewAffiliatesAsync_WhenFeedContainsAffiliateIds_EmitsAffiliates()
    {
        int trafficCallCount = 0;
        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/traffic", StringComparison.OrdinalIgnoreCase))
            {
                trafficCallCount++;
                return trafficCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"traffic":[{"affiliate_id":"a-1"},{"affiliate_id":"a-2"}]}""")
                    : TestHttpMessageHandler.JsonResponse("""{"traffic":[]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"traffic":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);

        IReadOnlyList<AffiliateEvent> events = await TakeAsync(detector.NewAffiliatesAsync(), expectedCount: 2);

        events.Select(static item => item.Id).Should().Equal("a-1", "a-2");
    }

    [Fact]
    public async Task StartAsync_WhenNewEventsAreDetected_RaisesEventHandlers()
    {
        int ordersCallCount = 0;
        int trafficCallCount = 0;
        int rewardsCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                ordersCallCount++;
                return ordersCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":"o-1"}],"limit":1,"offset":0,"count":1}""")
                    : TestHttpMessageHandler.JsonResponse("""{"orders":[],"limit":0,"offset":0,"count":0}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/user/feed/traffic", StringComparison.OrdinalIgnoreCase))
            {
                trafficCallCount++;
                return trafficCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"traffic":[{"affiliate_id":"a-1"}],"limit":1,"offset":0,"count":1}""")
                    : TestHttpMessageHandler.JsonResponse("""{"traffic":[],"limit":0,"offset":0,"count":0}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/user/feed/rewards", StringComparison.OrdinalIgnoreCase))
            {
                rewardsCallCount++;
                return rewardsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"rewards":[{"reward_id":"r-1"}],"limit":1,"offset":0,"count":1}""")
                    : TestHttpMessageHandler.JsonResponse("""{"rewards":[],"limit":0,"offset":0,"count":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);

        var orderIds = new List<string>();
        var affiliateIds = new List<string>();
        var rewardIds = new List<string>();
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(1));

        detector.OrderDetected += (_, args) => orderIds.Add(args.Order.Id);
        detector.AffiliateDetected += (_, args) => affiliateIds.Add(args.Affiliate.Id);
        detector.RewardDetected += (_, args) => rewardIds.Add(args.Reward.Id);

        Task runTask = detector.StartAsync(cancellationTokenSource.Token);
        while (orderIds.Count == 0 || affiliateIds.Count == 0 || rewardIds.Count == 0)
        {
            await Task.Delay(10);
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }
        }

        await cancellationTokenSource.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runTask);

        orderIds.Should().Contain("o-1");
        affiliateIds.Should().Contain("a-1");
        rewardIds.Should().Contain("r-1");
    }

    private static async Task<IReadOnlyList<T>> TakeAsync<T>(IAsyncEnumerable<T> source, int expectedCount)
    {
        List<T> values = [];
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(1));

        try
        {
            await foreach (T item in source.WithCancellation(cancellationTokenSource.Token))
            {
                values.Add(item);
                if (values.Count >= expectedCount)
                {
                    await cancellationTokenSource.CancelAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected once enough items are received.
        }

        return values;
    }
}
