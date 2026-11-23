using System.Runtime.CompilerServices;
using GoAffPro.Client.Events;
using GoAffPro.Client.Exceptions;
using GoAffPro.Client.Generated.Models;
using Polly.CircuitBreaker;

namespace GoAffPro.Client;

public sealed partial class GoAffProClient
{
    /// <inheritdoc />
    public event EventHandler<FeedPolledEventArgs>? FeedPolled;

    /// <inheritdoc />
    public async Task StartEventObserverAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default
    )
    {
        int size = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;

        _ = RewardDetected; // keep obsolete member in compiled output

        DateTimeOffset now = _time.GetUtcNow();
        DateTimeOffset lastOrders = OrderObserverStartTime ?? now;
        DateTimeOffset lastTraffic = TrafficObserverStartTime ?? now;
        DateTimeOffset lastPayouts = PayoutObserverStartTime ?? now;

        while (!cancellationToken.IsCancellationRequested)
        {
            lastOrders = await ObserveAsync(
                    GoAffProFeed.Orders,
                    lastOrders,
                    (from, to, ct) => PollOrdersAsync(from, to, size, ct),
                    order => OrderDetected?.Invoke(this, new OrderDetectedEventArgs(order)),
                    cancellationToken
                )
                .ConfigureAwait(false);
            lastTraffic = await ObserveAsync(
                    GoAffProFeed.Traffic,
                    lastTraffic,
                    (from, to, ct) => PollTrafficAsync(from, to, size, ct),
                    item => TrafficDetected?.Invoke(this, new TrafficDetectedEventArgs(item)),
                    cancellationToken
                )
                .ConfigureAwait(false);
            lastPayouts = await ObserveAsync(
                    GoAffProFeed.Payouts,
                    lastPayouts,
                    (from, to, ct) => PollPayoutsAsync(from, to, size, ct),
                    payout => PayoutDetected?.Invoke(this, new PayoutDetectedEventArgs(payout)),
                    cancellationToken
                )
                .ConfigureAwait(false);

            await Task.Delay(interval, _time, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UserOrderFeedItem> NewOrdersAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default
    )
    {
        int size = ValidatePageSize(pageSize);
        return TimeWindowFeedAsync(
            GoAffProFeed.Orders,
            OrderObserverStartTime,
            pollingInterval,
            (from, to, ct) => PollOrdersAsync(from, to, size, ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UserTrafficFeedItem> NewTrafficAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default
    )
    {
        int size = ValidatePageSize(pageSize);
        return TimeWindowFeedAsync(
            GoAffProFeed.Traffic,
            TrafficObserverStartTime,
            pollingInterval,
            (from, to, ct) => PollTrafficAsync(from, to, size, ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UserPayoutFeedItem> NewPayoutsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default
    )
    {
        int size = ValidatePageSize(pageSize);
        return TimeWindowFeedAsync(
            GoAffProFeed.Payouts,
            PayoutObserverStartTime,
            pollingInterval,
            (from, to, ct) => PollPayoutsAsync(from, to, size, ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UserProductFeedItem> NewProductsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default
    )
    {
        int size = ValidatePageSize(pageSize);
        return CursorFeedAsync(
            GoAffProFeed.Products,
            pollingInterval,
            (lastId, ct) => PollProductsAsync(lastId, size, ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UserTransactionItem> NewTransactionsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default
    )
    {
        int size = ValidatePageSize(pageSize);
        return CursorFeedAsync(
            GoAffProFeed.Transactions,
            pollingInterval,
            (lastId, ct) => PollTransactionsAsync(lastId, size, ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    [Obsolete(
        "Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18)."
    )]
    public async IAsyncEnumerable<UserRewardFeedItem> NewRewardsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _ = cancellationToken;
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    // One poll of a time window: the items between the last poll and now. A failed poll is reported and keeps the
    // window, so the next poll asks for the same items again and none are skipped.
    private async Task<DateTimeOffset> ObserveAsync<T>(
        GoAffProFeed feed,
        DateTimeOffset from,
        Func<DateTimeOffset, DateTimeOffset, CancellationToken, Task<IReadOnlyList<T>>> poll,
        Action<T> found,
        CancellationToken ct
    )
    {
        DateTimeOffset to = _time.GetUtcNow();
        IReadOnlyList<T>? items = await TryPollAsync(feed, ct2 => poll(from, to, ct2), ct)
            .ConfigureAwait(false);
        if (items is null)
        {
            return from;
        }

        foreach (T item in items)
        {
            found(item);
        }

        return to;
    }

    private async IAsyncEnumerable<T> TimeWindowFeedAsync<T>(
        GoAffProFeed feed,
        DateTimeOffset? start,
        TimeSpan? pollingInterval,
        Func<DateTimeOffset, DateTimeOffset, CancellationToken, Task<IReadOnlyList<T>>> poll,
        [EnumeratorCancellation] CancellationToken ct
    )
    {
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;
        DateTimeOffset from = start ?? _time.GetUtcNow();
        while (!ct.IsCancellationRequested)
        {
            DateTimeOffset to = _time.GetUtcNow();
            IReadOnlyList<T>? items = await TryPollAsync(feed, c => poll(from, to, c), ct)
                .ConfigureAwait(false);
            if (items is not null)
            {
                foreach (T item in items)
                {
                    yield return item;
                }

                from = to;
            }

            await Task.Delay(interval, _time, ct).ConfigureAwait(false);
        }
    }

    private async IAsyncEnumerable<T> CursorFeedAsync<T>(
        GoAffProFeed feed,
        TimeSpan? pollingInterval,
        Func<int?, CancellationToken, Task<(IReadOnlyList<T> Items, int? NextId)>> poll,
        [EnumeratorCancellation] CancellationToken ct
    )
    {
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;
        int? lastId = null;
        while (!ct.IsCancellationRequested)
        {
            (IReadOnlyList<T> Items, int? NextId)? page = await TryPollAsync(
                    feed,
                    async c =>
                        ((IReadOnlyList<T> Items, int? NextId)?)
                            await poll(lastId, c).ConfigureAwait(false),
                    ct
                )
                .ConfigureAwait(false);
            if (page is { } result)
            {
                lastId = result.NextId;
                foreach (T item in result.Items)
                {
                    yield return item;
                }
            }

            await Task.Delay(interval, _time, ct).ConfigureAwait(false);
        }
    }

    // Runs one poll and reports it through FeedPolled. A request that fails (network, HTTP status, an open
    // circuit, a timeout) returns null; only cancellation ends the caller's loop.
    private async Task<TResult?> TryPollAsync<TResult>(
        GoAffProFeed feed,
        Func<CancellationToken, Task<TResult>> poll,
        CancellationToken ct
    )
    {
        try
        {
            TResult result = await poll(ct).ConfigureAwait(false);
            FeedPolled?.Invoke(this, new FeedPolledEventArgs(feed, _time.GetUtcNow(), null));
            return result;
        }
        catch (Exception ex)
            when (ex
                    is HttpRequestException
                        or IOException
                        or GoAffProApiException
                        or BrokenCircuitException
                        or TimeoutException
                || (ex is OperationCanceledException && !ct.IsCancellationRequested)
            )
        {
            FeedPolled?.Invoke(this, new FeedPolledEventArgs(feed, _time.GetUtcNow(), ex));
            return default;
        }
    }
}
