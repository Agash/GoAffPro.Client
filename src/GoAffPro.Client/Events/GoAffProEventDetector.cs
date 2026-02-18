using System.Runtime.CompilerServices;
using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Polling-based event detector for GoAffPro feed endpoints.
/// </summary>
/// <remarks>
/// The detector keeps in-memory seen ID sets and does not persist state.
/// </remarks>
public sealed class GoAffProEventDetector
{
    private readonly IGoAffProClient _client;
    private readonly TimeSpan _pollingInterval;
    private readonly int _pageSize;
    private readonly HashSet<string> _seenAffiliateIds = [];
    private readonly HashSet<string> _seenOrderIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProEventDetector"/> class.
    /// </summary>
    /// <param name="client">Client used for feed polling operations.</param>
    /// <param name="pollingInterval">Polling interval. Defaults to 30 seconds.</param>
    /// <param name="pageSize">Number of feed records requested per poll. Must be greater than zero.</param>
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

    /// <summary>
    /// Raised when a new order item is detected.
    /// </summary>
    public event EventHandler<OrderDetectedEventArgs>? OrderDetected;

    /// <summary>
    /// Raised when a new affiliate/traffic item is detected.
    /// </summary>
    public event EventHandler<AffiliateDetectedEventArgs>? AffiliateDetected;

    /// <summary>
    /// Raised when a new reward item is detected.
    /// </summary>
    /// <remarks>
    /// Currently disabled because <c>/user/feed/rewards</c> is returning HTTP 404
    /// as observed on 2026-02-18.
    /// </remarks>
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public event EventHandler<RewardDetectedEventArgs>? RewardDetected;

    /// <summary>
    /// Starts continuous polling and raises events when new items are found.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token used to stop polling.</param>
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

    /// <summary>
    /// Streams newly detected order events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token used to stop polling.</param>
    /// <returns>An async stream of new order events.</returns>
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

    /// <summary>
    /// Streams newly detected affiliate events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token used to stop polling.</param>
    /// <returns>An async stream of new affiliate events.</returns>
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

    /// <summary>
    /// Streams newly detected reward events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token used to stop polling.</param>
    /// <returns>An async stream of reward events.</returns>
    /// <remarks>
    /// Currently disabled because <c>/user/feed/rewards</c> is returning HTTP 404
    /// as observed on 2026-02-18.
    /// </remarks>
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
