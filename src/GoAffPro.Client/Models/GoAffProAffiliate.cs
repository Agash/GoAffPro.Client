using System.Text.Json;

namespace GoAffPro.Client.Models;

public sealed record GoAffProAffiliate(
    string Id,
    string? Name,
    string? Email,
    string? CustomerId,
    string? RefCode,
    DateTimeOffset? CreatedAt,
    JsonElement RawPayload);
