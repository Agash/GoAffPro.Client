using System.Runtime.CompilerServices;
using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Polling-based event detector for GoAffPro feed endpoints.
/// </summary>
/// <remarks>
/// Uses time-based filtering to fetch only new items since the last poll.
/// The caller is responsible for any persistence of timestamps if needed across application restarts.
/// </remarks>
public sealed class GoAffProEventDetector
{
    private readonly IGoAffProClient _client;
    private readonly TimeSpan _pollingInterval;
    private readonly int _pageSize;
    private DateTimeOffset _lastOrderPoll = DateTimeOffset.UtcNow;
    private DateTimeOffset _lastAffiliatePoll = DateTimeOffset.UtcNow;

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
    /// Gets or sets the starting timestamp for order polling.
    /// </summary>
    /// <remarks>
    /// Set this before starting to backfill historical orders.
    /// </remarks>
    public DateTimeOffset? OrderStartTime { get; set; }

    /// <summary>
    /// Gets or sets the starting timestamp for affiliate polling.
    /// </summary>
    /// <remarks>
    /// Set this before starting to backfill historical affiliates.
    /// </remarks>
    public DateTimeOffset? AffiliateStartTime { get; set; }

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
        DateTimeOffset from = OrderStartTime ?? _lastOrderPoll;
        DateTimeOffset to = DateTimeOffset.UtcNow;

        IReadOnlyList<GoAffProOrder> orders = await _client
            .GetOrdersAsync(from: from, toDate: to, limit: _pageSize, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _lastOrderPoll = to;

        return orders.Select(order => new OrderEvent(order)).ToList();
    }

    private async Task<IReadOnlyList<AffiliateEvent>> PollAffiliatesAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset from = AffiliateStartTime ?? _lastAffiliatePoll;
        DateTimeOffset to = DateTimeOffset.UtcNow;

        IReadOnlyList<GoAffProAffiliate> affiliates = await _client
            .GetAffiliatesAsync(from: from, toDate: to, limit: _pageSize, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _lastAffiliatePoll = to;

        return affiliates.Select(affiliate => new AffiliateEvent(affiliate)).ToList();
    }
}
