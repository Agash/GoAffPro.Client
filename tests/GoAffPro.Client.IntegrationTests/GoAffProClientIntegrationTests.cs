using System.Text.Json;
using GoAffPro.Client.Generated.User.Sites;
using AggregateField = GoAffPro.Client.Generated.User.Stats.Aggregate.GetFieldsQueryParameterType;

namespace GoAffPro.Client.IntegrationTests;

public sealed class GoAffProClientIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CoreEndpoints_WhenAuthIsConfigured_ReturnWithoutThrowing()
    {
        var settings = IntegrationTestSettings.Load();
        string? token = await ResolveTokenAsync(settings);
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        await using var client = new GoAffProClient(new GoAffProClientOptions
        {
            BaseUrl = settings.BaseUrl,
            BearerToken = token,
        });

        _ = await client.Api.User.GetAsync(cancellationToken: CancellationToken.None);
        _ = await client.Api.User.Sites.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
            config.QueryParameters.StatusAsGetStatusQueryParameterType = GetStatusQueryParameterType.Approved;
            config.QueryParameters.FieldsAsGetFieldsQueryParameterType =
            [
                GetFieldsQueryParameterType.Id,
                GetFieldsQueryParameterType.Name,
                GetFieldsQueryParameterType.Logo,
            ];
        }, CancellationToken.None);
        _ = await client.Api.User.Stats.Aggregate.GetAsync(config =>
        {
            config.QueryParameters.FieldsAsGetFieldsQueryParameterType =
            [
                AggregateField.Total_sales,
                AggregateField.Revenue_generated,
            ];
        }, CancellationToken.None);
        _ = await client.Api.User.Feed.Orders.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
        }, CancellationToken.None);
        _ = await client.Api.User.Feed.Traffic.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
        }, CancellationToken.None);
        _ = await client.Api.User.Commissions.GetAsync(cancellationToken: CancellationToken.None);
        _ = await client.Api.Public.Sites.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
        }, CancellationToken.None);
    }

    private static async Task<string?> ResolveTokenAsync(IntegrationTestSettings settings)
    {
        string? token = Environment.GetEnvironmentVariable("GOAFFPRO_TEST_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        if (!string.IsNullOrWhiteSpace(settings.Token))
        {
            return settings.Token;
        }

        string? email = Environment.GetEnvironmentVariable("GOAFFPRO_TEST_EMAIL") ?? settings.Email;
        string? password = Environment.GetEnvironmentVariable("GOAFFPRO_TEST_PASSWORD") ?? settings.Password;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        await using var authClient = new GoAffProClient(new GoAffProClientOptions
        {
            BaseUrl = settings.BaseUrl,
        });

        return await authClient.LoginAsync(email, password, CancellationToken.None);
    }
}

internal sealed record IntegrationTestSettings(Uri BaseUrl, string? Token, string? Email, string? Password)
{
    public static IntegrationTestSettings Default { get; } = new(
        new Uri("https://api.goaffpro.com/v1/", UriKind.Absolute),
        null,
        null,
        null);

    public static IntegrationTestSettings Load()
    {
        string path = FindSettingsPath();
        if (!File.Exists(path))
        {
            return Default;
        }

        try
        {
            string json = File.ReadAllText(path);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("GoAffPro", out JsonElement root))
            {
                return Default;
            }

            string baseUrlRaw = root.TryGetProperty(nameof(BaseUrl), out JsonElement baseUrlNode)
                ? baseUrlNode.GetString() ?? Default.BaseUrl.ToString()
                : Default.BaseUrl.ToString();
            Uri baseUrl = Uri.TryCreate(baseUrlRaw, UriKind.Absolute, out Uri? parsed)
                ? parsed
                : Default.BaseUrl;

            string? token = root.TryGetProperty(nameof(Token), out JsonElement tokenNode) ? tokenNode.GetString() : null;
            string? email = root.TryGetProperty(nameof(Email), out JsonElement emailNode) ? emailNode.GetString() : null;
            string? password = root.TryGetProperty(nameof(Password), out JsonElement passwordNode) ? passwordNode.GetString() : null;
            return new IntegrationTestSettings(baseUrl, token, email, password);
        }
        catch (IOException)
        {
            return Default;
        }
        catch (JsonException)
        {
            return Default;
        }
    }

    private static string FindSettingsPath()
    {
        const string fileName = "appsettings.Test.local.json";
        string? current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            string candidate = Path.Combine(current, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            string repoCandidate = Path.Combine(current, "tests", "GoAffPro.Client.IntegrationTests", fileName);
            if (File.Exists(repoCandidate))
            {
                return repoCandidate;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        return fileName;
    }
}
