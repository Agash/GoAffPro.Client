using System.Text.Json;

namespace GoAffPro.Client.Models;

public sealed record GoAffProOrder(
    string Id,
    string? Number,
    decimal? Total,
    decimal? Commission,
    string? Currency,
    DateTimeOffset? CreatedAt,
    JsonElement RawPayload);
