using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using GoAffPro.Client.Events;
using GoAffPro.Client.Exceptions;
using GoAffPro.Client.Generated.Models;
using GoAffPro.Client.Policies;
using Microsoft.Extensions.Http;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace GoAffPro.Client;

/// <summary>
/// High-level GoAffPro API client exposing generated Kiota API surface and observer loops.
/// </summary>
public sealed class GoAffProClient : IGoAffProClient
{
    private readonly bool _disposeHttpClient;
    private readonly HttpClient _httpClient;
    private readonly IRequestAdapter _requestAdapter;

    /// <summary>
    /// Initializes a new client instance with internally managed <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="options">Runtime client options.</param>
    public GoAffProClient(GoAffProClientOptions options)
        : this(CreateHttpClient(ValidateOptions(options)), options, disposeHttpClient: true)
    {
    }

    /// <summary>
    /// Initializes a new client instance using an externally managed <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">Configured <see cref="HttpClient"/> instance.</param>
    /// <param name="options">Runtime client options.</param>
    public GoAffProClient(HttpClient httpClient, GoAffProClientOptions options)
        : this(httpClient, ValidateOptions(options), disposeHttpClient: false)
    {
    }

    private GoAffProClient(HttpClient httpClient, GoAffProClientOptions options, bool disposeHttpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;

        _httpClient.BaseAddress = BuildBaseUri(options.BaseUrl);
        _httpClient.Timeout = options.Timeout;

        string baseUrl = _httpClient.BaseAddress?.ToString()
                         ?? throw new InvalidOperationException("HttpClient.BaseAddress was not initialized.");

        IAuthenticationProvider authenticationProvider = new AnonymousAuthenticationProvider();
        _requestAdapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: _httpClient)
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
    public DateTimeOffset? AffiliateObserverStartTime { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? PayoutObserverStartTime { get; set; }

    /// <inheritdoc />
    public event EventHandler<OrderDetectedEventArgs>? OrderDetected;

    /// <inheritdoc />
    public event EventHandler<AffiliateDetectedEventArgs>? AffiliateDetected;

    /// <inheritdoc />
    public event EventHandler<PayoutDetectedEventArgs>? PayoutDetected;

    /// <inheritdoc />
    public event EventHandler<ProductDetectedEventArgs>? ProductDetected;

    /// <inheritdoc />
    public event EventHandler<TransactionDetectedEventArgs>? TransactionDetected;

    /// <inheritdoc />
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public event EventHandler<RewardDetectedEventArgs>? RewardDetected;

    /// <summary>
    /// Creates a new client and logs in immediately using the provided credentials.
    /// </summary>
    /// <param name="email">Affiliate account email.</param>
    /// <param name="password">Affiliate account password.</param>
    /// <param name="cancellationToken">Cancellation token for the login request.</param>
    /// <returns>A logged-in client instance with bearer token applied.</returns>
    public static async Task<GoAffProClient> CreateLoggedInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        GoAffProClient client = new(new GoAffProClientOptions());
        _ = await client.LoginAsync(email, password, cancellationToken).ConfigureAwait(false);
        return client;
    }

    /// <inheritdoc />
    public async Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var request = new global::GoAffPro.Client.Generated.User.Login.LoginPostRequestBody
        {
            Email = email,
            Password = password,
        };

