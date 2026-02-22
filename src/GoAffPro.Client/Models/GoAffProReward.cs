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
    /// <param name="affiliateId">ID of the affiliate who brought the order.</param>
    /// <param name="orderId">Order identifier linked to this reward.</param>
    /// <param name="type">Type of reward.</param>
    /// <param name="metadata">Additional metadata for the reward.</param>
    /// <param name="level">Reward level.</param>
    /// <param name="amount">Reward amount granted to the affiliate.</param>
    /// <param name="status">Reward approval status.</param>
    /// <param name="currency">Currency code associated with the reward amount.</param>
    /// <param name="createdAt">Timestamp when the reward item was created.</param>
    /// <param name="rawPayload">Original JSON payload returned by the feed endpoint.</param>
    public GoAffProReward(
        string id,
        string? affiliateId,
        string? orderId,
        string? type,
        string? metadata,
        int? level,
        decimal? amount,
        string? status,
        string? currency,
        DateTimeOffset? createdAt,
        JsonElement rawPayload)
    {
        Id = id;
        AffiliateId = affiliateId;
        OrderId = orderId;
        Type = type;
        Metadata = metadata;
        Level = level;
        Amount = amount;
        Status = status;
        Currency = currency;
        CreatedAt = createdAt;
        RawPayload = rawPayload;
    }

    /// <summary>
    /// Unique reward identifier.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// ID of the affiliate who brought the order.
    /// </summary>
    public string? AffiliateId { get; init; }

    /// <summary>
    /// Identifier of the order associated with this reward.
    /// </summary>
    public string? OrderId { get; init; }

    /// <summary>
    /// Type of reward (e.g., "signup_bonus", "sale_commission", "target_bonus", "wallet_adjustment", "recruitment_bonus").
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Additional metadata for the reward.
    /// </summary>
    public string? Metadata { get; init; }

    /// <summary>
    /// Reward level.
    /// </summary>
    public int? Level { get; init; }

    /// <summary>
    /// Reward amount granted to the affiliate.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Reward approval status (e.g., "approved" or "rejected").
    /// </summary>
    public string? Status { get; init; }

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
