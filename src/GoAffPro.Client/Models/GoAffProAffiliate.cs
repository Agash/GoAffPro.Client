using System.Text.Json;

namespace GoAffPro.Client.Models;

/// <summary>
/// Represents a single affiliate/traffic item returned by <c>/user/feed/traffic</c>.
/// </summary>
public sealed record GoAffProAffiliate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProAffiliate"/> record.
    /// </summary>
    /// <param name="id">Unique affiliate identifier from the feed payload.</param>
    /// <param name="name">Affiliate display name, if present.</param>
    /// <param name="email">Affiliate email address, if present.</param>
    /// <param name="customerId">Customer identifier associated with the affiliate event.</param>
    /// <param name="refCode">Referral code tied to the affiliate, if present.</param>
    /// <param name="createdAt">Timestamp associated with the traffic event.</param>
    /// <param name="rawPayload">Original JSON payload returned by the feed endpoint.</param>
    public GoAffProAffiliate(
        string id,
        string? name,
        string? email,
        string? customerId,
        string? refCode,
        DateTimeOffset? createdAt,
        JsonElement rawPayload)
    {
        Id = id;
        Name = name;
        Email = email;
        CustomerId = customerId;
        RefCode = refCode;
        CreatedAt = createdAt;
        RawPayload = rawPayload;
    }

    /// <summary>
    /// Unique affiliate identifier extracted from the feed payload.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Affiliate display name when provided by the API.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Affiliate email address when provided by the API.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Customer identifier associated with the traffic event.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    /// Referral code associated with the affiliate.
    /// </summary>
    public string? RefCode { get; init; }

    /// <summary>
    /// Timestamp associated with this feed item.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Original JSON payload for advanced scenarios not covered by typed properties.
    /// </summary>
    public JsonElement RawPayload { get; init; }
}