        LoginResponse response = await ExecuteUserAsync(
                async () => await Api.User.Login.PostAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false)
                      ?? new LoginResponse())
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            throw new GoAffProApiException(
                message: "GoAffPro login response does not contain an access token.",
                statusCode: HttpStatusCode.OK);
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

    /// <inheritdoc />
    public async Task StartEventObserverAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        int validatedPageSize = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? TimeSpan.FromSeconds(30);

        // Referenced intentionally so the temporarily-disabled event remains part of the public surface.
        _ = RewardDetected;

        DateTimeOffset lastOrderPoll = OrderObserverStartTime ?? DateTimeOffset.UtcNow;
        DateTimeOffset lastAffiliatePoll = AffiliateObserverStartTime ?? DateTimeOffset.UtcNow;
        DateTimeOffset lastPayoutPoll = PayoutObserverStartTime ?? DateTimeOffset.UtcNow;
        int? lastProductId = null;
        int? lastTransactionId = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset orderTo = DateTimeOffset.UtcNow;
            IReadOnlyList<UserOrderFeedItem> orders = await PollOrdersAsync(lastOrderPoll, orderTo, validatedPageSize, cancellationToken).ConfigureAwait(false);
            lastOrderPoll = orderTo;

            foreach (UserOrderFeedItem order in orders)
            {
                OrderDetected?.Invoke(this, new OrderDetectedEventArgs(order));
            }

            DateTimeOffset affiliateTo = DateTimeOffset.UtcNow;
            IReadOnlyList<UserTrafficFeedItem> affiliates = await PollAffiliatesAsync(lastAffiliatePoll, affiliateTo, validatedPageSize, cancellationToken).ConfigureAwait(false);
            lastAffiliatePoll = affiliateTo;

            foreach (UserTrafficFeedItem affiliate in affiliates)
            {
                AffiliateDetected?.Invoke(this, new AffiliateDetectedEventArgs(affiliate));
            }

            DateTimeOffset payoutTo = DateTimeOffset.UtcNow;
            IReadOnlyList<UserPayoutFeedItem> payouts = await PollPayoutsAsync(lastPayoutPoll, payoutTo, validatedPageSize, cancellationToken).ConfigureAwait(false);
            lastPayoutPoll = payoutTo;

            foreach (UserPayoutFeedItem payout in payouts)
            {
                PayoutDetected?.Invoke(this, new PayoutDetectedEventArgs(payout));
            }

            (IReadOnlyList<UserProductFeedItem> products, int? nextProductId) = await PollProductsAsync(
                lastProductId,
                validatedPageSize,
                cancellationToken).ConfigureAwait(false);
            lastProductId = nextProductId;

            foreach (UserProductFeedItem product in products)
            {
                ProductDetected?.Invoke(this, new ProductDetectedEventArgs(product));
            }

            (IReadOnlyList<UserTransactionItem> transactions, int? nextTransactionId) = await PollTransactionsAsync(
                lastTransactionId,
                validatedPageSize,
                cancellationToken).ConfigureAwait(false);
            lastTransactionId = nextTransactionId;

            foreach (UserTransactionItem transaction in transactions)
            {
                TransactionDetected?.Invoke(this, new TransactionDetectedEventArgs(transaction));
            }

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserOrderFeedItem> NewOrdersAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int validatedPageSize = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? TimeSpan.FromSeconds(30);

        DateTimeOffset lastPoll = OrderObserverStartTime ?? DateTimeOffset.UtcNow;
        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset to = DateTimeOffset.UtcNow;
            IReadOnlyList<UserOrderFeedItem> orders = await PollOrdersAsync(lastPoll, to, validatedPageSize, cancellationToken).ConfigureAwait(false);
            lastPoll = to;

            foreach (UserOrderFeedItem order in orders)
            {
                yield return order;
            }

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserTrafficFeedItem> NewAffiliatesAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int validatedPageSize = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? TimeSpan.FromSeconds(30);

        DateTimeOffset lastPoll = AffiliateObserverStartTime ?? DateTimeOffset.UtcNow;
        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset to = DateTimeOffset.UtcNow;
            IReadOnlyList<UserTrafficFeedItem> affiliates = await PollAffiliatesAsync(lastPoll, to, validatedPageSize, cancellationToken).ConfigureAwait(false);
            lastPoll = to;

            foreach (UserTrafficFeedItem affiliate in affiliates)
            {
                yield return affiliate;
            }

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserPayoutFeedItem> NewPayoutsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int validatedPageSize = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? TimeSpan.FromSeconds(30);

        DateTimeOffset lastPoll = PayoutObserverStartTime ?? DateTimeOffset.UtcNow;
        while (!cancellationToken.IsCancellationRequested)
        {
            DateTimeOffset to = DateTimeOffset.UtcNow;
            IReadOnlyList<UserPayoutFeedItem> payouts = await PollPayoutsAsync(lastPoll, to, validatedPageSize, cancellationToken).ConfigureAwait(false);
            lastPoll = to;

            foreach (UserPayoutFeedItem payout in payouts)
            {
                yield return payout;
            }

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserProductFeedItem> NewProductsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int validatedPageSize = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? TimeSpan.FromSeconds(30);
        int? lastProductId = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            (IReadOnlyList<UserProductFeedItem> products, int? nextProductId) = await PollProductsAsync(
                lastProductId,
                validatedPageSize,
                cancellationToken).ConfigureAwait(false);
            lastProductId = nextProductId;

            foreach (UserProductFeedItem product in products)
            {
                yield return product;
            }

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserTransactionItem> NewTransactionsAsync(
        TimeSpan? pollingInterval = null,
        int pageSize = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int validatedPageSize = ValidatePageSize(pageSize);
        TimeSpan interval = pollingInterval ?? TimeSpan.FromSeconds(30);
        int? lastTransactionId = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            (IReadOnlyList<UserTransactionItem> transactions, int? nextTransactionId) = await PollTransactionsAsync(
                lastTransactionId,
                validatedPageSize,
                cancellationToken).ConfigureAwait(false);
            lastTransactionId = nextTransactionId;

            foreach (UserTransactionItem transaction in transactions)
            {
                yield return transaction;
            }

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

    /// <summary>
    /// Disposes resources owned by this client instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            if (_requestAdapter is IDisposable disposableAdapter)
            {
                disposableAdapter.Dispose();
            }

            _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously disposes resources owned by this client instance.
    /// </summary>
    /// <returns>A completed value task after disposal.</returns>
    public ValueTask DisposeAsync()
    {
        if (_disposeHttpClient)
        {
            if (_requestAdapter is IDisposable disposableAdapter)
            {
                disposableAdapter.Dispose();
            }

            _httpClient.Dispose();
        }

        return ValueTask.CompletedTask;
    }

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
        DateTimeOffset from,
        DateTimeOffset to,
        int pageSize,
        CancellationToken cancellationToken)
    {
        UserOrderFeedResponse response = await ExecuteUserAsync(async () =>
            await Api.User.Feed.Orders.GetAsync(config =>
            {
                config.QueryParameters.CreatedAtMin = from.ToString("o");
                config.QueryParameters.CreatedAtMax = to.ToString("o");
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, cancellationToken).ConfigureAwait(false)
            ?? new UserOrderFeedResponse()).ConfigureAwait(false);

        return response.Orders is null || response.Orders.Count == 0 ? [] : (IReadOnlyList<UserOrderFeedItem>)response.Orders;
    }

    private async Task<IReadOnlyList<UserTrafficFeedItem>> PollAffiliatesAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int pageSize,
        CancellationToken cancellationToken)
    {
        UserTrafficFeedResponse response = await ExecuteUserAsync(async () =>
            await Api.User.Feed.Traffic.GetAsync(config =>
            {
                config.QueryParameters.StartTime = from.ToString("o");
                config.QueryParameters.EndTime = to.ToString("o");
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, cancellationToken).ConfigureAwait(false)
            ?? new UserTrafficFeedResponse()).ConfigureAwait(false);

        return response.Traffic is null || response.Traffic.Count == 0 ? [] : (IReadOnlyList<UserTrafficFeedItem>)response.Traffic;
    }

    private async Task<IReadOnlyList<UserPayoutFeedItem>> PollPayoutsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int pageSize,
        CancellationToken cancellationToken)
    {
        UserPayoutFeedResponse response = await ExecuteUserAsync(async () =>
            await Api.User.Feed.Payouts.GetAsync(config =>
            {
                config.QueryParameters.StartTime = from.ToString("o");
                config.QueryParameters.EndTime = to.ToString("o");
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, cancellationToken).ConfigureAwait(false)
            ?? new UserPayoutFeedResponse()).ConfigureAwait(false);

        return response.Payouts is null || response.Payouts.Count == 0 ? [] : (IReadOnlyList<UserPayoutFeedItem>)response.Payouts;
    }

    private async Task<(IReadOnlyList<UserProductFeedItem> Products, int? NextProductId)> PollProductsAsync(
        int? lastProductId,
        int pageSize,
        CancellationToken cancellationToken)
    {
        UserProductFeedResponse response = await ExecuteUserAsync(async () =>
            await Api.User.Feed.Products.GetAsync(config =>
            {
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, cancellationToken).ConfigureAwait(false)
            ?? new UserProductFeedResponse()).ConfigureAwait(false);

        if (response.Products is null || response.Products.Count == 0)
        {
            return ([], lastProductId);
        }

        int? maxProductId = response.Products
            .Select(item => ToInt(item.ProductId) ?? ToInt(item.Id))
            .Where(static value => value.HasValue)
            .Select(static value => value!.Value)
            .DefaultIfEmpty(lastProductId ?? int.MinValue)
            .Max();

        if (!lastProductId.HasValue)
        {
            return ([], maxProductId == int.MinValue ? null : maxProductId);
        }

        var products = response.Products
            .Where(item =>
            {
                int? id = ToInt(item.ProductId) ?? ToInt(item.Id);
                return id.HasValue && id.Value > lastProductId.Value;
            })
            .OrderBy(item => (ToInt(item.ProductId) ?? ToInt(item.Id)) ?? int.MinValue)
            .ToList();

        int? next = maxProductId == int.MinValue ? lastProductId : maxProductId;
        return (products, next);
    }

    private async Task<(IReadOnlyList<UserTransactionItem> Transactions, int? NextTransactionId)> PollTransactionsAsync(
        int? lastTransactionId,
        int pageSize,
        CancellationToken cancellationToken)
    {
        UserTransactionFeedResponse response = await ExecuteUserAsync(async () =>
            await Api.User.Feed.Transactions.GetAsync(config =>
            {
                config.QueryParameters.Limit = pageSize;
                config.QueryParameters.Offset = 0;
            }, cancellationToken).ConfigureAwait(false)
            ?? new UserTransactionFeedResponse()).ConfigureAwait(false);

        if (response.Transactions is null || response.Transactions.Count == 0)
        {
            return ([], lastTransactionId);
        }

        int? maxTransactionId = response.Transactions
            .Select(item => item.TxId)
            .Where(static value => value.HasValue)
            .Select(static value => value!.Value)
            .DefaultIfEmpty(lastTransactionId ?? int.MinValue)
            .Max();

        if (!lastTransactionId.HasValue)
        {
            return ([], maxTransactionId == int.MinValue ? null : maxTransactionId);
        }

        var transactions = response.Transactions
            .Where(item => item.TxId.HasValue && item.TxId.Value > lastTransactionId.Value)
            .OrderBy(item => item.TxId ?? int.MinValue)
            .ToList();

        int? next = maxTransactionId == int.MinValue ? lastTransactionId : maxTransactionId;
        return (transactions, next);
    }

    private static int? ToInt(UserProductFeedItem.UserProductFeedItem_product_id? value)
    {
        return value?.Integer.HasValue == true
            ? value.Integer.Value
            : int.TryParse(value?.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : null;
    }

    private static int? ToInt(UserProductFeedItem.UserProductFeedItem_id? value)
    {
        return value?.Integer.HasValue == true
            ? value.Integer.Value
            : int.TryParse(value?.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : null;
    }

    private static HttpClient CreateHttpClient(GoAffProClientOptions options)
    {
        // Ownership is transferred to HttpClient through the constructor.
#pragma warning disable CA2000
        PolicyHttpMessageHandler policyHandler = new(RetryPolicies.CreateCompositePolicy())
        {
            InnerHandler = new HttpClientHandler(),
        };
#pragma warning restore CA2000

        HttpClient client = new(policyHandler, disposeHandler: true)
        {
            BaseAddress = BuildBaseUri(options.BaseUrl),
            Timeout = options.Timeout
        };
        return client;
    }

    private static GoAffProClientOptions ValidateOptions(GoAffProClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options;
    }

    private static int ValidatePageSize(int pageSize)
    {
        return pageSize <= 0
            ? throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than zero.")
            : pageSize;
    }

    private static async Task<T> ExecuteUserAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw ConvertUserException(exception);
        }
    }

    private static GoAffProApiException ConvertUserException(ApiException exception)
    {
        int statusCodeValue = exception.ResponseStatusCode;
        HttpStatusCode statusCode = Enum.IsDefined(typeof(HttpStatusCode), statusCodeValue)
            ? (HttpStatusCode)statusCodeValue
            : HttpStatusCode.InternalServerError;

        return new GoAffProApiException(
            message: exception.Message,
            statusCode: statusCode,
            responseBody: null,
            innerException: exception);
    }
}
