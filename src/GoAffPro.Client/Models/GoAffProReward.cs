using System.Text.Json;

namespace GoAffPro.Client.Models;

public sealed record GoAffProReward(
    string Id,
    string? OrderId,
    decimal? Amount,
    string? Currency,
    DateTimeOffset? CreatedAt,
    JsonElement RawPayload);
