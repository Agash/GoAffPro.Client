using System.Text.Json;

namespace GoAffPro.Client.Models;

/// <summary>
/// Represents a single reward item from <c>/user/feed/rewards</c>.
/// </summary>
/// <remarks>
/// Reward feed retrieval is currently disabled in <see cref="GoAffProClient"/> because the endpoint
/// is returning HTTP 404 as observed on 2026-02-18.
/// </remarks>
public sealed record GoAffProReward
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProReward"/> record.
    /// </summary>
    /// <param name="id">Unique reward identifier.</param>
    /// <param name="orderId">Order identifier linked to this reward.</param>
    /// <param name="amount">Reward amount granted to the affiliate.</param>
    /// <param name="currency">Currency code associated with the reward amount.</param>
    /// <param name="createdAt">Timestamp when the reward item was created.</param>
    /// <param name="rawPayload">Original JSON payload returned by the feed endpoint.</param>
    public GoAffProReward(
        string id,
        string? orderId,
        decimal? amount,
        string? currency,
        DateTimeOffset? createdAt,
        JsonElement rawPayload)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        CreatedAt = createdAt;
        RawPayload = rawPayload;
    }

    /// <summary>
    /// Unique reward identifier.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Identifier of the order associated with this reward.
    /// </summary>
    public string? OrderId { get; init; }

    /// <summary>
    /// Reward amount granted to the affiliate.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Currency code associated with the reward amount.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Timestamp when this reward was created.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Original JSON payload for advanced scenarios not covered by typed properties.
    /// </summary>
    public JsonElement RawPayload { get; init; }
}
