namespace GoAffPro.Client;

public sealed class GoAffProClientOptions
{
    public Uri BaseUrl { get; set; } = new("https://api.goaffpro.com/v1/", UriKind.Absolute);

    public string? BearerToken { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
