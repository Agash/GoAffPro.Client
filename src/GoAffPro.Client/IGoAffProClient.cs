using GoAffPro.Client.Events;
using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client;

/// <summary>
/// Defines the GoAffPro API client surface used by streamer tooling such as
/// Streamer.bot integrations, subathon managers, and overlay tools.
/// </summary>
/// <remarks>
/// <para>
/// All types returned by this interface — whether from convenience helpers or
/// via <see cref="Api"/> directly — are the same generated Kiota types
/// (<see cref="UserSite"/>, <see cref="UserOrderFeedItem"/>, etc.).
/// There is no separate "model layer"; the generated types are the model layer.
/// </para>
/// <para>
/// Two interaction patterns are available:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Direct API:</b> <see cref="Api"/> gives full access to every endpoint
///     with all query parameters. Use this when you need control the convenience
///     methods don't expose.
///   </description></item>
///   <item><description>
///     <b>Convenience + streaming:</b> <c>Get*Async</c> helpers handle the
///     mandatory-but-easy-to-forget parameters (like <c>limit</c>/<c>offset</c>
///     and <c>fields</c>). <c>New*Async</c> streams continuously poll and yield
///     new items, suitable for real-time overlays.
///   </description></item>
/// </list>
/// <para>
/// Monetary values from the API are decimal strings (e.g. <c>"8.754800"</c>).
/// Use <see cref="GoAffProUtils.ParseMonetary"/> to convert them.
/// </para>
/// </remarks>
public interface IGoAffProClient : IDisposable, IAsyncDisposable
{
    // =========================================================================
    // Auth / identity
    // =========================================================================

    /// <summary>
    /// Gets the currently applied bearer token, or <see langword="null"/> when
    /// the client has not yet authenticated.
    /// </summary>
    string? BearerToken { get; }

    /// <summary>
    /// Gets the generated Kiota API client for direct, low-level access to all
    /// GoAffPro endpoints with full query-parameter control.
    /// </summary>
    global::GoAffPro.Client.Generated.GoAffProApiClient Api { get; }

    /// <summary>
    /// Authenticates with email and password, storing the returned bearer token
    /// for subsequent requests.
    /// </summary>
    /// <param name="email">Affiliate account email address.</param>
    /// <param name="password">Affiliate account password.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>The raw access token string returned by the login endpoint.</returns>
    /// <exception cref="Exceptions.GoAffProApiException">
    /// Thrown when the login request fails or the response contains no token.
    /// </exception>
    Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a bearer token directly, bypassing the login flow. Useful when
    /// a token is stored externally (e.g. in a secrets manager or config file).
    /// </summary>
    /// <param name="bearerToken">Token value without the <c>Bearer </c> prefix.</param>
    void SetBearerToken(string bearerToken);

    // =========================================================================
    // Convenience single-shot queries
    // =========================================================================

