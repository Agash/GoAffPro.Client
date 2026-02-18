using System.Runtime.CompilerServices;
using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

public sealed class GoAffProEventDetector
{
    private readonly IGoAffProClient _client;
    private readonly TimeSpan _pollingInterval;
    private readonly int _pageSize;
    private readonly HashSet<string> _seenAffiliateIds = [];
    private readonly HashSet<string> _seenOrderIds = [];

    public GoAffProEventDetector(
        IGoAffProClient client,
        TimeSpan? pollingInterval = null,
        int pageSize = 100)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than zero.");
        }

        _client = client;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(30);
        _pageSize = pageSize;
    }

    public event EventHandler<OrderDetectedEventArgs>? OrderDetected;

    public event EventHandler<AffiliateDetectedEventArgs>? AffiliateDetected;

    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public event EventHandler<RewardDetectedEventArgs>? RewardDetected;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Referenced intentionally so the temporarily-disabled event remains part of the public surface.
        _ = RewardDetected;

        while (!cancellationToken.IsCancellationRequested)
        {
            IReadOnlyList<OrderEvent> orders = await PollOrdersAsync(cancellationToken).ConfigureAwait(false);
            foreach (OrderEvent order in orders)
            {
                OrderDetected?.Invoke(this, new OrderDetectedEventArgs(order.Order));
            }

            IReadOnlyList<AffiliateEvent> affiliates = await PollAffiliatesAsync(cancellationToken).ConfigureAwait(false);
            foreach (AffiliateEvent affiliate in affiliates)
            {
                AffiliateDetected?.Invoke(this, new AffiliateDetectedEventArgs(affiliate.Affiliate));
            }

            await Task.Delay(_pollingInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    public async IAsyncEnumerable<OrderEvent> NewOrdersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IReadOnlyList<OrderEvent> events = await PollOrdersAsync(cancellationToken).ConfigureAwait(false);
            foreach (OrderEvent detectedEvent in events)
            {
                yield return detectedEvent;
            }

            await Task.Delay(_pollingInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    public async IAsyncEnumerable<AffiliateEvent> NewAffiliatesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IReadOnlyList<AffiliateEvent> events = await PollAffiliatesAsync(cancellationToken).ConfigureAwait(false);
            foreach (AffiliateEvent detectedEvent in events)
            {
                yield return detectedEvent;
            }

            await Task.Delay(_pollingInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public async IAsyncEnumerable<RewardEvent> NewRewardsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    private async Task<IReadOnlyList<OrderEvent>> PollOrdersAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<GoAffProOrder> orders = await _client.GetOrdersAsync(_pageSize, offset: 0, cancellationToken).ConfigureAwait(false);
        List<GoAffProOrder> newOrders = FilterNewById(orders, _seenOrderIds, static order => order.Id);
        return newOrders.ConvertAll(static order => new OrderEvent(order));
    }

    private async Task<IReadOnlyList<AffiliateEvent>> PollAffiliatesAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<GoAffProAffiliate> affiliates = await _client.GetAffiliatesAsync(_pageSize, offset: 0, cancellationToken).ConfigureAwait(false);
        List<GoAffProAffiliate> newAffiliates = FilterNewById(affiliates, _seenAffiliateIds, static affiliate => affiliate.Id);
        return newAffiliates.ConvertAll(static affiliate => new AffiliateEvent(affiliate));
    }

    private static List<T> FilterNewById<T>(
        IReadOnlyList<T> events,
        HashSet<string> seenIds,
        Func<T, string> idSelector)
    {
        var newEvents = new List<T>();
        foreach (T detectedEvent in events)
        {
            if (!seenIds.Add(idSelector(detectedEvent)))
            {
                continue;
            }

            newEvents.Add(detectedEvent);
        }

        return newEvents;
    }
}
