using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using GoAffPro.Client.Events;
using GoAffPro.Client.Exceptions;
using GoAffPro.Client.Generated.Models;
using GoAffPro.Client.Generated.User.Feed.Orders;
using GoAffPro.Client.Generated.User.Sites;
using GoAffPro.Client.Policies;
using Microsoft.Extensions.Http;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using GetSiteFieldsQueryParameterType = GoAffPro.Client.Generated.User.Sites.GetFieldsQueryParameterType;
using GetOrderFieldsQueryParameterType = GoAffPro.Client.Generated.User.Feed.Orders.GetFieldsQueryParameterType;

namespace GoAffPro.Client;

/// <summary>
/// High-level GoAffPro API client that wraps the generated Kiota client with
/// authentication management, resilience policies, and convenience methods suited
/// to streamer/vtuber tooling (Streamer.bot, subathon managers, overlays, etc.).
/// </summary>
/// <remarks>
/// <para>
/// All types returned — whether via convenience methods or <see cref="Api"/>
/// directly — are the same Kiota-generated types. There is no separate model
/// layer; the generated types <em>are</em> the models.
/// </para>
/// <para><b>Quick start</b></para>
/// <code>
/// // Authenticate
/// using var client = await GoAffProClient.CreateLoggedInAsync("you@example.com", "pass");
///
/// // Get your programs and referral links (same UserSite type as Api.User.Sites)
/// foreach (var site in await client.GetSitesAsync())
///     Console.WriteLine($"{site.Name}: {site.ReferralLink}  code={site.RefCode}");
///
/// // Stream new orders in real time
/// await foreach (var order in client.NewOrdersAsync(cancellationToken: cts.Token))
/// {
///     decimal? commission = GoAffProUtils.ParseMonetary(order.Commission?.String);
///     Console.WriteLine($"New order #{order.Number} — commission {commission} {order.Currency}");
/// }
/// </code>
/// </remarks>
public sealed class GoAffProClient : IGoAffProClient
{
    // All known order fields — requested in time-window observer polling.
    private static readonly GetOrderFieldsQueryParameterType[] _orderObserverFields =
    [
        GetOrderFieldsQueryParameterType.Id,
        GetOrderFieldsQueryParameterType.Number,
        GetOrderFieldsQueryParameterType.Total,
        GetOrderFieldsQueryParameterType.Subtotal,
        GetOrderFieldsQueryParameterType.Line_items,
        GetOrderFieldsQueryParameterType.Commission,
        GetOrderFieldsQueryParameterType.Created_at,
        GetOrderFieldsQueryParameterType.Currency,
        GetOrderFieldsQueryParameterType.Site_id,
        GetOrderFieldsQueryParameterType.Sub_id,
        GetOrderFieldsQueryParameterType.Conversion_details,
        GetOrderFieldsQueryParameterType.Website,
        GetOrderFieldsQueryParameterType.Store_name,
    ];

    // All known site fields — requested in GetSitesAsync.
    private static readonly GetSiteFieldsQueryParameterType[] _siteFields =
    [
        GetSiteFieldsQueryParameterType.Id,
        GetSiteFieldsQueryParameterType.Name,
        GetSiteFieldsQueryParameterType.Logo,
        GetSiteFieldsQueryParameterType.Website,
        GetSiteFieldsQueryParameterType.Status,
        GetSiteFieldsQueryParameterType.Currency,
        GetSiteFieldsQueryParameterType.Affiliate_portal,
        GetSiteFieldsQueryParameterType.Ref_code,
        GetSiteFieldsQueryParameterType.Referral_link,
        GetSiteFieldsQueryParameterType.Coupon,
    ];

    private readonly bool _disposeHttpClient;
    private readonly HttpClient _httpClient;
    private readonly IRequestAdapter _requestAdapter;
    private readonly GoAffProClientOptions _options;

    /// <summary>
    /// Initializes a new client instance with an internally managed
    /// <see cref="HttpClient"/> built from <paramref name="options"/>.
    /// </summary>
    /// <param name="options">Runtime client options.</param>
    public GoAffProClient(GoAffProClientOptions options)
        : this(CreateHttpClient(ValidateOptions(options)), ValidateOptions(options), disposeHttpClient: true)
    {
    }

