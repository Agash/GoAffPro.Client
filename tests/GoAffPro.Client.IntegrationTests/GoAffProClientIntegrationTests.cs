using GoAffPro.Client;

namespace GoAffPro.Client.IntegrationTests;

public sealed class GoAffProClientIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetOrdersAsync_WhenTokenIsConfigured_ReturnsFeedWithoutThrowing()
    {
        string? token = Environment.GetEnvironmentVariable("GOAFFPRO_TEST_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        await using var client = new GoAffProClient(new GoAffProClientOptions
        {
            BearerToken = token,
        });

        IReadOnlyList<global::GoAffPro.Client.Models.GoAffProOrder> orders =
            await client.GetOrdersAsync(limit: 5, offset: 0, CancellationToken.None);

        Assert.NotNull(orders);
    }
}
