using System.Text.Json;

namespace GoAffPro.Client.Models;

/// <summary>
/// Represents a single order item returned by <c>/user/feed/orders</c>.
/// </summary>
public sealed record GoAffProOrder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProOrder"/> record.
    /// </summary>
    /// <param name="id">GoAffPro order identifier.</param>
    /// <param name="number">Store-facing order number, when provided by the API.</param>
    /// <param name="total">Order total from the source store.</param>
    /// <param name="commission">Commission amount attributed to the affiliate for this order.</param>
    /// <param name="currency">Currency code associated with the order amounts.</param>
    /// <param name="createdAt">Timestamp when the order was created.</param>
    /// <param name="rawPayload">Original JSON payload returned by the feed endpoint.</param>
    public GoAffProOrder(
        string id,
        string? number,
        decimal? total,
        decimal? commission,
        string? currency,
        DateTimeOffset? createdAt,
        JsonElement rawPayload)
    {
        Id = id;
        Number = number;
        Total = total;
        Commission = commission;
        Currency = currency;
        CreatedAt = createdAt;
        RawPayload = rawPayload;
    }

    /// <summary>
    /// GoAffPro order identifier.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Store-facing order number, when available.
    /// </summary>
    public string? Number { get; init; }

    /// <summary>
    /// Order total from the source store.
    /// </summary>
    public decimal? Total { get; init; }

    /// <summary>
    /// Commission amount attributed to the affiliate.
    /// </summary>
    public decimal? Commission { get; init; }

    /// <summary>
    /// Currency code associated with monetary values in this order.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Timestamp when the order was created.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Original JSON payload for advanced scenarios not covered by typed properties.
    /// </summary>
    public JsonElement RawPayload { get; init; }
}
