using System.Net;
using System.Net.Http.Headers;
using System.Globalization;
using System.Text.Json;
using GoAffPro.Client.Exceptions;
using GoAffPro.Client.Models;
using GoAffPro.Client.Policies;
using Microsoft.Extensions.Http;

namespace GoAffPro.Client;

public sealed class GoAffProClient : IGoAffProClient
{
    private readonly bool _disposeHttpClient;
    private readonly HttpClient _httpClient;

    public GoAffProClient(GoAffProClientOptions options)
        : this(CreateHttpClient(ValidateOptions(options)), options, disposeHttpClient: true)
    {
    }

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

    public global::GoAffPro.Client.Generated.User.GoAffProUserClient User { get; }

    public global::GoAffPro.Client.Generated.Public.GoAffProPublicClient PublicApi { get; }

    public string? BearerToken { get; private set; }

    public static async Task<GoAffProClient> CreateLoggedInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        GoAffProClient client = new(new GoAffProClientOptions());
        await client.LoginAsync(email, password, cancellationToken).ConfigureAwait(false);
        return client;
    }

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

    public void SetBearerToken(string bearerToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bearerToken);

        BearerToken = bearerToken;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    public async Task<IReadOnlyList<GoAffProOrder>> GetOrdersAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        global::GoAffPro.Client.Generated.User.Response6 response = await ExecuteUserAsync(
                () => User.UserFeedOrdersAsync(
                    site_ids: null,
                    since_id: null,
                    max_id: null,
                    created_at_max: null,
                    created_at_min: null,
                    fields: [],
                    limit: limit,
                    offset: offset,
                    cancellationToken))
            .ConfigureAwait(false);

        return MapFeedItems(
            response.Orders ?? Array.Empty<object>(),
            TryMapOrder);
    }

    public async Task<IReadOnlyList<GoAffProAffiliate>> GetAffiliatesAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        global::GoAffPro.Client.Generated.User.Response8 response = await ExecuteUserAsync(
                () => User.UserFeedTrafficAsync(
                    site_ids: null,
                    start_time: null,
                    end_time: null,
                    since_id: null,
                    limit: limit,
                    offset: offset,
                    cancellationToken))
            .ConfigureAwait(false);

        return MapFeedItems(
            response.Traffic ?? Array.Empty<object>(),
            TryMapAffiliate);
    }

    [Obsolete("Disabled because /user/feed/rewards currently returns HTTP 404 (observed on 2026-02-18).")]
    public Task<IReadOnlyList<GoAffProReward>> GetRewardsAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        // Temporarily disabled because the GoAffPro endpoint currently returns HTTP 404.
        _ = limit;
        _ = offset;
        _ = cancellationToken;
        return Task.FromResult<IReadOnlyList<GoAffProReward>>(Array.Empty<GoAffProReward>());
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

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

    private static GoAffProOrder? TryMapOrder(JsonElement payload)
    {
        string? id = TryExtractId(payload, ["id", "order_id"]);
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return new GoAffProOrder(
            Id: id,
            Number: TryGetString(payload, "number"),
            Total: TryGetDecimal(payload, "total"),
            Commission: TryGetDecimal(payload, "commission"),
            Currency: TryGetString(payload, "currency"),
            CreatedAt: TryGetDateTimeOffset(payload, "created_at", "created"),
            RawPayload: payload);
    }

    private static GoAffProAffiliate? TryMapAffiliate(JsonElement payload)
    {
        string? id = TryExtractId(payload, ["affiliate_id", "id", "customer_id"]);
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return new GoAffProAffiliate(
            Id: id,
            Name: TryGetString(payload, "name"),
            Email: TryGetString(payload, "email"),
            CustomerId: TryGetString(payload, "customer_id"),
            RefCode: TryGetString(payload, "ref_code"),
            CreatedAt: TryGetDateTimeOffset(payload, "created_at", "created"),
            RawPayload: payload);
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
