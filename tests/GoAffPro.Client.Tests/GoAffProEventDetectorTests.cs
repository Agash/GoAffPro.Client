using FluentAssertions;
using GoAffPro.Client.Events;

namespace GoAffPro.Client.Tests;

public sealed class GoAffProEventDetectorTests
{
    [Fact]
    public async Task NewOrdersAsync_TracksLastPollTime()
    {
        DateTimeOffset? firstCallTime = null;
        DateTimeOffset? secondCallTime = null;
        int ordersCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                ordersCallCount++;
                if (ordersCallCount == 1)
                {
                    firstCallTime = DateTimeOffset.UtcNow;
                    return TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":"o-1"}]}""");
                }
                else
                {
                    secondCallTime = DateTimeOffset.UtcNow;
                    Uri uri = request.RequestUri;
                    return TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
                }
            }

            return TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);

        IReadOnlyList<OrderEvent> firstBatch = await TakeAsync(detector.NewOrdersAsync(), expectedCount: 1);
        await Task.Delay(10);
        IReadOnlyList<OrderEvent> secondBatch = await TakeAsync(detector.NewOrdersAsync(), expectedCount: 1);

        _ = firstBatch.Count.Should().Be(1);
        _ = firstBatch[0].Id.Should().Be("o-1");
        _ = secondBatch.Count.Should().Be(0);
    }

    [Fact]
    public async Task NewAffiliatesAsync_TracksLastPollTime()
    {
        int trafficCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/traffic", StringComparison.OrdinalIgnoreCase))
            {
                trafficCallCount++;
                return trafficCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"traffic":[{"affiliate_id":"a-1"}]}""")
                    : TestHttpMessageHandler.JsonResponse("""{"traffic":[]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"traffic":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);

        IReadOnlyList<AffiliateEvent> events = await TakeAsync(detector.NewAffiliatesAsync(), expectedCount: 1);

        _ = events.Count.Should().Be(1);
        _ = events[0].Id.Should().Be("a-1");
    }

    [Fact]
    public async Task NewOrdersAsync_WithStartTime_UsesStartTimeForFirstPoll()
    {
        bool startTimeUsed = false;
        var startTime = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                startTimeUsed = request.RequestUri.Query.Contains("created_at_min=", StringComparison.Ordinal);
                return TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);
        detector.OrderStartTime = startTime;

        _ = await TakeAsync(detector.NewOrdersAsync(), expectedCount: 0);

        _ = startTimeUsed.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_WhenNewEventsAreDetected_RaisesEventHandlers()
    {
        int ordersCallCount = 0;
        int trafficCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                ordersCallCount++;
                return ordersCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":"o-1"}]}""")
                    : TestHttpMessageHandler.JsonResponse("""{"orders":[]}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/user/feed/traffic", StringComparison.OrdinalIgnoreCase))
            {
                trafficCallCount++;
                return trafficCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"traffic":[{"affiliate_id":"a-1"}]}""")
                    : TestHttpMessageHandler.JsonResponse("""{"traffic":[]}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100);

        var orderIds = new List<string>();
        var affiliateIds = new List<string>();
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(1));

        detector.OrderDetected += (_, args) => orderIds.Add(args.Order.Id);
        detector.AffiliateDetected += (_, args) => affiliateIds.Add(args.Affiliate.Id);

        Task runTask = detector.StartAsync(cancellationTokenSource.Token);
        while (orderIds.Count == 0 || affiliateIds.Count == 0)
        {
            await Task.Delay(10);
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }
        }

        await cancellationTokenSource.CancelAsync();
        try
        {
            await runTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is observed during Task.Delay.
        }

        _ = orderIds.Should().Contain("o-1");
        _ = affiliateIds.Should().Contain("a-1");
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
