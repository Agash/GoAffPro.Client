using GoAffPro.Client.Events;
using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client;

/// <summary>
/// Defines the high-level GoAffPro client API used by the wrapper and event observer loops.
/// </summary>
public interface IGoAffProClient : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the currently configured bearer token.
    /// </summary>
    string? BearerToken { get; }

    /// <summary>
    /// Gets the generated Kiota API client root for all GoAffPro endpoints.
    /// </summary>
    global::GoAffPro.Client.Generated.GoAffProApiClient Api { get; }

    /// <summary>
    /// Gets or sets the starting timestamp for order observer polling.
    /// </summary>
    DateTimeOffset? OrderObserverStartTime { get; set; }

    /// <summary>
    /// Gets or sets the starting timestamp for affiliate observer polling.
    /// </summary>
    DateTimeOffset? AffiliateObserverStartTime { get; set; }

    /// <summary>
    /// Gets or sets the starting timestamp for payout observer polling.
    /// </summary>
    DateTimeOffset? PayoutObserverStartTime { get; set; }

    /// <summary>
    /// Raised when a new order item is detected in observer mode.
    /// </summary>
    event EventHandler<OrderDetectedEventArgs>? OrderDetected;

    /// <summary>
    /// Raised when a new affiliate/traffic item is detected in observer mode.
    /// </summary>
    event EventHandler<AffiliateDetectedEventArgs>? AffiliateDetected;

    /// <summary>
    /// Raised when a new payout item is detected in observer mode.
    /// </summary>
    event EventHandler<PayoutDetectedEventArgs>? PayoutDetected;

    ///// <summary>
    ///// Raised when a new product item is detected in observer mode.
    ///// </summary>
    //event EventHandler<ProductDetectedEventArgs>? ProductDetected;

    ///// <summary>
    ///// Raised when a new transaction item is detected in observer mode.
    ///// </summary>
    //event EventHandler<TransactionDetectedEventArgs>? TransactionDetected;

    /// <summary>
    /// Raised when a new reward item is detected.
    /// </summary>
    /// <remarks>
    /// Currently disabled because <c>/user/feed/rewards</c> returns 404.
    /// </remarks>
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    event EventHandler<RewardDetectedEventArgs>? RewardDetected;

    /// <summary>
    /// Authenticates with email/password and stores the returned bearer token for subsequent requests.
    /// </summary>
    /// <param name="email">Affiliate account email.</param>
    /// <param name="password">Affiliate account password.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>The access token returned by the login endpoint.</returns>
    Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the bearer token used for authenticated user endpoints.
    /// </summary>
    /// <param name="bearerToken">Token value without the <c>Bearer</c> prefix.</param>
    void SetBearerToken(string bearerToken);

    /// <summary>
    /// Starts continuous polling and raises observer events for new feed items.
    /// </summary>
    /// <param name="pollingInterval">Polling interval. Defaults to 30 seconds.</param>
    /// <param name="pageSize">Number of records requested per poll.</param>
    /// <param name="cancellationToken">Cancellation token used to stop polling.</param>
    Task StartEventObserverAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams newly detected order feed events using time-window polling.
    /// </summary>
    IAsyncEnumerable<UserOrderFeedItem> NewOrdersAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams newly detected affiliate feed events using time-window polling.
    /// </summary>
    IAsyncEnumerable<UserTrafficFeedItem> NewAffiliatesAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams newly detected payout feed events using time-window polling.
    /// </summary>
    IAsyncEnumerable<UserPayoutFeedItem> NewPayoutsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams newly detected products by polling the first page and comparing incremental IDs.
    /// </summary>
    IAsyncEnumerable<UserProductFeedItem> NewProductsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams newly detected transactions by polling the first page and comparing incremental transaction IDs.
    /// </summary>
    IAsyncEnumerable<UserTransactionItem> NewTransactionsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams newly detected reward feed events.
    /// </summary>
    /// <remarks>
    /// Currently disabled because <c>/user/feed/rewards</c> returns 404.
    /// </remarks>
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    IAsyncEnumerable<UserRewardFeedItem> NewRewardsAsync(CancellationToken cancellationToken = default);
}
