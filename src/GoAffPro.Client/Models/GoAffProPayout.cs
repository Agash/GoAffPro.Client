using System.Text.Json;

namespace GoAffPro.Client.Models;

/// <summary>
/// Represents a single payout item from <c>/user/feed/payouts</c>.
/// </summary>
public sealed record GoAffProPayout
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProPayout"/> record.
    /// </summary>
    /// <param name="id">Unique payout identifier.</param>
    /// <param name="affiliateId">ID of the affiliate who received the payout.</param>
    /// <param name="amount">Payout amount.</param>
    /// <param name="status">Payout status (e.g., "paid", "pending").</param>
    /// <param name="paymentMethod">Method used for the payout.</param>
    /// <param name="transactionId">Transaction identifier for the payout.</param>
    /// <param name="currency">Currency code associated with the payout amount.</param>
    /// <param name="createdAt">Timestamp when the payout was created.</param>
    /// <param name="rawPayload">Original JSON payload returned by the feed endpoint.</param>
    public GoAffProPayout(
        string id,
        string? affiliateId,
        decimal? amount,
        string? status,
        string? paymentMethod,
        string? transactionId,
        string? currency,
        DateTimeOffset? createdAt,
        JsonElement rawPayload)
    {
        Id = id;
        AffiliateId = affiliateId;
        Amount = amount;
        Status = status;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        Currency = currency;
        CreatedAt = createdAt;
        RawPayload = rawPayload;
    }

    /// <summary>
    /// Unique payout identifier.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// ID of the affiliate who received the payout.
    /// </summary>
    public string? AffiliateId { get; init; }

    /// <summary>
    /// Payout amount.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Payout status (e.g., "paid", "pending").
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Method used for the payout.
    /// </summary>
    public string? PaymentMethod { get; init; }

    /// <summary>
    /// Transaction identifier for the payout.
    /// </summary>
    public string? TransactionId { get; init; }

    /// <summary>
    /// Currency code associated with the payout amount.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Timestamp when the payout was created.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Original JSON payload for advanced scenarios not covered by typed properties.
    /// </summary>
    public JsonElement RawPayload { get; init; }
}