    /// <summary>
    /// Initializes a new client instance using an externally managed
    /// <see cref="HttpClient"/>. The caller retains ownership and disposes it.
    /// </summary>
    /// <param name="httpClient">Pre-configured HTTP client instance.</param>
    /// <param name="options">Runtime client options.</param>
    public GoAffProClient(HttpClient httpClient, GoAffProClientOptions options)
        : this(httpClient, ValidateOptions(options), disposeHttpClient: false)
    {
    }

    private GoAffProClient(HttpClient httpClient, GoAffProClientOptions options, bool disposeHttpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;

        _httpClient.BaseAddress = BuildBaseUri(options.BaseUrl);
        _httpClient.Timeout = options.Timeout;

        string baseUrl = _httpClient.BaseAddress?.ToString()
                         ?? throw new InvalidOperationException("HttpClient.BaseAddress was not initialized.");

        IAuthenticationProvider authProvider = new AnonymousAuthenticationProvider();
        _requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: _httpClient)
        {
            BaseUrl = baseUrl.TrimEnd('/'),
        };

        Api = new global::GoAffPro.Client.Generated.GoAffProApiClient(_requestAdapter);

        if (!string.IsNullOrWhiteSpace(options.BearerToken))
        {
            SetBearerToken(options.BearerToken);
        }
    }

    /// <inheritdoc />
    public global::GoAffPro.Client.Generated.GoAffProApiClient Api { get; }

    /// <inheritdoc />
    public string? BearerToken { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? OrderObserverStartTime { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? TrafficObserverStartTime { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? PayoutObserverStartTime { get; set; }

    /// <inheritdoc />
    public event EventHandler<OrderDetectedEventArgs>? OrderDetected;

    /// <inheritdoc />
    public event EventHandler<TrafficDetectedEventArgs>? TrafficDetected;

    /// <inheritdoc />
    public event EventHandler<PayoutDetectedEventArgs>? PayoutDetected;

    /// <inheritdoc />
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public event EventHandler<RewardDetectedEventArgs>? RewardDetected;

    // =========================================================================
    // Factory
    // =========================================================================

    /// <summary>
    /// Creates a new client and immediately authenticates using the provided credentials.
    /// </summary>
    /// <param name="email">Affiliate account email address.</param>
    /// <param name="password">Affiliate account password.</param>
    /// <param name="options">Optional client options. Defaults to production settings.</param>
    /// <param name="cancellationToken">Cancellation token for the login request.</param>
    /// <returns>A logged-in <see cref="GoAffProClient"/> with bearer token applied.</returns>
    public static async Task<GoAffProClient> CreateLoggedInAsync(
        string email,
        string password,
        GoAffProClientOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        GoAffProClient client = new(options ?? new GoAffProClientOptions());
        _ = await client.LoginAsync(email, password, cancellationToken).ConfigureAwait(false);
        return client;
    }

    // =========================================================================
    // Auth
    // =========================================================================

    /// <inheritdoc />
    public async Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        LoginResponse response = await ExecuteAsync(async () =>
            await Api.User.Login.PostAsync(
                new Generated.User.Login.LoginPostRequestBody { Email = email, Password = password },
                cancellationToken: cancellationToken).ConfigureAwait(false)
            ?? new LoginResponse()).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            throw new GoAffProApiException(
                "GoAffPro login response does not contain an access token.",
                HttpStatusCode.OK);
        }

        SetBearerToken(response.AccessToken);
        return response.AccessToken;
    }

    /// <inheritdoc />
    public void SetBearerToken(string bearerToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bearerToken);
        BearerToken = bearerToken;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    // =========================================================================
    // Convenience single-shot queries
    // Returns the same generated types as Api.* — no separate model layer.
    // =========================================================================

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserSite>> GetSitesAsync(
        string? status = "APPROVED",
        CancellationToken cancellationToken = default)
    {
        UserSiteListResponse response = await ExecuteAsync(async () =>
            await Api.User.Sites.GetAsync(config =>
            {
                config.QueryParameters.Limit = 200; // generous; most affiliates have < 10 programs
                config.QueryParameters.Offset = 0;
                config.QueryParameters.FieldsAsGetFieldsQueryParameterType = _siteFields;
                if (!string.IsNullOrWhiteSpace(status))
                {
                    config.QueryParameters.StatusAsGetStatusQueryParameterType = status.ToUpperInvariant() switch
                    {
                        "APPROVED" => GetStatusQueryParameterType.Approved,
                        "PENDING"  => GetStatusQueryParameterType.Pending,
                        "BLOCKED"  => GetStatusQueryParameterType.Blocked,
                        _          => null,
                    };
                }
            }, cancellationToken).ConfigureAwait(false)
            ?? new UserSiteListResponse()).ConfigureAwait(false);

        return response.Sites is { Count: > 0 }
            ? (IReadOnlyList<UserSite>)response.Sites
            : [];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserStatsAggregateItem>> GetAggregateStatsAsync(
        string? siteIds = null,
        CancellationToken cancellationToken = default)
    {
        // limit + offset are empirically required — endpoint silently returns empty without them.
        UserStatsAggregateResponse response = await ExecuteAsync(async () =>
            await Api.User.Stats.Aggregate.GetAsync(config =>
            {
                config.QueryParameters.SiteIds = siteIds;
                config.QueryParameters.Limit = 200;
                config.QueryParameters.Offset = 0;
                config.QueryParameters.FieldsAsGetFieldsQueryParameterType =
                [
                    Generated.User.Stats.Aggregate.GetFieldsQueryParameterType.Total_sales,
                    Generated.User.Stats.Aggregate.GetFieldsQueryParameterType.Other_commission_earned,
                    Generated.User.Stats.Aggregate.GetFieldsQueryParameterType.Revenue_generated,
                    Generated.User.Stats.Aggregate.GetFieldsQueryParameterType.Sale_commission_earned,
                    Generated.User.Stats.Aggregate.GetFieldsQueryParameterType.Commission_paid,
                ];
            }, cancellationToken).ConfigureAwait(false)
            ?? new UserStatsAggregateResponse()).ConfigureAwait(false);

        return response.Data is { Count: > 0 }
            ? (IReadOnlyList<UserStatsAggregateItem>)response.Data
            : [];
    }

    /// <inheritdoc />
    public async Task<UserCommissionsResponse?> GetCommissionsAsync(
        string siteIds,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
            await Api.User.Commissions.GetAsync(config =>
            {
                config.QueryParameters.SiteIds = siteIds;
            }, cancellationToken).ConfigureAwait(false))
        .ConfigureAwait(false);
    }

    // =========================================================================
    // Observer loop
    // =========================================================================

    /// <inheritdoc />
    public async Task StartEventObserverAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        int validatedPageSize = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;

        _ = RewardDetected; // keep obsolete member in compiled output

        DateTimeOffset lastOrderPoll     = OrderObserverStartTime     ?? DateTimeOffset.UtcNow;
        DateTimeOffset lastTrafficPoll = TrafficObserverStartTime ?? DateTimeOffset.UtcNow;
        DateTimeOffset lastPayoutPoll    = PayoutObserverStartTime    ?? DateTimeOffset.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset orderTo = DateTimeOffset.UtcNow;
            foreach (UserOrderFeedItem order in await PollOrdersAsync(lastOrderPoll, orderTo, validatedPageSize, cancellationToken).ConfigureAwait(false))
                OrderDetected?.Invoke(this, new OrderDetectedEventArgs(order));
            lastOrderPoll = orderTo;

            DateTimeOffset trafficTo = DateTimeOffset.UtcNow;
            foreach (UserTrafficFeedItem item in await PollTrafficAsync(lastTrafficPoll, trafficTo, validatedPageSize, cancellationToken).ConfigureAwait(false))
                TrafficDetected?.Invoke(this, new TrafficDetectedEventArgs(item));
            lastTrafficPoll = trafficTo;

            DateTimeOffset payoutTo = DateTimeOffset.UtcNow;
            foreach (UserPayoutFeedItem payout in await PollPayoutsAsync(lastPayoutPoll, payoutTo, validatedPageSize, cancellationToken).ConfigureAwait(false))
                PayoutDetected?.Invoke(this, new PayoutDetectedEventArgs(payout));
            lastPayoutPoll = payoutTo;

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    // =========================================================================
    // Streaming async enumerables
    // =========================================================================

    /// <inheritdoc />
    public async IAsyncEnumerable<UserOrderFeedItem> NewOrdersAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int size = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;
        DateTimeOffset lastPoll = OrderObserverStartTime ?? DateTimeOffset.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset to = DateTimeOffset.UtcNow;
            foreach (UserOrderFeedItem order in await PollOrdersAsync(lastPoll, to, size, cancellationToken).ConfigureAwait(false))
                yield return order;
            lastPoll = to;
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserTrafficFeedItem> NewTrafficAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int size = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;
        DateTimeOffset lastPoll = TrafficObserverStartTime ?? DateTimeOffset.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset to = DateTimeOffset.UtcNow;
            foreach (UserTrafficFeedItem item in await PollTrafficAsync(lastPoll, to, size, cancellationToken).ConfigureAwait(false))
                yield return item;
            lastPoll = to;
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserPayoutFeedItem> NewPayoutsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int size = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;
        DateTimeOffset lastPoll = PayoutObserverStartTime ?? DateTimeOffset.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset to = DateTimeOffset.UtcNow;
            foreach (UserPayoutFeedItem payout in await PollPayoutsAsync(lastPoll, to, size, cancellationToken).ConfigureAwait(false))
                yield return payout;
            lastPoll = to;
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserProductFeedItem> NewProductsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int size = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;
        int? lastId = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            (IReadOnlyList<UserProductFeedItem> products, int? nextId) = await PollProductsAsync(lastId, size, cancellationToken).ConfigureAwait(false);
            lastId = nextId;
            foreach (UserProductFeedItem product in products)
                yield return product;
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserTransactionItem> NewTransactionsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int size = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? _options.DefaultPollingInterval;
        int? lastId = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            (IReadOnlyList<UserTransactionItem> transactions, int? nextId) = await PollTransactionsAsync(lastId, size, cancellationToken).ConfigureAwait(false);
            lastId = nextId;
            foreach (UserTransactionItem tx in transactions)
                yield return tx;
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public async IAsyncEnumerable<UserRewardFeedItem> NewRewardsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    // =========================================================================
    // Disposal
    // =========================================================================

    /// <summary>Disposes resources owned by this client.</summary>
    public void Dispose()
    {
        if (!_disposeHttpClient) return;
        (_requestAdapter as IDisposable)?.Dispose();
        _httpClient.Dispose();
    }

    /// <summary>Asynchronously disposes resources owned by this client.</summary>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    // =========================================================================
    // Internal helpers
    // =========================================================================

    /// <summary>
    /// Normalizes a base URL to ensure it ends with a trailing slash.
    /// Called by the DI registration to set <see cref="HttpClient.BaseAddress"/>.
    /// </summary>
    internal static Uri BuildBaseUri(Uri? baseUrl)
    {
        string normalized = baseUrl?.ToString() ?? "https://api.goaffpro.com/v1/";
        if (!normalized.EndsWith('/'))
        {
            normalized += "/";
        }

        return new Uri(normalized, UriKind.Absolute);
    }

    private async Task<IReadOnlyList<UserOrderFeedItem>> PollOrdersAsync(
        DateTimeOffset from, DateTimeOffset to, int pageSize, CancellationToken ct)
    {
        // created_at_min/max use ISO-8601 UTC. limit + offset are required by the API
        // or it silently returns empty results (confirmed empirically 2026-02).
        UserOrderFeedResponse response = await ExecuteAsync(async () =>
            await Api.User.Feed.Orders.GetAsync(config =>
            {
                config.QueryParameters.CreatedAtMin = from;
                config.QueryParameters.CreatedAtMax = to;
                config.QueryParameters.FieldsAsGetFieldsQueryParameterType = _orderObserverFields;
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, ct).ConfigureAwait(false)
            ?? new UserOrderFeedResponse()).ConfigureAwait(false);

        return response.Orders is { Count: > 0 } ? (IReadOnlyList<UserOrderFeedItem>)response.Orders : [];
    }

    private async Task<IReadOnlyList<UserTrafficFeedItem>> PollTrafficAsync(
        DateTimeOffset from, DateTimeOffset to, int pageSize, CancellationToken ct)
    {
        // limit + offset are required by the API or it silently returns empty results.
        UserTrafficFeedResponse response = await ExecuteAsync(async () =>
            await Api.User.Feed.Traffic.GetAsync(config =>
            {
                config.QueryParameters.StartTime = from;
                config.QueryParameters.EndTime = to;
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, ct).ConfigureAwait(false)
            ?? new UserTrafficFeedResponse()).ConfigureAwait(false);

        return response.Traffic is { Count: > 0 } ? (IReadOnlyList<UserTrafficFeedItem>)response.Traffic : [];
    }

    private async Task<IReadOnlyList<UserPayoutFeedItem>> PollPayoutsAsync(
        DateTimeOffset from, DateTimeOffset to, int pageSize, CancellationToken ct)
    {
        UserPayoutFeedResponse response = await ExecuteAsync(async () =>
            await Api.User.Feed.Payouts.GetAsync(config =>
            {
                config.QueryParameters.StartTime = from;
                config.QueryParameters.EndTime = to;
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, ct).ConfigureAwait(false)
            ?? new UserPayoutFeedResponse()).ConfigureAwait(false);

        return response.Payouts is { Count: > 0 } ? (IReadOnlyList<UserPayoutFeedItem>)response.Payouts : [];
    }

    private async Task<(IReadOnlyList<UserProductFeedItem>, int? NextId)> PollProductsAsync(
        int? lastId, int pageSize, CancellationToken ct)
    {
        UserProductFeedResponse response = await ExecuteAsync(async () =>
            await Api.User.Feed.Products.GetAsync(config =>
            {
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, ct).ConfigureAwait(false)
            ?? new UserProductFeedResponse()).ConfigureAwait(false);

        if (response.Products is not { Count: > 0 })
            return ([], lastId);

        int? maxId = response.Products
            .Select(p => ToNullableInt(p.ProductId) ?? ToNullableInt(p.Id))
            .Where(v => v.HasValue).Select(v => v!.Value)
            .DefaultIfEmpty(lastId ?? int.MinValue).Max();

        if (!lastId.HasValue)
            return ([], maxId == int.MinValue ? null : maxId);

        var newItems = response.Products
            .Where(p => { int? id = ToNullableInt(p.ProductId) ?? ToNullableInt(p.Id); return id > lastId; })
            .OrderBy(p => ToNullableInt(p.ProductId) ?? ToNullableInt(p.Id))
            .ToList();

        return (newItems, maxId == int.MinValue ? lastId : maxId);
    }

    private async Task<(IReadOnlyList<UserTransactionItem>, int? NextId)> PollTransactionsAsync(
        int? lastId, int pageSize, CancellationToken ct)
    {
        UserTransactionFeedResponse response = await ExecuteAsync(async () =>
            await Api.User.Feed.Transactions.GetAsync(config =>
            {
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, ct).ConfigureAwait(false)
            ?? new UserTransactionFeedResponse()).ConfigureAwait(false);

        if (response.Transactions is not { Count: > 0 })
            return ([], lastId);

        int? maxId = response.Transactions
            .Select(t => t.TxId).Where(v => v.HasValue).Select(v => v!.Value)
            .DefaultIfEmpty(lastId ?? int.MinValue).Max();

        if (!lastId.HasValue)
            return ([], maxId == int.MinValue ? null : maxId);

        var newItems = response.Transactions
            .Where(t => t.TxId > lastId)
            .OrderBy(t => t.TxId)
            .ToList();

        return (newItems, maxId == int.MinValue ? lastId : maxId);
    }

    private static int? ToNullableInt(UserProductFeedItem.UserProductFeedItem_product_id? v) =>
        v?.Integer ?? (int.TryParse(v?.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null);

    private static int? ToNullableInt(UserProductFeedItem.UserProductFeedItem_id? v) =>
        v?.Integer ?? (int.TryParse(v?.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null);

    private static HttpClient CreateHttpClient(GoAffProClientOptions options)
    {
#pragma warning disable CA2000 // ownership transferred to HttpClient
        PolicyHttpMessageHandler handler = new(RetryPolicies.CreateCompositePolicy(options))
        {
            InnerHandler = new HttpClientHandler(),
        };
#pragma warning restore CA2000
        return new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = BuildBaseUri(options.BaseUrl),
            Timeout = options.Timeout,
        };
    }

    private static GoAffProClientOptions ValidateOptions(GoAffProClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options;
    }

    private static int ValidatePageSize(int pageSize) =>
        pageSize > 0 ? pageSize : throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than zero.");

    private static async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            int code = ex.ResponseStatusCode;
            HttpStatusCode status = Enum.IsDefined(typeof(HttpStatusCode), code)
                ? (HttpStatusCode)code
                : HttpStatusCode.InternalServerError;

            throw new GoAffProApiException(ex.Message, status, innerException: ex);
        }
    }
}
