using System.Globalization;

namespace GoAffPro.Client;

/// <summary>
/// Utility helpers for working with GoAffPro API response values.
/// </summary>
/// <remarks>
/// The GoAffPro API returns most monetary values as decimal strings
/// (e.g. <c>"8.754800"</c>, <c>"1000.23"</c>) rather than JSON numbers.
/// Use <see cref="ParseMonetary"/> to convert these safely.
/// </remarks>
public static class GoAffProUtils
{
    private const string _strictUtcIsoTimestampQueryFormat = "yyyy-MM-dd'T'HH:mm:ss'.000Z'";

    /// <summary>
    /// Parses a monetary decimal string returned by the GoAffPro API using
    /// invariant-culture rules.
    /// </summary>
    /// <param name="value">
    /// Raw string value from the API (e.g. <c>"8.754800"</c>, <c>"1000.23"</c>).
    /// </param>
    /// <returns>
    /// Parsed <see cref="decimal"/>, or <see langword="null"/> when the string is
    /// absent, empty, or not a valid decimal.
    /// </returns>
    /// <example>
    /// <code>
    /// // In the orders feed, commission arrives as a string:
    /// decimal? commission = GoAffProUtils.ParseMonetary(order.Commission?.String);
    ///
    /// // In the aggregate stats, monetary fields are also strings:
    /// decimal? earned = GoAffProUtils.ParseMonetary(
    ///     item.AdditionalData.TryGetValue("sale_commission_earned", out var v)
    ///         ? v?.ToString() : null);
    /// </code>
    /// </example>
    public static decimal? ParseMonetary(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result)
            ? result
            : null;
    }

    /// <summary>
    /// Parses an ISO-8601 timestamp string returned by the GoAffPro API.
    /// </summary>
    /// <param name="value">
    /// Raw timestamp string from the API
    /// (e.g. <c>"2026-02-24T11:57:26.000Z"</c>).
    /// </param>
    /// <returns>
    /// Parsed <see cref="DateTimeOffset"/> in UTC, or <see langword="null"/> when
    /// the string is absent or unparseable.
    /// </returns>
    public static DateTimeOffset? ParseTimestamp(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset result)
            ? result.ToUniversalTime()
            : null;
    }

    /// <summary>
    /// Formats a timestamp using the preferred GoAffPro query-parameter wire form.
    /// </summary>
    /// <param name="value">Timestamp value to convert to UTC.</param>
    /// <returns>A UTC timestamp string formatted as <c>yyyy-MM-ddTHH:mm:ss.000Z</c>.</returns>
    public static string FormatTimestampQuery(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString(_strictUtcIsoTimestampQueryFormat, CultureInfo.InvariantCulture);
    }
}
