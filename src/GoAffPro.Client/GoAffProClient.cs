using System.Net;
using System.Net.Http.Headers;
using System.Globalization;
using System.Text.Json;
using GoAffPro.Client.Exceptions;
using GoAffPro.Client.Models;
using GoAffPro.Client.Policies;
using Microsoft.Extensions.Http;

namespace GoAffPro.Client;

/// <summary>
/// High-level GoAffPro API client that wraps generated NSwag clients and provides
/// typed feed helpers for common workflows.
/// </summary>
public sealed class GoAffProClient : IGoAffProClient
{
    private readonly bool _disposeHttpClient;
    private readonly HttpClient _httpClient;

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

        User = new global::GoAffPro.Client.Generated.User.GoAffProUserClient(_httpClient)
        {
            BaseUrl = baseUrl,
        };
        PublicApi = new global::GoAffPro.Client.Generated.Public.GoAffProPublicClient(_httpClient)
        {
            BaseUrl = baseUrl,
        };

        if (!string.IsNullOrWhiteSpace(options.BearerToken))
        {
            SetBearerToken(options.BearerToken);
        }
    }

    /// <inheritdoc />
    public global::GoAffPro.Client.Generated.User.GoAffProUserClient User { get; }

    /// <inheritdoc />
    public global::GoAffPro.Client.Generated.Public.GoAffProPublicClient PublicApi { get; }

    /// <inheritdoc />
    public string? BearerToken { get; private set; }

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
        await client.LoginAsync(email, password, cancellationToken).ConfigureAwait(false);
        return client;
    }

    /// <inheritdoc />
    public async Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var request = new global::GoAffPro.Client.Generated.User.Body
        {
            Email = email,
            Password = password,
        };

        global::GoAffPro.Client.Generated.User.Response response = await ExecuteUserAsync(
                () => User.UserLoginAsync(request, cancellationToken))
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Access_token))
        {
            throw new GoAffProApiException(
                message: "GoAffPro login response does not contain an access token.",
                statusCode: HttpStatusCode.OK);
        }

        SetBearerToken(response.Access_token);
        return response.Access_token;
    }

    /// <inheritdoc />
    public void SetBearerToken(string bearerToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bearerToken);

        BearerToken = bearerToken;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GoAffProOrder>> GetOrdersAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        global::GoAffPro.Client.Generated.User.Response6 response = await ExecuteUserAsync(
                () => User.UserFeedOrdersAsync(
                    site_ids: null,
                    since_id: null,
                    max_id: null,
                    created_at_max: toDate?.ToString("o"),
                    created_at_min: from?.ToString("o"),
                    fields: [],
                    limit: limit,
                    offset: offset,
                    cancellationToken))
            .ConfigureAwait(false);

        return MapFeedItems(
            response.Orders ?? Array.Empty<object>(),
            TryMapOrder);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GoAffProAffiliate>> GetAffiliatesAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        global::GoAffPro.Client.Generated.User.Response8 response = await ExecuteUserAsync(
                () => User.UserFeedTrafficAsync(
                    site_ids: null,
                    start_time: from?.ToString("o"),
                    end_time: toDate?.ToString("o"),
                    since_id: null,
                    limit: limit,
                    offset: offset,
                    cancellationToken))
            .ConfigureAwait(false);

        return MapFeedItems(
            response.Traffic ?? Array.Empty<object>(),
            TryMapAffiliate);
    }

    /// <inheritdoc />
    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public Task<IReadOnlyList<GoAffProReward>> GetRewardsAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        _ = from;
        _ = toDate;
        _ = limit;
        _ = offset;
        _ = cancellationToken;
        return Task.FromResult<IReadOnlyList<GoAffProReward>>(Array.Empty<GoAffProReward>());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GoAffProPayout>> GetPayoutsAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        object response = await ExecuteUserAsync(
                () => User.UserFeedPayoutsAsync(
                    site_ids: null,
                    start_time: from?.ToString("o"),
                    end_time: toDate?.ToString("o"),
                    since_id: null,
                    limit: limit,
                    offset: offset,
                    cancellationToken))
            .ConfigureAwait(false);

        return MapFeedItems(
            ExtractPayoutItems(response),
            TryMapPayout);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GoAffProProduct>> GetProductsAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        object response = await ExecuteUserAsync(
                () => User.UserFeedProductsAsync(
                    limit: limit,
                    offset: offset,
                    cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return MapFeedItems(
            ExtractProductItems(response),
            TryMapProduct);
    }

    /// <summary>
    /// Disposes resources owned by this client instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
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

    private static HttpClient CreateHttpClient(GoAffProClientOptions options)
    {
        // Ownership is transferred to HttpClient through the constructor.
#pragma warning disable CA2000
        PolicyHttpMessageHandler policyHandler = new(RetryPolicies.CreateCompositePolicy())
        {
            InnerHandler = new HttpClientHandler(),
        };
#pragma warning restore CA2000

        HttpClient client = new(policyHandler, disposeHandler: true);
        client.BaseAddress = BuildBaseUri(options.BaseUrl);
        client.Timeout = options.Timeout;
        return client;
    }

    private static GoAffProClientOptions ValidateOptions(GoAffProClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options;
    }

    private static async Task<T> ExecuteUserAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (global::GoAffPro.Client.Generated.User.GoAffProUserClientApiException exception)
        {
            throw ConvertUserException(exception);
        }
    }

    private static GoAffProApiException ConvertUserException(global::GoAffPro.Client.Generated.User.GoAffProUserClientApiException exception)
    {
        HttpStatusCode statusCode = Enum.IsDefined(typeof(HttpStatusCode), exception.StatusCode)
            ? (HttpStatusCode)exception.StatusCode
            : HttpStatusCode.InternalServerError;

        return new GoAffProApiException(
            message: exception.Message,
            statusCode: statusCode,
            responseBody: exception.Response,
            innerException: exception);
    }

    private static List<TModel> MapFeedItems<TModel>(
        IEnumerable<object> items,
        Func<JsonElement, TModel?> map)
        where TModel : class
    {
        var results = new List<TModel>();
        foreach (object item in items)
        {
            JsonElement payload = item is JsonElement jsonElement
                ? jsonElement.Clone()
                : JsonSerializer.SerializeToElement(item);

            TModel? mapped = map(payload);
            if (mapped is null)
            {
                continue;
            }

            results.Add(mapped);
        }

        return results;
    }

    private static object[] ExtractPayoutItems(object response)
    {
        if (response is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
        {
            if (jsonElement.TryGetProperty("payouts", out JsonElement payouts))
            {
                return JsonSerializer.Deserialize<object[]>(payouts.GetRawText()) ?? [];
            }
            return [];
        }
        return [];
    }

    private static object[] ExtractProductItems(object response)
    {
        if (response is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
        {
            if (jsonElement.TryGetProperty("products", out JsonElement products))
            {
                return JsonSerializer.Deserialize<object[]>(products.GetRawText()) ?? [];
            }
            return [];
        }
        return [];
    }

    private static GoAffProOrder? TryMapOrder(JsonElement payload)
    {
        string? id = TryExtractId(payload, ["id", "order_id"]);
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return new GoAffProOrder(
            id: id,
            number: TryGetString(payload, "number"),
            total: TryGetDecimal(payload, "total"),
            subtotal: TryGetDecimal(payload, "subtotal"),
            affiliateId: TryGetString(payload, "affiliate_id"),
            commission: TryGetDecimal(payload, "commission"),
            status: TryGetString(payload, "status"),
            currency: TryGetString(payload, "currency"),
            createdAt: TryGetDateTimeOffset(payload, "created_at", "created"),
            rawPayload: payload);
    }

    private static GoAffProAffiliate? TryMapAffiliate(JsonElement payload)
    {
        string? id = TryExtractId(payload, ["affiliate_id", "id", "customer_id"]);
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return new GoAffProAffiliate(
            id: id,
            name: TryGetString(payload, "name"),
            firstName: TryGetString(payload, "first_name"),
            lastName: TryGetString(payload, "last_name"),
            email: TryGetString(payload, "email"),
            customerId: TryGetString(payload, "customer_id"),
            refCode: TryGetString(payload, "ref_code"),
            phone: TryGetString(payload, "phone"),
            country: TryGetString(payload, "country"),
            groupId: TryGetInt32(payload, "group_id"),
            createdAt: TryGetDateTimeOffset(payload, "created_at", "created"),
            rawPayload: payload);
    }

    private static GoAffProReward? TryMapReward(JsonElement payload)
    {
        string? id = TryExtractId(payload, ["id", "reward_id"]);
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return new GoAffProReward(
            id: id,
            affiliateId: TryGetString(payload, "affiliate_id"),
            orderId: TryGetString(payload, "order_id"),
            type: TryGetString(payload, "type"),
            metadata: TryGetString(payload, "metadata"),
            level: TryGetInt32(payload, "level"),
            amount: TryGetDecimal(payload, "amount"),
            status: TryGetString(payload, "status"),
            currency: TryGetString(payload, "currency"),
            createdAt: TryGetDateTimeOffset(payload, "created_at", "created"),
            rawPayload: payload);
    }

    private static GoAffProPayout? TryMapPayout(JsonElement payload)
    {
        string? id = TryExtractId(payload, ["id", "payout_id"]);
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return new GoAffProPayout(
            id: id,
            affiliateId: TryGetString(payload, "affiliate_id"),
            amount: TryGetDecimal(payload, "amount"),
            status: TryGetString(payload, "status"),
            paymentMethod: TryGetString(payload, "payment_method"),
            transactionId: TryGetString(payload, "transaction_id"),
            currency: TryGetString(payload, "currency"),
            createdAt: TryGetDateTimeOffset(payload, "created_at", "created"),
            rawPayload: payload);
    }

    private static GoAffProProduct? TryMapProduct(JsonElement payload)
    {
        string? id = TryExtractId(payload, ["id", "product_id"]);
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return new GoAffProProduct(
            id: id,
            name: TryGetString(payload, "name"),
            description: TryGetString(payload, "description"),
            price: TryGetDecimal(payload, "price"),
            salePrice: TryGetDecimal(payload, "sale_price"),
            imageUrl: TryGetString(payload, "image_url"),
            productUrl: TryGetString(payload, "product_url"),
            category: TryGetString(payload, "category"),
            sku: TryGetString(payload, "sku"),
            currency: TryGetString(payload, "currency"),
            rawPayload: payload);
    }

    private static string? TryExtractId(JsonElement item, IReadOnlyCollection<string> candidates)
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (string candidate in candidates)
        {
            if (!TryGetProperty(item, candidate, out JsonElement value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                string? stringValue = value.GetString();
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    return stringValue;
                }
            }
            else if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            {
                return value.ToString();
            }
        }

        return null;
    }

    private static string? TryGetString(JsonElement item, string propertyName)
    {
        if (!TryGetProperty(item, propertyName, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => value.ToString(),
            _ => null,
        };
    }

    private static decimal? TryGetDecimal(JsonElement item, string propertyName)
    {
        if (!TryGetProperty(item, propertyName, out JsonElement value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out decimal decimalValue))
        {
            return decimalValue;
        }

        if (value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsed))
        {
            return parsed;
        }

        return null;
    }

    private static int? TryGetInt32(JsonElement item, string propertyName)
    {
        if (!TryGetProperty(item, propertyName, out JsonElement value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
        {
            return intValue;
        }

        if (value.ValueKind == JsonValueKind.String
            && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            return parsed;
        }

        return null;
    }

    private static DateTimeOffset? TryGetDateTimeOffset(JsonElement item, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(item, propertyName, out JsonElement value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsed))
            {
                return parsed;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long unix))
            {
                // GoAffPro can return unix timestamp values in seconds.
                return DateTimeOffset.FromUnixTimeSeconds(unix);
            }
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement item, string propertyName, out JsonElement value)
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        if (item.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        foreach (JsonProperty property in item.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