    /// <summary>
    /// Retrieves the affiliate programs the authenticated user is enrolled in.
    /// </summary>
    /// <param name="status">
    /// Membership status filter. Defaults to <c>"APPROVED"</c>.
    /// Pass <see langword="null"/> to return programs in all statuses.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A read-only list of <see cref="UserSite"/> records. Each contains
    /// <see cref="UserSite.ReferralLink"/>, <see cref="UserSite.RefCode"/>,
    /// <see cref="UserSite.Coupon"/>, and related fields.
    /// </returns>
    /// <remarks>
    /// This is the primary entry point for streamer tooling — it tells you
    /// which affiliate programs you are enrolled in and what your referral links are.
    /// Equivalent to calling <c>Api.User.Sites.GetAsync</c> with <c>fields</c>,
    /// <c>limit</c>, <c>offset</c>, and <c>status</c> already set.
    /// </remarks>
    Task<IReadOnlyList<UserSite>> GetSitesAsync(
        string? status = "APPROVED",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves aggregated statistics across all enrolled stores (or a subset).
    /// </summary>
    /// <param name="siteIds">
    /// Optional comma-separated store IDs to restrict results to.
    /// Pass <see langword="null"/> for all enrolled stores.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A read-only list of per-store <see cref="UserStatsAggregateItem"/> records,
    /// each containing <c>total_sales</c> (count), <c>revenue_generated</c>,
    /// <c>commission_paid</c>, and <c>sale_commission_earned</c> as decimal strings.
    /// Use <see cref="GoAffProUtils.ParseMonetary"/> to parse the monetary strings.
    /// </returns>
    /// <remarks>
    /// Automatically supplies the <c>limit</c>, <c>offset</c>, and <c>fields</c>
    /// parameters that are required for the endpoint to return data.
    /// </remarks>
    Task<IReadOnlyList<UserStatsAggregateItem>> GetAggregateStatsAsync(
        string? siteIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the commission structure for the authenticated affiliate.
    /// Returns the commission tiers for each enrolled store, ordered the same
    /// way as <c>siteIds</c> was ordered in the request.
    /// </summary>
    /// <param name="siteIds">
    /// Store ID(s) to filter to a single store. This parameter is required, can
    /// be a comma-separated list of multiple store IDs, and should reference stores
    /// the affiliate is enrolled in or the positional return value will be a null
    /// value; does not silently fail.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The raw <see cref="UserCommissionsResponse"/> containing <c>standard</c>,
    /// <c>special</c>, and <c>royalties</c> commission tiers per enrolled store.
    /// </returns>
    Task<UserCommissionsResponse?> GetCommissionsAsync(
        string siteIds,
        CancellationToken cancellationToken = default);

    // =========================================================================
    // Observer timestamps
    // =========================================================================

    /// <summary>
    /// Gets or sets the starting timestamp for order observer polling.
    /// The first poll retrieves orders created on or after this time.
    /// Defaults to <see cref="DateTimeOffset.UtcNow"/> when <see langword="null"/>.
    /// </summary>
    DateTimeOffset? OrderObserverStartTime { get; set; }

    /// <summary>
    /// Gets or sets the starting timestamp for traffic observer polling.
    /// Defaults to <see cref="DateTimeOffset.UtcNow"/> when <see langword="null"/>.
    /// </summary>
    DateTimeOffset? TrafficObserverStartTime { get; set; }

    /// <summary>
    /// Gets or sets the starting timestamp for payout observer polling.
    /// Defaults to <see cref="DateTimeOffset.UtcNow"/> when <see langword="null"/>.
    /// </summary>
    DateTimeOffset? PayoutObserverStartTime { get; set; }

    // =========================================================================
    // Observer events
    // =========================================================================

    /// <summary>
    /// Raised when a new order is detected during observer polling.
    /// Subscribe to trigger alerts or overlay updates when a referral converts.
    /// </summary>
    event EventHandler<OrderDetectedEventArgs>? OrderDetected;

    /// <summary>
    /// Raised when a new referral traffic (click) event is detected.
    /// </summary>
    event EventHandler<TrafficDetectedEventArgs>? TrafficDetected;

    /// <summary>
    /// Raised when a new payout is detected during observer polling.
    /// </summary>
    event EventHandler<PayoutDetectedEventArgs>? PayoutDetected;

    /// <summary>
    /// Raised when a new reward item is detected.
    /// Currently disabled — <c>/user/feed/rewards</c> returns HTTP 404 (observed 2026-02-18).
    /// </summary>
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    event EventHandler<RewardDetectedEventArgs>? RewardDetected;

    // =========================================================================
    // Observer loop
    // =========================================================================

    /// <summary>
    /// Starts a continuous polling loop that raises <see cref="OrderDetected"/>,
    /// <see cref="TrafficDetected"/>, and <see cref="PayoutDetected"/> events.
    /// Blocks until <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="pollingInterval">
    /// Time between sweeps. Defaults to
    /// <see cref="GoAffProClientOptions.DefaultPollingInterval"/> (30 s).
    /// </param>
    /// <param name="pageSize">Records requested per poll. Defaults to 100.</param>
    /// <param name="cancellationToken">Token to stop the loop gracefully.</param>
    Task StartEventObserverAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    // =========================================================================
    // Async streaming
    // =========================================================================

    /// <summary>
    /// Streams new <see cref="UserOrderFeedItem"/> objects using time-window polling.
    /// Each yielded value is a new order detected since the previous poll.
    /// </summary>
    /// <param name="pollingInterval">Polling interval. Defaults to 30 s.</param>
    /// <param name="pageSize">Records per poll. Defaults to 100.</param>
    /// <param name="cancellationToken">Token to stop streaming.</param>
    IAsyncEnumerable<UserOrderFeedItem> NewOrdersAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams new <see cref="UserTrafficFeedItem"/> (referral click) events
    /// using time-window polling.
    /// </summary>
    /// <param name="pollingInterval">Polling interval. Defaults to 30 s.</param>
    /// <param name="pageSize">Records per poll. Defaults to 100.</param>
    /// <param name="cancellationToken">Token to stop streaming.</param>
    IAsyncEnumerable<UserTrafficFeedItem> NewTrafficAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams new <see cref="UserPayoutFeedItem"/> objects using time-window polling.
    /// </summary>
    /// <param name="pollingInterval">Polling interval. Defaults to 30 s.</param>
    /// <param name="pageSize">Records per poll. Defaults to 100.</param>
    /// <param name="cancellationToken">Token to stop streaming.</param>
    IAsyncEnumerable<UserPayoutFeedItem> NewPayoutsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams new <see cref="UserProductFeedItem"/> objects using ID-based polling.
    /// </summary>
    /// <param name="pollingInterval">Polling interval. Defaults to 30 s.</param>
    /// <param name="pageSize">Records per poll. Defaults to 100.</param>
    /// <param name="cancellationToken">Token to stop streaming.</param>
    /// <remarks>
    /// <c>/user/feed/products</c> has been observed to hang or time out. Consider
    /// a shorter <see cref="GoAffProClientOptions.Timeout"/> when using this stream.
    /// </remarks>
    IAsyncEnumerable<UserProductFeedItem> NewProductsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams new <see cref="UserTransactionItem"/> objects using ID-based polling.
    /// </summary>
    /// <param name="pollingInterval">Polling interval. Defaults to 30 s.</param>
    /// <param name="pageSize">Records per poll. Defaults to 100.</param>
    /// <param name="cancellationToken">Token to stop streaming.</param>
    /// <remarks>
    /// <c>/user/feed/transactions</c> has been observed to return HTTP 500 with
    /// non-JSON bodies. Handle <see cref="Exceptions.GoAffProApiException"/> in consumers.
    /// </remarks>
    IAsyncEnumerable<UserTransactionItem> NewTransactionsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Currently a no-op. <c>/user/feed/rewards</c> returns HTTP 404 (observed 2026-02-18).
    /// </summary>
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    IAsyncEnumerable<UserRewardFeedItem> NewRewardsAsync(CancellationToken cancellationToken = default);
}
