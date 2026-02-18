namespace GoAffPro.Client;

public interface IGoAffProClient : IDisposable, IAsyncDisposable
{
    string? BearerToken { get; }

    global::GoAffPro.Client.Generated.User.GoAffProUserClient User { get; }

    global::GoAffPro.Client.Generated.Public.GoAffProPublicClient PublicApi { get; }

    Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    void SetBearerToken(string bearerToken);

    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProOrder>> GetOrdersAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProAffiliate>> GetAffiliatesAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<global::GoAffPro.Client.Models.GoAffProReward>> GetRewardsAsync(
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default);
}
