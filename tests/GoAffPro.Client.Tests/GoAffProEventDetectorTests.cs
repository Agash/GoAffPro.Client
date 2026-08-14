using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Tests;

[TestClass]
public sealed class GoAffProEventDetectorTests
{
    [TestMethod]
    public async Task NewOrdersAsync_TracksLastPollTimeAsync()
    {
        int ordersCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                ordersCallCount++;
                return ordersCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":"o-1"}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":100,"offset":0}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<UserOrderFeedItem> firstBatch = await TakeAsync(client.NewOrdersAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100), expectedCount: 1);
        await Task.Delay(10);
        IReadOnlyList<UserOrderFeedItem> secondBatch = await TakeAsync(client.NewOrdersAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100), expectedCount: 1);

        Assert.HasCount(1, firstBatch);
        Assert.AreEqual("o-1", firstBatch[0].Id?.String);
        Assert.IsEmpty(secondBatch);
    }

    [TestMethod]
    public async Task NewAffiliatesAsync_TracksLastPollTimeAsync()
    {
        int trafficCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/traffic", StringComparison.OrdinalIgnoreCase))
            {
                trafficCallCount++;
                return trafficCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"traffic":[{"affiliate_id":"a-1"}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"traffic":[],"count":0,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"traffic":[],"count":0,"limit":100,"offset":0}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<UserTrafficFeedItem> events = await TakeAsync(client.NewTrafficAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100), expectedCount: 1);

        Assert.HasCount(1, events);
        Assert.AreEqual("a-1", events[0].AffiliateId?.String);
    }

    [TestMethod]
    public async Task NewOrdersAsync_WithStartTime_UsesStartTimeForFirstPollAsync()
    {
        bool startTimeUsed = false;
        var startTime = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                startTimeUsed = request.RequestUri.Query.Contains("created_at_min=", StringComparison.Ordinal);
                return TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":100,"offset":0}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });
        client.OrderObserverStartTime = startTime;

        _ = await TakeAsync(client.NewOrdersAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100), expectedCount: 0);

        Assert.IsTrue(startTimeUsed);
    }

    [TestMethod]
    public async Task NewOrdersAsync_SendsRequiredFieldsQueryAsync()
    {
        string? query = null;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                query = request.RequestUri.Query;
                return TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":100,"offset":0}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        _ = await TakeAsync(client.NewOrdersAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100), expectedCount: 0);

        Assert.IsNotNull(query);
        Assert.Contains("fields=id,number,total,subtotal,line_items,commission,created_at,currency,site_id,sub_id,conversion_details", query);
    }

    [TestMethod]
    public async Task NewPayoutsAsync_TracksLastPollTimeAsync()
    {
        int payoutsCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/payouts", StringComparison.OrdinalIgnoreCase))
            {
                payoutsCallCount++;
                return payoutsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"payouts":[{"id":"p-1","created_at":"2026-01-01T00:00:00Z"}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"payouts":[],"count":0,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<UserPayoutFeedItem> firstBatch = await TakeAsync(client.NewPayoutsAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100), expectedCount: 1);
        await Task.Delay(10);
        IReadOnlyList<UserPayoutFeedItem> secondBatch = await TakeAsync(client.NewPayoutsAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100), expectedCount: 1);

        Assert.HasCount(1, firstBatch);
        Assert.AreEqual("p-1", firstBatch[0].Id?.String);
        Assert.IsEmpty(secondBatch);
    }

    [TestMethod]
    public async Task NewProductsAsync_DetectsIdsAboveInitialBaselineAsync()
    {
        int productsCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/products", StringComparison.OrdinalIgnoreCase))
            {
                productsCallCount++;
                return productsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"products":[{"id":1},{"id":2}],"count":2,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"products":[{"id":2},{"id":3}],"count":2,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<UserProductFeedItem> products = await TakeAsync(
            client.NewProductsAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100),
            expectedCount: 1);

        Assert.HasCount(1, products);
        Assert.AreEqual(3, products[0].Id?.Integer);
    }

    [TestMethod]
    public async Task NewTransactionsAsync_DetectsIdsAboveInitialBaselineAsync()
    {
        int transactionsCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/transactions", StringComparison.OrdinalIgnoreCase))
            {
                transactionsCallCount++;
                return transactionsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"transactions":[{"tx_id":10}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"transactions":[{"tx_id":11}],"count":1,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        IReadOnlyList<UserTransactionItem> transactions = await TakeAsync(
            client.NewTransactionsAsync(pollingInterval: TimeSpan.FromMilliseconds(5), pageSize: 100),
            expectedCount: 1);

        Assert.HasCount(1, transactions);
        Assert.AreEqual(11, transactions[0].TxId);
    }

    [TestMethod]
    public async Task StartEventObserverAsync_WhenNewEventsAreDetected_RaisesEventHandlersAsync()
    {
        int ordersCallCount = 0;
        int trafficCallCount = 0;
        int payoutsCallCount = 0;
        // Product/transaction observer polling is intentionally disabled due to upstream API instability.
        int productsCallCount = 0;
        int transactionsCallCount = 0;

        using var handler = new TestHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/user/feed/orders", StringComparison.OrdinalIgnoreCase))
            {
                ordersCallCount++;
                return ordersCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"orders":[{"id":"o-1"}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"orders":[],"count":0,"limit":100,"offset":0}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/user/feed/traffic", StringComparison.OrdinalIgnoreCase))
            {
                trafficCallCount++;
                return trafficCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"traffic":[{"affiliate_id":"a-1"}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"traffic":[],"count":0,"limit":100,"offset":0}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/user/feed/payouts", StringComparison.OrdinalIgnoreCase))
            {
                payoutsCallCount++;
                return payoutsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"payouts":[{"id":"p-1","created_at":"2026-01-01T00:00:00Z"}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"payouts":[],"count":0,"limit":100,"offset":0}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/user/feed/products", StringComparison.OrdinalIgnoreCase))
            {
                productsCallCount++;
                return productsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"products":[{"id":1},{"id":2}],"count":2,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"products":[{"id":2},{"id":3}],"count":2,"limit":100,"offset":0}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/user/feed/transactions", StringComparison.OrdinalIgnoreCase))
            {
                transactionsCallCount++;
                return transactionsCallCount == 1
                    ? TestHttpMessageHandler.JsonResponse("""{"transactions":[{"tx_id":10}],"count":1,"limit":100,"offset":0}""")
                    : TestHttpMessageHandler.JsonResponse("""{"transactions":[{"tx_id":11}],"count":1,"limit":100,"offset":0}""");
            }

            return TestHttpMessageHandler.JsonResponse("""{}""");
        });

        using var httpClient = new HttpClient(handler);
        using var client = new GoAffProClient(httpClient, new GoAffProClientOptions { BaseUrl = new Uri("https://example.test/v1/", UriKind.Absolute) });

        var orderIds = new List<string>();
        var affiliateIds = new List<string>();
        var payoutIds = new List<string>();
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(1));

        client.OrderDetected += (_, args) => orderIds.Add(args.Order.Id?.String ?? args.Order.OrderId?.String ?? string.Empty);
        client.TrafficDetected += (_, args) => affiliateIds.Add(args.Traffic.AffiliateId?.String ?? args.Traffic.Id?.String ?? string.Empty);
        client.PayoutDetected += (_, args) => payoutIds.Add(args.Payout.Id?.String ?? args.Payout.PayoutId?.String ?? string.Empty);

        //client.ProductDetected += (_, args) =>
        //{
        //    if (args.Product.Id?.Integer is int productId)
        //    {
        //        productIds.Add(productId);
        //    }
        //};
        //client.TransactionDetected += (_, args) =>
        //{
        //    if (args.Transaction.TxId is int txId)
        //    {
        //        transactionIds.Add(txId);
        //    }
        //};

        Task runTask = client.StartEventObserverAsync(
            pollingInterval: TimeSpan.FromMilliseconds(5),
            pageSize: 100,
            cancellationToken: cancellationTokenSource.Token);

        while (orderIds.Count == 0 ||
               affiliateIds.Count == 0 ||
               payoutIds.Count == 0)
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

        Assert.Contains("o-1", orderIds);
        Assert.Contains("a-1", affiliateIds);
        Assert.Contains("p-1", payoutIds);
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
