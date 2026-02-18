namespace GoAffPro.Client;

/// <summary>
/// Configuration options for <see cref="GoAffProClient"/>.
/// </summary>
public sealed class GoAffProClientOptions
{
    /// <summary>
    /// Base API URL for GoAffPro. Defaults to <c>https://api.goaffpro.com/v1/</c>.
    /// </summary>
    public Uri BaseUrl { get; set; } = new("https://api.goaffpro.com/v1/", UriKind.Absolute);

    /// <summary>
    /// Optional bearer token to apply at client startup.
    /// </summary>
    public string? BearerToken { get; set; }

    /// <summary>
    /// HTTP timeout used for outbound API requests.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
