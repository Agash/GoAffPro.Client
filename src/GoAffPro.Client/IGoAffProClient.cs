namespace GoAffPro.Client;

/// <summary>
/// Defines the high-level GoAffPro client API used by the library wrapper and event detector.
/// </summary>
public interface IGoAffProClient : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the currently configured bearer token.
    /// </summary>
    string? BearerToken { get; }

    /// <summary>
    /// Gets the generated NSwag client for <c>/user/*</c> endpoints.
    /// </summary>
    global::GoAffPro.Client.Generated.User.GoAffProUserClient User { get; }

    /// <summary>
    /// Gets the generated NSwag client for <c>/public/*</c> endpoints.
    /// </summary>
    global::GoAffPro.Client.Generated.Public.GoAffProPublicClient PublicApi { get; }

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
    /// Fetches order feed items from <c>/user/feed/orders</c>.
    /// </summary>
    /// <param name="from">Filter orders created on or after this timestamp.</param>
    /// <param name="toDate">Filter orders created on or before this timestamp.</param>
    /// <param name="limit">Maximum number of records to request.</param>
    /// <param name="offset">Offset into the feed result set.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>Mapped order feed records.</returns>
    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProOrder>> GetOrdersAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches traffic/affiliate feed items from <c>/user/feed/traffic</c>.
    /// </summary>
    /// <param name="from">Filter traffic created on or after this timestamp.</param>
    /// <param name="toDate">Filter traffic created on or before this timestamp.</param>
    /// <param name="limit">Maximum number of records to request.</param>
    /// <param name="offset">Offset into the feed result set.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>Mapped affiliate feed records.</returns>
    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProAffiliate>> GetAffiliatesAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches reward feed items from <c>/user/feed/rewards</c>.
    /// </summary>
    /// <param name="from">Filter rewards created on or after this timestamp.</param>
    /// <param name="toDate">Filter rewards created on or before this timestamp.</param>
    /// <param name="limit">Maximum number of records to request.</param>
    /// <param name="offset">Offset into the feed result set.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>Mapped reward feed records.</returns>
    /// <remarks>
    /// This method is currently disabled because the endpoint is returning HTTP 404
    /// as observed on 2026-02-18.
    /// </remarks>
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProReward>> GetRewardsAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches payout feed items from <c>/user/feed/payouts</c>.
    /// </summary>
    /// <param name="from">Filter payouts created on or after this timestamp.</param>
    /// <param name="toDate">Filter payouts created on or before this timestamp.</param>
    /// <param name="limit">Maximum number of records to request.</param>
    /// <param name="offset">Offset into the feed result set.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>Mapped payout feed records.</returns>
    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProPayout>> GetPayoutsAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches product feed items from <c>/user/feed/products</c>.
    /// </summary>
    /// <param name="limit">Maximum number of records to request.</param>
    /// <param name="offset">Offset into the feed result set.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>Mapped product feed records.</returns>
    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProProduct>> GetProductsAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);
}
